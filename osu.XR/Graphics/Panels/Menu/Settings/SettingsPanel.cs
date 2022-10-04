﻿using osu.Game.Graphics.Cursor;
using osu.Game.Overlays.Settings;
using osu.XR.Allocation;
using osu.XR.Graphics.Panels.Menu;

namespace osu.XR.Graphics.Panels.Settings;

public abstract class SettingsPanel : MenuPanel {
	SectionsContainer sections;
	public SettingsPanel () {
		ContentSize = new Vector2( Game.Overlays.SettingsPanel.PANEL_WIDTH, Game.Overlays.SettingsPanel.PANEL_WIDTH / ASPECT_RATIO );
		Content.Add( new OsuTooltipContainer(null) {
			RelativeSizeAxes = Axes.Both,
			Child = sections = new SectionsContainer( false, this )
		} );
	}

	protected abstract IEnumerable<SettingsSection> CreateSections ();
	protected virtual Drawable CreateHeader () => new Container();
	protected virtual Drawable CreateFooter () => new Container();

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		return new ExcludingDependencyContainer( base.CreateChildDependencies( parent ), t => t.Name != "IOverlayManager" );
	}

	class SectionsContainer : Game.Overlays.SettingsPanel {
		SettingsPanel source;
		public SectionsContainer ( bool showSidebar, SettingsPanel source ) : base( showSidebar ) {
			AutoSizeAxes = Axes.None;
			RelativeSizeAxes = Axes.Both;
			this.source = source;
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Show();
		}

		protected override Drawable CreateHeader ()
			=> source.CreateHeader();
		protected override Drawable CreateFooter ()
			=> source.CreateFooter();

		protected override IEnumerable<SettingsSection> CreateSections ()
			=> source.CreateSections();
	}
}