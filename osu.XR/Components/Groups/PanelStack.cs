using osu.Framework.Bindables;
using osu.XR.Components.Panels;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components.Groups {
	public abstract class PanelStack : XrObject {
		protected abstract Vector3 TargetPosition { get; }
		protected abstract Quaternion TargetRotation { get; }
		public readonly BindableList<FlatPanel> Panels = new();

		protected virtual double TransitionDuration => 100;
		public override bool RemoveCompletedTransforms => true;
		protected virtual Vector3 PanelOffset => new Vector3( 0.04f, 0, 0.02f );
		public PanelStack () {
			Panels.BindCollectionChanged( (_,e) => {
				if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ) {
					foreach ( FlatPanel i in e.NewItems ) {
						Add( i );
					}
				}
				else if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ) {
					foreach ( FlatPanel i in e.NewItems ) {
						Remove( i );
					}
				}
				else {
					throw new InvalidOperationException( "This collection does not support this operation yet." );
				}
			}, true );
		}
		protected override void Update () { // TODO add sidebar
			base.Update();

			// doing this every frame makes it have an Easing.Out-like curve
			this.MoveTo( TargetPosition, TransitionDuration ); // ISSUE detaches from hand when closing
			this.RotateTo( TargetRotation, TransitionDuration );

			if ( Panels.Any() ) {
				var size = new Vector3( Panels.Max( x => x.Mesh.BoundingBox.Size.X ) / 2, 0, 0 );
				for ( int i = 0; i < Panels.Count; i++ ) {
					var panel = Panels[ i ];
					// TODO 3D anchor and origin on XRObject
					// this emulates anchor = rightcentre on XY plane and origin = rightcentre on XY plane
					var offset = new Vector3( panel.Mesh.BoundingBox.Size.X / 2, 0, 0 );
					panel.Offset = size - offset;
					panel.MoveTo( PanelOffset * i, TransitionDuration );
				}
			}
		}

		public void Focus ( FlatPanel panel ) {
			if ( Panels.Contains( panel ) ) {
				Panels.Remove( panel );
				Panels.Insert( 0, panel );
			}
		}

		public override void Hide () {
			foreach ( var i in Panels ) {
				i.Hide();
			}
		}

		public override void Show () {
			foreach ( var i in Panels ) {
				i.Show();
			}
		}
	}
}
