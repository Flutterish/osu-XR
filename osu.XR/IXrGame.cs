using osu.XR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR {
	public interface IXrGame {
		XrScene CreateScene ();
	}
}
