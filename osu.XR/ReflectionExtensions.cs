using System.Linq;
using System.Reflection;

namespace osu.XR {
	public static class ReflectionExtensions {
		public static T GetField<T> ( this object self )
			=> (T)self.GetType().GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).First( x => x.FieldType.IsAssignableTo( typeof( T ) ) ).GetValue( self );

		public static T GetField<T> ( this object self, string name )
			=> (T)self.GetType().GetFields( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).First( x => x.Name == name && x.FieldType.IsAssignableTo( typeof( T ) ) ).GetValue( self );

		public static T GetProperty<T> ( this object self )
			=> (T)self.GetType().GetProperties( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).First( x => x.PropertyType.IsAssignableTo( typeof( T ) ) ).GetValue( self );

		public static T GetProperty<T> ( this object self, string name )
			=> (T)self.GetType().GetProperties( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).First( x => x.Name == name && x.PropertyType.IsAssignableTo( typeof( T ) ) ).GetValue( self );

		public static MethodInfo GetMethod ( this object self, string name )
			=> self.GetType().GetMethod( name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
	}
}
