using osu.Framework.XR.Components;
using osu.XR.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace osu.XR {
	public static class Extensions {
		public static IInspectable? GetClosestInspectable ( this Drawable3D self ) {
			if ( self is IInspectable inspectable ) return inspectable;
			else if ( self.Parent is not null ) return self.Parent.GetClosestInspectable();
			else return null;
		}

		public static string GetInspectorName ( this Drawable3D drawable ) {
			return string.IsNullOrWhiteSpace( drawable.Name ) ? drawable.GetType().Name : drawable.Name;
		}
	}
}
