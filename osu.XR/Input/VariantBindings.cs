using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Input.Actions;
using osu.XR.Input.Actions.Gestures;
using osu.XR.Input.Migration;
using osu.XR.IO;
using System.Text.Json;

namespace osu.XR.Input;

public class VariantBindings : UniqueCompositeActionBinding<IHasBindingType, (BindingType, Hand?)> {
	public override LocalisableString Name => $"Variant {Variant}";
	protected override (BindingType, Hand?) GetKey ( IHasBindingType action ) => (action.Type, action is IIsHanded handed ? handed.Hand : null);

	public readonly int Variant;
	SaveData? unloaded;
	public VariantBindings ( int variant ) {
		Variant = variant;
	}

	public override bool ShouldBeSaved => base.ShouldBeSaved || unloaded != null;

	protected override object CreateSaveData ( IEnumerable<IHasBindingType> children, BindingsSaveContext context ) => unloaded ?? new SaveData {
		Name = context.VariantName( Variant ),
		Actions = context.ActionsChecksum( Variant ),
		Bindings = CreateSaveDataAsArray( children, context )
	};

	public static VariantBindings? Load ( JsonElement data, int variant, BindingsSaveContext context ) => Load<VariantBindings, SaveData>( data, context.SetVaraint( variant ), static (save, ctx) => {
		var variant = ctx.Variant!.Value;
		if ( !ctx.Ruleset!.AvailableVariants.Contains( variant ) ) {
			ctx.Error( $@"Could not load variant {variant}", save );
			return new VariantBindings( variant ) {
				unloaded = save
			};
		}

		var bindings = new VariantBindings( variant );
		var checksum = ctx.ActionsChecksum( variant );
		var declared = save.Actions;
		if ( declared is null ) {
			ctx.Warning( @"Variant does not have an action checksum", save );
		}
		else {
			var expected = $@"save declared {string.Join( ", ", declared.Select( x => $@"(ID {x.Key} = {x.Value})" ) )}, but it was {string.Join( ", ", checksum.Select( x => $@"(ID {x.Key} = {x.Value})" ) )}";

			if ( declared.Values.Except( checksum.Values ).Any() ) {
				ctx.Warning( $@"Variant action checksum has non-existent actions - {expected}", save );
			}
			else if ( declared.Except( checksum ).Any() ) {
				ctx.Warning( $@"Variant action checksum is invalid - {expected}", save );
			}
			// TODO try to fix this?
		}
		if ( ctx.VariantName( variant ) != save.Name ) {
			ctx.Warning( @"Variant name mismatch", save );
		}

		bindings.LoadChildren( save.Bindings, ctx, static (data, ctx) => {
			if ( !ctx.DeserializeBindingData<ChildSaveData>( data, out var childData ) )
				return null;

			return childData.Type switch {
				BindingType.Buttons => ButtonBinding.Load( data, ctx ),
				BindingType.Joystick => JoystickBindings.Load( data, ctx ),
				BindingType.Gestures => GestureBindings.Load( data, ctx ),
				_ => null
			};
		} );
		return bindings;
	} );

	[FormatVersion( "" )]
	public struct ChildSaveData {
		public BindingType Type;

		public static implicit operator ChildSaveData ( V1ChildSaveData from ) => new() {
			Type = from.Type.EndsWith( "Buttons" ) ? BindingType.Buttons 
				: from.Type.EndsWith( "Joystick" ) ? BindingType.Joystick
				: from.Type == "Clap" ? BindingType.Gestures
				: throw new InvalidDataException()
		};
	}

	[FormatVersion( "[Initial]" )]
	public struct V1ChildSaveData {
		public string Type;
	}

	[FormatVersion( "" )]
	[FormatVersion( "[Initial]" )]
	public struct SaveData {
		public string Name;
		public Dictionary<int, string>? Actions;
		public object[] Bindings;
	}
}

public enum BindingType {
	Buttons,
	Joystick,
	Clap,
	Gestures
}

public interface IHasBindingType : IActionBinding {
	BindingType Type { get; }
}