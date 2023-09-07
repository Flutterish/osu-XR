using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.Input.Migration;
using osu.XR.IO;
using osu.XR.Localisation.Bindings;
using System.Text.Json;

namespace osu.XR.Input.Actions;

public class ButtonBinding : ActionBinding, IHasBindingType, IIsHanded {
	public override LocalisableString Name => Hand is Hand.Left ? TypesStrings.ButtonsLeft : TypesStrings.ButtonsRight;
	public override bool ShouldBeSaved => Primary.ShouldBeSaved || Secondary.ShouldBeSaved;
	public override Drawable CreateEditor () => new ButtonEditor( this );
	public override ButtonsHandler CreateHandler () => new( this );

	public readonly RulesetAction Primary = new();
	public readonly RulesetAction Secondary = new();

	public readonly Hand Hand;
	Hand IIsHanded.Hand => Hand;
	public BindingType Type => BindingType.Buttons;
	public ButtonBinding ( Hand hand ) {
		Hand = hand;
		TrackSetting( Primary );
		TrackSetting( Secondary );
	}

	protected override object CreateSaveData ( BindingsSaveContext context ) => new SaveData() {
		Type = BindingType.Buttons,
		Hand = Hand,
		Primary = context.SaveAction( Primary ),
		Secondary = context.SaveAction( Secondary )
	};

	public static ButtonBinding? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<ButtonBinding, SaveData>( data, ctx, static (save, ctx) => {
		var buttons = new ButtonBinding( save.Hand );
		ctx.LoadAction( buttons.Primary, save.Primary );
		ctx.LoadAction( buttons.Secondary, save.Secondary );
		return buttons;
	} );

	[FormatVersion( "" )]
	public struct SaveData {
		public BindingType Type;
		public Hand Hand;
		public ActionData? Primary;
		public ActionData? Secondary;

		public static implicit operator SaveData ( V1SaveData from ) => new() {
			Type = BindingType.Buttons,
			Hand = from.Type == "Left Buttons" ? Hand.Left : Hand.Right,
			Primary = from.Primary,
			Secondary = from.Secondary
		};
	}

	[FormatVersion( "[Initial]" )]
	public struct V1SaveData {
		public string Type;
		public ActionData? Primary;
		public ActionData? Secondary;
	}
}
