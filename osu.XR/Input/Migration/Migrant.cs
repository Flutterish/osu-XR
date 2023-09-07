using System.Reflection;

namespace osu.XR.Input.Migration;

public class Migrant {
	Dictionary<(Type target, string? formatVersion), (Func<object, object> migrator, Type detectedType)?> migrators = new();
	public (Func<object, object> migrator, Type detectedType)? GetMigrator<T> ( string? formatVersion ) {
		var key = (typeof(T), formatVersion);
		if ( !migrators.TryGetValue( key, out var migrator ) ) {
			migrators.Add( key, migrator = CreateMigrator<T>( formatVersion ) );
		}

		return migrator;
	}

	public static (Func<object, object> migrator, Type detectedType)? CreateMigrator<T> ( string? formatVersion ) {
		var path = new Stack<Type>();
		if ( !populateMigrationPath( typeof( T ), path, formatVersion ?? string.Empty ) )
			return null;

		var type = path.Pop();
		var convertFrom = type;
		Func<object, object> migrator = x => x;
		while ( path.TryPop( out var convertTo ) ) {
			var oldMigrator = migrator;
			var nextMigrator = convertTo.GetMethod( "op_Implicit", BindingFlags.Static | BindingFlags.Public, new Type[] { convertFrom } )!;
			migrator = x => nextMigrator.Invoke( null, new object?[] { oldMigrator( x ) } )!;

			convertFrom = convertTo;
		}

		return (migrator, type);
	}

	static bool populateMigrationPath ( Type type, Stack<Type> path, string formatVersion ) {
		if ( path.Contains( type ) )
			return false; // loop

		var declaredVersions = type.GetCustomAttributes<FormatVersionAttribute>().Select( x => x.Name ?? string.Empty );
		if ( !declaredVersions.Any() )
			return false; // not a save format

		path.Push( type );
		if ( declaredVersions.Contains( formatVersion ) ) {
			return true;
		}

		var migrators = type.GetMethods( BindingFlags.Static | BindingFlags.Public ).Where( x => x.Name == "op_Implicit" && x.ReturnType == type );
		foreach ( var i in migrators ) {
			var argumentType = i.GetParameters()[0].ParameterType;
			if ( populateMigrationPath( argumentType, path, formatVersion ) ) {
				return true;
			}
		}
		path.Pop();
		return false;
	}
}