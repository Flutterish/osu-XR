using osu.XR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR {
	public abstract class XrGame : Framework.Game {
		public abstract XrScene CreateScene ();
	}
}
