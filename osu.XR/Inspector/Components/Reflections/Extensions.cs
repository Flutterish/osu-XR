using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Reflections {
	public static class Extensions {

		static readonly Type[] simpleTypes = new Type[] {
			typeof(string),
			typeof(decimal),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(TimeSpan),
			typeof(Guid)
		};
		public static bool IsSimpleType ( this Type type ) {
			return type.IsPrimitive
				|| type.IsEnum
				|| type.IsValueType // these can be decomposed but its meh
				|| simpleTypes.Contains( type )
				|| ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> ) && type.GetGenericArguments()[ 0 ].IsSimpleType() );
		}

		static IEnumerable<ReflectedValue<object>> getDeclaredValues ( object obj, Type type ) {
			foreach ( var i in type.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) ) {
				if ( i.GetGetMethod() != null ) yield return new ReflectedValue<object>( obj, i );
			}

			foreach ( var i in type.GetFields( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) ) {
				yield return new ReflectedValue<object>( obj, i );
			}
		}

		public static IEnumerable<(Type type, IEnumerable<ReflectedValue<object>> values)> GetDeclaredSections ( this object obj ) {
			Type type = obj.GetType();

			while ( type != null && type != typeof( object ) ) {
				var declared = getDeclaredValues( obj, type );
				if ( declared.Any() ) yield return (type, declared);

				type = type.BaseType;
			}
		}
	}
}
