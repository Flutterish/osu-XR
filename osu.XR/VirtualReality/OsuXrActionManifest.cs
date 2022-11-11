using OpenVR.NET.Manifest;

namespace osu.XR.VirtualReality;

public class OsuXrActionManifest : ActionManifest<VrActionCategory, VrAction> {
	public OsuXrActionManifest () {
		ActionSets = new() {
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.General },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Poses },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Configuration }
		};
		Actions = new() {
			new() { Category = VrActionCategory.General, Name = VrAction.LeftButton, Type = ActionType.Boolean },
			new() { Category = VrActionCategory.General, Name = VrAction.RightButton, Type = ActionType.Boolean },

			new() { Category = VrActionCategory.Configuration, Name = VrAction.ToggleMenu, Type = ActionType.Boolean },

			new() { Category = VrActionCategory.Poses, Name = VrAction.ControllerTip, Type = ActionType.Pose }
		};
	}
}

public enum VrActionCategory {
	General,
	Poses,
	Configuration
}

public enum VrAction {
	LeftButton,
	RightButton,

	ControllerTip,

	ToggleMenu
}