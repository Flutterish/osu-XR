using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.XR.Components.Panels;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
		private Sidebar sidebar = new();
		private FlatPanel sidebarPanel = new();
		public PanelStack () {
			sidebarPanel.Source.Add( sidebar );
			sidebarPanel.BypassAutoSizeAxes = Axes3D.All;
			sidebarPanel.AutoOffsetAnchorX = 0.5f;
			sidebarPanel.AutoOffsetOriginX = -0.5f;
			sidebarPanel.PanelHeight = 0.5;
			sidebarPanel.Height = 500;
			sidebarPanel.RelativeSizeAxes = Framework.Graphics.Axes.X;
			sidebarPanel.AutosizeX();
			sidebarPanel.PanelAutoScaleAxes = Framework.Graphics.Axes.X;
			Add( sidebarPanel );
			sidebarPanel.Position = new Vector3( 0.02f, 0, 0 );

			Panels.BindCollectionChanged( (_,e) => {
				if ( ignorePanelListEvents ) return;
				if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ) {
					foreach ( FlatPanel i in e.NewItems ) {
						Add( i );
						i.AutoOffsetOriginX = 0.5f;
						i.AutoOffsetAnchorX = 0.5f;
						// HACK we should make out own sidebar
						var button = new SidebarButton();
						( typeof( SidebarButton ).GetField( "iconContainer", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( button ) as ConstrainedIconContainer ).Icon = (i as IHasIcon)?.CreateIcon() ?? new SpriteIcon { Icon = FontAwesome.Solid.QuestionCircle };
						( typeof( SidebarButton ).GetField( "headerText", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( button ) as SpriteText ).Text = (i as IHasName)?.DisplayName ?? "Unnamed Panel";
						button.Action = () => {
							Focus( i );
							sidebar.State = ExpandedState.Contracted;
						};
						sidebar.Add( button );
					}
				}
				else if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ) {
					// TODO remove panels from panel stack
					throw new InvalidOperationException( "This collection does not support this operation yet." );
				}
				else {
					throw new InvalidOperationException( "This collection does not support this operation yet." );
				}
			}, true );
		}

		protected override void Update () {
			base.Update();

			// doing this every frame makes it have an Easing.Out-like curve
			this.MoveTo( TargetPosition, TransitionDuration );
			this.RotateTo( TargetRotation, TransitionDuration );

			for ( int i = 0; i < Panels.Count; i++ ) {
				Panels[ i ].MoveTo( PanelOffset * i, TransitionDuration );
			}
		}

		private bool ignorePanelListEvents = false;
		public void Focus ( FlatPanel panel ) {
			if ( Panels.Contains( panel ) ) {
				ignorePanelListEvents = true;
				Panels.Remove( panel );
				Panels.Insert( 0, panel );
				ignorePanelListEvents = false;
			}
		}

		public override void Hide () {
			foreach ( var i in Panels ) {
				i.Hide();
			}
			sidebar.Hide();
			sidebar.State = ExpandedState.Contracted;
		}

		public override void Show () {
			foreach ( var i in Panels ) {
				i.Show();
			}
			sidebar.Show();
			sidebar.State = ExpandedState.Expanded;
		}
	}

	public interface IHasName {
		string DisplayName { get; }
	}

	public interface IHasIcon {
		Drawable CreateIcon ();
	}
}
