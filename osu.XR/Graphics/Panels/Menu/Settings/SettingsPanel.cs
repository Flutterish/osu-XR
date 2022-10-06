﻿using osu.Game.Graphics.Cursor;
using osu.XR.Allocation;
using osu.XR.Graphics.Panels.Menu;

namespace osu.XR.Graphics.Panels.Settings;

public abstract class SettingsPanel : MenuPanel {
	public SettingsPanel () {
		ContentSize = new Vector2( Game.Overlays.SettingsPanel.PANEL_WIDTH, Game.Overlays.SettingsPanel.PANEL_WIDTH / ASPECT_RATIO );
		Content.Add( new OsuTooltipContainer(null) {
			RelativeSizeAxes = Axes.Both,
			Child = CreateSectionsContainer()
		} );
	}

	protected abstract SectionsContainer CreateSectionsContainer ();

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		return new ExcludingDependencyContainer( base.CreateChildDependencies( parent ), t => t.Name != "IOverlayManager" );
	}

	public abstract class SectionsContainer : Game.Overlays.SettingsPanel {
		public SectionsContainer ( bool showSidebar ) : base( showSidebar ) {
			AutoSizeAxes = Axes.None;
			RelativeSizeAxes = Axes.Both;
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Show();
		}
	}
}