using osu.Framework.Configuration;
using osu.XR.Configuration.Presets;

namespace osu.XR.Configuration;

public class ConfigManagerCurrents<TLookup> : IReadOnlyTypedSettingSource<TLookup> where TLookup : struct, Enum {
	Dictionary<TLookup, IBindable> Backings = new();
	Dictionary<TLookup, IBindable> Currents = new();
	Dictionary<TLookup, IBindable> Active = new();

	Dictionary<TLookup, ITypedSetting> typedBackings = new();
	Dictionary<TLookup, ITypedSetting> typedActive = new();
	public IReadOnlyDictionary<TLookup, ITypedSetting> TypedSettings => typedActive;

	public void AddBindable<TValue> ( TLookup lookup, Bindable<TValue> bindable ) {
		var active = bindable;
		typedActive[lookup] = new TypedSetting<TValue>( active );
		Active[lookup] = active;

		var backing = active.GetBoundCopy();
		typedBackings[lookup] = new TypedSetting<TValue>( backing );
		Backings[lookup] = backing;
		Currents[lookup] = backing;
	}

	public ConfigPreset<TLookup> CreatePreset ( IEnumerable<TLookup>? settings = null ) {
		var preset = new ConfigPreset<TLookup>();
		foreach ( var i in settings ?? Backings.Keys ) {
			typedBackings[i].CopyTo( preset, i );
		}

		return preset;
	}

	public ConfigPreset<TLookup> DeserializePreset ( Stream stream ) {
		return ConfigPreset<TLookup>.Deserialize( stream, this );
	}

	public void ApplyPreset ( ConfigPreset<TLookup> preset ) {
		if ( preset == currentPreset )
			return;

		SetCurrentPreset( null );
		foreach ( var (lookup, setting) in preset.TypedSettings ) {
			setting.CopyTo( typedBackings[lookup] );
		}
	}

	ConfigPreset<TLookup>? currentPreset;
	public void SetCurrentPreset ( ConfigPreset<TLookup>? preset ) {
		if ( currentPreset == preset )
			return;
		
		if ( currentPreset != null ) {
			currentPreset.Keys.CollectionChanged -= onPresetKeysChanged;
			foreach ( var (lookup, setting) in currentPreset.TypedSettings ) {
				setCurrent( lookup, Backings[lookup] );
			}
		}
		
		currentPreset = preset;

		if ( currentPreset != null ) {
			currentPreset.Keys.BindCollectionChanged( onPresetKeysChanged, true );
		}
	}

	private void onPresetKeysChanged ( object? _, System.Collections.Specialized.NotifyCollectionChangedEventArgs e ) {
		if ( e.OldItems != null ) {
			foreach ( TLookup lookup in e.OldItems ) {
				setCurrent( lookup, Backings[lookup] );
			}
		}

		if ( e.NewItems != null ) {
			foreach ( TLookup lookup in e.NewItems ) {
				setCurrent( lookup, currentPreset!.TypedSettings[lookup].Bindable );
			}
		}
	}

	void setCurrent ( TLookup lookup, IBindable bindable ) {
		Active[lookup].UnbindFrom( Currents[lookup] );
		Currents[lookup] = bindable;
		Active[lookup].BindTo( bindable );
	}
}

public interface IConfigManager<TLookup> : IConfigManager where TLookup : struct, Enum {
	Bindable<TValue> GetBindable<TValue> ( TLookup lookup );
	void BindWith<TValue> ( TLookup lookup, Bindable<TValue> bindable );
}

public interface IConfigManagerWithCurrents<TLookup> : IConfigManager<TLookup> where TLookup : struct, Enum {
	ConfigManagerCurrents<TLookup> Currents { get; }
}

public static class IConfigManagerWithCurrentsExtensions {
	public static void SetCurrentPreset<TLookup> ( this IConfigManagerWithCurrents<TLookup> @this, ConfigPreset<TLookup>? preset ) where TLookup : struct, Enum {
		@this.Currents.SetCurrentPreset( preset );
	}

	public static void ApplyPreset<TLookup> ( this IConfigManagerWithCurrents<TLookup> @this, ConfigPreset<TLookup> preset ) where TLookup : struct, Enum {
		@this.Currents.ApplyPreset( preset );
	}
}