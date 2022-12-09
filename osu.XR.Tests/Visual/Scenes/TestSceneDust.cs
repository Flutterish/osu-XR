using osu.XR.Graphics.Sceneries;
using osu.XR.Graphics.Sceneries.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests.Visual.Scenes;

public partial class TestSceneDust : Basic3DTestScene {
	public TestSceneDust () {
		Scene.Add( new DustEmitter() );
	}
}
