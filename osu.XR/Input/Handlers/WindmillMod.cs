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

	protected override void Update () {
		base.Update();
		var headset = VR.TrackedDevices.OfType<Headset>().SingleOrDefault();
		var player = headset != null ? headset.Position : Vector3.Zero;
		var playerRot = headset != null ? headset.Rotation : Quaternion.Identity;
		player -= new Vector3( 0, 0.15f, 0 );

		if ( leftTip?.FetchDataForNextFrame() is PoseInput leftPose ) {
			var pos = playerRot.DecomposeAroundAxis( Vector3.UnitY ).Inverted().Apply( leftPose.Position - player );
			pos.Y = -pos.Y;
			pos.Z -= 0.1f;
			pos.X += 0.1f;
			LeftJoystickPosition.Value = pos.Normalized().Xy;
		}
		else {
			LeftJoystickPosition.Value = Vector2.Zero;
		}

		if ( rightTip?.FetchDataForNextFrame() is PoseInput rightPose ) {
			var pos = playerRot.DecomposeAroundAxis( Vector3.UnitY ).Inverted().Apply( rightPose.Position - player );
			pos.Y = -pos.Y;
			pos.Z -= 0.1f;
			pos.X -= 0.1f;
			RightJoystickPosition.Value = pos.Normalized().Xy;
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
