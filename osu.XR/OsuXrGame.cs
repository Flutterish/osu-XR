using osu.Framework.XR.Components;
using osu.Framework.XR.Physics;
using osu.XR.Graphics;
using osu.XR.Graphics.Panels;
using osu.XR.Graphics.Panels.Menu;
using osu.XR.Graphics.Scenes;

namespace osu.XR;

public class OsuXrGame : OsuXrGameBase {
	OsuXrScene scene;
	OsuGamePanel osuPanel;
	PhysicsSystem physics = new();
	SceneMovementSystem movementSystem;
	PanelInteractionSystem panelInteraction;

	public OsuXrGame () {
		scene = new() {
			RelativeSizeAxes = Axes.Both
		};
		scene.Camera.Z = -5;
		scene.Camera.Y = 1;
		scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 ),
			Y = 1.8f
		} );

		physics.AddSubtree( scene.Root );
		Add( movementSystem = new( scene ) { RelativeSizeAxes = Axes.Both } );
		Add( panelInteraction = new( scene, physics ) { RelativeSizeAxes = Axes.Both } );

		scene.Add( new HandheldMenu() { Y = 1 } );
	}

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );

		deps.CacheAs( osuPanel.OsuDependencies );

		return base.CreateChildDependencies( deps );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		Add( scene );
		scene.Add( new GridScene() );
	}
}
