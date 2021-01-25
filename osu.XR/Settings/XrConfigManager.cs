using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.XR.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Settings {
	public class XrConfigManager : InMemoryConfigManager<XrConfigSetting> {
		protected override void InitialiseDefaults () {
			base.InitialiseDefaults();
			Set( XrConfigSetting.InputMode, InputMode.SinglePointer );
			Set( XrConfigSetting.SinglePointerTouch, false );
			Set( XrConfigSetting.TapOnPress, false );
			Set( XrConfigSetting.Deadzone, 20, 0, 100 );
		}

		protected override void PerformLoad () {
			// TODO PerformLoad
		}

		protected override bool PerformSave () {
			return false; // TODO PerformSave
		}
	}

	public enum XrConfigSetting {
		InputMode,
		SinglePointerTouch,
		TapOnPress,
		Deadzone
	}
}
