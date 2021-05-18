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
		Dictionary<CustomInput, CustomRulesetInputBindingHandler> handlers = new();
		public readonly PlayerInfo Info;
		BindableList<CustomInput> inputs;

		public InjectedInput ( BindableList<CustomInput> inputs, PlayerInfo info ) {
			Info = info;
			this.inputs = inputs;

			inputs.BindCollectionChanged( inputsChanged, true );
		}

		private void inputsChanged ( object _, NotifyCollectionChangedEventArgs a ) {
			if ( a.Action == NotifyCollectionChangedAction.Add ) {
				if ( a.NewItems is null ) return;

				foreach ( CustomInput i in a.NewItems ) {
					var handler = i.CreateHandler();
					handlers.Add( i, handler );
					handler.InjectedInput = this;
					AddInternal( handler );
				}
			}
			else {
				if ( a.OldItems is null ) return;

				foreach ( CustomInput i in a.OldItems ) {
					handlers.Remove( i, out var handler );
					handler.InjectedInput = null;
					RemoveInternal( handler );
				}
			}
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			inputs.CollectionChanged -= inputsChanged;
		}
	}
}
