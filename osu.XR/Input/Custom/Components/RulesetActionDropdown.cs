using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom.Components {
	public class RulesetActionDropdown : SettingsDropdown<string> {
		[Resolved]
		protected List<object> rulesetActions { get; private set; }

		public readonly Bindable<object> RulesetAction = new();
		bool bindlock;

		public RulesetActionDropdown () {
			Current = new Bindable<string>( "None" );

			RulesetAction.ValueChanged += v => {
				if ( bindlock ) return;
				bindlock = true;
				Current.Value = ( v.NewValue is null ) ? Current.Default : v.NewValue.GetDescription();
				bindlock = false;
			};
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Items = rulesetActions.Select( x => x.GetDescription() ).Prepend( "None" );
			Current.BindValueChanged( v => {
				if ( bindlock ) return;
				bindlock = true;
				RulesetAction.Value = ( v.NewValue == Current.Default ) ? null : rulesetActions.FirstOrDefault( x => x.GetDescription() == v.NewValue );
				bindlock = false;
			} );
		}
	}
}
