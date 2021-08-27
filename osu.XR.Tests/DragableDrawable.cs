﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests {
	public class DragableDrawable : CompositeDrawable {
		Drawable drawable;

		public DragableDrawable ( Drawable drawable ) {
			AutoSizeAxes = Axes.Both;
			AddInternal( this.drawable = drawable );
		}

		protected override void Update () {
			base.Update();

			Origin = drawable.Origin;
			Anchor = drawable.Anchor;
		}

		protected override bool OnDragStart ( DragStartEvent e ) {
			return true;
		}

		protected override void OnDrag ( DragEvent e ) {
			Position += e.Delta;
		}
	}
}
