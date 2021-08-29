using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using osu.XR.Drawables.Containers;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace osu.XR.Inspector {
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

			if ( drawable is CompositeDrawable3D or CompositeDrawable and not Drawable3D ) {
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

	public class HierarchyInspector : HierarchyView<HierarchyInspectorStep, Drawable>, IHasName {
		public SearchTextBox SearchTextBox;
		private AdvancedSearchContainer<string> searchContainer;

		protected override FlowContainer<Drawable> CreateContentContainer () => searchContainer = new AdvancedSearchContainer<string> {
			AutoSizeAxes = Axes.Y,
			RelativeSizeAxes = Axes.X,
			Direction = FillDirection.Vertical,
			RecursionMode = RecursiveFilterMode.ChildrenFirst
		};

		public HierarchyInspector ( Drawable value ) : base( value ) {
			SelectedDrawable.Value = value;
			StepSelected += s => SelectedDrawable.Value = s.Value;

			Add( new ExpandableSection {
				Title = "Legend",
				Children = new Drawable[] {
					makeIconText( HierarchyIcons.Selected, " - This is the element you are inspecting." ),
					makeIconText( HierarchyIcons.Container, " - This element contains children. Use the arrow on the right to see them!", $"(search term: '{HierarchyIcons.ContainerTerm}')", () => addSearchTerm( HierarchyIcons.ContainerTerm ) ),
					makeIconText( HierarchyIcons.SwapProjection, " - This element is 3D and contains 2D children or vice versa." ),
					makeIconText( HierarchyIcons.No( HierarchyIcons.Container ), " - This element has children, but hides them." ),
					makeIconText( HierarchyIcons.HasSettings, " - This element has settings. When you inspect it, you can edit them here.", $"(search term: '{HierarchyIcons.HasSettingsTerm}')", () => addSearchTerm( HierarchyIcons.HasSettingsTerm ) ),
					makeIconText( HierarchyIcons.HasSettingsPersistent, " - The settings of this element persist after you close the game.", $"(search term: '{HierarchyIcons.HasSettingsPersistentTerm}')", () => addSearchTerm( HierarchyIcons.HasSettingsPersistentTerm ) ),
					makeIconText( HierarchyIcons.HasVisuals, " - This element will display its configuration in 3D space when inspected.", $"(search term: '{HierarchyIcons.HasVisualsTerm}')", () => addSearchTerm( HierarchyIcons.HasVisualsTerm ) ),
					makeIconText( HierarchyIcons.Experimental, " - This element is experimental and might not work as expected.", $"(search term: '{HierarchyIcons.ExperimentalTerm}')", () => addSearchTerm( HierarchyIcons.ExperimentalTerm ) ),
					makeIconText( HierarchyIcons.TwoD, " - This element is 2D.", $"(search term: '{HierarchyIcons.TwoDTerm}')", () => addSearchTerm( HierarchyIcons.TwoDTerm ) ),
					makeIconText( HierarchyIcons.ThreeD, " - This element is 3D.", $"(search term: '{HierarchyIcons.ThreeDTerm}')", () => addSearchTerm( HierarchyIcons.ThreeDTerm ) ),
				},
				Margin = new MarginPadding { Horizontal = 15, Top = 10 }
			} );

			Insert( -1, SearchTextBox = new SearchTextBox {
				RelativeSizeAxes = Axes.X,
				Width = 0.95f,
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre,
				Margin = new MarginPadding { Vertical = 4 }
			} );
			SearchTextBox.Current.BindValueChanged( v => {
				searchContainer.SearchTerm = v.NewValue ?? "";
			}, true );
		}

		private void addSearchTerm ( string term ) {
			if ( SearchTextBox.Current.Value.EndsWith( " " ) ) {
				SearchTextBox.Current.Value += term;
			}
			else {
				SearchTextBox.Current.Value += " " + term;
			}
		}

		public readonly Bindable<Drawable> SelectedDrawable = new();
		protected override HierarchyInspectorStep CreateTop ( Drawable value ) {
			var r = new HierarchyInspectorStep( value );
			r.SelectedDrawable.BindTo( SelectedDrawable );
			return r;
		}
		protected override HierarchyInspectorStep CreateParent ( HierarchyInspectorStep top )
			=> CreateParent( top.Value );
		public static HierarchyInspectorStep CreateParent ( Drawable value ) {
			if ( value.Parent is not null )
				return new HierarchyInspectorStep( value.Parent );
			return null;
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

	public class HierarchyInspectorStep : HierarchyStep<Drawable> {
		public HierarchyInspectorStep ( Drawable value ) : base( value ) {
			properties = value.GetHierarchyProperties();
		}
		IEnumerable<HierarchyProperty> properties;
		public readonly Bindable<Drawable> SelectedDrawable = new();

		protected override void OnChildCreated ( HierarchyStep<Drawable> child ) {
			base.OnChildCreated( child );
			( child as HierarchyInspectorStep ).SelectedDrawable.BindTo( SelectedDrawable );
		}
		protected override void OnParentCreated ( HierarchyStep<Drawable> parent ) {
			base.OnParentCreated( parent );
			( parent as HierarchyInspectorStep ).SelectedDrawable.BindTo( SelectedDrawable );
		}

		public static readonly MethodInfo getInternalChildrenMethod = typeof( CompositeDrawable ).GetProperty( nameof( InternalChildren ), BindingFlags.NonPublic | BindingFlags.Instance ).GetGetMethod( nonPublic: true );
		public static readonly Func<CompositeDrawable, IReadOnlyList<Drawable>> getInternalChildren = x => getInternalChildrenMethod.Invoke( x, Array.Empty<object>() ) as IReadOnlyList<Drawable>;
		public static readonly MethodInfo getAliveInternalChildrenMethod = typeof( CompositeDrawable ).GetProperty( nameof( AliveInternalChildren ), BindingFlags.NonPublic | BindingFlags.Instance ).GetGetMethod( nonPublic: true );
		public static readonly Func<CompositeDrawable, IReadOnlyList<Drawable>> getAliveInternalChildren = x => getAliveInternalChildrenMethod.Invoke( x, Array.Empty<object>() ) as IReadOnlyList<Drawable>;
		IEnumerable<Drawable> selectedIcon;
		protected override void LoadComplete () {
			base.LoadComplete();

			if ( Value is CompositeDrawable3D d3 ) {
				d3.BindLocalHierarchyChange( childAdded, childRemoved, true );
			}
			else if ( contains2DChildren( Value ) ) {
				OnUpdate += _ => {
					var composite = Value as CompositeDrawable;

					var previous = Children.Keys;
					var current = getInternalChildren( composite ).Where( x => x is not ISelfNotInspectable );
					var @new = current.Except( previous );
					var removed = previous.Except( current );

					foreach ( var i in @new ) {
						AddChild( i );
					}
					foreach ( var i in removed ) {
						RemoveChild( i );
					}
				};
			}

			selectedIcon = Label.AddText( " " ).Concat( Label.AddIcon( HierarchyIcons.Selected, f => {
				f.Font = OsuFont.GetFont( size: 12 );
				f.Colour = HierarchyIcons.IconColor;
			} ) );

			SelectedDrawable.BindValueChanged( v => {
				foreach ( var i in selectedIcon ) {
					i.FadeTo( v.NewValue == Value ? 1 : 0, 300 );
					i.ScaleTo( new Vector2( v.NewValue == Value ? 1 : 0, 1 ), 300, Easing.Out );
				}
			}, true );
			FinishTransforms( true );

			foreach ( var i in properties ) {
				if ( i is HierarchyProperty.Is2D or HierarchyProperty.Is3D ) continue;

				Label.AddText( " " );
				if ( i is HierarchyProperty.HidesChildren ) {
					Label.AddArbitraryDrawable( HierarchyIcons.No( HierarchyIcons.Container ) );
				}
				else {
					Label.AddIcon( i switch {
						HierarchyProperty.HasSettings => HierarchyIcons.HasSettings,
						HierarchyProperty.HasVisuals => HierarchyIcons.HasVisuals,
						HierarchyProperty.IsContainer => HierarchyIcons.Container,
						HierarchyProperty.IsExperimental => HierarchyIcons.Experimental,
						HierarchyProperty.SavesSettings => HierarchyIcons.HasSettingsPersistent,
						HierarchyProperty.SwapsProjection => HierarchyIcons.SwapProjection,
						_ => throw new UnreachableCodeException()
					}, f => {
						f.Font = OsuFont.GetFont( size: 12 );
						f.Colour = HierarchyIcons.IconColor;
					} );
				}
			}
		}

		private void childAdded ( Drawable3D parent, Drawable3D child ) {
			if ( child is ISelfNotInspectable ) return;
			AddChild( child );
		}
		private void childRemoved ( Drawable3D parent, Drawable3D child ) {
			RemoveChild( child );
		}

		static bool contains3DChildren ( Drawable drawable )
			=> drawable is CompositeDrawable3D; // not including "or Scene" because we really only care about having an observable hierarchy and not really the type of children
		static bool contains2DChildren ( Drawable drawable )
			=> drawable is Panel or CompositeDrawable and not Drawable3D;

		protected override HierarchyStep<Drawable> CreateChild ( Drawable value )
			=> new HierarchyInspectorStep( value );
		protected override HierarchyStep<Drawable> CreateParent ()
			=> HierarchyInspector.CreateParent( Value );

		public override Drawable CreateIcon () {
			return new SpriteIcon {
				Icon = properties.Contains( HierarchyProperty.Is3D ) ? FontAwesome.Solid.Cube : FontAwesome.Solid.StickyNote,
				Height = 10,
				Width = 10,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				Colour = HierarchyIcons.IconColor.Opacity( 0.4f )
			};
		}
		public override string Title => Value.GetInspectorName();
		public override IEnumerable<string> FilterTerms => properties.Select( x => x switch {
			HierarchyProperty.HasSettings => HierarchyIcons.HasSettingsTerm,
			HierarchyProperty.HasVisuals => HierarchyIcons.HasVisualsTerm,
			HierarchyProperty.Is2D => HierarchyIcons.TwoDTerm,
			HierarchyProperty.Is3D => HierarchyIcons.ThreeDTerm,
			HierarchyProperty.IsContainer => HierarchyIcons.ContainerTerm,
			HierarchyProperty.IsExperimental => HierarchyIcons.ExperimentalTerm,
			HierarchyProperty.SavesSettings => HierarchyIcons.HasSettingsPersistentTerm,
			_ => ""
		} ).Where( x => x != "" ).Append( Title );

		protected override void Dispose ( bool isDisposing ) {
			if ( Value is CompositeDrawable3D d3 ) {
				d3.ChildAdded -= childAdded;
				d3.ChildRemoved -= childRemoved;
			}

			base.Dispose( isDisposing );
		}
	}
}
