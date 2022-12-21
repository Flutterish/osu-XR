using osu.Framework.Localisation;
using osu.XR.Input.Handlers;
using osu.XR.Input.Migration;
using osu.XR.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace osu.XR.Input;

/// <summary>
/// An action performed by the player which triggers an input
/// </summary>
public abstract class ActionBinding : IActionBinding {
	public abstract LocalisableString Name { get; }
	public abstract bool ShouldBeSaved { get; }

	public abstract Drawable? CreateEditor ();
	public virtual ActionBindingHandler? CreateHandler () => null;

	public abstract object CreateSaveData ( BindingsSaveContext context );

	protected static T? Load<T, Tdata> ( JsonElement data, BindingsSaveContext ctx, Func<Tdata, BindingsSaveContext, T> factory, JsonSerializerOptions? options = null ) {
		if ( !data.DeserializeBindingData<Tdata>( ctx, out var save, options ) )
			return default;
		return factory( save, ctx );
	}

	protected void TrackSetting<T> ( IBindable<T> bindable, [CallerArgumentExpression(nameof(bindable))] string? member = null ) {
		bindable.BindValueChanged( _ => OnSettingsChanged() );
	}
	protected void OnSettingsChanged () {
		SettingsChanged?.Invoke();
	}

	/// <summary>
	/// Invoked when settings of this, or nested settings are changed, thus invalidating the save file
	/// </summary>
	public event Action? SettingsChanged;
}

public interface IActionBinding {
	LocalisableString Name { get; }
	bool ShouldBeSaved { get; }

	Drawable? CreateEditor ();
	ActionBindingHandler? CreateHandler ();

	object CreateSaveData ( BindingsSaveContext context );

	/// <summary>
	/// Invoked when settings of this, or nested settings are changed, thus invalidating the save file
	/// </summary>
	event Action? SettingsChanged;
}