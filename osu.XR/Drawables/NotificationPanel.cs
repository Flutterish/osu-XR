using osu.Game.Overlays;

namespace osu.XR.Drawables {
	public class NotificationPanel : NotificationOverlay {
		public NotificationPanel () {
			
		}

		protected override bool StartHidden => true;

		protected override void LoadComplete () {
			base.LoadComplete();
			Hide();
		}
	}
}
