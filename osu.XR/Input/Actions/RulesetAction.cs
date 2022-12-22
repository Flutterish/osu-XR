using osu.XR.IO;

namespace osu.XR.Input.Actions;

public class RulesetAction : Bindable<object?> {
	public ActionData? NotLoaded;
	public override object? Value { 
		get => base.Value;
		set {
			base.Value = value;
			NotLoaded = null;
		}
	}

	public bool ShouldBeSaved => NotLoaded != null || Value != null;
}
