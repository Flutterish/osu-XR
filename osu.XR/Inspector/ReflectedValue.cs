using osu.Framework;
using osu.Framework.Extensions.TypeExtensions;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace osu.XR.Inspector {
	public class ReflectedValue<T> {
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
		public ReflectedValue ( object source, string name ) {
			var type = source.GetType();
			if ( type.GetField( name, flags ) is FieldInfo field ) {
				if ( field.FieldType != typeof(T) ) {
					throw new InvalidCastException( $"Field `{name}` of `{source}` is of type `{field.FieldType}`, but the expected type was `{typeof(T)}`" );
				}

				Getter = createFieldGetter( field, source );
				Setter = createFieldSetter( field, source );
			}
			else if ( type.GetProperty( name, flags ) is PropertyInfo prop ) {
				if ( prop.PropertyType != typeof(T) ) {
					throw new InvalidCastException( $"Property `{name}` of `{source}` is of type `{prop.PropertyType}`, but the expected type was `{typeof(T)}`" );
				}

				Getter = createPropertyGetter( prop.GetGetMethod( true ), source );
				Setter = createPropertySetter( prop.GetSetMethod( true ), source );
			}
			else throw new InvalidOperationException( $"No field or property named `{name}` exists in `{source}`" );
		}

		private static Func<T> createFieldGetter ( FieldInfo field, object source ) {
			if ( !RuntimeInfo.SupportsJIT ) return () => (T)field.GetValue( source );

			string methodName = $"{typeof( T ).ReadableName()}.{field.Name}.get_{Guid.NewGuid():N}";
			DynamicMethod setterMethod = new DynamicMethod( methodName, typeof( T ), new Type[] {}, true );
			ILGenerator gen = setterMethod.GetILGenerator();
			gen.Emit( OpCodes.Ldarg_0 );
			gen.Emit( OpCodes.Ldfld, field );
			gen.Emit( OpCodes.Ret );
			return setterMethod.CreateDelegate<Func<T>>();
		}

		private static Action<T> createFieldSetter ( FieldInfo field, object source ) {
			if ( !RuntimeInfo.SupportsJIT ) return value => field.SetValue( source, value );

			string methodName = $"{typeof( T ).ReadableName()}.{field.Name}.set_{Guid.NewGuid():N}";
			DynamicMethod setterMethod = new DynamicMethod( methodName, null, new Type[] { typeof( T ) }, true );
			ILGenerator gen = setterMethod.GetILGenerator();
			gen.Emit( OpCodes.Ldarg_0 );
			gen.Emit( OpCodes.Ldarg_1 );
			gen.Emit( OpCodes.Stfld, field );
			gen.Emit( OpCodes.Ret );
			return setterMethod.CreateDelegate<Action<T>>();
		}

		private static Func<T> createPropertyGetter ( MethodInfo getter, object source ) {
			if ( !RuntimeInfo.SupportsJIT ) return () => (T)getter.Invoke( source, Array.Empty<object>() );

			return getter.CreateDelegate<Func<T>>();
		}

		private static Action<T> createPropertySetter ( MethodInfo setter, object source ) {
			if ( !RuntimeInfo.SupportsJIT ) return value => setter.Invoke( source, new object[] { value } );

			return setter.CreateDelegate<Action<T>>();
		}

		public readonly Func<T> Getter;
		public readonly Action<T> Setter;

		public T Value {
			get => Getter();
			set => Setter( value );
		}
	}
}
