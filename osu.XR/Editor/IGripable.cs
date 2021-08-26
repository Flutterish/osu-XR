using osu.Framework.Bindables;

namespace osu.XR.Editor {
	public interface IGripable {
		Bindable<bool> CanBeGripped { get; }
		Bindable<bool> AllowsGripMovement { get; }
		Bindable<bool> AllowsGripScaling { get; }
		Bindable<bool> AllowsGripRotation { get; }

		void OnGripped ( object source, GripGroup group );
		void OnGripReleased ( object source, GripGroup group );
	}
}
