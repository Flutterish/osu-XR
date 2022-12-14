using osu.Framework.Input.Bindings;
using osu.XR.Osu;
using System.Reflection;

namespace osu.XR.Input;

[Cached]
public partial class InjectedInput : CompositeDrawable {
	PlayerInfo info;
	public InjectedInput ( PlayerInfo info, VariantBindings bindings ) {
		this.info = info;
		AddInternal( bindings.CreateHandler() );
	}

	MethodInfo? press;
	public void TriggerPressed ( object action ) {
		press ??= typeof( KeyBindingContainer<> ).MakeGenericType( info.RulesetActionType ).GetMethod( nameof( KeyBindingContainer<int>.TriggerPressed ) )!;
		press.Invoke( info.KeyBindingContainer, new object[] { action } );
	}

	MethodInfo? release;
	public void TriggerReleased ( object action ) {
		release ??= typeof( KeyBindingContainer<> ).MakeGenericType( info.RulesetActionType ).GetMethod( nameof( KeyBindingContainer<int>.TriggerReleased ) )!;
		release.Invoke( info.KeyBindingContainer, new object[] { action } );
	}
}
