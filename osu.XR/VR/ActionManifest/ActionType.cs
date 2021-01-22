using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.VR.ActionManifest {
	public enum ActionType {
		/// <summary>
		/// Boolean actions are used for triggers and buttons
		/// </summary>
		Boolean,
		/// <summary>
		/// Vecotr1 actions are read from triggers, 1D trackpads and joysticks 
		/// </summary>
		Vector1,
		/// <summary>
		/// Vector2 actions are read from trackpads and joysticks
		/// </summary>
		Vector2,
		Vector3,
		/// <summary>
		/// Vibration is used to activate haptics
		/// </summary>
		Vibration,
		Pose,
		Skeleton
	}
}
