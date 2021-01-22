using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.XR.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR {
	public abstract class XrGame : Framework.Game {
		public XrScene Scene { get; protected set; }
	}
}
