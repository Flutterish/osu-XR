using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class NotificationPanel : NotificationOverlay {
		public NotificationPanel () {
			
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Hide();
		}
	}
}
