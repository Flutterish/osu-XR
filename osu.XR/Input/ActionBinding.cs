using osu.Framework.Localisation;
using System.Runtime.CompilerServices;

namespace osu.XR.Input;

/// <summary>
/// An action performed by the player which triggers an input
/// </summary>
public abstract class ActionBinding {
	public abstract LocalisableString Name { get; }
	public abstract bool ShouldBeSaved { get; }

	public abstract Drawable CreateEditor ();

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