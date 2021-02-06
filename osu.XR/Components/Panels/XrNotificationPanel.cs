using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Notifications;
using osu.XR.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components.Panels {
	public class XrNotificationPanel : FlatPanel {
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
	}
}
