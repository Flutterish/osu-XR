using OpenVR.NET.Manifest;

namespace osu.XR.VirtualReality;

public class OsuXrActionManifest : ActionManifest<VrActionCategory, VrAction> {
	public OsuXrActionManifest () {
		SupportsDominantHandSetting = true;
		ActionSets = new() {
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.General },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Poses },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Configuration },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Haptics },
			new() { Type = ActionSetType.LeftRight, Name = VrActionCategory.Skeleton }
		};
		Actions = new() {
			new() { Category = VrActionCategory.General, Name = VrAction.LeftButton, Type = ActionType.Boolean },
			new() { Category = VrActionCategory.General, Name = VrAction.RightButton, Type = ActionType.Boolean },
			new() { Category = VrActionCategory.General, Name = VrAction.Teleport, Type = ActionType.Boolean },
			new() { Category = VrActionCategory.General, Name = VrAction.Scroll, Type = ActionType.Vector2 },

			new() { Category = VrActionCategory.Configuration, Name = VrAction.ToggleMenu, Type = ActionType.Boolean },

			new() { Category = VrActionCategory.Haptics, Name = VrAction.Feedback, Type = ActionType.Vibration },

			new() { Category = VrActionCategory.Poses, Name = VrAction.ControllerTip, Type = ActionType.Pose },

			new() { Category = VrActionCategory.Skeleton, Name = VrAction.LeftHandSkeleton, Type = ActionType.LeftHandSkeleton },
			new() { Category = VrActionCategory.Skeleton, Name = VrAction.RightHandSkeleton, Type = ActionType.RightHandSkeleton }
		};
		DefaultBindings = new() {
			new() {
				ControllerType = "knuckles",
				Path = "DefaultBindings/knuckles.json"
			},
			new() {
				ControllerType = "vive_controller",
				Path = "DefaultBindings/vive_controller.json"
			},
			new() {
				ControllerType = "oculus_touch",
				Path = "DefaultBindings/oculus_touch.json"
			}
		};
	}
}

public enum VrActionCategory {
	General,
	Poses,
	Configuration,
	Haptics,
	Skeleton
}

public enum VrAction {
	LeftButton,
	RightButton,
	Scroll,
	Teleport,

	ControllerTip,

	ToggleMenu,

	Feedback,

	LeftHandSkeleton,
	RightHandSkeleton
}