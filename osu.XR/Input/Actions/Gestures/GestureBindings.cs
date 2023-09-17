using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Migration;
using osu.XR.IO;
using System.Text.Json;

namespace osu.XR.Input.Actions.Gestures;

public class GestureBindings : UniqueCompositeActionBinding<IHasGestureType, GestureType>, IHasBindingType {
	public override LocalisableString Name => @"Gestures";
	public BindingType Type => BindingType.Gestures;

	public override Drawable? CreateEditor () => new FullCompositeEditor<IHasGestureType, GestureType>( this, new IHasGestureType[] {
		new ClapBinding(),
		new WindmillBinding()
	} );

	protected override GestureType GetKey ( IHasGestureType action ) {
		return action.Type;
	}

	protected override object CreateSaveData ( IEnumerable<IHasGestureType> children, BindingsSaveContext context ) => new SaveData {
		Type = BindingType.Gestures,
		Data = children.Select( x => new ChildSaveData { Type = x.Type, Data = x.GetSaveData( context ) } as object ).ToArray()
	};

	public static GestureBindings? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<GestureBindings, SaveData>( data, ctx, static ( save, ctx ) => {
		var gestures = new GestureBindings();
		gestures.LoadChildren<ChildSaveData>( save.Data, ctx, static ( save, ctx ) => save.Type switch {
			GestureType.Clap => ClapBinding.Load( (JsonElement)save.Data, ctx ),
			GestureType.Windmill => WindmillBinding.Load( (JsonElement)save.Data, ctx ),
			_ => null
		} );
		return gestures;
	} );

	[FormatVersion( "" )]
	public struct ChildSaveData {
		public GestureType Type;
		public object Data;
	}

	[FormatVersion( "Gestures" )]
	public struct SaveData {
		public BindingType Type;
		public object[] Data;

		public static implicit operator SaveData ( ClapBinding.NoSubmenuSaveData data ) => new() { // TODO we need context for proper migration
			Type = BindingType.Gestures,
			Data = new object[] { JsonSerializer.SerializeToElement( new ChildSaveData { Type = GestureType.Clap, Data = data }, BindingsSaveContext.DefaultOptions ) }
		};
	}
}
