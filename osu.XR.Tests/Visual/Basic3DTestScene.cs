using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Testing;
using osuTK.Graphics;

namespace osu.XR.Tests.Visual;

public class Basic3DTestScene : TestScene3D {
	public Basic3DTestScene () {
		Scene.Camera.Z = -10;

		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Red,
			Scale = new( 10, 0.01f, 0.01f ),
			Origin = new( -1, 0, 0 ),
			Alpha = 0.7f
		} );
		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Green,
			Scale = new( 0.01f, 10, 0.01f ),
			Origin = new( 0, -1, 0 ),
			Alpha = 0.7f
		} );
		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Blue,
			Scale = new( 0.01f, 0.01f, 10 ),
			Origin = new( 0, 0, -1 ),
			Alpha = 0.7f
		} );
	}
}
