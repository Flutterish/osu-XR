using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.XR.Graphics.Settings;
using System.Globalization;

namespace osu.XR.Configuration;

public class OsuXrConfigManager : InMemoryConfigManager<OsuXrSetting> {
	protected override void InitialiseDefaults () {
		SetDefault( OsuXrSetting.InputMode, InputMode.SinglePointer );
		SetDefault( OsuXrSetting.SinglePointerTouch, false );
		SetDefault( OsuXrSetting.TapOnPress, false );
		SetDefault( OsuXrSetting.Deadzone, 20, 0, 100 );

		SetDefault( OsuXrSetting.ScreenArc, MathF.PI * 0.7f, MathF.PI / 18, MathF.PI * 2 );
		SetDefault( OsuXrSetting.ScreenRadius, 1.6f, 0.4f, 4 );
		SetDefault( OsuXrSetting.ScreenHeight, 1.8f, 0f, 3 );

		SetDefault( OsuXrSetting.ScreenResolutionX, 1920, 500, 7680 );
		SetDefault( OsuXrSetting.ScreenResolutionY, 1080, 400, 4320 );

		SetDefault( OsuXrSetting.DisableTeleport, false );
		SetDefault( OsuXrSetting.DominantHand, HandSetting.Auto );
		SetDefault( OsuXrSetting.ShadowType, FeetSymbols.None );
		SetDefault( OsuXrSetting.ShowDust, true );
		SetDefault( OsuXrSetting.SkyboxType, SkyBoxType.Solid );
		base.InitialiseDefaults();

		PerformLoad();
	}

	protected override void PerformLoad () {
		base.PerformLoad();

		Presets.Add( DefaultPreset );
		Presets.Add( PresetTouchscreenBig );
		Presets.Add( PresetTouchscreenSmall );
	}

	Dictionary<OsuXrSetting, Func<object>> getters = new();
	Dictionary<OsuXrSetting, Action<string>> setters = new();

	protected override void AddBindable<TBindable> ( OsuXrSetting lookup, Bindable<TBindable> bindable ) {
		getters.Add( lookup, () => bindable.Value );
		setters.Add( lookup, s => bindable.Parse(s) );
		base.AddBindable( lookup, bindable );
	}

	public readonly BindableList<ConfigurationPreset<OsuXrSetting>> Presets = new();
	public ConfigurationPreset<OsuXrSetting>? AppliedPreset { get; private set; }
	ConfigurationPreset<OsuXrSetting>? resetPreview;
	public void ApplyPresetPreview ( ConfigurationPreset<OsuXrSetting>? preset ) {
		if ( AppliedPreset != null ) {
			AppliedPreset.SettingChanged -= onPreviewSettingChanged;
		}
		AppliedPreset = preset;
		if ( AppliedPreset is null ) {
			LoadPreset( resetPreview! );
			resetPreview = null;
		}
		else {
			resetPreview ??= CreateFullPreset();
			LoadPreset( AppliedPreset );
			AppliedPreset.SettingChanged += onPreviewSettingChanged;
		}
	}

	private void onPreviewSettingChanged ( OsuXrSetting lookup, object value ) {
		if ( setters.TryGetValue( lookup, out var set ) ) {
			set( value.ToString()! );
		}
	}

	public ConfigurationPreset<OsuXrSetting> CreateFullPreset () {
		var preset = new ConfigurationPreset<OsuXrSetting>();
		foreach ( var (key, get) in getters ) {
			preset[key] = get();
		}
		return preset;
	}

	public void LoadPreset ( ConfigurationPreset<OsuXrSetting> preset ) {
		foreach ( var (lookup, value) in preset.ConfigStore ) {
			if ( setters.TryGetValue( lookup, out var set ) ) {
				set( value.ToString(null, CultureInfo.InvariantCulture)! );
			}
		}
	}

	public readonly ConfigurationPreset<OsuXrSetting> DefaultPreset = new() {
		Name = "Default",
		[OsuXrSetting.InputMode] = InputMode.SinglePointer,
		[OsuXrSetting.SinglePointerTouch] = false,
		[OsuXrSetting.ScreenArc] = MathF.PI * 0.7f,
		[OsuXrSetting.ScreenRadius] = 1.6f,
		[OsuXrSetting.ScreenHeight] = 1.8f,
		[OsuXrSetting.ScreenResolutionX] = 1920,
		[OsuXrSetting.ScreenResolutionY] = 1080
	};

	public readonly ConfigurationPreset<OsuXrSetting> PresetTouchscreenBig = new() {
		Name = "Touchscreen Big",
		[OsuXrSetting.InputMode] = InputMode.TouchScreen,
		[OsuXrSetting.ScreenArc] = 1.2f,
		[OsuXrSetting.ScreenRadius] = 1.01f,
		[OsuXrSetting.ScreenHeight] = 1.47f,
		[OsuXrSetting.ScreenResolutionX] = 3840 / 2,
		[OsuXrSetting.ScreenResolutionY] = 2552 / 2
	};

	public readonly ConfigurationPreset<OsuXrSetting> PresetTouchscreenSmall = new() {
		Name = "Touchscreen Small",
		[OsuXrSetting.InputMode] = InputMode.TouchScreen,
		[OsuXrSetting.ScreenArc] = 1.2f,
		[OsuXrSetting.ScreenRadius] = 0.69f,
		[OsuXrSetting.ScreenHeight] = 1.58f,
		[OsuXrSetting.ScreenResolutionX] = 3840 / 2,
		[OsuXrSetting.ScreenResolutionY] = 2552 / 2
	};
}

public static class PresetExtensions {
	public static SettingPresetComponent<OsuXrSetting, Tvalue> PresetComponent<Tvalue> ( this IHasCurrentValue<Tvalue> self, OsuXrConfigManager config, OsuXrSetting lookup ) {
		return new( lookup, self, config );
	}
}