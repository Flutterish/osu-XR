using osu.Game.Overlays.Notifications;
using osu.XR.Drawables.Containers;

namespace osu.XR.Drawables {
	public class NotificationsDrawable : ConfigurationContainer {
		public NotificationsDrawable () {
			Title = "Notifications";
			Description = "messages and stuff";

			messageContainer = new NotificationSection( "Messages", "Clear all" );
			errorContainer = new NotificationSection( "Errors", "Clear all" );

			AddSection( errorContainer );
			AddSection( messageContainer );
		}

		NotificationSection messageContainer;
		NotificationSection errorContainer;

		public void PostMessage ( Notification notification ) {
			messageContainer.Add( notification, 0 );
		}

		public void PostError ( Notification notification ) {
			errorContainer.Add( notification, 0 );
		}
	}
}