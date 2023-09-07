using osu.Framework.XR.Graphics.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public interface IPointerBase {
	void AddToScene ( Scene scene );
	void RemoveFromScene ( Scene scene );
	void SetTint ( Colour4 tint );
}
