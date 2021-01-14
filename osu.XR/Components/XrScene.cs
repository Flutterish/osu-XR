using osu.Framework.Graphics;
using osu.XR.Projection;
using osu.XR.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Components {
	public class XrScene : Drawable {
		public readonly XrObject Root = new EmptyXrObject();
		public Camera Camera;

		public static implicit operator XrObject ( XrScene scene )
			=> scene.Root;
	}
}
