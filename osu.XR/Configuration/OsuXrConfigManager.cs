using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.XR.Configuration.Presets;
using osu.XR.IO;

namespace osu.XR.Configuration;

public class OsuXrConfigManager : InMemoryConfigManager<OsuXrSetting>, IConfigManagerWithCurrents<OsuXrSetting> {
	Storage Storage;
	public ConfigManagerCurrents<OsuXrSetting> Currents { get; } = new();
	public OsuXrConfigManager ( Storage storage ) {
		Storage = storage;
	}
	
	protected override void InitialiseDefaults () {
		SetDefault( OsuXrSetting.InputMode, InputMode.SinglePointer );
		SetDefault( OsuXrSetting.TouchPointers, false );
		SetDefault( OsuXrSetting.TapStrum, false );
		SetDefault( OsuXrSetting.Deadzone, 20, 0, 100 );

		SetDefault( OsuXrSetting.ScreenArc, MathF.PI * 0.7f, MathF.PI / 18, MathF.PI * 2 );
		SetDefault( OsuXrSetting.ScreenRadius, 1.6f, 0.4f, 4 );
		SetDefault( OsuXrSetting.ScreenHeight, 1.8f, 0f, 3 );

		SetDefault( OsuXrSetting.ScreenResolutionX, 1920, 500, 7680 );
		SetDefault( OsuXrSetting.ScreenResolutionY, 1080, 400, 4320 );

		SetDefault( OsuXrSetting.DisableTeleport, false );
		SetDefault( OsuXrSetting.HandSkeletonMotionRange, MotionRange.WithController );
		SetDefault( OsuXrSetting.HandSkeletonFingers, Fingers.Index );
		SetDefault( OsuXrSetting.ShadowType, FeetSymbols.None );

		SetDefault( OsuXrSetting.CameraMode, CameraMode.Disabled );
		SetDefault( OsuXrSetting.ShowInputDisplay, false );
		base.InitialiseDefaults();
	}

	protected override void PerformLoad () {
		if ( Storage.Exists( "XrSettings.json" ) ) {
			this.ApplyPreset( load( Storage, "XrSettings.json" ) );
		}

		if ( !Storage.ExistsDirectory( "Presets" ) ) {
			Presets.Add( new( Currents, DefaultPreset ) );
			Presets.Add( new( Currents, PresetTouchscreenBig ) );
			Presets.Add( new( Currents, PresetTouchscreenSmall ) );
			return;
		}

		var presetStorage = Storage.GetStorageForDirectory( "Presets" );
		foreach ( var i in presetStorage.GetFiles( "." ) ) {
			if ( i.EndsWith( "~" ) )
				continue;

			Presets.Add( load( presetStorage, i ) );
		}
	}

	ConfigPreset<OsuXrSetting> load ( Storage storage, string file ) {
		if ( storage.ReadWithBackup( file ) is Stream stream ) {
			using ( stream ) {
				return Currents.DeserializePreset( stream );
			}
		}

		return new();
	}

	protected override bool PerformSave () {
		write( Storage, "XrSettings.json", createFullSavePreset() );

		var presetStorage = Storage.GetStorageForDirectory( "Presets" );

		HashSet<string> saved = new();

		int j = 1;
		foreach ( var i in Presets ) {
			var name = $"Preset_{j.ToString().PadLeft(4, '0')}_{i.Name}.json";
			foreach ( var c in Path.GetInvalidFileNameChars() ) {
				name = name.Replace( c, '~' );
			}

			saved.Add( name );
			saved.Add( name + "~" );
			write( presetStorage, name, i );
			j++;
		}

		foreach ( var i in presetStorage.GetFiles( "." ) ) {
			if ( saved.Contains( i ) )
				continue;

			presetStorage.Delete( i );
		}

		return true;
	}

	void write ( Storage storage, string path, ConfigPreset<OsuXrSetting> preset ) {
		using var stream = storage.WriteWithBackup( path );
		preset.Serialize( stream );
	}

	public readonly BindableList<ConfigPreset<OsuXrSetting>> Presets = new();

	ConfigPreset<OsuXrSetting> createFullSavePreset () {
		return Currents.CreatePreset();
	}

	public ConfigPreset<OsuXrSetting> CreateFullPreset () {
		return Currents.CreatePreset( ConfigStore.Keys.Except( new[] { OsuXrSetting.CameraMode, OsuXrSetting.ShowInputDisplay } ) );
	}

	public static readonly ConfigurationPresetLiteral<OsuXrSetting> DefaultPreset = new() {
		Name = @"Default",
		[OsuXrSetting.InputMode] = InputMode.SinglePointer,
		[OsuXrSetting.TouchPointers] = false,
		[OsuXrSetting.ScreenArc] = MathF.PI * 0.7f,
		[OsuXrSetting.ScreenRadius] = 1.6f,
		[OsuXrSetting.ScreenHeight] = 1.8f,
		[OsuXrSetting.ScreenResolutionX] = 1920,
		[OsuXrSetting.ScreenResolutionY] = 1080
	};

	public static readonly ConfigurationPresetLiteral<OsuXrSetting> PresetTouchscreenBig = new() {
		Name = @"Touchscreen Big",
		[OsuXrSetting.InputMode] = InputMode.TouchScreen,
		[OsuXrSetting.ScreenArc] = 1.2f,
		[OsuXrSetting.ScreenRadius] = 1.01f,
		[OsuXrSetting.ScreenHeight] = 1.47f,
		[OsuXrSetting.ScreenResolutionX] = 3840 / 2,
		[OsuXrSetting.ScreenResolutionY] = 2552 / 2
	};

	public static readonly ConfigurationPresetLiteral<OsuXrSetting> PresetTouchscreenSmall = new() {
		Name = @"Touchscreen Small",
		[OsuXrSetting.InputMode] = InputMode.TouchScreen,
		[OsuXrSetting.ScreenArc] = 1.2f,
		[OsuXrSetting.ScreenRadius] = 0.69f,
		[OsuXrSetting.ScreenHeight] = 1.58f,
		[OsuXrSetting.ScreenResolutionX] = 3840 / 2,
		[OsuXrSetting.ScreenResolutionY] = 2552 / 2
	};

	protected override void AddBindable<TBindable> ( OsuXrSetting lookup, Bindable<TBindable> backing ) {
		Currents.AddBindable( lookup, backing );
		ConfigStore[lookup] = backing;
	}
}
