using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.XR.Input;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using osuTK;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using static osu.Framework.XR.Input.VirtualInputManager;

namespace osu.XR.Input.Custom {
	public struct PlayerInfo {
		public Player Player;
		public DrawableRuleset DrawableRuleset;
		public PassThroughInputManager InputManager;
		public Type RulesetActionType;
		public KeyBindingContainer KeyBindingContainer;
		public Bindable<IReadOnlyList<Mod>> Mods;
		public int Variant;
	}

	public class InjectedInput : CompositeDrawable {
		Dictionary<CustomBinding, CustomBindingHandler> handlers = new();
		public readonly PlayerInfo Info;
		BindableList<CustomBinding> inputs;

		public InjectedInput ( BindableList<CustomBinding> inputs, PlayerInfo info ) {
			RelativeSizeAxes = Axes.Both;
			Info = info;
			this.inputs = inputs;

			inputs.BindCollectionChanged( inputsChanged, true );

			Info.InputManager.GetMethod( "AddHandler" ).Invoke( info.InputManager, new object[] { mouseHandler } );
		}

		private void inputsChanged ( object _, NotifyCollectionChangedEventArgs a ) {
			if ( a.Action == NotifyCollectionChangedAction.Add ) {
				if ( a.NewItems is null ) return;

				foreach ( CustomBinding i in a.NewItems ) {
					var handler = i.CreateHandler();
					handlers.Add( i, handler );
					AddInternal( handler );
				}
			}
			else {
				if ( a.OldItems is null ) return;

				foreach ( CustomBinding i in a.OldItems ) {
					handlers.Remove( i, out var handler );
					RemoveInternal( handler );
				}
			}
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			inputs.CollectionChanged -= inputsChanged;
		}

		public void TriggerPress ( object action, CustomBindingHandler source ) {
			if ( action is null ) return;

			Info.KeyBindingContainer.GetMethod( nameof( KeyBindingContainer<int>.TriggerPressed ) ).Invoke( Info.KeyBindingContainer, new object[] { action } );
		}

		public void TriggerRelease ( object action, CustomBindingHandler source ) {
			if ( action is null ) return;

			Info.KeyBindingContainer.GetMethod( nameof( KeyBindingContainer<int>.TriggerReleased ) ).Invoke( Info.KeyBindingContainer, new object[] { action } );
		}

		VirtualMouseHandler mouseHandler = new();
		public void MoveTo ( Vector2 position, bool isNormalized = false ) {
			var quad = Info.InputManager.ScreenSpaceDrawQuad;
			
			if ( Info.InputManager.UseParentInput ) {
				Info.InputManager.UseParentInput = false;

				mousePos = quad.Size / 2;
			}


			if ( isNormalized ) {
				var scale = Math.Min( quad.Width, quad.Height ) / 2;
				position *= scale;
			}

			mouseHandler.EmulateMouseMove( mousePos = position + quad.Size / 2 );
		}

		Vector2 mousePos;
		public void MoveBy ( Vector2 position, bool isNormalized = false ) {
			var quad = Info.InputManager.ScreenSpaceDrawQuad;

			if ( Info.InputManager.UseParentInput ) {
				Info.InputManager.UseParentInput = false;

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
}
