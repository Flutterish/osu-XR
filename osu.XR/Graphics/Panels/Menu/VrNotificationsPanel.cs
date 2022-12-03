using osu.Framework.Audio;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;

namespace osu.XR.Graphics.Panels.Menu;

public partial class VrNotificationsPanel : MenuPanel {
	FillFlowContainer<NotificationSection> sections;

	public VrNotificationsPanel () {
		Content.Add( new Box {
			RelativeSizeAxes = Axes.Both,
			Colour = ColourProvider.Background4
		} );
		Content.Add( new OsuScrollContainer {
			Masking = true,
			RelativeSizeAxes = Axes.Both,
			Children = new[] {
				new FillFlowContainer {
					Direction = FillDirection.Vertical,
					AutoSizeAxes = Axes.Y,
					RelativeSizeAxes = Axes.X,
					Children = new Drawable[] {
						new SettingsHeader( "Vr Notifications", "" ),
						sections = new FillFlowContainer<NotificationSection> {
							Direction = FillDirection.Vertical,
							AutoSizeAxes = Axes.Y,
							RelativeSizeAxes = Axes.X,
							Children = new[] {
								new NotificationSection(AccountsStrings.NotificationsTitle, new[] { typeof(SimpleNotification) }, "Clear All"),
								new NotificationSection(@"Running Tasks", new[] { typeof(ProgressNotification) }, @"Cancel All"),
							}
						}
					}
				}
			}
		} );
	}

	int runningDepth;
	public void Post ( Notification notification ) => Schedule( () => {
		++runningDepth;

		if ( notification is IHasCompletionTarget hasCompletionTarget )
			hasCompletionTarget.CompletionTarget = Post;

		notification.Closed += () => playSample( "UI/overlay-pop-out" );
		playSample( notification.PopInSampleName );

		var ourType = notification.GetType();
		var section = sections.Children.First( s => s.AcceptedNotificationTypes.Any( accept => accept.IsAssignableFrom( ourType ) ) );
		int depth = notification.DisplayOnTop ? -runningDepth : runningDepth;
		section.Add( notification, depth );
	} );

	[Resolved]
	private AudioManager audio { get; set; } = null!;
	private void playSample ( string sampleName ) {
		audio.Samples.Get( sampleName )?.Play();
	}
}
