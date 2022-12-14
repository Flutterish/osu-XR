using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Input.Actions;

public class ButtonBinding : ActionBinding, IIsHanded, IHasEditor {
	public override LocalisableString Name => Hand is Hand.Left ? TypesStrings.ButtonsLeft : TypesStrings.ButtonsRight;
	public override bool ShouldBeSaved => Primary.Value != null || Secondary.Value != null;
	public Drawable CreateEditor () => new ButtonEditor( this );

	public readonly Bindable<object?> Primary = new();
	public readonly Bindable<object?> Secondary = new();

	public readonly Hand Hand;
	Hand IIsHanded.Hand => Hand;
	public ButtonBinding ( Hand hand ) {
		Hand = hand;
		TrackSetting( Primary );
		TrackSetting( Secondary );
	}

}
