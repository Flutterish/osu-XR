using OpenVR.NET.Manifest;

namespace osu.XR.VirtualReality;

public class OsuXrActionManifest : ActionManifest<VrActionCategory, VrAction> {
	public OsuXrActionManifest () {
		ActionSets = new() {
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.General },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Poses },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Configuration },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Haptics }
		};
		Actions = new() {
			new() { Category = VrActionCategory.General, Name = VrAction.LeftButton, Type = ActionType.Boolean },
			new() { Category = VrActionCategory.General, Name = VrAction.RightButton, Type = ActionType.Boolean },
			new() { Category = VrActionCategory.General, Name = VrAction.Teleport, Type = ActionType.Boolean },
			new() { Category = VrActionCategory.General, Name = VrAction.Scoll, Type = ActionType.Vector2 },

			new() { Category = VrActionCategory.Configuration, Name = VrAction.ToggleMenu, Type = ActionType.Boolean },

			new() { Category = VrActionCategory.Haptics, Name = VrAction.Feedback, Type = ActionType.Vibration },

			new() { Category = VrActionCategory.Poses, Name = VrAction.ControllerTip, Type = ActionType.Pose }
		};
	}
}

public enum VrActionCategory {
	General,
	Poses,
	Configuration,
	Haptics
}

public enum VrAction {
	LeftButton,
	RightButton,
	Scoll,
	Teleport,

	ControllerTip,

	ToggleMenu,

	Feedback
}