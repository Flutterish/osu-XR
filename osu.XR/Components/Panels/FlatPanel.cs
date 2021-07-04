using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using System;

namespace osu.XR.Components.Panels {
	/// <summary>
	/// A flat 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public class FlatPanel : InteractivePanel {
		public readonly BindableDouble PanelWidthBindable = new( 1 );
		public readonly BindableDouble PanelHeightBindable = new( 1 );
		public double PanelWidth { get => PanelWidthBindable.Value; set => PanelWidthBindable.Value = value; }
		public double PanelHeight { get => PanelHeightBindable.Value; set => PanelHeightBindable.Value = value; }
		private Axes panelAutoScaleAxes = Axes.None;
		public Axes PanelAutoScaleAxes {
			get => panelAutoScaleAxes;
			set {
				panelAutoScaleAxes = value;
				IsMeshInvalidated = true;
			}
		}

		public FlatPanel () {
			PanelWidthBindable.ValueChanged += _ => IsMeshInvalidated = true;
			PanelHeightBindable.ValueChanged += _ => IsMeshInvalidated = true;
		}

		protected override void RecalculateMesh () {
			float width;
			float height;
			if ( PanelAutoScaleAxes == Axes.Both ) {
				throw new InvalidOperationException( $"{nameof( FlatPanel )} {nameof( PanelAutoScaleAxes )} can not be {Axes.Both}. There is no reference size." );
			}
			else if ( PanelAutoScaleAxes == Axes.X ) {
				height = (float)PanelHeight;
				width = height * ( (float)MainTexture.Width / MainTexture.Height );
			}
			else if ( PanelAutoScaleAxes == Axes.Y ) {
				width = (float)PanelWidth;
				height = width / ( (float)MainTexture.Width / MainTexture.Height );
			}
			else {
				width = (float)PanelWidth;
				height = (float)PanelHeight;
			}

			Mesh.Clear();
			Panel.Shapes.MakeFlat( Mesh, width, height );
		}
	}
}
