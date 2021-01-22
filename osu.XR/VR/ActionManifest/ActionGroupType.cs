using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.VR.ActionManifest {
	public enum ActionGroupType {
		/// <summary>
		/// These actions can be bound on each controller separately
		/// </summary>
		LeftRight,
		/// <summary>
		/// These actions can be bound only on one controller
		/// </summary>
		Single,
		/// <summary>
		/// These actions are hidden
		/// </summary>
		Hidden
	}
}
