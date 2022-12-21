using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using osu.Framework.Extensions.TypeExtensions;
using osu.XR.IO;

namespace osu.XR.Input.Migration;

public static class JsonExtensions
{
    static JsonSerializerOptions defaultOptions = new() { IncludeFields = true };
    static JsonExtensions()
    {
        defaultOptions.Converters.Add(new JsonStringEnumConverter());
    }

    static Migrant GetMigrant ( Type type ) {
		if ( !migrants.TryGetValue( type, out var migrant ) )
			migrants.Add( type, migrant = new( type ) );
        return migrant;
	}
    static Dictionary<Type, Migrant> migrants = new(); 
    public static bool DeserializeBindingData<T>(this JsonElement json, BindingsSaveContext context, [NotNullWhen(true)] out T? value, JsonSerializerOptions? options = null)
    {
        if (typeof(T) == typeof(JsonElement))
        {
            value = (T)(object)json;
            return true;
        }

        if ( !GetMigrant( typeof(T) ).Migrate( json, context, out value, options ) )
            return false;

		foreach ( var i in typeof(T).GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ) ) {
			if ( !i.IsNullable() && i.GetValue( value ) == null ) {
				context.Error( $@"Invalid binding data - '{i.Name}' must be present", value );
				return false;
			}
		}
		foreach ( var i in typeof(T).GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance ).Where( x => x.CanRead && x.GetIndexParameters().Length == 0 ) ) {
			if ( !i.IsNullable() && i.GetValue( value ) == null ) {
				context.Error( $@"Invalid binding data - '{i.Name}' must be present", value );
				return false;
			}
		}
        return true;
    }

    class Migrant {
        public readonly Type MigratesTo;
        Dictionary<string, (Type format, Func<object, object> migration)> formatbyVersion = new();

		public Migrant ( Type migratesTo ) { // TODO 2+ deep migration
			MigratesTo = migratesTo;
            foreach ( var i in migratesTo.GetCustomAttributes<MigrateFromAttribute>() ) {
				var migration = migratesTo.GetMethod( "op_Implicit", BindingFlags.Static | BindingFlags.Public, new Type[] { i.Format } );
				if ( migration is null )
					throw new InvalidOperationException( $"{migratesTo.ReadableName()} declared that it can be migrated to from {i.Format.ReadableName()}, but no implicit conversion exists" );
                formatbyVersion.Add( i.VersionName, (i.Format, data => migration.Invoke( null, new object?[] { data } )! ) );
            }
		}

		static bool Deserialize<T> ( JsonElement json, BindingsSaveContext context, [NotNullWhen( true )] out T? value, JsonSerializerOptions? options = null ) {
			try {
				value = json.Deserialize<T>( options ?? defaultOptions );
				return value != null;
			}
			catch ( Exception e ) {
				context.Error( @"Could not load binding data", json, e );
				value = default;
				return false;
			}
		}

		static bool Deserialize ( Type type, JsonElement json, BindingsSaveContext context, [NotNullWhen( true )] out object? value, JsonSerializerOptions? options = null ) {
			try {
				value = json.Deserialize( type, options ?? defaultOptions );
				return value != null;
			}
			catch ( Exception e ) {
				context.Error( @"Could not load binding data", json, e );
				value = default;
				return false;
			}
		}

		public bool Migrate<T> ( JsonElement json, BindingsSaveContext context, [NotNullWhen(true)] out T? value, JsonSerializerOptions? options = null ) {
            if ( !formatbyVersion.Any() ) {
                return Deserialize( json, context, out value, options );
			}

            VersionContainer versionContainer;
            try {
                versionContainer = json.Deserialize<VersionContainer>( defaultOptions );
            }
            catch {
				context.Error( @"Could not load binding version", json );
				value = default;
				return false;
			}

            var version = versionContainer.FormatVersion;
            if ( version is null ) {
				return Deserialize( json, context, out value, options );
			}

            if ( !formatbyVersion.TryGetValue( version, out var formatData ) ) {
				context.Error( @"Unknown binding data version", json );
				value = default;
				return false;
			}

            var (format, migration) = formatData;
            if ( !Deserialize( format, json, context, out var data ) ) {
				value = default;
				return false;
			}

			value = (T)migration( data );
			return true;
		}

        struct VersionContainer {
			public string? FormatVersion;
        }
	}
}