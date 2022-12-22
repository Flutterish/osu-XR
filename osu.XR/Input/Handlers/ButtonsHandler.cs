using osu.Framework.XR.VirtualReality;
using osu.XR.Input.Actions;

namespace osu.XR.Input.Handlers;

public partial class ButtonsHandler : ActionBindingHandler {
	public ButtonsHandler ( ButtonBinding source ) : base( source ) {
		GetController( source.Hand, c => {
			Primary.BindTo( Input.GetAction<BooleanAction>( VrAction.LeftButton, c ).ValueBindable );
			Secondary.BindTo( Input.GetAction<BooleanAction>( VrAction.RightButton, c ).ValueBindable );
		} );

		RegisterAction( source.Primary, Primary );
		RegisterAction( source.Secondary, Secondary );
	}

	public readonly BindableBool Primary = new();
	public readonly BindableBool Secondary = new();
}
