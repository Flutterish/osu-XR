namespace osu.XR.Input.Actions.Gestures;

public interface IHasGestureType : IActionBinding {
	GestureType Type { get; }
}
