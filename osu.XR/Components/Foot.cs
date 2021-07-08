using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.Framework.XR.Maths;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class Foot : Model {
		public readonly Bindable<Vector3> TargetPosition = new Bindable<Vector3>();
		Vector3 goalPosition;

		public Foot () {
			TargetPosition.BindValueChanged( v => {
				if ( ( goalPosition - v.NewValue ).Length > 0.22f ) {
					this.MoveTo( v.NewValue, 200, Easing.Out );
					this.RotateTo( ( v.NewValue - goalPosition ).Normalized().LookRotation(), 100 );

					goalPosition = v.NewValue;
				}
			}, true );
		}
	}
}
