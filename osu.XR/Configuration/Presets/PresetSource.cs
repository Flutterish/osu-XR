namespace osu.XR.Configuration.Presets;

public class PresetSource<T> where T : IPreset<T> {
	public PresetSource ( LeftRight slideoutDirection ) {
		SlideoutDirection = slideoutDirection;
	}

	public readonly BindableBool ShowOnlyPresetItems = new();
	public readonly BindableBool IsSlideoutEnabled = new();
	public readonly LeftRight SlideoutDirection;

	public readonly BindableList<T> Presets = new();
	public readonly Bindable<T?> SelectedPreset = new();
}

public enum LeftRight {
	Left,
	Right
}
