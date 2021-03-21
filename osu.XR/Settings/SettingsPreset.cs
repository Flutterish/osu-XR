using osu.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace osu.XR.Settings {
	public class SettingsPreset<T> where T : struct, Enum {
		public Dictionary<T, dynamic> values = new();

		static Ti CastTo<Ti> ( dynamic o ) {
			return (Ti)o;
		}
		static dynamic CastToReflected ( dynamic o, Type type ) {
			if ( type.IsEnum ) o = CastToReflected( o, type.GetEnumUnderlyingType() );

			return typeof( SettingsPreset<T> ).GetMethod( nameof( CastTo ), BindingFlags.NonPublic | BindingFlags.Static ).MakeGenericMethod( type ).Invoke( null, new object[] { o } );
		}

		public void Load ( ConfigManager<T> config, SettingsPreset<T> typeLookup ) {
			foreach ( var (k,v) in values ) {
				( (dynamic)config ).SetValue( k, CastToReflected( v, typeLookup.values[ k ].GetType() ) );
			}
		}

		public SettingsPreset () { }
		public SettingsPreset ( ConfigManager<T> config, SettingsPreset<T> typeLookup ) {
			foreach ( var (k,v) in typeLookup.values ) {
				values.Add( k, typeof( ConfigManager<T> ).GetMethod( nameof( ConfigManager<T>.Get ) ).MakeGenericMethod( v.GetType() as Type ).Invoke( config, new object[] { k } ) );
			}
		}
	}
}
