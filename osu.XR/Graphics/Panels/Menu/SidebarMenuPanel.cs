using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Panels.Menu;

public partial class SidebarMenuPanel : MenuPanel {
	SettingsSidebar sidebar;

	public SidebarMenuPanel () {
		ContentAutoSizeAxes = Axes.X;

		Content.Add( sidebar = new SettingsSidebar() );
	}

	public void AddButton ( IconUsage icon, LocalisableString header, Action onSelected ) {
		sidebar.Add( new SidebarIconButton {
			Section = new FakeSection( icon, header ),
			Action = onSelected
		} );
	}

	float contentWidth = 0;
	protected override void UpdateAfterChildren () {
		if ( contentWidth != Content.DrawWidth ) {
			contentWidth = Content.DrawWidth;
			InvalidateMesh();
		}

		base.UpdateAfterChildren();
	}

	protected override void RegenrateMesh () {
		var h = PANEL_HEIGHT / 2;
		var w = PANEL_HEIGHT / PREFFERED_CONTENT_HEIGHT * (int)Content.DrawWidth;

		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( 0, h, 0 ),
			TR = new Vector3( w, h, 0 ),
			BL = new Vector3( 0, -h, 0 ),
			BR = new Vector3( w, -h, 0 )
		} );
	}

	partial class FakeSection : SettingsSection {
		IconUsage icon;
		LocalisableString header;

		public FakeSection ( IconUsage icon, LocalisableString header ) {
			this.icon = icon;
			this.header = header;
		}

		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = icon };

		public override LocalisableString Header => header;
	}
}