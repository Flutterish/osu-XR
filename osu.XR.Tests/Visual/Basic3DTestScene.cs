using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using osu.XR.Graphics;
using osuTK.Graphics;

namespace osu.XR.Tests.Visual;

public abstract partial class Basic3DTestScene : TestScene3D {
	public readonly PhysicsSystem Physics = new();
	public readonly PanelInteractionSystem InteractionSystem = new();
	public readonly BasicPanelInteractionSource MouseInteractionSource;

	public Basic3DTestScene () {
		Physics.AddSubtree( Scene.Root );
		Add( MouseInteractionSource = new BasicPanelInteractionSource( Scene, Physics, InteractionSystem ) { RelativeSizeAxes = Axes.Both } );
		Scene.Camera.Z = -10;

		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Red,
			Scale = new( 10, 0.01f, 0.01f ),
			Origin = new( -1, 0, 0 ),
			Alpha = 0.7f
		} );
		for ( int i = 1; i < 6; i++ ) {
			Scene.Add( new BasicModel() {
				Mesh = BasicMesh.UnitCube,
				Colour = Color4.Red,
				Scale = new( 0.03f ),
				Position = new( i, 0, 0 ),
				Alpha = 0.7f
			} );
		}
		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Green,
			Scale = new( 0.01f, 10, 0.01f ),
			Origin = new( 0, -1, 0 ),
			Alpha = 0.7f
		} );
		for ( int i = 1; i < 6; i++ ) {
			Scene.Add( new BasicModel() {
				Mesh = BasicMesh.UnitCube,
				Colour = Color4.Green,
				Scale = new( 0.03f ),
				Position = new( 0, i, 0 ),
				Alpha = 0.7f
			} );
		}
		Scene.Add( new BasicModel() {
			Mesh = BasicMesh.UnitCube,
			Colour = Color4.Blue,
			Scale = new( 0.01f, 0.01f, 10 ),
			Origin = new( 0, 0, -1 ),
			Alpha = 0.7f
		} );
		for ( int i = 1; i < 6; i++ ) {
			Scene.Add( new BasicModel() {
				Mesh = BasicMesh.UnitCube,
				Colour = Color4.Blue,
				Scale = new( 0.03f ),
				Position = new( 0, 0, i ),
				Alpha = 0.7f
			} );
		}
	}

	protected override Scene CreateScene ()
		=> new OsuXrScene();
}
