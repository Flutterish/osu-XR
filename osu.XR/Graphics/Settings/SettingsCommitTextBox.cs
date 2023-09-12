using osu.Framework.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Settings;

public partial class SettingsCommitTextBox : SettingsItem<string> {
	public SettingsCommitTextBox () {
		ShowsDefaultIndicator = false;
	}

	BindableWithCurrent<string> current = new();
	new public Bindable<string> Current {
		get => current;
		set {
			current.Current = value;
		}
	}

	protected override Drawable CreateControl () {
		var textBox = new OutlinedTextBox {
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
			((OutlinedTextBox)Control).Text = v.NewValue;
		}, true );
	}

	void onTextBoxCommit ( TextBox _sender, bool newText ) {
		Current.Value = _sender.Text;
		Current.TriggerChange();
	}
}
