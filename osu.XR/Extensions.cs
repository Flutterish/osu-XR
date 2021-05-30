using osu.Framework.XR.Components;
using osu.XR.Inspector;
#nullable enable

namespace osu.XR {
	public static class Extensions {
		/// <summary>
		/// Gets the first <see cref="IInspectable"/> <see cref="Drawable3D"/> traveling upwards.
		/// </summary>
		public static IInspectable? GetClosestInspectable ( this Drawable3D self ) {
			if ( self is IInspectable inspectable ) return inspectable;
			else if ( self.Parent is not null ) return self.Parent.GetClosestInspectable();
			else return null;
		}

		public static string GetInspectorName ( this Drawable3D drawable ) {
			return string.IsNullOrWhiteSpace( drawable.Name ) ? drawable.GetType().Name : drawable.Name;
		}

		/// <summary>
		/// Gets the first non <see cref="ISelfNotInspectable"/> <see cref="Drawable3D"/> traveling upwards.
		/// </summary>
		public static Drawable3D? GetValidInspectable ( this Drawable3D self ) {
			Drawable3D? inspectable = self;
			while ( inspectable is not null and ISelfNotInspectable ) {
				inspectable = inspectable.Parent;
			}
			return inspectable;
		}
	}
}
