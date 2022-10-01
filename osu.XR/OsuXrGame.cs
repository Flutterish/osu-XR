﻿using osu.Framework.XR.Graphics.Rendering;
using osu.XR.Graphics.Panels;
using osu.XR.Osu;

namespace osu.XR;

public class OsuXrGame : OsuXrGameBase {
	Scene Scene;
	CurvedPanel osuPanel;
	OsuGameContainer gameContainer;

	public OsuXrGame () {
		Scene = new() {
			RelativeSizeAxes = Axes.Both
		};
		Scene.Camera.Z = -5;
		Scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 )
		} );
		osuPanel.Content.Add( gameContainer = new() );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		Add( Scene );
	}
}
