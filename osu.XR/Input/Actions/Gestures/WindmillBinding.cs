using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.Input.Migration;
using osu.XR.IO;
using System.Text.Json;

namespace osu.XR.Input.Actions.Gestures;

public class WindmillBinding : ActionBinding, IHasGestureType {
	public override LocalisableString Name => @"Windmill";
	public override bool ShouldBeSaved => IsLeftEnabled.Value || IsRightEnabled.Value;

	public readonly BindableBool IsLeftEnabled = new( false );
	public readonly BindableBool IsRightEnabled = new( false );
	public override Drawable? CreateEditor () => new WindmillEditor( this );
	public override ActionBindingHandler? CreateHandler () => new WindmillMod( this );

	protected override object CreateSaveData ( BindingsSaveContext context ) => new SaveData {
		IsLeftEnabled = IsLeftEnabled.Value,
		IsRightEnabled = IsRightEnabled.Value
	};

	public static WindmillBinding? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<WindmillBinding, SaveData>( data, ctx, static ( save, ctx ) => {
		var windmill = new WindmillBinding();
		windmill.IsLeftEnabled.Value = save.IsLeftEnabled;
		windmill.IsRightEnabled.Value = save.IsRightEnabled;
		return windmill;
	} );

	public GestureType Type => GestureType.Windmill;

	[FormatVersion( "" )]
	public struct SaveData {
		public bool IsLeftEnabled;
		public bool IsRightEnabled;
	}
}
