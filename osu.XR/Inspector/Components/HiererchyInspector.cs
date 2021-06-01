using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.XR.Components.Groups;
using osu.XR.Components.Panels;
using osu.XR.Drawables;
using osuTK;
using osuTK.Graphics;
using osuTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components {
	public static class HierarchyIcons {
		public static readonly IconUsage Container = FontAwesome.Solid.Sitemap;
		public static readonly IconUsage HasSettings = FontAwesome.Solid.Cog;
		public static readonly IconUsage HasSettingsPersistent = FontAwesome.Solid.Download;
		public static readonly IconUsage HasVisuals = FontAwesome.Solid.Eye;
		public static readonly IconUsage Selected = FontAwesome.Solid.Search;
		public static readonly IconUsage Experimental = FontAwesome.Solid.Flask;
		public static readonly IconUsage SwapProjection = FontAwesome.Solid.Random;
		public static readonly IconUsage TwoD = FontAwesome.Solid.StickyNote;
		public static readonly IconUsage ThreeD = FontAwesome.Solid.Cube;

		public static readonly Color4 IconColor = Color4.GhostWhite;

		public static Drawable No ( IconUsage icon, float em = 16 ) {
			return new Container {
				Size = new Vector2( em ),
				Children = new Drawable[] {
					new SpriteIcon {
						Icon = icon,
						RelativeSizeAxes = Axes.Both,
						Size = new Vector2( 14f / 16 ),
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre,
						FillMode = FillMode.Fit
					},
					new SpriteIcon {
						Icon = FontAwesome.Solid.Slash,
						RelativeSizeAxes = Axes.Both,
						Colour = Color4.Red
					}
				}
			};
		}
	}

	// TODO flesh out the logic
	public class HiererchyInspector : FillFlowContainer, IHasName {
		public System.Action<Drawable> DrawablePrevieved;
		public System.Action<Drawable> DrawableSelected;

		public HiererchyInspector ( Drawable drawable ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			if ( drawable.Parent is Drawable parent ) {
				Add( new HierarchyButton( parent ) { 
					Hovered = () => DrawablePrevieved?.Invoke( parent ),
					HoverLost = () => DrawablePrevieved?.Invoke( null ),
					Action = () => DrawableSelected?.Invoke( parent ),
					Margin = new MarginPadding { Horizontal = 15 }
				} );
			}

			Add( new HierarchyStep( drawable, true ) {
				DrawablePrevieved = d => DrawablePrevieved?.Invoke( d ),
				DrawableSelected = d => DrawableSelected?.Invoke( d ),
				Margin = new MarginPadding { Horizontal = 15 }
			} );

			Add( new ExpandableSection {
				Title = "Legend",
				Children = new Drawable[] {
					makeIconText( HierarchyIcons.Selected, " - This is the element you are inspecting." ),
					makeIconText( HierarchyIcons.Container, " - This element contains children. Use the arrow on the right to see them!" ),
					makeIconText( HierarchyIcons.SwapProjection, " - This element is 3D and contains 2D children or vice versa." ),
					makeIconText( HierarchyIcons.No( HierarchyIcons.Container ), " - This element has children, but hides them." ),
					makeIconText( HierarchyIcons.HasSettings, " - This element has settings. When you inspect it, you can edit them here." ),
					makeIconText( HierarchyIcons.HasSettingsPersistent, " - The settings of this element persist after you close the game." ),
					makeIconText( HierarchyIcons.HasVisuals, " - This element will display its configuration in 3D space when inspected." ),
					makeIconText( HierarchyIcons.Experimental, " - This element is experimental and might not work as expected." ),
					makeIconText( HierarchyIcons.TwoD, " - This element is 2D." ),
					makeIconText( HierarchyIcons.ThreeD, " - This element is 3D." ),
				},
				Margin = new MarginPadding { Horizontal = 15, Top = 10 }
			} );
		}

		Drawable makeIconText ( IconUsage icon, string message ) {
			var text = new OsuTextFlowContainer {
				AutoSizeAxes = Axes.Y,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				TextAnchor = Anchor.CentreLeft,
				Margin = new MarginPadding { Left = 10, Top = 4 }
			};

			text.OnUpdate += _ => text.Width = text.Parent.DrawWidth - 20;

			text.AddIcon( icon, f => {
				f.Font = OsuFont.GetFont( size: 12 );
				f.Colour = HierarchyIcons.IconColor;
			} );
			text.AddText( message );

			return text;
		}

		Drawable makeIconText ( Drawable icon, string message ) {
			var text = new OsuTextFlowContainer {
				AutoSizeAxes = Axes.Y,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				TextAnchor = Anchor.CentreLeft,
				Margin = new MarginPadding { Left = 10, Top = 4 }
			};

			text.OnUpdate += _ => text.Width = text.Parent.DrawWidth - 20;

			text.AddArbitraryDrawable( icon );
			text.AddText( message );

			return text;
		}

		public string DisplayName => "Hierarchy";
	}

	public class HierarchyStep : FillFlowContainer {
		Drawable current;
		public System.Action<Drawable> DrawablePrevieved;
		public System.Action<Drawable> DrawableSelected;

		public HierarchyStep ( Drawable drawable, bool isCurrent = false ) {
			current = drawable;

			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			HierarchyButton button;
			Add( button = new HierarchyButton( drawable, isCurrent ) {
				Margin = new MarginPadding { Left = 5 },
				Hovered = () => DrawablePrevieved?.Invoke( drawable ),
				HoverLost = () => DrawablePrevieved?.Invoke( null ),
				Action = () => DrawableSelected?.Invoke( drawable )
			} );

			if ( current is not IChildrenNotInspectable ) {
				if ( current is CompositeDrawable3D ) {
					CalmOsuAnimatedButton toggleButton;
					button.Add( toggleButton = new CalmOsuAnimatedButton {
						Origin = Anchor.CentreRight,
						Anchor = Anchor.CentreRight,
						Action = () => toggle(),
						Height = 15,
						Width = 80,
					} );

					toggleButton.Add( new SpriteIcon {
						RelativeSizeAxes = Axes.Both,
						Icon = FontAwesome.Solid.ChevronDown,
						FillMode = FillMode.Fit,
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre
					} );
				}
				else if ( current is CompositeDrawable comp and ( not Drawable3D or Panel ) ) {
					CalmOsuAnimatedButton toggleButton;
					button.Add( toggleButton = new CalmOsuAnimatedButton {
						Origin = Anchor.CentreRight,
						Anchor = Anchor.CentreRight,
						Action = () => toggle(),
						Height = 15,
						Width = 80,
					} );

					toggleButton.Add( new SpriteIcon {
						RelativeSizeAxes = Axes.Both,
						Icon = FontAwesome.Solid.ChevronDown,
						FillMode = FillMode.Fit,
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre
					} );
				}
			}
		}

		bool state = false;
		void toggle () {
			state = !state;
			if ( state )
				expand();
			else
				contract();
		}
		void expand () {
			if ( current is CompositeDrawable3D composite ) {
				composite.ChildAdded += childAdded;
				composite.ChildRemoved += childRemoved;
				foreach ( var i in composite.Children )
					childAdded( composite, i );
			}
			else if ( current is CompositeDrawable comp ) {
				OnUpdate += watch2dHierarchy;
			}
		}
		private void watch2dHierarchy ( Drawable obj ) {
			var composite = this.current as CompositeDrawable;

			var previous = map.Keys;
			var current = composite.GetProperty<IReadOnlyList<Drawable>>( "InternalChildren" ).Where( x => x is not ISelfNotInspectable );
			var @new = current.Except( previous );
			var removed = previous.Except( current );

			foreach ( var i in @new ) {
				var step = new HierarchyStep( i ) {
					Margin = new MarginPadding { Left = 10 },
					DrawablePrevieved = d => DrawablePrevieved?.Invoke( d ),
					DrawableSelected = d => DrawableSelected?.Invoke( d )
				};
				map.Add( i, step );
				Add( step );
			}
			foreach ( var i in removed ) {
				map.Remove( i, out var step );
				Remove( step );
				step.Dispose();
			}
		}
		void contract () {
			if ( current is CompositeDrawable3D composite ) {
				composite.ChildAdded -= childAdded;
				composite.ChildRemoved -= childRemoved;
				foreach ( var i in map3d.Keys.ToArray() ) {
					childRemoved( composite, i );
				}
			}
			else if ( current is CompositeDrawable comp ) {
				OnUpdate -= watch2dHierarchy;
				foreach ( var i in map.Keys.ToArray() ) {
					map.Remove( i, out var step );
					Remove( step );
					step.Dispose();
				}
			}
		}

		Dictionary<Drawable3D, Drawable> map3d = new();
		Dictionary<Drawable, Drawable> map = new();
		private void childRemoved ( Drawable3D parent, Drawable3D child ) {
			if ( !map3d.ContainsKey( child ) ) return;

			map3d.Remove( child, out var step );
			Remove( step );
			step.Dispose();
		}

		private void childAdded ( Drawable3D parent, Drawable3D child ) {
			if ( !child.IsInspectable() ) return;

			var step = new HierarchyStep( child ) {
				Margin = new MarginPadding { Left = 10 },
				DrawablePrevieved = d => DrawablePrevieved?.Invoke( d ),
				DrawableSelected = d => DrawableSelected?.Invoke( d )
			};
			map3d.Add( child, step );
			Add( step );
		}

		protected override void Update () {
			base.Update();
			Width = Parent.DrawWidth - Margin.Right - Margin.Left;
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );

			if ( state ) {
				if ( current is CompositeDrawable3D composite ) {
					composite.ChildAdded -= childAdded;
					composite.ChildRemoved -= childRemoved;
				}
				else if ( current is CompositeDrawable comp ) {
					OnUpdate -= watch2dHierarchy;
				}
			}
		}
	}

	public class HierarchyButton : CalmOsuAnimatedButton {
		Drawable current;
		OsuTextFlowContainer text;
		public HierarchyButton ( Drawable drawable, bool isCurrent = false ) {
			current = drawable;
			AutoSizeAxes = Axes.Y;

			Add( text = new OsuTextFlowContainer {
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				TextAnchor = Anchor.CentreLeft
			} );

			text.AddText( " " );
			if ( drawable is Drawable3D ) {
				text.AddIcon( HierarchyIcons.ThreeD, f => {
					f.Font = OsuFont.GetFont( size: 10 );
					f.Colour = HierarchyIcons.IconColor.Opacity( 0.4f );
				} );
			}
			else {
				text.AddIcon( HierarchyIcons.TwoD, f => {
					f.Font = OsuFont.GetFont( size: 10 );
					f.Colour = HierarchyIcons.IconColor.Opacity( 0.4f );
				} );
			}
			text.AddText( " " );
			text.AddText( drawable.GetInspectorName() );
			if ( isCurrent ) {
				text.AddText( " " );
				text.AddIcon( HierarchyIcons.Selected, f => {
					f.Font = OsuFont.GetFont( size: 12 );
					f.Colour = HierarchyIcons.IconColor;
				} );
			}
			if ( drawable is CompositeDrawable3D or ( CompositeDrawable and not Drawable3D ) ) {
				text.AddText( " " );
				if ( drawable is IChildrenNotInspectable ) {
					text.AddArbitraryDrawable( HierarchyIcons.No( HierarchyIcons.Container ) );
				}
				else {
					text.AddIcon( HierarchyIcons.Container, f => {
						f.Font = OsuFont.GetFont( size: 12 );
						f.Colour = HierarchyIcons.IconColor;
					} );
				}
			}
			if ( drawable is IConfigurableInspectable config ) {
				text.AddText( " " );
				text.AddIcon( HierarchyIcons.HasSettings, f => {
					f.Font = OsuFont.GetFont( size: 12 );
					f.Colour = HierarchyIcons.IconColor;
				} );
				if ( config.AreSettingsPersistent ) {
					text.AddText( " " );
					text.AddIcon( HierarchyIcons.HasSettingsPersistent, f => {
						f.Font = OsuFont.GetFont( size: 12 );
						f.Colour = HierarchyIcons.IconColor;
					} );
				}
			}
			if ( drawable is IHasInspectorVisuals ) {
				text.AddText( " " );
				text.AddIcon( HierarchyIcons.HasVisuals, f => {
					f.Font = OsuFont.GetFont( size: 12 );
					f.Colour = HierarchyIcons.IconColor;
				} );
			}
			if ( drawable is IExperimental ) {
				text.AddText( " " );
				text.AddIcon( HierarchyIcons.Experimental, f => {
					f.Font = OsuFont.GetFont( size: 12 );
					f.Colour = HierarchyIcons.IconColor;
				} );
			}
			if ( drawable is Panel or Scene ) {
				text.AddText( " " );
				text.AddIcon( HierarchyIcons.SwapProjection, f => {
					f.Font = OsuFont.GetFont( size: 12 );
					f.Colour = HierarchyIcons.IconColor;
				} );
			}
		}

		protected override void Update () {
			base.Update();
			Width = Parent.DrawWidth - Margin.Right - Margin.Left;
		}
	}
}
