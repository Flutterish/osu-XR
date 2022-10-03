using osu.Framework.Graphics.Sprites;
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
	PanelStack<MenuPanel> handheldMenu;
	SidebarMenuPanel handheldMenuSidebar;

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
		scene.Add( handheldMenu = new PopoutPanelStack<MenuPanel> {
			Children = new MenuPanel[] {
				settings = new VrSettingsPanel(),
				notifications = new VrNotificationsPanel()
			}
		} );
		scene.Add( handheldMenuSidebar = new SidebarMenuPanel() { X = MenuPanel.PANEL_WIDTH / 2 + 0.01f, EulerY = 0.5f } );
		handheldMenuSidebar.AddButton( FontAwesome.Solid.Cog, "Settings", () => handheldMenu.FocusPanel( settings ) );
		handheldMenuSidebar.AddButton( FontAwesome.Solid.ExclamationCircle, "Notifications", () => handheldMenu.FocusPanel( notifications ) );
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
