namespace osu.XR.Configuration.Presets;

public interface IPreset<TSelf> where TSelf : IPreset<TSelf> {
	Bindable<string> Name { get; }

	TSelf Clone ();

	bool NeedsToBeSaved { get; set; }
}
