namespace osu.XR.Graphics.Settings;

public class SettingPresetContainer<T> where T : struct, Enum {
	public readonly BindableBool IsEditingBindable = new( false );
	HashSet<ISettingPresetComponent<T>> selectedComponents = new();

	public void Add ( ISettingPresetComponent<T> component )
		=> selectedComponents.Add( component );

	public bool IsComponentSelected ( ISettingPresetComponent<T> component )
		=> selectedComponents.Contains( component );
}
