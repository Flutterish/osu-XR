using osu.Framework.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Settings;

public abstract partial class SettingsUnitTextBox : SettingsItem<double> {
	public SettingsUnitTextBox () {
		ShowsDefaultIndicator = false;
	}

	protected abstract bool TryParse ( string text, out double value );
	protected abstract string Format ( double value );

	BindableNumberWithCurrent<double> current = new();
	new public BindableNumber<double> Current {
		get => current;
		set {
			current.Current = value;
		}
	}

	protected override Drawable CreateControl () {
		var textBox = new OutlinedUnitTextBox {
			RelativeSizeAxes = Axes.X,
			CommitOnFocusLost = true
		};

		textBox.OnCommit += onTextBoxCommit;
		return textBox;
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		Current.BindValueChanged( v => {
			base.Current.Value = v.NewValue;
			((OutlinedUnitTextBox)Control).Text = Format( v.NewValue );
		}, true );
	}

	void onTextBoxCommit ( TextBox _sender, bool newText ) {
		var sender = (OutlinedUnitTextBox)_sender;

		if ( string.IsNullOrWhiteSpace( sender.Text ) ) {
			Current.Value = 0;
			Current.TriggerChange();
		}
		else if ( TryParse( sender.Text, out var value ) ) {
			Current.Value = value;
			Current.TriggerChange();
		}
		else {
			Current.TriggerChange();
			sender.NotifyError();
		}
	}

	public partial class OutlinedUnitTextBox : OutlinedTextBox, IHasCurrentValue<double> {
		BindableWithCurrent<double> current = new();
		Bindable<double> IHasCurrentValue<double>.Current {
			get => current;
			set => current.Current = value;
		}

		public void NotifyError () {
			NotifyInputError();
		}
	}
}

public partial class SettingsPercentageTextBox : SettingsUnitTextBox {
	protected override bool TryParse ( string text, out double value ) {
		if ( double.TryParse( text.TrimEnd( '%' ), out value ) ) {
			value /= 100;
			return true;
		}
		return false;
	}

	protected override string Format ( double value ) {
		return $"{value:P0}";
	}
}

public partial class SettingsAngleTextBox : SettingsUnitTextBox {
	protected override bool TryParse ( string text, out double value ) {
		return double.TryParse( text.TrimEnd( '°' ), out value );
	}

	protected override string Format ( double value ) {
		return $"{value:N1}°";
	}
}
