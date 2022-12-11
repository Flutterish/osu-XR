using osu.Framework.Audio;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.XR.VirtualReality;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Settings;
using osu.Game.Resources.Localisation.Web;
using System.Runtime.InteropServices;

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
	AudioManager audio { get; set; } = null!;
	void playSample ( string sampleName ) {
		audio.Samples.Get( sampleName )?.Play();
	}

	[BackgroundDependencyLoader(permitNulls: true)]
	private void load ( VrCompositor? comp ) {
		if ( comp is null )
			return;

		comp.Initialized += comp => {
			var vr = comp.VR!;
			vr.Events.OnOpenVrEvent += onOpenVrEvent;
			vr.Events.OnLog += onLog;
			vr.Events.OnException += onException;
		};
	}

	void onException ( string msg, OpenVR.NET.EventType type, object? ctx, Exception ex ) {
		Post( new SimpleErrorNotification() { Text = $"Exception ~ {type} - {msg}" } );
		Logger.Error( ex, $"{type} - {msg}", "openvr-net", true );
	}

	void onLog ( string msg, OpenVR.NET.EventType type, object? ctx ) {
		Post( new SimpleNotification() { Text = $"OpenVR.NET ~ {type} - {msg}" } );
		Logger.Log( $"{type} - {msg}", "openvr-net", LogLevel.Important );
	}

	void onOpenVrEvent ( Valve.VR.EVREventType type, OpenVR.NET.Devices.VrDevice? device, float age, in Valve.VR.VREvent_Data_t data ) {
		var bytes = MemoryMarshal.AsBytes(stackalloc Valve.VR.VREvent_Data_t[1] { data });
		Logger.Log( $"Type: {type} - Device: {(device is null ? "None" : $"{device.GetType().ReadableName()} : {device.DeviceIndex}")} - {Convert.ToHexString(bytes)}", "openvr", LogLevel.Debug );
	}
}
