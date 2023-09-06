using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.Game.Overlays;

namespace osu.XR.Graphics.Input;

public partial class ControllerInputDisplay : Container {
	[Resolved]
	protected VrCompositor VR { get; private set; } = null!;
	[Resolved]
	protected VrInput Input { get; private set; } = null!;

	Hand hand;
	public ControllerInputDisplay ( Hand hand ) {
		this.hand = hand;
		GetController( load );
	}

	[Cached]
	OverlayColourProvider colourProvider = new( OverlayColourScheme.Purple );

	void load ( Controller controller ) {
		Size = new( 160, 100 );

		// TODO haptic display?

		Add( new ButtonNub {
			Size = new( 26 ),
			Position = new( Width - 38, 0 ),
			Colour = colourProvider.Colour0,
			Current = Input.GetAction<BooleanAction>( VrAction.LeftButton, controller ).ValueBindable
		} );
		Add( new ButtonNub {
			Size = new( 26 ),
			Position = new( Width - 26, (Height - 24) / 2 ),
			Colour = colourProvider.Colour0,
			Current = Input.GetAction<BooleanAction>( VrAction.RightButton, controller ).ValueBindable
		} );
		Add( new ButtonNub {
			Size = new( 40, 16 ),
			Position = new( 0, Height - 16 ),
			Colour = colourProvider.Colour0,
			Current = Input.GetAction<BooleanAction>( VrAction.ToggleMenu, controller ).ValueBindable
		} );
		Add( new ButtonNub {
			Size = new( 40, 16 ),
			Position = new( 8, 0 ),
			Colour = colourProvider.Colour0,
			Current = Input.GetAction<BooleanAction>( VrAction.Teleport, controller ).ValueBindable
		} );
		Add( new JoystickDisplay {
			Size = new( 80 ),
			Position = new( 45, (Height - 60) / 2 - 10 ),
			Colour = colourProvider.Colour0,
			JoystickPosition = Input.GetAction<Vector2Action>( VrAction.Scroll, controller ).ValueBindable
		} );

		if ( hand != Hand.Right )
			return;

		foreach ( var i in Children ) {
			i.X = -i.X;
			i.Origin = Anchor.TopRight;
			i.Anchor = Anchor.TopRight;
		}
	}

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

	protected void GetController ( Action<Controller> action ) {
		GetDevice( d => {
			if ( d is Controller controller && controller.Role == (hand == Hand.Left ? Valve.VR.ETrackedControllerRole.LeftHand : Valve.VR.ETrackedControllerRole.RightHand) ) {
				action( controller );
				return true;
			}
			return false;
		} );
	}

	List<Action<VrDevice>> disposeActions = new();
	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			foreach ( var i in disposeActions ) {
				VR.DeviceDetected -= i;
			}
		}

		base.Dispose( isDisposing );
	}
}
