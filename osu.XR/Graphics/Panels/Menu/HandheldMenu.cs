using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Graphics;

namespace osu.XR.Graphics.Panels.Menu;

public partial class HandheldMenu : CompositeDrawable3D {
	public readonly PanelStack<MenuPanel> Panels;
	public readonly SidebarMenuPanel Sidebar;

	public readonly VrSettingsPanel Settings;
	public readonly VrNotificationsPanel Notifications;
	public readonly SceneManagementPanel Scenery;
	public readonly BindingsPanel Bindings;
	public readonly ChangelogPanel Changelog;
	public readonly HelpPanel Help;

	public HandheldMenu () {
		// new ConfigPanel(), Notifications, Inspector, InputBindings, new ChangelogPanel(), new SceneManagerPanel()
		AddInternal( Panels = new PopoutPanelStack<MenuPanel> {
			Children = new MenuPanel[] {
				Settings = new VrSettingsPanel(),
				Bindings = new BindingsPanel(),
				Scenery = new SceneManagementPanel(),
				Notifications = new VrNotificationsPanel(),
				Help = new HelpPanel(),
				Changelog = new ChangelogPanel()
			}
		} );
		AddInternal( Sidebar = new SidebarMenuPanel() { X = MenuPanel.PANEL_WIDTH / 2 + 0.01f, EulerY = 0.5f } );
		var presets = Settings.PresetSettings;
		AddInternal( presets );
		presets.OriginX = MenuPanel.PANEL_WIDTH / 2;
		presets.X = -(MenuPanel.PANEL_WIDTH / 2 + 0.01f);
		presets.EulerY = -0.5f;

		Sidebar.AddButton( FontAwesome.Solid.Cog, Localisation.MenuStrings.Settings, () => Panels.FocusPanel( Settings ) );
		Sidebar.AddButton( FontAwesome.Solid.Gamepad, Localisation.MenuStrings.Ruleset, () => Panels.FocusPanel( Bindings ) );
		Sidebar.AddButton( FontAwesome.Solid.Image, Localisation.MenuStrings.Scenery, () => Panels.FocusPanel( Scenery ) );
		Sidebar.AddButton( FontAwesome.Solid.ExclamationCircle, Localisation.MenuStrings.Notifications, () => Panels.FocusPanel( Notifications ) );
		Sidebar.AddButton( FontAwesome.Solid.QuestionCircle, @"Help", () => Panels.FocusPanel( Help ) );
		Sidebar.AddButton( FontAwesome.Solid.Clipboard, @"Changelog", () => Panels.FocusPanel( Changelog ) );

		VisibilityChanged += onVisibilityChanged;
		Panels.FocusPanel( Notifications );
	}

	private void onVisibilityChanged ( Drawable3D _, bool isVisible ) {
		foreach ( var i in Panels.Children ) {
			i.IsColliderEnabled = isVisible;
		}
		Sidebar.IsColliderEnabled = isVisible;
	}

	public override float Alpha {
		get => base.Alpha; 
		set {
			base.Alpha = value;

			foreach ( var i in Panels.Children.Append( Settings.PresetSettings ) )
				i.ParentAlpha = Alpha;
			Sidebar.Alpha = Alpha;
		}
	}
}
