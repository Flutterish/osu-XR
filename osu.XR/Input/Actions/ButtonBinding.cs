using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Input.Actions;

public class ButtonBinding : ActionBinding {
	public override LocalisableString Name => Hand is Hand.Left ? TypesStrings.ButtonsLeft : TypesStrings.ButtonsRight;

	public readonly Bindable<object> Primary = new();
	public readonly Bindable<object> Secondary = new();

	public readonly Hand Hand;
	public ButtonBinding ( Hand hand ) {
		Hand = hand;
		TrackSetting( Primary );
		TrackSetting( Secondary );
	}
}
