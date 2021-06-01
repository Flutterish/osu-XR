using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics.Containers;
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components {
	public static class HierarchyIcons {
		public static readonly IconUsage Container = FontAwesome.Solid.Sitemap;
		public static readonly string ContainerTerm = "+has-children";
		public static readonly IconUsage HasSettings = FontAwesome.Solid.Cog;
		public static readonly string HasSettingsTerm = "+has-settings";
		public static readonly IconUsage HasSettingsPersistent = FontAwesome.Solid.Download;
		public static readonly string HasSettingsPersistentTerm = "+saves-settings";
		public static readonly IconUsage HasVisuals = FontAwesome.Solid.Eye;
		public static readonly string HasVisualsTerm = "+has-visuals";
		public static readonly IconUsage Selected = FontAwesome.Solid.Search;
		public static readonly IconUsage Experimental = FontAwesome.Solid.Flask;
		public static readonly string ExperimentalTerm = "+experimental";
		public static readonly IconUsage SwapProjection = FontAwesome.Solid.Random;
		public static readonly IconUsage TwoD = FontAwesome.Solid.StickyNote;
		public static readonly string TwoDTerm = "+2d";
		public static readonly IconUsage ThreeD = FontAwesome.Solid.Cube;
		public static readonly string ThreeDTerm = "+3d";

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

	public enum HierarchyProperty {
		IsContainer,
		HidesChildren,
		SwapsProjection,
		HasSettings,
		SavesSettings,
		HasVisuals,
		IsExperimental,
		Is2D,
		Is3D
	}

	public static class HierarchyPropertyExtensions {
		public static IEnumerable<HierarchyProperty> GetHierarchyProperties ( this Drawable drawable ) {
			if ( drawable is Drawable3D ) {
				yield return HierarchyProperty.Is3D;
			}
			else {
				yield return HierarchyProperty.Is2D;
			}

			if ( drawable is CompositeDrawable3D or ( CompositeDrawable and not Drawable3D ) ) {
				if ( drawable is IChildrenNotInspectable ) {
					yield return HierarchyProperty.HidesChildren;
				}
				else {
					yield return HierarchyProperty.IsContainer;
				}
			}
			if ( drawable is IConfigurableInspectable config ) {
				yield return HierarchyProperty.HasSettings;
				if ( config.AreSettingsPersistent ) {
					yield return HierarchyProperty.SavesSettings;
				}
			}
			if ( drawable is IHasInspectorVisuals ) {
				yield return HierarchyProperty.HasVisuals;
			}
			if ( drawable is IExperimental ) {
				yield return HierarchyProperty.IsExperimental;
			}
			if ( drawable is Panel or Scene ) {
				yield return HierarchyProperty.SwapsProjection;
			}
		}
	}

	public class HiererchyInspector : AdvancedSearchContainer<HierarchyProperty>, IHasName {
		public readonly Bindable<Drawable> SelectedDrawable = new();
		public System.Action<Drawable> DrawablePrevieved;
		public System.Action<Drawable> DrawableSelected;
		FillFlowContainer headers;
		HierarchyStep top;
		public Action<string> SearchTermRequested;

		public HiererchyInspector () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			Add( headers = new FilterableFillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical
			} );

			Add( new ExpandableSection {
				Title = "Legend",
				Children = new Drawable[] {
					makeIconText( HierarchyIcons.Selected, " - This is the element you are inspecting." ),
					makeIconText( HierarchyIcons.Container, " - This element contains children. Use the arrow on the right to see them!", $"(search term: '{HierarchyIcons.ContainerTerm}')", () => SearchTermRequested?.Invoke( HierarchyIcons.ContainerTerm ) ),
					makeIconText( HierarchyIcons.SwapProjection, " - This element is 3D and contains 2D children or vice versa." ),
					makeIconText( HierarchyIcons.No( HierarchyIcons.Container ), " - This element has children, but hides them." ),
					makeIconText( HierarchyIcons.HasSettings, " - This element has settings. When you inspect it, you can edit them here.", $"(search term: '{HierarchyIcons.HasSettingsTerm}')", () => SearchTermRequested?.Invoke( HierarchyIcons.HasSettingsTerm ) ),
					makeIconText( HierarchyIcons.HasSettingsPersistent, " - The settings of this element persist after you close the game.", $"(search term: '{HierarchyIcons.HasSettingsPersistentTerm}')", () => SearchTermRequested?.Invoke( HierarchyIcons.HasSettingsPersistentTerm ) ),
					makeIconText( HierarchyIcons.HasVisuals, " - This element will display its configuration in 3D space when inspected.", $"(search term: '{HierarchyIcons.HasVisualsTerm}')", () => SearchTermRequested?.Invoke( HierarchyIcons.HasVisualsTerm ) ),
					makeIconText( HierarchyIcons.Experimental, " - This element is experimental and might not work as expected.", $"(search term: '{HierarchyIcons.ExperimentalTerm}')", () => SearchTermRequested?.Invoke( HierarchyIcons.ExperimentalTerm ) ),
					makeIconText( HierarchyIcons.TwoD, " - This element is 2D.", $"(search term: '{HierarchyIcons.TwoDTerm}')", () => SearchTermRequested?.Invoke( HierarchyIcons.TwoDTerm ) ),
					makeIconText( HierarchyIcons.ThreeD, " - This element is 3D.", $"(search term: '{HierarchyIcons.ThreeDTerm}')", () => SearchTermRequested?.Invoke( HierarchyIcons.ThreeDTerm ) ),
				},
				Margin = new MarginPadding { Horizontal = 15, Top = 10 }
			} );

			RecursionMode = RecursiveFilterMode.ChildrenFirst;

			SelectedDrawable.BindValueChanged( x => {
				var drawable = x.NewValue;

				void addParent () {
					if ( drawable.Parent is Drawable parent ) {
						headers.Add( new HierarchyButton( parent ) {
							Hovered = () => DrawablePrevieved?.Invoke( parent ),
							HoverLost = () => DrawablePrevieved?.Invoke( null ),
							Action = () => DrawableSelected?.Invoke( parent ),
							Margin = new MarginPadding { Horizontal = 15 }
						} );
					}
				}
				void addTop () {
					headers.Add( top = new HierarchyStep( drawable ) {
						DrawablePrevieved = d => DrawablePrevieved?.Invoke( d ),
						DrawableSelected = d => DrawableSelected?.Invoke( d ),
						Margin = new MarginPadding { Horizontal = 15 }
					} );
					top.SelectedDrawable.BindTo( SelectedDrawable );
				}

				if ( top is not null ) {
					if ( drawable == top.Current.Parent ) {
						headers.Clear( false );
						addParent();
						var previousTop = top;
						previousTop.SelectedDrawable.UnbindFrom( SelectedDrawable );
						addTop();
						top.MergeChild( previousTop );
					}
					else if ( top.IsStepVisible( drawable, out var step ) ) {
						headers.Clear( false );
						addParent();
						var parent = step.Parent as HierarchyStep;
						parent?.Remove( step );
						step.SelectedDrawable.UnbindFrom( parent?.SelectedDrawable ?? SelectedDrawable );
						step.DrawablePrevieved = d => DrawablePrevieved?.Invoke( d );
						step.DrawableSelected = d => DrawableSelected?.Invoke( d );
						step.Margin = new MarginPadding { Horizontal = 15 };
						headers.Add( top = step );
						top.SelectedDrawable.BindTo( SelectedDrawable );
					}
					else {
						headers.Clear( true );
						addParent();
						addTop();
					}
				}
				else {
					headers.Clear( true );
					addParent();
					addTop();
				}
			} );
		}

		Drawable makeIconText ( IconUsage icon, string message, string hint = null, Action onHintClicked = null ) {
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
			if ( hint is not null ) {
				if ( onHintClicked is null ) {
					text.AddText( " " + hint, f => f.Alpha = 0.4f );
				}
				else {
					OsuTextFlowContainer hintText = new OsuTextFlowContainer {
						AutoSizeAxes = Axes.Both,
						Margin = new MarginPadding { Horizontal = 5 }
					};

					CalmOsuAnimatedButton button;
					text.AddArbitraryDrawable( button = new CalmOsuAnimatedButton {
						AutoSizeAxes = Axes.Both,
						Action = onHintClicked
					} );
					button.Add( hintText );

					hintText.AddText( hint, f => f.Alpha = 0.4f );
				}
			}

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

	public class HierarchyStep : FillFlowContainer, IFilterable<HierarchyProperty>, IHasFilterableChildren {
		public readonly Drawable Current;
		public readonly Bindable<Drawable> SelectedDrawable = new();
		public System.Action<Drawable> DrawablePrevieved;
		public System.Action<Drawable> DrawableSelected;

		bool contains3DChildren ( Drawable drawable )
			=> drawable is not IChildrenNotInspectable and CompositeDrawable3D; // not including "or Scene" because we really only care about having an observable hierarchy and not really the type of children
		bool contains2DChildren ( Drawable drawable )
			=> drawable is not IChildrenNotInspectable and ( Panel or ( CompositeDrawable and not Drawable3D ) );

		IEnumerable<HierarchyProperty> props;
		public HierarchyStep ( Drawable drawable ) {
			Current = drawable;
			props = Current.GetHierarchyProperties();

			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			HierarchyButton button;
			Add( button = new HierarchyButton( drawable ) {
				Margin = new MarginPadding { Left = 5 },
				Hovered = () => DrawablePrevieved?.Invoke( drawable ),
				HoverLost = () => DrawablePrevieved?.Invoke( null ),
				Action = () => DrawableSelected?.Invoke( drawable )
			} );
			button.SelectedDrawable.BindTo( SelectedDrawable );

			if ( contains3DChildren( Current ) || contains2DChildren( Current ) ) {
				CalmOsuAnimatedButton toggleButton;
				button.Add( toggleButton = new CalmOsuAnimatedButton {
					Origin = Anchor.CentreRight,
					Anchor = Anchor.CentreRight,
					Action = IsExpanded.Toggle,
					RelativeSizeAxes = Axes.Y,
					Width = 80,
				} );

				toggleButton.Add( new Container {
					RelativeSizeAxes = Axes.X,
					Height = 15,
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre,
					Child = dropdownChevron = new SpriteIcon {
						RelativeSizeAxes = Axes.Both,
						Icon = FontAwesome.Solid.ChevronDown,
						FillMode = FillMode.Fit,
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre
					}
				} );

				IsExpanded.BindValueChanged( v => {
					if ( v.NewValue )
						expand();
					else
						contract();
				} );
			}
		}

		public bool IsStepVisible ( Drawable goal, out HierarchyStep step ) {
			if ( Current == goal ) {
				step = this;
				return true;
			}
			else {
				foreach ( var i in map.Values ) {
					if ( i.IsStepVisible( goal, out step ) ) return true;
				}
				step = null;
				return false;
			}
		}

		SpriteIcon dropdownChevron;
		public readonly BindableBool IsExpanded = new( false );
		void expand () {
			dropdownChevron.FadeTo( 0.3f, 100 );

			if ( Current is CompositeDrawable3D composite ) {
				composite.ChildAdded += childAdded3D;
				composite.ChildRemoved += childRemoved3D;
				foreach ( var i in composite.Children )
					childAdded3D( composite, i );
			}
			else if ( Current is CompositeDrawable comp ) {
				OnUpdate += watch2dHierarchy;
			}
		}
		public static readonly MethodInfo getInternalChildrenMethod = typeof( CompositeDrawable ).GetProperty( nameof( InternalChildren ), BindingFlags.NonPublic | BindingFlags.Instance ).GetGetMethod( nonPublic: true );
		public static readonly Func<CompositeDrawable, IReadOnlyList<Drawable>> getInternalChildren = x => getInternalChildrenMethod.Invoke( x, Array.Empty<object>() ) as IReadOnlyList<Drawable>;
		public static readonly MethodInfo getAliveInternalChildrenMethod = typeof( CompositeDrawable ).GetProperty( nameof( AliveInternalChildren ), BindingFlags.NonPublic | BindingFlags.Instance ).GetGetMethod( nonPublic: true );
		public static readonly Func<CompositeDrawable, IReadOnlyList<Drawable>> getAliveInternalChildren = x => getAliveInternalChildrenMethod.Invoke( x, Array.Empty<object>() ) as IReadOnlyList<Drawable>;
		private void watch2dHierarchy ( Drawable obj ) {
			var composite = this.Current as CompositeDrawable;

			var previous = map.Keys;
			var current = getInternalChildren( composite ).Where( X => X is not ISelfNotInspectable );
			var @new = current.Except( previous );
			var removed = previous.Except( current );

			foreach ( var i in @new ) {
				addChild( i );
			}
			foreach ( var i in removed ) {
				removeChild( i );
			}
		}
		void contract () {
			dropdownChevron.FadeTo( 1f, 100 );

			if ( Current is CompositeDrawable3D composite ) {
				composite.ChildAdded -= childAdded3D;
				composite.ChildRemoved -= childRemoved3D;
			}
			else if ( Current is CompositeDrawable comp ) {
				OnUpdate -= watch2dHierarchy;
			}

			foreach ( var i in map.Keys.ToArray() ) {
				removeChild( i );
			}
		}

		private void childRemoved3D ( Drawable3D parent, Drawable3D child ) {
			removeChild( child );
		}
		private void childAdded3D ( Drawable3D parent, Drawable3D child ) {
			addChild( child );
		}

		Dictionary<Drawable, HierarchyStep> map = new();
		void addChild ( Drawable child ) {
			if ( !child.IsInspectable() ) return;

			var step = new HierarchyStep( child ) {
				Margin = new MarginPadding { Left = 10 },
				DrawablePrevieved = d => DrawablePrevieved?.Invoke( d ),
				DrawableSelected = d => DrawableSelected?.Invoke( d )
			};
			step.SelectedDrawable.BindTo( SelectedDrawable );
			step.onChangeHandler = onChangeHandler;
			map.Add( child, step );
			Add( step );

			onChangeHandler?.Invoke( step );
			step.FinishTransforms();
		}
		void removeChild ( Drawable child ) {
			if ( !map.ContainsKey( child ) ) return;

			map.Remove( child, out var step );
			Remove( step );
			step.Dispose();
		}

		public void MergeChild ( HierarchyStep child ) {
			IsExpanded.Value = true;

			removeChild( child.Current );

			child.Margin = new MarginPadding { Left = 10 };
			child.DrawablePrevieved = d => DrawablePrevieved?.Invoke( d );
			child.DrawableSelected = d => DrawableSelected?.Invoke( d );
			child.SelectedDrawable.BindTo( SelectedDrawable );
			map.Add( child.Current, child );
			Add( child );
			child.onChangeHandler = onChangeHandler;
		}

		protected override void Update () {
			base.Update();
			Width = Parent.DrawWidth - Margin.Right - Margin.Left;
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );

			if ( IsExpanded.Value ) {
				if ( Current is CompositeDrawable3D composite ) {
					composite.ChildAdded -= childAdded3D;
					composite.ChildRemoved -= childRemoved3D;
				}
				else if ( Current is CompositeDrawable comp ) {
					OnUpdate -= watch2dHierarchy;
				}
			}
		}

		public bool MatchingFilter {
			set {
				this.FadeTo( value ? 1 : 0, 200 );
				this.ScaleTo( new Vector2( 1, value ? 1 : 0 ), 200 );
			}
		}
		public bool FilteringActive { set { } }
		public IEnumerable<string> FilterTerms => props.Select( x => {
			return x switch {
				HierarchyProperty.HasSettings => HierarchyIcons.HasSettingsTerm,
				HierarchyProperty.HasVisuals => HierarchyIcons.HasVisualsTerm,
				HierarchyProperty.Is2D => HierarchyIcons.TwoDTerm,
				HierarchyProperty.Is3D => HierarchyIcons.ThreeDTerm,
				HierarchyProperty.IsContainer => HierarchyIcons.ContainerTerm,
				HierarchyProperty.IsExperimental => HierarchyIcons.ExperimentalTerm,
				HierarchyProperty.SavesSettings => HierarchyIcons.HasSettingsPersistentTerm,
				_ => ""
			};
		} ).Append( Current.GetInspectorName() );

		public bool MatchesCustomTerms ( IEnumerable<HierarchyProperty> terms )
			=> terms.All( x => props.Contains( x ) );

		public IEnumerable<IFilterable> FilterableChildren => map.Values;

		Action<IFilterable> onChangeHandler;
		public void SetChangeNotificationTarget ( Action<IFilterable> onChangeHandler ) {
			this.onChangeHandler = onChangeHandler;
		}
	}

	public class HierarchyButton : CalmOsuAnimatedButton {
		Drawable current;
		OsuTextFlowContainer text;

		public readonly Bindable<Drawable> SelectedDrawable = new();
		IEnumerable<Drawable> selectedIcon;

		public HierarchyButton ( Drawable drawable ) {
			current = drawable;
			AutoSizeAxes = Axes.Y;

			Add( text = new OsuTextFlowContainer {
				AutoSizeAxes = Axes.Y,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				TextAnchor = Anchor.CentreLeft
			} );
			text.OnUpdate += d => d.Width = this.DrawWidth - 80;

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

			selectedIcon = text.AddText( " " ).Concat( text.AddIcon( HierarchyIcons.Selected, f => {
				f.Font = OsuFont.GetFont( size: 12 );
				f.Colour = HierarchyIcons.IconColor;
			} ) );

			SelectedDrawable.BindValueChanged( v => {
				foreach ( var i in selectedIcon ) {
					i.FadeTo( ( v.NewValue == current ) ? 1 : 0, 300 );
					i.ScaleTo( new Vector2( ( v.NewValue == current ) ? 1 : 0, 1 ), 300, Easing.Out );
				}
			}, true );
			FinishTransforms( true );

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
