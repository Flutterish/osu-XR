using osu.XR.Input.Actions;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class ButtonEditor : FillFlowContainer {
	public ButtonEditor ( ButtonBinding source ) {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		var handler = source.CreateHandler();
		Children = new Drawable[] {
			handler,
			new ButtonSetup( true, source.Primary, handler.Primary ),
			new ButtonSetup( false, source.Secondary, handler.Secondary )
		};
	}

	private partial class ButtonSetup : CompositeDrawable {
		public ActivationIndicator Indicator;
		public RulesetActionDropdown ActionDropdown;
		public ButtonSetup ( bool isPrimary, Bindable<object?> action, Bindable<bool> current ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

			AddInternal( ActionDropdown = new RulesetActionDropdown {
				LabelText = isPrimary ? ButtonsStrings.Primary : ButtonsStrings.Secondary,
			} );
			AddInternal( Indicator = new ActivationIndicator {
				Margin = new MarginPadding { Right = 16 },
				Origin = Anchor.TopRight,
				Anchor = Anchor.TopRight,
				Current = current
			} );
			ActionDropdown.RulesetAction.BindTo( action );
		}
	}
}
