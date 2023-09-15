namespace osu.XR.Bindables;

public class BindableProxy<TValue> : BindableWithCurrent<TValue> {
	public readonly Bindable<TValue> Destination;
	public BindableProxy ( Bindable<TValue> destination ) {
		Destination = destination;

		ValueChanged += v => {
			if ( EqualityComparer<TValue>.Default.Equals( Value, Destination.Value ) )
				return;

			bool wasDisabled = Destination.Disabled;
			Destination.Disabled = false;
			Destination.Value = v.NewValue;
			Destination.Disabled = wasDisabled;

			UpdatedFromCurrent?.Invoke( v );
		};

		Destination.ValueChanged += v => {
			if ( EqualityComparer<TValue>.Default.Equals( Value, Destination.Value ) )
				return;

			bool wasDisabled = Disabled;
			Disabled = false;
			Value = v.NewValue;
			Disabled = wasDisabled;

			UpdatedFromDestination?.Invoke( v );
		};

		DefaultChanged += v => {
			bool wasDisabled = Destination.Disabled;
			Destination.Disabled = false;
			Destination.Default = v.NewValue;
			Destination.Disabled = wasDisabled;
		};

		Destination.DefaultChanged += v => {
			bool wasDisabled = Disabled;
			Disabled = false;
			Default = v.NewValue;
			Disabled = wasDisabled;
		};

		Value = Destination.Value;
		Default = Destination.Default;
	}

	public event Action<ValueChangedEvent<TValue>>? UpdatedFromDestination;
	public event Action<ValueChangedEvent<TValue>>? UpdatedFromCurrent;
}
