using osu.Framework.Input.Bindings;
using osu.Framework.XR.Graphics.Panels;
using osu.XR.Input.Handlers;
using osu.XR.Osu;
using System.Reflection;

namespace osu.XR.Input;

[Cached]
public partial class InjectedInput : CompositeDrawable {
	public readonly PlayerInfo PlayerInfo;
	public InjectedInput ( PlayerInfo info, VariantBindings bindings ) {
		RelativeSizeAxes = Axes.Both;
		PlayerInfo = info;
		AddInternal( bindings.CreateHandler() );

		var addHandler = info.InputManager.GetType().GetMethod( "AddHandler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )!;

		addHandler.Invoke( info.InputManager, new object[] { mouseHandler } );
		addHandler.Invoke( info.InputManager, new object[] { touchHandler } );
	}

	VirtualMouseHandler mouseHandler = new();
	VirtualTouchHandler touchHandler = new();
	Dictionary<object, InjectedInputCursor> customCursors = new();
	public InjectedInputCursor GetCursor ( object source ) {
		if ( !customCursors.TryGetValue( source, out var cursor ) ) {
			customCursors.Add( source, cursor = new( this ) );
			AddInternal( cursor );
		}

		PlayerInfo.InputManager.UseParentInput = false;
		foreach ( var i in customCursors.Values ) {
			i.Alpha = customCursors.Count == 1 ? 0 : 1;
		}
		return cursor;
	}

	public void DeleteCursor ( object source ) {
		if ( !customCursors.Remove( source, out var cursor ) )
			return;

		foreach ( var i in customCursors.Values ) {
			i.Alpha = customCursors.Count == 1 ? 0 : 1;
		}

		RemoveInternal( cursor, disposeImmediately: true );
		touchHandler.EmulateTouchUp( cursor );
		if ( customCursors.Count == 1 ) {
			touchHandler.EmulateTouchUp( customCursors.Values.First() );
		}
		if ( customCursors.Count == 0 ) {
			PlayerInfo.InputManager.UseParentInput = true;
		}
	}

	public void MoveTo ( InjectedInputCursor cursor, Vector2 position ) {
		if ( customCursors.Count == 1 ) {
			mouseHandler.EmulateMouseMove( position );
		}
		else {
			touchHandler.EmulateTouchMove( cursor, position );
		}
	}

	/// <summary>
	/// Handlers which modify how other handlers work register themselves there.
	/// </summary>
	[Cached]
	BindableList<HandlerMod> handlerMods = new();

	MethodInfo? press;
	public void TriggerPressed ( object action ) {
		press ??= typeof( KeyBindingContainer<> ).MakeGenericType( PlayerInfo.RulesetActionType ).GetMethod( nameof( KeyBindingContainer<int>.TriggerPressed ) )!;
		press.Invoke( PlayerInfo.KeyBindingContainer, new object[] { action } );
	}

	MethodInfo? release;
	public void TriggerReleased ( object action ) {
		release ??= typeof( KeyBindingContainer<> ).MakeGenericType( PlayerInfo.RulesetActionType ).GetMethod( nameof( KeyBindingContainer<int>.TriggerReleased ) )!;
		release.Invoke( PlayerInfo.KeyBindingContainer, new object[] { action } );
	}
}
