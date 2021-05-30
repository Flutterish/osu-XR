using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.XR.Inspector;
#nullable enable

namespace osu.XR {
	public static class Extensions {
		public static IInspectable? GetClosestInspectable ( this Drawable3D self )
			=> ( self.IsInspectable() && self is IInspectable inspectable ) ? inspectable : self.Parent?.GetClosestInspectable();

		public static string GetInspectorName ( this Drawable drawable )
			=> string.IsNullOrWhiteSpace( drawable.Name ) ? drawable.GetType().Name : drawable.Name;

		public static Drawable3D? GetValidInspectable ( this Drawable3D self )
			=> self.IsInspectable() ? self : self.Parent?.GetValidInspectable();

		public static bool DoParentsAllowInspection ( this Drawable3D self )
			=> self.Parent is null ? true : ( self.Parent is IChildrenNotInspectable ? false : self.Parent.DoParentsAllowInspection() );

		public static bool IsInspectable ( this Drawable3D self )
			=> self is not ISelfNotInspectable && self.DoParentsAllowInspection();
	}
}
