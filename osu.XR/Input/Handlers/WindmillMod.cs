using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Input.Actions.Gestures;

namespace osu.XR.Input.Handlers;

public partial class WindmillMod : HandlerMod {
	public WindmillMod ( WindmillBinding source ) : base( source ) {
		IsLeftEnabled.BindTarget = source.IsLeftEnabled;
		IsRightEnabled.BindTarget = source.IsRightEnabled;

		GetController( Hand.Left, c => {
			leftTip = Input.GetAction<PoseAction>( VrAction.ControllerTip, c );
		} );
		GetController( Hand.Right, c => {
			rightTip = Input.GetAction<PoseAction>( VrAction.ControllerTip, c );
		} );
	}

	PoseAction? leftTip;
	PoseAction? rightTip;

	Vector3? lastLeftTipPosition;
	Vector3? lastRightTipPosition;
	protected override void Update () {
		base.Update();
		var headset = VR.TrackedDevices.OfType<Headset>().SingleOrDefault();
		var player = VR.ActivePlayer;
		var playerRot = headset != null ? headset.Rotation : Quaternion.Identity; // TODO maybe devices should be in player-space?

		if ( player != null ) {
			playerRot = player.InGlobalSpace( playerRot );
		}

		void move ( Bindable<Vector2> joystick, Vector3 delta ) {
			const float range = 0.12f;
			var change = delta.Xy / range;
			var next = new Vector2() {
				X = float.Clamp( joystick.Value.X + change.X, -1, 1 ),
				Y = float.Clamp( joystick.Value.Y - change.Y, -1, 1 )
			};
			joystick.Value = next.Length > 1 ? next.Normalized() : next;
		}

		if ( leftTip?.FetchDataForNextFrame() is PoseInput leftPose ) {
			if ( lastLeftTipPosition is Vector3 last ) {
				var delta = playerRot.DecomposeAroundAxis( Vector3.UnitY ).Inverted().Apply( leftPose.Position - last );
				move( LeftJoystickPosition, delta );
			}
			
			lastLeftTipPosition = leftPose.Position;
		}
		else {
			LeftJoystickPosition.Value = Vector2.Zero;
		}

		if ( rightTip?.FetchDataForNextFrame() is PoseInput rightPose ) {
			if ( lastRightTipPosition is Vector3 last ) {
				var delta = playerRot.DecomposeAroundAxis( Vector3.UnitY ).Inverted().Apply( rightPose.Position - last );
				move( RightJoystickPosition, delta );
			}

			lastRightTipPosition = rightPose.Position;
		}
		else {
			RightJoystickPosition.Value = Vector2.Zero;
		}
	}

	public readonly Bindable<bool> IsLeftEnabled = new();
	public readonly Bindable<bool> IsRightEnabled = new();
	public readonly Bindable<Vector2> LeftJoystickPosition = new();
	public readonly Bindable<Vector2> RightJoystickPosition = new();
}
