using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.XR.Components;
using osu.XR.VR;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR {
	public abstract class XrGame : Framework.Game {
		public XrScene Scene { get; protected set; }
		public VrManager VrManager { get; private set; }

		protected override void LoadComplete () {
			// HACK we should not alter the scene graph this way, but i dont see another way yet
			base.LoadComplete();
			var parent = Parent as Container<Drawable>;
			parent.Remove( this );
			parent.Add( VrManager = new VrManager { RelativeSizeAxes = Framework.Graphics.Axes.Both, Child = this } );
		}
	}
}
