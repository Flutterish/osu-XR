using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.XR.Drawables {
	public class NotificationPanel : ConfigurationContainer {
		public NotificationPanel () {
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