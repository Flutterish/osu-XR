using osu.Framework.Localisation;
using System.Runtime.CompilerServices;

namespace osu.XR.Input;

/// <summary>
/// An action performed by the player which triggers an input
/// </summary>
public abstract class ActionBinding : IActionBinding {
	public abstract LocalisableString Name { get; }
	public abstract bool ShouldBeSaved { get; }

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

	/// <summary>
	/// Invoked when settings of this, or nested settings are changed, thus invalidating the save file
	/// </summary>
	event Action? SettingsChanged;
}

public interface IHasEditor {
	Drawable CreateEditor ();
}