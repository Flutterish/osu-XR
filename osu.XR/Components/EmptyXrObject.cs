using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Components {
	public class EmptyXrObject : XrObject {
		protected override XrObjectDrawNode CreateDrawNode () => null;
	}
}
