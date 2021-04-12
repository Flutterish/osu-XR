using osu.Framework.Platform;
using osu.Game.Configuration;
using System;
using System.ComponentModel;
using System.IO;

namespace osu.XR.Settings {
	public class XrConfigManager : InMemoryConfigManager<XrConfigSetting> {
		Storage storage;
		public XrConfigManager ( Storage storage ) {
			this.storage = storage.GetStorageForDirectory( "XR" );

			Load();
		}

		protected override void InitialiseDefaults () {
			base.InitialiseDefaults();
			SetDefault( XrConfigSetting.InputMode, InputMode.SinglePointer );
			SetDefault( XrConfigSetting.SinglePointerTouch, false );
			SetDefault( XrConfigSetting.TapOnPress, false );
			SetDefault( XrConfigSetting.Deadzone, 20, 0, 100 );

			SetDefault( XrConfigSetting.ScreenArc, MathF.PI * 1.2f, MathF.PI / 18, MathF.PI * 2 );
			SetDefault( XrConfigSetting.ScreenRadius, 1.6f, 0.4f, 4 );
			SetDefault( XrConfigSetting.ScreenHeight, 1.8f, 0f, 3 );

			SetDefault( XrConfigSetting.ScreenResolutionX, 1920 * 2, 500, 7680 );
			SetDefault( XrConfigSetting.ScreenResolutionY, 1080, 400, 4320 );

			SetDefault( XrConfigSetting.RenderToScreen, false );
		}

		public static readonly SettingsPreset<XrConfigSetting> DefaultPreset = new() {
			values = new() {
				[XrConfigSetting.InputMode]				= InputMode.SinglePointer,
				[XrConfigSetting.SinglePointerTouch]	= false,
				[XrConfigSetting.TapOnPress]			= false,
				[XrConfigSetting.Deadzone]				= 20,
				[XrConfigSetting.ScreenArc]				= MathF.PI * 1.2f,
				[XrConfigSetting.ScreenRadius]			= 1.6f,
				[XrConfigSetting.ScreenHeight]			= 1.8f,
				[XrConfigSetting.ScreenResolutionX]		= 1920 * 2,
				[XrConfigSetting.ScreenResolutionY]		= 1080
			}
		};

		public static readonly SettingsPreset<XrConfigSetting> PresetTouchscreenBig = new() {
			values = new() {
				[XrConfigSetting.InputMode]				= InputMode.TouchScreen,
				[XrConfigSetting.SinglePointerTouch]	= false,
				[XrConfigSetting.TapOnPress]			= false,
				[XrConfigSetting.Deadzone]				= 20,
				[XrConfigSetting.ScreenArc]				= 1.2f,
				[XrConfigSetting.ScreenRadius]			= 1.01f,
				[XrConfigSetting.ScreenHeight]			= 1.47f,
				[XrConfigSetting.ScreenResolutionX]		= 3840,
				[XrConfigSetting.ScreenResolutionY]		= 2552
			}
		};

		public static readonly SettingsPreset<XrConfigSetting> PresetTouchscreenSmall = new() {
			values = new() {
				[XrConfigSetting.InputMode]				= InputMode.TouchScreen,
				[XrConfigSetting.SinglePointerTouch]	= false,
				[XrConfigSetting.TapOnPress]			= false,
				[XrConfigSetting.Deadzone]				= 20,
				[XrConfigSetting.ScreenArc]				= 1.2f,
				[XrConfigSetting.ScreenRadius]			= 0.69f /*nice*/,
				[XrConfigSetting.ScreenHeight]			= 1.58f,
				[XrConfigSetting.ScreenResolutionX]		= 3840,
				[XrConfigSetting.ScreenResolutionY]		= 2552
			}
		};

		const string saveFilePath = "XrSettings.json";
		protected override void PerformLoad () {
			try {
				if ( storage.Exists( saveFilePath ) ) {
					using var s = storage.GetStream( saveFilePath );
					var reader = new StreamReader( s );
					Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsPreset<XrConfigSetting>>( reader.ReadToEnd() ).Load( this, DefaultPreset );
				}
			}
			catch { }
		}

		protected override bool PerformSave () {
			var preset = new SettingsPreset<XrConfigSetting>( this, DefaultPreset );
			using var s = storage.GetStream( saveFilePath, FileAccess.Write, mode: FileMode.Create );
			var writer = new StreamWriter( s );
			writer.Write( Newtonsoft.Json.JsonConvert.SerializeObject( preset, Newtonsoft.Json.Formatting.Indented ) );
			writer.Flush();
			return true;
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

		RenderToScreen
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
