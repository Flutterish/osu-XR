using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Components;
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
	public class InspectorPanel : ConfigurationContainer {
		public readonly Bindable<Drawable3D> SelectedElementBindable = new();
		public readonly Bindable<Drawable3D> InspectedElementBindable = new();
		public readonly BindableBool IsSelectingBindable = new( false );
		public readonly BindableBool GranularSelectionBindable = new( false );
		FormatedTextContainer selectedName;
		FormatedTextContainer inspectedName;

		Selection selection = new();
		Selection helperSelection = new() { Tint = Color4.Yellow };

		public InspectorPanel () {
			Title = "Inspector";
			Description = "inspect and modify properties\nthese settings are not persistent";

			SelectedElementBindable.BindValueChanged( v => {
				helperSelection.Select( getSelected() );
				selectedName.Text = $"Selected: **{v.NewValue?.GetInspectorName() ?? "Nothing"}**";
			}, true );

			IsSelectingBindable.BindValueChanged( v => {
				if ( !v.NewValue ) {
					InspectedElementBindable.Value = getSelected();
					SelectedElementBindable.Value = null;
				}
			} );

			InspectedElementBindable.BindValueChanged( v => {
				ClearSections();
				selection.Select( v.NewValue );
				inspectedName.Text = $"Inspected: ||{v.NewValue?.GetInspectorName() ?? "Nothing"}||";

				if ( v.NewValue is null ) return;
				// TODO add inspector sections
				AddSection( new HiererchyInspector( v.NewValue ) {
					DrawablePrevieved = d => SelectedElementBindable.Value = d,
					DrawableSelected = d => InspectedElementBindable.Value = d
				} );
				AddSection( new ReflectionsInspector( v.NewValue, "Inspected" ) );
			}, true );
		}

		protected override Drawable CreateStickyHeader ( SearchTextBox search ) {
			return new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,
				Children = new Drawable[] {
					new SettingsCheckbox { LabelText = "Select element to inspect", Current = IsSelectingBindable },
					new SettingsCheckbox { LabelText = "Granular selection", Current = GranularSelectionBindable },
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

		Drawable3D getSelected () {
			if ( GranularSelectionBindable.Value ) {
				return SelectedElementBindable.Value?.GetValidInspectable();
			}
			else {
				return ( SelectedElementBindable.Value?.GetClosestInspectable() as Drawable3D ) ?? SelectedElementBindable.Value?.GetValidInspectable();
			}
		}
	}
}
