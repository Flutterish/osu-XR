using osu.Framework.Graphics.Sprites;
using osu.XR.Graphics.Panels.Menu;
using osu.XR.Graphics.Panels.Settings;
using osu.XR.Graphics.Panels;

namespace osu.XR.Tests.Visual.Panels;

public class TestSceneHandheldMenu : Osu3DTestScene {
	VrSettingsPanel settings;
	VrNotificationsPanel notifications;
	PanelStack<MenuPanel> handheldMenu;
	SidebarMenuPanel handheldMenuSidebar;

	public TestSceneHandheldMenu () {
		Scene.Add( handheldMenu = new PopoutPanelStack<MenuPanel> {
			Children = new MenuPanel[] {
				settings = new VrSettingsPanel(),
				notifications = new VrNotificationsPanel()
			}
		} );
		Scene.Add( handheldMenuSidebar = new SidebarMenuPanel() { X = MenuPanel.PANEL_WIDTH / 2 + 0.01f, EulerY = 0.5f } );
		handheldMenuSidebar.AddButton( FontAwesome.Solid.Cog, "Settings", () => handheldMenu.FocusPanel( settings ) );
		handheldMenuSidebar.AddButton( FontAwesome.Solid.ExclamationCircle, "Notifications", () => handheldMenu.FocusPanel( notifications ) );
	}
}
