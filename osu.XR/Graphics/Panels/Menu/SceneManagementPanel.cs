using osu.Framework.Extensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;
using osu.XR.Graphics.Sceneries;
using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Panels.Menu;

public partial class SceneManagementPanel : SettingsPanel {
	protected override Sections CreateSectionsContainer ()
		=> new( showSidebar: false );

	[Cached]
	public partial class Sections : SectionsContainer {
		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		Dictionary<ISceneryComponent, SettingsSection> componentSettings = new();
		[BackgroundDependencyLoader]
		private void load ( SceneryContainer? sceneryContainer ) {
			if ( sceneryContainer == null )
				return;

			sceneryContainer.Scenery.Components.BindCollectionChanged( (_, e) => {
				var container = (FillFlowContainer<SettingsSection>)presets.Parent;
				if ( container != null )
					container.SetLayoutPosition( presets, 1f );

				if ( e.OldItems != null ) {
					foreach ( ISceneryComponent i in e.OldItems ) {
						componentSettings.Remove( i, out var section );
						SectionsContainer.Remove( section!, disposeImmediately: true );
					}
				}
				if ( e.NewItems != null ) {
					foreach ( ISceneryComponent i in e.NewItems ) {
						var section = i is IConfigurableSceneryComponent configurable ? configurable.CreateSettings() : new SceneryComponentSettingsSection( i );
						componentSettings.Add( i, section );
						SectionsContainer.Add( section );

						section.RemoveRequested += () => {
							sceneryContainer.Scenery.Components.Remove( i );
						};
					}
				}
			}, true );
		}

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( @"Scenery Editor", Localisation.SceneryStrings.Flavour );

		PresetsSection presets = new();
		protected override IEnumerable<SettingsSection> CreateSections () {
			yield return presets;
		}

		partial class PresetsSection : SettingsSection {
			public override LocalisableString Header => @"Presets";
			public override Drawable CreateIcon () => new SpriteIcon {
				Icon = FontAwesome.Solid.BoxOpen
			};

			[BackgroundDependencyLoader]
			private void load () {
				Add( new LoadSection() );
			}
		}

		partial class LoadSection : SettingsSubsection {
			protected override LocalisableString Header => @"Load";

			[BackgroundDependencyLoader]
			private void load ( SceneryContainer? scenery, Sections sections ) {
				foreach ( var i in Enum.GetValues<SceneryType>() ) {
					Add( new SettingsButton {
						Text = i.GetLocalisableDescription(),
						Action = () => {
							scenery?.LoadPreset( i );
							sections.SectionsContainer.ScrollTo( this );
						}
					} );
				}
			}
		}
	}
}
