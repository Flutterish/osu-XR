using OpenVR.NET;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.XR.Components;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Rendering;
using osu.XR.Input;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using static osu.Framework.XR.Components.Drawable3D.DrawNode3D;

namespace osu.XR.Components.Panels {
	/// <summary>
	/// A 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public abstract class Panel : Model, IHasCollider {
		public PhysicsLayer PhysicsLayer { get; set; } = PhysicsLayer.All;
		public readonly VirtualInputManager EmulatedInput = new VirtualInputManager { RelativeSizeAxes = Axes.Both };
		private PlatformActionContainer platformActions = new() { RelativeSizeAxes = Axes.Both };
		public Container Source => platformActions;
		/// <summary>
		/// Non-stretching scaling applied to the content
		/// </summary>
		public Bindable<Vector2> ContentScale = new( Vector2.One );
		public BufferedCapture SourceCapture { get; } = new BufferedCapture { RelativeSizeAxes = Axes.Both };
		protected bool IsMeshInvalidated = true;

		/// <summary>
		/// Makes the content use the 2D height of this panel and its own width.
		/// </summary>
		public Panel AutosizeX () {
			EmulatedInput.RelativeSizeAxes = Axes.Y;
			SourceCapture.RelativeSizeAxes = Axes.Y;
			platformActions.RelativeSizeAxes = Axes.Y;

			EmulatedInput.AutoSizeAxes = Axes.X;
			SourceCapture.AutoSizeAxes = Axes.X;
			platformActions.AutoSizeAxes = Axes.X;

			return this;
		}
		/// <summary>
		/// Makes the content use the 2D width of this panel and its own height.
		/// </summary>
		public Panel AutosizeY () {
			EmulatedInput.RelativeSizeAxes = Axes.X;
			SourceCapture.RelativeSizeAxes = Axes.X;
			platformActions.RelativeSizeAxes = Axes.X;

			EmulatedInput.AutoSizeAxes = Axes.Y;
			SourceCapture.AutoSizeAxes = Axes.Y;
			platformActions.AutoSizeAxes = Axes.Y;

			return this;
		}
		/// <summary>
		/// Makes the content use the its own width and height.
		/// </summary>
		public Panel AutosizeBoth () {
			EmulatedInput.RelativeSizeAxes = Axes.None;
			SourceCapture.RelativeSizeAxes = Axes.None;
			platformActions.RelativeSizeAxes = Axes.None;

			EmulatedInput.AutoSizeAxes = Axes.Both;
			SourceCapture.AutoSizeAxes = Axes.Both;
			platformActions.AutoSizeAxes = Axes.Both;

			return this;
		}

		public Panel () {
			UseGammaCorrection = true;

			ContentScale.ValueChanged += v => {
				SourceCapture.Size = v.NewValue;
				IsMeshInvalidated = true;
			};

			SourceCapture.Add( EmulatedInput );
			EmulatedInput.Add( platformActions );
			AddDrawable( SourceCapture );

			ShouldBeDepthSorted = true;
		}

		protected abstract void RecalculateMesh ();

		/// <summary>
		/// The texture position from top left.
		/// </summary>
		public Vector2 TexturePositionAt ( int trisIndex, Vector3 position ) {
			var face = Faces[ trisIndex ];
			var barycentric = Triangles.Barycentric( face, position );
			var tris = Mesh.Tris[ trisIndex ];
			var textureCoord =
				  Mesh.TextureCoordinates[ (int)tris.A ] * barycentric.X
				+ Mesh.TextureCoordinates[ (int)tris.B ] * barycentric.Y
				+ Mesh.TextureCoordinates[ (int)tris.C ] * barycentric.Z;
			return new Vector2( MainTexture.Width * textureCoord.X, MainTexture.Height * ( 1 - textureCoord.Y ) );
		}

		private Vector2 lastTextureSize;
		public override void BeforeDraw ( DrawSettings settings ) {
			if ( SourceCapture is null ) return;
			if ( SourceCapture.Capture is null ) return;
			MainTexture = SourceCapture.Capture;
			if ( MainTexture.Size != lastTextureSize ) {
				IsMeshInvalidated = true;
				lastTextureSize = MainTexture.Size;
			}
		}

		protected override void Update () {
			base.Update();
			if ( IsMeshInvalidated ) {
				RecalculateMesh();
			}
		}

		public virtual bool IsColliderEnabled => Source.Any( x => x.IsPresent );
		public override void Show () {
			this.FadeIn( 300, Easing.Out );
			foreach ( var i in Source ) {
				i.Show();
			}
		}
		public override void Hide () {
			this.FadeOut( 300, Easing.Out ).Then().Schedule( () => {
				foreach ( var i in Source ) {
					i.Hide();
				}
			} );
		}
	}
}
