using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Testing;
using osuTK.Graphics;

namespace osu.XR.Tests.Visual;

public class TestSceneSample : TestScene3D {
	public TestSceneSample () {
		Scene.Add( new BasicModel { Mesh = BasicMesh.UnitCube, Colour = Color4.YellowGreen } );
	}
}
