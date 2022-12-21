using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Input.Actions;
using osu.XR.Input.Migration;
using osu.XR.IO;
using System.Text.Json;

namespace osu.XR.Input;

public class VariantBindings : UniqueCompositeActionBinding<IHasBindingType, (BindingType, Hand?)> {
	public override LocalisableString Name => $"Variant {Variant}";
	protected override (BindingType, Hand?) GetKey ( IHasBindingType action ) => (action.Type, action is IIsHanded handed ? handed.Hand : null);

	public readonly int Variant;
	public VariantBindings ( int variant ) {
		Variant = variant;
	}

	protected override object CreateSaveData ( IEnumerable<IHasBindingType> children, BindingsSaveContext context ) => new SaveData {
		Name = context.VariantName( Variant ),
		Actions = context.ActionsChecksum( Variant ),
		Bindings = CreateSaveDataAsArray( children, context )
	};

	public static VariantBindings? Load ( JsonElement data, int variant, BindingsSaveContext context ) => Load<VariantBindings, SaveData>( data, context.SetVaraint( variant ), static (save, ctx) => {
		var variant = new VariantBindings( ctx.Variant );
		var checksum = ctx.ActionsChecksum( ctx.Variant );
		var declared = save.Actions;
		if ( declared is null ) {
			ctx.Warning( @"Variant does not have an action checksum", save );
		}
		else {
			if ( declared.Values.Except( checksum.Values ).Any() ) {
				ctx.Warning( @"Variant action checksum has non-existent actions", save );
			}
			else if ( declared.Except( checksum ).Any() ) {
				ctx.Warning( @"Variant action checksum is invalid", save );
			}
			// TODO try to fix this?
		}
		if ( ctx.VariantName( ctx.Variant ) != save.Name ) {
			ctx.Warning( @"Variant name mismatch", save );
		}
		variant.LoadChildren( save.Bindings, ctx, static (data, ctx) => {
			if ( !data.DeserializeBindingData<ChildSaveData>( ctx, out var childData ) )
				return null;

			return childData.Type switch {
				BindingType.Buttons => ButtonBinding.Load( data, ctx ),
				BindingType.Joystick => JoystickBindings.Load( data, ctx ),
				BindingType.Clap => ClapBinding.Load( data, ctx ),
				_ => null
			};
		} );
		return variant;
	} );

	[MigrateFrom(typeof(V1ChildSaveData), "[Initial]")]
	public struct ChildSaveData {
		public BindingType Type;

		public static implicit operator ChildSaveData ( V1ChildSaveData from ) => new() {
			Type = from.Type.EndsWith( "Buttons" ) ? BindingType.Buttons 
				: from.Type.EndsWith( "Joystick" ) ? BindingType.Joystick
				: BindingType.Clap
		};
	}

	public struct V1ChildSaveData {
		public string Type;
	}

	public struct SaveData {
		public string Name;
		public Dictionary<int, string>? Actions;
		public object[] Bindings;
	}
}

public enum BindingType {
	Buttons,
	Joystick,
	Clap
}

public interface IHasBindingType : IActionBinding {
	BindingType Type { get; }
}