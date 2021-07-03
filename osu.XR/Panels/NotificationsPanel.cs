using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Notifications;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using osu.XR.Panels;

namespace osu.XR.Components.Panels {
	public class NotificationsPanel : HandheldPanel<NotificationsDrawable> {
		public void PostMessage ( Notification notification )
			=> Content.PostMessage( notification );

		public void PostError ( Notification notification )
			=> Content.PostError( notification );

		protected override NotificationsDrawable CreateContent ()
			=> new();

		public override string DisplayName => "Notifications";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.ExclamationCircle };
	}
}
