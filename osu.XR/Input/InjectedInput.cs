using osu.Framework.Input.Bindings;
using osu.Framework.XR.Graphics.Panels;
using osu.XR.Input.Handlers;
using osu.XR.Osu;
using System.Reflection;

namespace osu.XR.Input;

[Cached]
public partial class InjectedInput : CompositeDrawable {
	PlayerInfo info;
	public InjectedInput ( PlayerInfo info, VariantBindings bindings ) {
		this.info = info;
		AddInternal( bindings.CreateHandler() );

		info.InputManager.GetType().GetMethod( "AddHandler", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )!.Invoke( info.InputManager, new object[] { mouseHandler } );
	}

	/// <summary>
	/// Handlers which modify how other handlers work register themselves there.
	/// </summary>
	[Cached]
	BindableList<HandlerMod> handlerMods = new();

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

	VirtualMouseHandler mouseHandler = new();
	public void MoveTo ( Vector2 position, bool isNormalized = false ) {
		var quad = info.InputManager.ScreenSpaceDrawQuad;

		if ( info.InputManager.UseParentInput ) {
			info.InputManager.UseParentInput = false;

			mousePos = quad.Size / 2;
		}


		if ( isNormalized ) {
			var scale = Math.Min( quad.Width, quad.Height ) / 2;
			position *= scale;
		}

		mouseHandler.EmulateMouseMove( mousePos = position + quad.Size / 2 );
	}

	Vector2 mousePos;
	public void MoveBy ( Vector2 position, bool isNormalized = false ) { // TODO allow multiple cursors
		var quad = info.InputManager.ScreenSpaceDrawQuad;

		if ( info.InputManager.UseParentInput ) {
			info.InputManager.UseParentInput = false;

			mousePos = quad.Size / 2;
		}

		if ( isNormalized ) {
			var scale = Math.Min( quad.Width, quad.Height ) / 2;
			position *= scale;
		}

		mousePos += position;
		mousePos = new Vector2(
			Math.Clamp( mousePos.X, 0, quad.Width ),
			Math.Clamp( mousePos.Y, 0, quad.Height )
		);
		mouseHandler.EmulateMouseMove( mousePos );
	}
}
