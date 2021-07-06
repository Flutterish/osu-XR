using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.XR.Components.Panels;
using osuTK;
using System;
using System.Reflection;

namespace osu.XR.Components.Groups {
	public abstract class MenuStack<T> : CompositeDrawable3D where T : Drawable3D {
		public readonly BindableList<T> Elements = new();

		public override bool RemoveCompletedTransforms => true;
		private Sidebar sidebar = new();
		private FlatPanel sidebarPanel = new();
		public MenuStack () {
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

			Elements.BindCollectionChanged( (_,e) => {
				if ( ignorePanelListEvents ) return;
				if ( e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ) {
					foreach ( T i in e.NewItems ) {
						Add( i );
						i.AutoOffsetOriginX = 0.5f;
						i.AutoOffsetAnchorX = 0.5f;
						// HACK we should make our own sidebar
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

				applyTransforms();
			}, true );
		}

		protected abstract void ApplyTransformsTo ( T element, int index, float progress );

		private bool ignorePanelListEvents = false;
		public void Focus ( T panel ) {
			if ( Elements.Contains( panel ) ) {
				ignorePanelListEvents = true;
				Elements.Remove( panel );
				Elements.Insert( 0, panel );
				ignorePanelListEvents = false;

				applyTransforms();
			}
		}

		void applyTransforms () {
			for ( int i = 0; i < Elements.Count; i++ ) {
				ApplyTransformsTo( Elements[ i ], i, (float)i / Elements.Count );
			}
		}

		public override void Hide () {
			foreach ( var i in Elements ) {
				i.Hide();
			}
			sidebar.FadeOut( 300 );
			sidebar.State = ExpandedState.Contracted;
		}

		public override void Show () {
			foreach ( var i in Elements ) {
				i.Show();
			}
			sidebar.FadeIn( 300 );
			sidebar.State = ExpandedState.Expanded;
			applyTransforms();
		}
	}
}
