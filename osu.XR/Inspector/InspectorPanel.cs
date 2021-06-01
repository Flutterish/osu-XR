using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables;
using osu.XR.Inspector.Components;
using osu.XR.Inspector.Components.Reflections;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector {
	public class InspectorPanel : ConfigurationContainer, INotInspectable {
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

		public InspectorPanel () {
			Title = "Inspector";
			Description = "inspect and modify properties\nthese settings are not persistent";

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

				HiererchyInspector hierarchy = keepSections.OfType<HiererchyInspector>().FirstOrDefault();
				if ( v.NewValue is null ) {
					ClearSections();
				}
				else {
					ClearSections( x => !keepSections.Contains( x ), true );
					ClearSections( false );
				}

				AddSection( hierarchy ??= new HiererchyInspector() {
					DrawablePrevieved = d => SelectedElementBindable.Value = d,
					DrawableSelected = d => {
						keepSections.Add( hierarchy );
						InspectedElementBindable.Value = d;
					}
				} );
				hierarchy.SelectedDrawable.Value = v.NewValue;
				hierarchy.Current = SearchTextBox.Current;
				hierarchy.SearchTermRequested = t => {
					if ( SearchTextBox.Current.Value.EndsWith( " " ) ) {
						SearchTextBox.Current.Value += t;
					}
					else {
						SearchTextBox.Current.Value += " " + t;
					}
				};

				if ( v.NewValue is IConfigurableInspectable config ) {
					foreach ( var i in config.CreateInspectorSubsections() ) {
						AddSection( i );
					}
				}
				AddSection( new ReflectionsInspector( v.NewValue, "Inspected" ) );
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
