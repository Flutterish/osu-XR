using osu.XR.Configuration;

namespace osu.XR.Graphics.Settings;

public class SettingPresetContainer<Tlookup> where Tlookup : struct, Enum {
	/// <summary>
	/// Whether this preset can be edited
	/// </summary>
	public readonly BindableBool IsEditingBindable = new( false );
	/// <summary>
	/// Whether only the components which are part of this preset should be visible
	/// </summary>
	public readonly BindableBool IsPreviewBindable = new( false );

	public readonly Bindable<ConfigurationPreset<Tlookup>?> SelectedPresetBindable = new();
	public readonly BindableList<ConfigurationPreset<OsuXrSetting>> Presets = new();

	public void Set<Tvalue> ( ISettingPresetComponent<Tlookup> component, Tvalue value ) {
		if ( SelectedPresetBindable.Value is ConfigurationPreset<Tlookup> preset )
			preset[component.Lookup] = value;
	}

	public void Remove ( ISettingPresetComponent<Tlookup> component ) {
		if ( SelectedPresetBindable.Value is ConfigurationPreset<Tlookup> preset )
			preset.Remove( component.Lookup );
	}

	public bool IsInPreset ( ISettingPresetComponent<Tlookup> component )
		=> SelectedPresetBindable.Value is ConfigurationPreset<Tlookup> preset && preset.Keys.Contains( component.Lookup );

	public bool IsVisible ( ISettingPresetComponent<Tlookup> component )
		=> IsPreviewBindable.Value
		? IsInPreset( component )
		: IsEditingBindable.Value
		? !IsInPreset( component )
		: true;
}
