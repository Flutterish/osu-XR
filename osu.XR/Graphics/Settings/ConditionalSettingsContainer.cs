using osu.Framework.Graphics.UserInterface;

namespace osu.XR.Graphics.Settings;

public class ConditionalSettingsContainer<T> : FillFlowContainer, IHasCurrentValue<T> where T : notnull {
	BindableWithCurrent<T> current = new();
	public Bindable<T> Current {
		get => current;
		set => current.Current = value;
	}

	public ConditionalSettingsContainer () {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;

		current.BindValueChanged( _ => updateContents() );
	}

	void updateContents () {
		Clear( disposeChildren: false );
		if ( conditionalDrawables.TryGetValue( current.Value, out var drawables ) )
			AddRange( drawables );
	}

	Dictionary<T, Drawable[]> conditionalDrawables = new();
	public void Add ( T condition, Drawable[] drawables ) {
		conditionalDrawables.Add( condition, drawables );
		if ( current.Value.Equals( condition ) )
			updateContents();
	}

	public Drawable[] this[T condition] {
		set => Add( condition, value );
	}
}
