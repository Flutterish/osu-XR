using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Transforms;
using osu.Framework.XR.Physics;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Configuration;

namespace osu.XR.Graphics.VirtualReality;

public partial class UserTrackingDrawable3D : Container3D {
	BindableList<VrDevice> activeDevices = new();
	[BackgroundDependencyLoader( true )]
	private void load ( VrCompositor? vr, OsuXrConfigManager? config ) {
		if ( vr != null ) {
			activeDevices.BindTarget = vr.ActiveDevices;
		}

		if ( config != null ) {
			config.BindWith( OsuXrSetting.InputMode, inputMode );
		}

		game.VrControllers.BindCollectionChanged( (_, e) => {
			if ( e.NewItems == null )
				return;

			foreach ( VrController i in e.NewItems ) {
				i.ToggleMenuPressed += onToggleMenuPressed;
			}
		}, true );

		IsOpen = false;
	}

	private void onToggleMenuPressed ( VrController controller ) {
		if ( IsOpen && (HoldingController == controller || HoldingController == null) ) {
			IsOpen = false;
		}
		else {
			IsOpen = true;
			openingController = controller;
		}
	}

	protected override void Dispose ( bool isDisposing ) {
		base.Dispose( isDisposing );

		foreach ( var i in game.VrControllers ) {
			i.ToggleMenuPressed -= onToggleMenuPressed;
		}
	}

	[Resolved]
	OsuXrGame game { get; set; } = null!;
	VrController? openingController;
	bool isOpen;
	public bool IsOpen {
		get => isOpen;
		set {
			IsVisible = value;
			isOpen = value;
			if ( !value )
				openingController = null;
		}
	}

	Bindable<InputMode> inputMode = new();
	public VrController? HoldingController {
		get {
			if ( activeDevices.OfType<Controller>().Count() == 0 ) return null;
			if ( inputMode.Value == InputMode.SinglePointer ) return game.SecondaryActiveController;
			if ( openingController?.IsEnabled == true ) return openingController;
			return null;
		}
	}

	private Vector3 retainedPosition;
	protected Vector3 TargetPosition {
		get {
			if ( !IsOpen ) return retainedPosition;
			if ( HoldingController is null ) {
				if ( game.Headset is null )
					return Vector3.Zero;
				return retainedPosition = game.Headset.Position + game.Headset.Rotation.Apply( Vector3.UnitZ ) * 0.5f;
			}
			else {
				return retainedPosition = HoldingController.Position + HoldingController.Forward * 0.2f + HoldingController.Up * 0.05f;
			}
		}
	}

	private Quaternion retainedRotation;
	protected Quaternion TargetRotation {
		get {
			if ( !IsOpen ) return retainedRotation;
			if ( HoldingController is null ) {
				if ( game.Headset is null )
					return Quaternion.Identity;
				return retainedRotation = game.Headset.Rotation;
			}
			else {
				return retainedRotation = HoldingController.Rotation * Quaternion.FromAxisAngle( Vector3.UnitX, MathF.PI * 0.25f );
			}
		}
	}

	protected override void Update () {
		base.Update();

		//if ( HoldingController != previousHoldingController ) {
		//	if ( previousHoldingController is not null ) previousHoldingController.HeldObjects.Remove( this );
		//	previousHoldingController = HoldingController;
		//	if ( previousHoldingController is not null ) previousHoldingController.HeldObjects.Add( this );
		//}
		//if ( Elements.Any( x => x.IsColliderEnabled ) ) {
		//	if ( VR.EnabledControllerCount == 0 ) {
		//		Hide();
		//	}
		//	foreach ( var i in Elements ) {
		//		i.RequestedInputMode = HoldingController == Game.MainController ? PanelInputMode.Inverted : PanelInputMode.Regular;
		//	}
		//}

		// doing this every frame makes it have an Easing.Out-like curve
		this.MoveTo( TargetPosition, 100 );
		this.RotateTo( TargetRotation, 100 );
	}
}
