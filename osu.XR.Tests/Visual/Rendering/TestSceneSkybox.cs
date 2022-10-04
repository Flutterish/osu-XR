﻿using osu.XR.Graphics.Scenes;
using osu.XR.Graphics.Scenes.Components;

namespace osu.XR.Tests.Visual.Rendering;

public class TestSceneSkybox : Basic3DTestScene {
	public TestSceneSkybox () {
		Scene.Add( new VerticalGradientSkyBox() );
	}
}
