namespace osu.XR.Input.Migration;

[AttributeUsage( AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true )]
public class MigrateFromAttribute : Attribute {
	public readonly Type Format;
	public readonly string VersionName;

	public MigrateFromAttribute ( Type format, string? versionName ) {
		Format = format;
		VersionName = versionName ?? string.Empty;
	}
}
