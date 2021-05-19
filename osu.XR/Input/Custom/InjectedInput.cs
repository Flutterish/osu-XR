using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public struct PlayerInfo {
		public Player Player;
		public DrawableRuleset DrawableRuleset;
		public PassThroughInputManager InputManager;
		public Type RulesetActionType;
		public KeyBindingContainer KeyBindingContainer;
		public Bindable<IReadOnlyList<Mod>> Mods;
	}

	public class InjectedInput : CompositeDrawable {
		Dictionary<CustomBinding, CustomBindingHandler> handlers = new();
		public readonly PlayerInfo Info;
		BindableList<CustomBinding> inputs;

		public InjectedInput ( BindableList<CustomBinding> inputs, PlayerInfo info ) {
			Info = info;
			this.inputs = inputs;

			inputs.BindCollectionChanged( inputsChanged, true );
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
	}
}
