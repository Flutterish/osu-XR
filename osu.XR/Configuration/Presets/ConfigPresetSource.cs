using osu.Framework.XR;
using osu.XR.Bindables;

namespace osu.XR.Configuration.Presets;

public class ConfigPresetSource<TLookup> : PresetSource<ConfigPreset<TLookup>> where TLookup : struct, Enum {
	public ConfigPresetSource ( PresetViewType viewType, LeftRight slideoutDirection ) : base( viewType, slideoutDirection ) {
		SelectedPreset.BindValueChanged( v => {
			keys.Current = v.NewValue?.Keys;
			if ( v.NewValue == null )
				keys.Clear();
		}, true );

		keys.BindCollectionChanged( (_, e) => {
			if ( e.OldItems != null ) {
				foreach ( TLookup lookup in e.OldItems ) {
					GetIsInPresetBindable( lookup ).Value = false;
				}
			}

			if ( e.NewItems != null ) {
				foreach ( TLookup lookup in e.NewItems ) {
					GetIsInPresetBindable( lookup ).Value = true;
				}
			}
		}, true );
	}

	BindableListWithCurrent<TLookup> keys = new();

	public void Set<TValue> ( TLookup lookup, TValue value ) {
		SelectedPreset.Value?.SetTypedSetting( lookup, value );
	}

	public void Remove ( TLookup lookup ) {
		SelectedPreset.Value?.RemoveTypedSetting( lookup );
	}

	Dictionary<TLookup, BindableBool> isInPresetBindables = new();
	Dictionary<TLookup, BindableBool> isVisibleBindables = new();

	public BindableBool GetIsInPresetBindable ( TLookup lookup ) {
		if ( !isInPresetBindables.TryGetValue( lookup, out var bindable ) )
			isInPresetBindables.Add( lookup, bindable = new() );
		return bindable;
	}

	public BindableBool GetIsVisibleBindable ( TLookup lookup ) {
		if ( !isVisibleBindables.TryGetValue( lookup, out var bindable ) ) {
			bindable = new();
			if ( ViewType == PresetViewType.Preset ) {
				GetIsInPresetBindable( lookup ).BindValueChanged( v => {
					bindable.Value = v.NewValue;
				}, true );
			}
			else if ( ViewType == PresetViewType.ItemList ) {
				(IsSlideoutEnabled, GetIsInPresetBindable( lookup )).BindValuesChanged( (isSlideoutEnabled, isInPreset) => {
					bindable.Value = isSlideoutEnabled ? !isInPreset : true;
				}, true );
			}
			else {
				bindable.Value = false;
			}

			isVisibleBindables.Add( lookup, bindable );
		}
		return bindable;
	}
}
