using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Input.Actions;
using osu.XR.IO;

namespace osu.XR.Input;

public class VariantBindings : UniqueCompositeActionBinding<IHasBindingType, (BindingType, Hand?)> {
	public override LocalisableString Name => $"Variant {Variant}";
	protected override (BindingType, Hand?) GetKey ( IHasBindingType action ) => (action.Type, action is IIsHanded handed ? handed.Hand : null);

	public readonly int Variant;
	public VariantBindings ( int variant ) {
		Variant = variant;
	}

	protected override object CreateSaveData ( IEnumerable<IHasBindingType> children, BindingsSaveContext context ) => new SaveData {
		Actions = context.ActionsChecksum( Variant ),
		Bindings = children.Select( x => new ChildSaveData { Type = x.Type, Data = x.CreateSaveData( context ) } ).ToArray()
	};

	public struct SaveData {
		public Dictionary<int, string>? Actions;
		public ChildSaveData[] Bindings;
	}

	public struct ChildSaveData {
		public BindingType Type;
		public object Data;
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