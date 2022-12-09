using osu.Game.Overlays.Settings;
using osu.XR.Graphics.Panels.Settings;

namespace osu.XR.Graphics.Panels.Menu;

public partial class SceneManagementPanel : SettingsPanel {
	protected override Sections CreateSectionsContainer ()
		=> new( showSidebar: false );

	public partial class Sections : SectionsContainer {
		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( "Scene Manager", "change up the scenery" );

		protected override void LoadComplete () {
			base.LoadComplete();

		}
	}
}
