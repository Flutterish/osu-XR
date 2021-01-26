﻿using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.XR.Drawables;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class BeatingGroup : XrObject {
		public readonly Bindable<Beat> BindableBeat = new();

		public BeatingGroup () {
			BindableBeat.ValueChanged += v => {
				this.FinishTransforms();
				this.ScaleTo( new Vector3( (float)(1 - v.NewValue.AverageAmplitude * 0.04) ), 50 ).Then().ScaleTo( Vector3.One, 100 );
			};
		}

		[BackgroundDependencyLoader]
		private void load ( BeatProvider beat ) {
			BindableBeat.BindTo( beat.BindableBeat );
		}
	}
}
