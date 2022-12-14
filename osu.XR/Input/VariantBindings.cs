using osu.Framework.Localisation;
using osu.Framework.XR.VirtualReality;
using osu.XR.Input.Actions;

namespace osu.XR.Input;

public class VariantBindings : UniqueCompositeActionBinding<(Type, Hand?)> {
	public override LocalisableString Name => $"Variant {Variant}";
	protected override (Type, Hand?) GetKey ( ActionBinding action ) => (action.GetType(), action is IIsHanded handed ? handed.Hand : null);

	public readonly int Variant;
	public VariantBindings ( int variant ) {
		Variant = variant;
	}
}
