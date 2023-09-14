using osu.XR.Bindables;

namespace osu.XR.Configuration.Presets;

public class ConfigurationPresetSource<TLookup> : PresetSource<ConfigurationPreset<TLookup>> where TLookup : struct, Enum {
	public ConfigurationPresetSource ( PresetViewType viewType, LeftRight slideoutDirection ) : base( viewType, slideoutDirection ) {
		SelectedPreset.BindValueChanged( v => {
			keys.Current = v.NewValue?.Keys;
		}, true );
	}

	BindableListWithCurrent<TLookup> keys = new();
	public BindableList<TLookup> Keys => keys;

	public void Set<TValue> ( TLookup lookup, TValue value ) {
		SelectedPreset.Value?.SetTypedSetting( lookup, value );
	}

	public void Remove ( TLookup lookup ) {
		SelectedPreset.Value?.RemoveTypedSetting( lookup );
	}

	public bool IsInPreset ( TLookup lookup )
		=> SelectedPreset.Value?.Keys.Contains( lookup ) == true;

	public bool IsVisible ( TLookup lookup )
		=> ViewType == PresetViewType.Preset
			? IsInPreset( lookup )
		: ViewType == PresetViewType.ItemList
			? IsSlideoutEnabled.Value
			? !IsInPreset( lookup )
			: true
		: false;
}
