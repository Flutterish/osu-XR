using osu.Framework.Bindables;
using System;

namespace osu.XR.Input.Custom.Components {
	public class RulesetActionBinding {
		public readonly BindableBool IsActive = new();
		public readonly Bindable<object> RulesetAction = new();
		public RulesetActionBinding () {
			IsActive.BindValueChanged( v => {
				if ( RulesetAction.Value == null ) return;
				if ( v.NewValue )
					Press?.Invoke( RulesetAction.Value );
				else
					Release?.Invoke( RulesetAction.Value );
			} );
			RulesetAction.BindValueChanged( v => {
				if ( IsActive.Value ) {
					if ( v.OldValue != null )
						Release?.Invoke( v.OldValue );
					if ( v.NewValue != null )
						Press?.Invoke( v.NewValue );
				}
			} );
		}
		public event Action<object> Press;
		public event Action<object> Release;
	}
}