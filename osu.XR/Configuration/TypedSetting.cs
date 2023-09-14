namespace osu.XR.Configuration;

public class TypedSetting<TValue> : ITypedSetting {
	public readonly Bindable<TValue> Bindable;

	public TypedSetting ( Bindable<TValue> bindable ) {
		Bindable = bindable;
	}

	IBindable ITypedSetting.Bindable => Bindable;
	public object? GetValue () => Bindable.Value;
	public void Parse ( object? value ) => Bindable.Parse( value );

	public void RevertToDefault () => Bindable.SetDefault();
	public void SaveDefault () => Bindable.Default = Bindable.Value;

	public void CopyTo ( ITypedSetting other ) => ((TypedSetting<TValue>)other).Bindable.Value = Bindable.Value;
	public void CopyTo<TLookup> ( ITypedSettingSource<TLookup> source, TLookup key ) where TLookup : struct, Enum {
		if ( source.TypedSettings.TryGetValue( key, out var other ) )
			CopyTo( other );
		else
			source.AddTypedSetting( key, Bindable.Value );
	}
}

public interface ITypedSetting {
	IBindable Bindable { get; }
	object? GetValue ();
	void Parse ( object? value );

	void RevertToDefault ();
	void SaveDefault ();

	void CopyTo ( ITypedSetting other );
	void CopyTo<TLookup> ( ITypedSettingSource<TLookup> source, TLookup key ) where TLookup : struct, Enum;
}

public interface ITypedSettingSource<TLookup> where TLookup : struct, Enum {
	IReadOnlyDictionary<TLookup, ITypedSetting> TypedSettings { get; }
	void AddTypedSetting<TValue> ( TLookup key, TValue value );
	void RemoveTypedSetting ( TLookup key );
	public void SetTypedSetting<TValue> ( TLookup key, TValue value ) {
		if ( TypedSettings.TryGetValue( key, out var setting ) )
			((TypedSetting<TValue>)setting).Bindable.Value = value;
		else
			AddTypedSetting( key, value );
	}
}