using osu.XR.Graphics.VirtualReality.Pointers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Graphics.VirtualReality;

public interface IControllerRelay {
	IEnumerable<PointerButton> GetButtonsFor ( VrAction action );
	void ScrollBy ( Vector2 amount );
}
