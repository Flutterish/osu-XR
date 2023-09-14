namespace osu.XR.Configuration.Presets;

public class ConfigurationPresetSource<TLookup> : PresetSource<ConfigurationPreset<TLookup>> where TLookup : struct, Enum {
	public ConfigurationPresetSource ( LeftRight slideoutDirection ) : base( slideoutDirection ) { }



	public void Set<TValue> ( TLookup lookup, TValue value ) {
		if ( SelectedPreset.Value is ConfigurationPreset<TLookup> preset )
			preset.SetTypedSetting( lookup, value );
	}

	public void Remove ( TLookup lookup ) {
		if ( SelectedPreset.Value is ConfigurationPreset<TLookup> preset )
			preset.RemoveTypedSetting( lookup );
	}

	public bool IsInPreset ( TLookup lookup )
		=> SelectedPreset.Value is ConfigurationPreset<TLookup> preset && preset.Keys.Contains( lookup );

	public bool IsVisible ( TLookup lookup )
		=> ShowOnlyPresetItems.Value
		? IsInPreset( lookup )
		: IsSlideoutEnabled.Value
		? !IsInPreset( lookup )
		: true;
}
