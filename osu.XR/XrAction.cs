using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR {
	public enum XrActionGroup {
		Pointer,
		Configuration,
		Haptics
	}

	public enum XrAction {
		// Pointer
		MouseLeft,
		MouseRight,
		Scroll,

		// Configuration
		ToggleMenu,

		// Haptics
		Feedback
	}
}
