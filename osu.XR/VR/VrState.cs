using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.VR {
	[Flags]
	public enum VrState {
		NotInitialized = 1,
		OK = 2,
		HeadsetNotDetected = 4,
		UnknownError = 8
	}
}
