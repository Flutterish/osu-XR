using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;

namespace osu.XR.Input.Handlers;

public abstract partial class JoystickHandler : ActionBindingHandler {
	public override LocalisableString Name => $@"{Hand switch { Framework.XR.VirtualReality.Hand.Left => "Left ", Framework.XR.VirtualReality.Hand.Right => "Right ", _ => "" }}Joystick {Source.Name}";
	public readonly Hand? Hand;

	public JoystickHandler ( Hand? hand, IActionBinding source ) : base( source ) {
		this.Hand = hand;
		GetController( hand, c => {
			actualJoystickPosition.BindTo( Input.GetAction<Vector2Action>( VrAction.Scroll, c ).ValueBindable );
		} );

		actualJoystickPosition.BindValueChanged( v => {
			correctedJoystickPosition.Value = new( v.NewValue.X, -v.NewValue.Y );
		} );

		JoystickPosition.Current = correctedJoystickPosition;
		windmill.BindValueChanged( v => {
			isWindmillEnabled.UnbindBindings();
			isWindmillEnabled.Value = false;

			if ( v.NewValue != null ) {
				isWindmillEnabled.BindTo( hand == Framework.XR.VirtualReality.Hand.Left ? v.NewValue.IsLeftEnabled : v.NewValue.IsRightEnabled );
			}
		} );

		isWindmillEnabled.BindValueChanged( v => {
			if ( v.NewValue ) {
				JoystickPosition.Current = hand == Framework.XR.VirtualReality.Hand.Left ? windmill.Value!.LeftJoystickPosition : windmill.Value!.RightJoystickPosition;
			}
			else {
				JoystickPosition.Current = correctedJoystickPosition;
			}
		} );

		HandlerMods.BindCollectionChanged( (_,e) => {
			windmill.Value = HandlerMods.OfType<WindmillMod>().FirstOrDefault();
		}, true );
	}

	Bindable<Vector2> actualJoystickPosition = new();
	Bindable<Vector2> correctedJoystickPosition = new();

	public readonly Bindable<WindmillMod?> windmill = new();
	public readonly Bindable<bool> isWindmillEnabled = new();

	public readonly BindableWithCurrent<Vector2> JoystickPosition = new();
}
