using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Notifications;
using osu.XR.Components.Groups;
using osu.XR.Drawables;

namespace osu.XR.Components.Panels {
	public class XrNotificationPanel : FlatPanel, IHasName, IHasIcon {
		[Resolved]
		private OsuGameXr Game { get; set; }
		NotificationPanel notifications = new NotificationPanel();

		public XrNotificationPanel () {
			PanelAutoScaleAxes = Axes.X;
			PanelHeight = 0.5;
			RelativeSizeAxes = Axes.None;
			Height = 500;
			AutosizeX();
			Source.Add( notifications );
			this.Hide();
		}

		public void Post ( Notification notification )
			=> notifications.Post( notification );

		public string DisplayName => "Notifications";

		public Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.ExclamationCircle };
	}
}
