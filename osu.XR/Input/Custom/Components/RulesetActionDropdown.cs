using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

		public RulesetActionDropdown () {
			Current = new Bindable<string>( "None" );
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Items = rulesetActions.Select( x => x.ToString() ).Prepend( "None" );
		}
	}
}
