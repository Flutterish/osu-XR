using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Editor {
	public interface IGripable {
		bool CanBeGripped { get; }
		bool AllowsGripMovement { get; }
		bool AllowsGripScaling { get; }
		bool AllowsGripRotation { get; }
		void OnGripped ( object source, GripGroup group );
		void OnGripReleased ( object source, GripGroup group );
	}
}
