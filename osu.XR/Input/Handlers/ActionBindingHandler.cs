using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;

namespace osu.XR.Input.Handlers;

public partial class ActionBindingHandler : Component {
	[Resolved]
	protected OsuXrGameBase Game { get; private set; } = null!;
	[Resolved]
	protected VrCompositor VR { get; private set; } = null!;
	[Resolved]
	protected VrInput Input { get; private set; } = null!;

	protected void RegisterAction ( Bindable<object?> action, Bindable<bool> state ) {

	}
	public event Action<object>? ActionPressed;
	public event Action<object>? ActionReleased;

	List<Action<VrDevice>> disposeActions = new();
	protected void GetDevice ( Func<VrDevice, bool> validator ) {
		void perform () {
			foreach ( var i in VR.TrackedDevices ) {
				if ( validator( i ) )
					return;
			}

			void onDeviceDetected ( VrDevice device ) {
				if ( validator( device ) ) {
					VR.DeviceDetected -= onDeviceDetected;
					disposeActions.Remove( onDeviceDetected );
				}
			}
			
			VR.DeviceDetected += onDeviceDetected;
			disposeActions.Add( onDeviceDetected );
		}

		if ( IsLoaded )
			perform();
		else
			OnLoadComplete += _ => perform();
	}

	protected void GetController ( Hand? hand, Action<Controller?> action ) {
		GetDevice( d => {
			if ( d is Controller controller && controller.Role == (hand == Hand.Left ? Valve.VR.ETrackedControllerRole.LeftHand : Valve.VR.ETrackedControllerRole.RightHand) ) {
				action( controller );
				return true;
			}
			return false;
		} );
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			foreach ( var i in disposeActions ) {
				VR.DeviceDetected -= i;
			}
		}

		base.Dispose( isDisposing );
	}
}