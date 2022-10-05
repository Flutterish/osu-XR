using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.XR.Graphics.Settings;

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
		SetDefault( OsuXrSetting.DominantHand, Hand.Auto );
		SetDefault( OsuXrSetting.ShadowType, FeetSymbols.None );
		SetDefault( OsuXrSetting.ShowDust, true );
		SetDefault( OsuXrSetting.SkyboxType, SkyBoxType.Solid );
		base.InitialiseDefaults();
	}

	public IReadOnlyDictionary<OsuXrSetting, Type> SettingTypes => settingTypes;
	Dictionary<OsuXrSetting, Type> settingTypes = new();
	Dictionary<OsuXrSetting, Func<object>> getters = new();

	public object Get ( OsuXrSetting lookup )
		=> getters[lookup]();

	protected override void AddBindable<TBindable> ( OsuXrSetting lookup, Bindable<TBindable> bindable ) {
		settingTypes.Add( lookup, typeof(TBindable) );
		getters.Add( lookup, () => bindable.Value );
		base.AddBindable( lookup, bindable );
	}

	public ConfigurationPreset<OsuXrSetting> CreateFullPreset () {
		var preset = new ConfigurationPreset<OsuXrSetting>();
		foreach ( var (key, type) in SettingTypes ) {
			preset.Add( key, (Get(key), type) );
		}

		return preset;
	}
}

public static class PresetExtensions {
	public static SettingPresetComponent<OsuXrSetting, Tvalue> PresetComponent<Tvalue> ( this IHasCurrentValue<Tvalue> self, OsuXrConfigManager config, OsuXrSetting lookup ) {
		self.Current = config.GetBindable<Tvalue>( lookup );
		return new( lookup, self );
	}
}