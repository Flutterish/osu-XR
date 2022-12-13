using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.XR.Localisation;

namespace osu.XR.Graphics.Bindings.Editors;
public partial class RulesetActionDropdown : SettingsDropdown<LocalisableString> {
	[Resolved(name: "RulesetActions")]
	protected List<object> RulesetActions { get; private set; } = null!;

	public readonly Bindable<object?> RulesetAction = new();
	bool bindlock;
	LocalisableString none = BindingsStrings.ActionNone;

	public RulesetActionDropdown () {
		Current = new Bindable<LocalisableString>( none );

		RulesetAction.ValueChanged += v => {
			if ( bindlock ) return;
			bindlock = true;
			Current.Value = ( v.NewValue is null ) ? none : v.NewValue.GetLocalisableDescription();
			bindlock = false;
		};
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		Items = RulesetActions.Select( x => x.GetLocalisableDescription() ).Prepend( none );
		Current.BindValueChanged( v => {
			if ( bindlock ) return;
			bindlock = true;
			RulesetAction.Value = ( v.NewValue == none ) ? null : RulesetActions.FirstOrDefault( x => x.GetLocalisableDescription() == v.NewValue );
			bindlock = false;
		} );
	}
}
