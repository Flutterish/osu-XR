using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components {
	public class HiererchyInspector : FillFlowContainer, IHasName {
		Drawable3D current;
		public HiererchyInspector ( Drawable3D drawable ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			current = drawable;

			if ( current.Parent is not null ) addButton( current.Parent, 20 );
			addButton( current, 30 );
			if ( current is CompositeDrawable3D composite ) {
				composite.ChildAdded += childAdded;
				composite.ChildRemoved += childRemoved;
				foreach ( var i in composite.Children ) {
					childAdded( composite, i );
				}
			}
		}

		private void childRemoved ( Drawable3D parent, Drawable3D child ) {
			if ( child is not ISelfNotInspectable )
				removeButton( child );
		}

		private void childAdded ( Drawable3D parent, Drawable3D child ) {
			if ( child is not ISelfNotInspectable )
				addButton( child, 40 );
		}

		Dictionary<Drawable3D, Drawable> map = new();
		public System.Action<Drawable3D> DrawablePrevieved;
		public System.Action<Drawable3D> DrawableSelected;

		void addButton ( Drawable3D drawable, float indent ) {
			OsuTextFlowContainer text;

			var button = new CalmOsuAnimatedButton {
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = indent },
				Hovered = () => DrawablePrevieved?.Invoke( drawable ),
				HoverLost = () => DrawablePrevieved?.Invoke( null ),
				Action = () => DrawableSelected?.Invoke( drawable )
			};
			if ( drawable == current ) {
				button.Hovered = null;
				button.HoverLost = null;
				button.Action = null;
			}

			button.Add( text = new OsuTextFlowContainer {
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X
			} );

			button.OnUpdate += b => {
				b.Width = DrawWidth - indent;
			};

			text.AddText( " " + drawable.GetInspectorName() );

			map.Add( drawable, button );
			Add( button );
		}
		void removeButton ( Drawable3D drawable ) {
			map.Remove( drawable, out var button );
			Remove( button );
		}

		public string DisplayName => "Hierarchy";

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			if ( current is CompositeDrawable3D composite ) {
				composite.ChildAdded -= childAdded;
				composite.ChildRemoved -= childRemoved;
			}
		}
	}
}
