using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.IO;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Input.Actions;

public class ButtonBinding : ActionBinding, IHasBindingType, IIsHanded {
	public override LocalisableString Name => Hand is Hand.Left ? TypesStrings.ButtonsLeft : TypesStrings.ButtonsRight;
	public override bool ShouldBeSaved => Primary.Value != null || Secondary.Value != null;
	public override Drawable CreateEditor () => new ButtonEditor( this );
	public override ButtonsHandler CreateHandler () => new( this );

	public readonly Bindable<object?> Primary = new();
	public readonly Bindable<object?> Secondary = new();

	public readonly Hand Hand;
	Hand IIsHanded.Hand => Hand;
	public BindingType Type => BindingType.Buttons;
	public ButtonBinding ( Hand hand ) {
		Hand = hand;
		TrackSetting( Primary );
		TrackSetting( Secondary );
	}

	public override object CreateSaveData ( BindingsSaveContext context ) => new SaveData() {
		Hand = Hand,
		Primary = context.SaveAction( Primary ),
		Secondary = context.SaveAction( Secondary )
	};

	public struct SaveData {
		public Hand Hand;
		public ActionData? Primary;
		public ActionData? Secondary;
	}
}
