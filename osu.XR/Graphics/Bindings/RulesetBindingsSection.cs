using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.XR.IO;
using osu.XR.Localisation.Config;
using osu.XR.Osu;

namespace osu.XR.Graphics.Bindings;

public partial class RulesetBindingsSection : FillFlowContainer {
	protected readonly Bindable<BindingsFile> Bindings = new();

	SettingsHeader? header;
	public RulesetBindingsSection () {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		setHeader( null );
		ruleset.ValueChanged += v => {
			onRulesetChanged( getRuleset( v.NewValue ) );
		};

		Bindings.BindValueChanged( v => {
			if ( selectedVariant != null )
				Remove( selectedVariant, disposeImmediately: false );
			selectedVariant = null;

			foreach ( var i in variantSections.Values ) {
				i.Dispose();
			}
			variantSections.Clear();
			onRulesetChanged( getRuleset( ruleset.Value ) );
		} );
	}

	void setHeader ( Ruleset? ruleset ) {
		if ( header != null ) {
			Remove( header, disposeImmediately: true );
			header = null;
		}

		Insert( -1, header = new SettingsHeader( 
			BindingsStrings.Header, 
			ruleset is null ? BindingsStrings.FlavourNone : BindingsStrings.Flavour( ruleset.RulesetInfo.Name )
		) );
	}

	Bindable<RulesetInfo?> ruleset = new();

	[BackgroundDependencyLoader]
	private void load ( OsuDependencies osu, Bindable<BindingsFile> bindings ) {
		ruleset.BindTo( osu.Ruleset );
		Bindings.BindTo( bindings );
	}

	static Ruleset? getRuleset ( RulesetInfo? info ) {
		Ruleset ruleset;
		try {
			if ( info is null || ( ruleset = info.CreateInstance() ) is null ) {
				return null;
			}
		}
		catch {
			return null;
		}
		return ruleset;
	}

	VariantBindingsSection? selectedVariant;
	SettingsDropdown<LocalisableString>? settings;
	void onRulesetChanged ( Ruleset? ruleset ) {
		if ( settings != null ) {
			Remove( settings, disposeImmediately: true );
			settings = null;
		}
		if ( selectedVariant != null ) {
			Remove( selectedVariant, disposeImmediately: false );
			selectedVariant = null;
		}

		setHeader( ruleset );
		if ( ruleset is null )
			return;

		var variants = ruleset.AvailableVariants;
		if ( variants.Skip( 1 ).Any() ) {
			Add( settings = new SettingsDropdown<LocalisableString> {
				ShowsDefaultIndicator = false,
				LabelText = BindingsStrings.Variant,
				Items = variants.Select( x => ruleset.GetVariantName( x ) ),
				Current = new Bindable<LocalisableString>( ruleset.GetVariantName( variants.FirstOrDefault() ) )
			} );

			settings.Current.BindValueChanged( v => setVariant( ruleset, variants.FirstOrDefault( x => ruleset.GetVariantName(x) == v.NewValue ) ) );
		}

		setVariant( ruleset, variants.FirstOrDefault() );
	}

	Dictionary<(string, int), VariantBindingsSection> variantSections = new();
	void setVariant ( Ruleset ruleset, int variant ) {
		if ( selectedVariant != null )
			Remove( selectedVariant, disposeImmediately: false );

		if ( !variantSections.TryGetValue( (ruleset.ShortName, variant), out var section ) ) {
			var bindings = Bindings.Value;
			if ( !bindings.TryGetChild( ruleset.ShortName, out var rulesetBindings ) ) {
				bindings.Add( rulesetBindings = new( ruleset.ShortName ) );
			}
			rulesetBindings.Ruleset = ruleset;
			if ( !rulesetBindings.TryGetChild( variant, out var variantBindings ) ) {
				rulesetBindings.Add( variantBindings = new( variant ) );
			}

			variantSections[(ruleset.ShortName, variant)] = section = new( ruleset, variant ) { Bindings = variantBindings };
		}

		Add( selectedVariant = section );
	}
}
