﻿using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.XR.Drawables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

			Set( XrConfigSetting.ScreenArc, MathF.PI * 1.2f, MathF.PI / 18, MathF.PI * 2 );
			Set( XrConfigSetting.ScreenRadius, 1.6f, 0.4f, 4 );
			Set( XrConfigSetting.ScreenHeight, 1.8f, 0f, 3 );

			Set( XrConfigSetting.ScreenResolutionX, 1920 * 2, 500, 7680 );
			Set( XrConfigSetting.ScreenResolutionY, 1080, 400, 4320 );
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
		Deadzone,

		ScreenArc,
		ScreenRadius,
		ScreenHeight,

		ScreenResolutionX,
		ScreenResolutionY,
	}

	public enum InputMode {
		[Description( "Single Pointer" )]
		SinglePointer,
		[Description( "Two Pointers" )]
		DoublePointer,
		[Description( "Touchscreen" )]
		TouchScreen
	}
}