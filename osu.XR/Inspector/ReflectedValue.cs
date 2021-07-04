using osu.Framework.Extensions.TypeExtensions;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace osu.XR.Inspector {
	public class ReflectedValue<T> {
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
		public ReflectedValue ( object source, string name ) {
			DeclaredName = name;
			var type = source.GetType();
			if ( type.GetField( name, flags ) is FieldInfo field ) {
				if ( !field.FieldType.IsAssignableTo( typeof( T ) ) ) {
					throw new InvalidCastException( $"Field `{name}` of `{source}` is of type `{field.FieldType}`, but the expected type was `{typeof( T )}`" );
				}

				DeclaredType = field.FieldType;
				IsReadonly = isReadonly( field );
				Getter = createFieldGetter( field, source );
				Setter = createFieldSetter( field, source );
			}
			else if ( type.GetProperty( name, flags ) is PropertyInfo prop ) {
				if ( !prop.PropertyType.IsAssignableTo( typeof( T ) ) ) {
					throw new InvalidCastException( $"Property `{name}` of `{source}` is of type `{prop.PropertyType}`, but the expected type was `{typeof( T )}`" );
				}

				DeclaredType = prop.PropertyType;
				IsReadonly = isReadonly( prop );
				Getter = createPropertyGetter( prop.GetGetMethod( true ), source );
				Setter = createPropertySetter( prop.GetSetMethod( true ), source );
			}
			else throw new InvalidOperationException( $"No field or property named `{name}` exists in `{source}`" );
		}

		static readonly Type isExternalInitType = typeof( System.Runtime.CompilerServices.IsExternalInit );
		static bool isReadonly ( PropertyInfo prop )
			=> !prop.CanWrite || prop.SetMethod is null || prop.SetMethod.IsPrivate || prop.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Contains( isExternalInitType );
		static bool isReadonly ( FieldInfo field )
			=> field.IsInitOnly;

		public ReflectedValue ( object source, FieldInfo field ) {
			DeclaredName = field.Name;
			DeclaredType = field.FieldType;
			isReadonly( field );

			if ( !field.FieldType.IsAssignableTo( typeof( T ) ) ) {
				throw new InvalidCastException( $"Field `{field.Name}` of `{source}` is of type `{field.FieldType}`, but the expected type was `{typeof( T )}`" );
			}

			Getter = createFieldGetter( field, source );
			Setter = createFieldSetter( field, source );
		}

		public ReflectedValue ( object source, PropertyInfo prop ) {
			DeclaredName = prop.Name;
			DeclaredType = prop.PropertyType;
			IsReadonly = isReadonly( prop );

			if ( !prop.PropertyType.IsAssignableTo( typeof( T ) ) ) {
				throw new InvalidCastException( $"Property `{prop.Name}` of `{source}` is of type `{prop.PropertyType}`, but the expected type was `{typeof( T )}`" );
			}

			Getter = createPropertyGetter( prop.GetGetMethod( true ), source );
			Setter = createPropertySetter( prop.GetSetMethod( true ), source );
		}

		private static Func<T> createFieldGetter ( FieldInfo field, object source ) {
			/*if ( !RuntimeInfo.SupportsJIT )*/
			return () => (T)field.GetValue( source );

			string methodName = $"{field.FieldType.ReadableName()}.{field.Name}.get_{Guid.NewGuid():N}";
			DynamicMethod setterMethod = new DynamicMethod( methodName, field.FieldType, new Type[] { }, true );
			ILGenerator gen = setterMethod.GetILGenerator();
			gen.Emit( OpCodes.Ldarg_0 );
			gen.Emit( OpCodes.Ldfld, field );
			gen.Emit( OpCodes.Ret );

			if ( typeof( T ) == field.FieldType )
				return setterMethod.CreateDelegate<Func<T>>();
			else
				return setterMethod.CreateDelegate( typeof( Func<> ).MakeGenericType( field.FieldType ) ) as Func<T>;
		}

		private static Action<T> createFieldSetter ( FieldInfo field, object source ) {
			/*if ( !RuntimeInfo.SupportsJIT )*/
			return value => field.SetValue( source, value );

			string methodName = $"{field.FieldType.ReadableName()}.{field.Name}.set_{Guid.NewGuid():N}";
			DynamicMethod setterMethod = new DynamicMethod( methodName, null, new Type[] { field.FieldType }, true );
			ILGenerator gen = setterMethod.GetILGenerator();
			gen.Emit( OpCodes.Ldarg_0 );
			gen.Emit( OpCodes.Ldarg_1 );
			gen.Emit( OpCodes.Stfld, field );
			gen.Emit( OpCodes.Ret );

			if ( typeof( T ) == field.FieldType )
				return setterMethod.CreateDelegate<Action<T>>();
			else
				return setterMethod.CreateDelegate( typeof( Action<> ).MakeGenericType( field.FieldType ) ) as Action<T>;
		}

		private static Func<T> createPropertyGetter ( MethodInfo getter, object source ) {
			if ( getter is null ) return null;
			/*if ( !RuntimeInfo.SupportsJIT )*/
			return () => (T)getter.Invoke( source, Array.Empty<object>() );

			if ( typeof( T ) == getter.ReturnType )
				return getter.CreateDelegate<Func<T>>();
			else
				return getter.CreateDelegate( typeof( Func<> ).MakeGenericType( getter.ReturnType ) ) as Func<T>;
		}

		private static Action<T> createPropertySetter ( MethodInfo setter, object source ) {
			if ( setter is null ) return null;
			/*if ( !RuntimeInfo.SupportsJIT )*/
			return value => setter.Invoke( source, new object[] { value } );

			if ( typeof( T ) == setter.GetParameters()[ 0 ].ParameterType )
				return setter.CreateDelegate<Action<T>>();
			else
				return setter.CreateDelegate( typeof( Action<> ).MakeGenericType( setter.GetParameters()[ 0 ].ParameterType ) ) as Action<T>;
		}

		public readonly string DeclaredName;
		public readonly Type DeclaredType;
		public readonly Func<T> Getter;
		public readonly Action<T> Setter;
		public readonly bool IsReadonly;

		public T Value {
			get => Getter();
			set => Setter( value );
		}
	}
}
