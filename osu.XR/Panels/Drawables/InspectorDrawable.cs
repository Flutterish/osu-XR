using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables.Containers;
using osuTK.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Inspector {
	public class InspectorDrawable : ConfigurationContainer, INotInspectable {
		public readonly Bindable<Drawable> SelectedElementBindable = new();
		public readonly Bindable<Drawable> InspectedElementBindable = new();
		public readonly BindableBool IsSelectingBindable = new( false );
		public readonly BindableBool Targets2DDrawables = new( false );
		public readonly BindableBool GranularSelectionBindable = new( false );
		FormatedTextContainer selectedName;
		FormatedTextContainer inspectedName;

		Selection3D selection3d = new();
		Selection3D helperSelection3d = new() { Tint = Color4.Yellow };

		Selection2D selection2d = new();
		Selection2D helperSelection2d = new();

		public InspectorDrawable () {
			Title = "Inspector";
			Description = "inspect and modify properties";

			SelectedElementBindable.BindValueChanged( v => {
				if ( v.NewValue is Drawable3D d3 ) {
					helperSelection3d.Select( d3 );
					helperSelection2d.Select( null );
				}
				else {
					helperSelection3d.Select( null );
					helperSelection2d.Select( v.NewValue );
				}
				selectedName.Text = $"Selected: **{v.NewValue?.GetInspectorName() ?? "Nothing"}**";
			}, true );

			InspectedElementBindable.BindValueChanged( v => {
				if ( v.NewValue is Drawable3D d3 ) {
					selection3d.Select( d3 );
					selection2d.Select( null );
				}
				else {
					selection3d.Select( null );
					selection2d.Select( v.NewValue );
				}
				inspectedName.Text = $"Inspected: ||{v.NewValue?.GetInspectorName() ?? "Nothing"}||";

				HierarchyInspector hierarchy = keepSections.OfType<HierarchyInspector>().FirstOrDefault();
				if ( v.NewValue is null ) {
					ClearSections();
				}
				else {
					ClearSections( x => !keepSections.Contains( x ), true );
					ClearSections( false );
				}
				if ( v.NewValue is not null ) {
					if ( hierarchy is null ) {
						AddSection( hierarchy = new HierarchyInspector( v.NewValue ) );
						hierarchy.StepHovered += s => {
							SelectedElementBindable.Value = s.Value;
						};
						hierarchy.StepHoverLost += s => {
							SelectedElementBindable.Value = null;
						};
						hierarchy.StepSelected += s => {
							keepSections.Add( hierarchy );
							InspectedElementBindable.Value = s.Value;
						};
						hierarchy.SearchTermRequested = t => {
							if ( SearchTextBox.Current.Value.EndsWith( " " ) ) {
								SearchTextBox.Current.Value += t;
							}
							else {
								SearchTextBox.Current.Value += " " + t;
							}
						};
					}
					else {
						AddSection( hierarchy );
						hierarchy.FocusOn( v.NewValue );
					}
				}

				if ( v.NewValue is IConfigurableInspectable config ) {
					if ( config.CreateWarnings() is Drawable warning ) {
						AddSection( warning );
					}
					AddSection( config.CreateInspectorSubsection() );
				}
#if DEBUG
				AddSection( new ReflectionsInspector( v.NewValue, "Inspected" ) );
#endif
				keepSections.Clear();
			}, true );
		}
		List<Drawable> keepSections = new();

		protected override Drawable CreateStickyHeader ( SearchTextBox search ) {
			return new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,
				Children = new Drawable[] {
					new SettingsCheckbox { LabelText = "Select element to inspect", Current = IsSelectingBindable },
					new SettingsCheckbox { LabelText = "Granular selection", Current = GranularSelectionBindable },
					new SettingsCheckbox { LabelText = "Target 2D elements", Current = Targets2DDrawables },
					selectedName = new FormatedTextContainer( () => new FontSetings { Size = 20 } ) {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Margin = new MarginPadding { Left = 15 }
					},
					inspectedName = new FormatedTextContainer( () => new FontSetings { Size = 20 } ) {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Margin = new MarginPadding { Left = 15 }
					},
					search
				}
			};
		}

		Drawable getSelected ( Drawable drawable ) {
			if ( GranularSelectionBindable.Value ) {
				return drawable?.GetValidInspectable();
			}
			else {
				return ( drawable?.GetClosestInspectable() as Drawable ) ?? drawable?.GetValidInspectable();
			}
		}

		public void Select ( Drawable drawable ) {
			SelectedElementBindable.Value = getSelected( drawable );
		}
		public void Inspect ( Drawable drawable ) {
			InspectedElementBindable.Value = getSelected( drawable );
		}
	}
}
