using osu.Framework.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Settings {
	public class SettingsPreset<T> where T : struct, Enum {
		public Dictionary<T, dynamic> values = new();

		public void Load ( ConfigManager<T> config ) {
			foreach ( var (k,v) in values ) {
				( (dynamic)config ).Set( k, v );
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
