using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Settings;

public partial class SettingsColourPicker : SettingsItem<Colour4> {
	protected override Drawable CreateControl () {
		return new Control();
	}

	new partial class Control : CompositeDrawable, IHasCurrentValue<Colour4> {
		ColourDisplay display;
		public Control () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			AddInternal( display = new MaxWidthColourDisplay {
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre
			} );
		}

		public Bindable<Colour4> Current {
			get => display.Current;
			set => display.Current = value;
		}

		partial class MaxWidthColourDisplay : ColourDisplay {
			[BackgroundDependencyLoader]
			private void load () {
				Width = 1;
				RelativeSizeAxes = Axes.X;

				var circle = (OsuClickableContainer)((FillFlowContainer)InternalChild).Children[0];
				var text = (OsuSpriteText)circle.Children[1];

				text.Font = text.Font.With( size: 20 );
			}
		}
	}
}
