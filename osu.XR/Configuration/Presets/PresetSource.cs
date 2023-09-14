namespace osu.XR.Configuration.Presets;

public class PresetSource<T> where T : IPreset<T> {
	public PresetSource ( PresetViewType viewType, LeftRight slideoutDirection ) {
		SlideoutDirection = slideoutDirection;
		ViewType = viewType;
	}

	public readonly PresetViewType ViewType;
	public readonly LeftRight SlideoutDirection;
	public readonly BindableBool IsSlideoutEnabled = new();

	public readonly BindableList<T> Presets = new();
	public readonly Bindable<T?> SelectedPreset = new();
}

public enum LeftRight {
	Left,
	Right
}

public enum PresetViewType {
	Preset,
	ItemList
}