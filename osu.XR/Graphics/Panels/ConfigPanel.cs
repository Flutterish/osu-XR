using osu.Game.Overlays.Settings;
using osu.XR.Allocation;

namespace osu.XR.Graphics.Panels;

public abstract class ConfigPanel : OsuPanel {
	SectionsContainer sections;
	public ConfigPanel ( bool showSidebar ) {
		ContentSize = new Vector2( 480, 600 );
		Content.Add( sections = new SectionsContainer( showSidebar, this ) );
	}

	protected override void RegenrateMesh () {
		var w = 0.4f / 2;
		var h = 0.5f / 2;

		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( -w, h, 0 ),
			TR = new Vector3( w, h, 0 ),
			BL = new Vector3( -w, -h, 0 ),
			BR = new Vector3( w, -h, 0 )
		} );
	}

	protected abstract IEnumerable<SettingsSection> CreateSections ();

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		return new ExcludingDependencyContainer( base.CreateChildDependencies( parent ), t => t.Name != "IOverlayManager" );
	}

	protected override void Update () {
		base.Update();
		sections.Show();
	}

	class SectionsContainer : osu.Game.Overlays.SettingsPanel {
		ConfigPanel source;
		public SectionsContainer ( bool showSidebar, ConfigPanel source ) : base( showSidebar ) {
			AutoSizeAxes = Axes.None;
			RelativeSizeAxes = Axes.Both;
			this.source = source;
		}

		protected override IEnumerable<SettingsSection> CreateSections ()
			=> source.CreateSections();
	}
}