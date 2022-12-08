﻿using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Graphics;
using osu.XR.Graphics.Panels.Settings;

namespace osu.XR.Graphics.Panels.Menu;

public partial class HandheldMenu : CompositeDrawable3D {
	public readonly PanelStack<MenuPanel> Panels;
	public readonly SidebarMenuPanel Sidebar;

	public readonly VrSettingsPanel Settings;
	public readonly VrNotificationsPanel Notifications;

	public HandheldMenu () {
		// new ConfigPanel(), Notifications, Inspector, InputBindings, new ChangelogPanel(), new SceneManagerPanel()
		AddInternal( Panels = new PopoutPanelStack<MenuPanel> {
			Children = new MenuPanel[] {
				Settings = new VrSettingsPanel(),
				Notifications = new VrNotificationsPanel()
			}
		} );
		AddInternal( Sidebar = new SidebarMenuPanel() { X = MenuPanel.PANEL_WIDTH / 2 + 0.01f, EulerY = 0.5f } );

		Sidebar.AddButton( FontAwesome.Solid.Cog, "Settings", () => Panels.FocusPanel( Settings ) );
		Sidebar.AddButton( FontAwesome.Solid.ExclamationCircle, "Notifications", () => Panels.FocusPanel( Notifications ) );

		VisibilityChanged += onVisibilityChanged;
	}

	private void onVisibilityChanged ( Drawable3D _, bool isVisible ) {
		foreach ( var i in Panels.Children ) {
			i.IsColliderEnabled = isVisible;
		}
		Sidebar.IsColliderEnabled = isVisible;
	}

	protected override void Update () {
		base.Update();
		foreach ( var i in Panels.Children )
			i.ParentAlpha = Alpha;
		Sidebar.Alpha = Alpha;
	}
}
