using osu.Framework.XR.Components;
using osu.Framework.XR.Physics;
using osu.XR.Graphics;
using osu.XR.Graphics.Panels;
using osu.XR.Graphics.Panels.Menu;
using osu.XR.Graphics.Panels.Settings;
using osu.XR.Graphics.Scenes;

namespace osu.XR;

public class OsuXrGame : OsuXrGameBase {
	OsuXrScene scene;
	OsuGamePanel osuPanel;
	PhysicsSystem physics = new();
	SceneMovementSystem movementSystem;
	PanelInteractionSystem panelInteraction;

	VrSettingsPanel settings;
	VrNotificationsPanel notifications;

	public OsuXrGame () {
		scene = new() {
			RelativeSizeAxes = Axes.Both
		};
		scene.Camera.Z = -5;
		scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 )
		} );

		physics.AddSubtree( scene.Root );
		Add( movementSystem = new( scene ) { RelativeSizeAxes = Axes.Both } );
		Add( panelInteraction = new( scene, physics ) { RelativeSizeAxes = Axes.Both } );

		// new ConfigPanel(), Notifications, Inspector, InputBindings, new ChangelogPanel(), new SceneManagerPanel()
		scene.Add( settings = new VrSettingsPanel() );
		scene.Add( notifications = new VrNotificationsPanel() { X = 0.1f, Z = 0.2f } );
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
