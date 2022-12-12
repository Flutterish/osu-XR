using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Configuration;

namespace osu.XR.Graphics.Panels.Menu;

public partial class SceneManagementPanel : SettingsPanel {
	protected override Sections CreateSectionsContainer ()
		=> new( showSidebar: false );

	public partial class Sections : SectionsContainer {
		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( Localisation.SceneryStrings.Header, Localisation.SceneryStrings.Flavour );

		protected override IEnumerable<SettingsSection> CreateSections () {
			yield return new Section();
		}

		partial class Section : SettingsSection {
			public Section () {
				
			}

			public override Drawable CreateIcon () => new SpriteIcon {
				Icon = FontAwesome.Solid.Image
			};

			[BackgroundDependencyLoader]
			private void load ( OsuXrConfigManager config ) {
				Add( new SettingsEnumDropdown<SceneryType> { 
					Current = config.GetBindable<SceneryType>( OsuXrSetting.SceneryType ), 
					LabelText = Localisation.SceneryStrings.Type 
				} );
			}

			public override LocalisableString Header => string.Empty;
		}
	}
}
