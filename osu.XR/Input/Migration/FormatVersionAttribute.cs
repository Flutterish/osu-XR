namespace osu.XR.Input.Migration;

/// <summary>
/// Specifies a version name for save data. This is used for migration to newer formats. Reserves a "FormatVersion" field.<br/>
/// <see langword="public"/> <see langword="static"/> <see langword="implicit"/> <see langword="operator"/>s on the target type are used to convert between formats.
/// </summary>
[AttributeUsage( AttributeTargets.Struct, AllowMultiple = true )]
public class FormatVersionAttribute : Attribute {
	public string Name = string.Empty;

	public FormatVersionAttribute ( string name ) {
		Name = name;
	}
}
