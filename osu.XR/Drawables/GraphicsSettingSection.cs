using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.XR.Components.Groups;
using osu.XR.Settings;

namespace osu.XR.Drawables {
	public class GraphicsSettingSection : FillFlowContainer, IHasName, IHasIcon {
        public GraphicsSettingSection () {
            Direction = FillDirection.Vertical;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
        }

        public string DisplayName => "Graphics";

        public Drawable CreateIcon () => new SpriteIcon {
            Icon = FontAwesome.Solid.Laptop
        };

        [BackgroundDependencyLoader]
        private void load ( XrConfigManager config ) {
            Children = new Drawable[] {
                new SettingsSlider<float,RadToDegreeSliderBar> { Current = config.GetBindable<float>( XrConfigSetting.ScreenArc ), LabelText = "Screen arc" },
                new SettingsSlider<float,MetersSliderBar> { Current = config.GetBindable<float>( XrConfigSetting.ScreenRadius ), LabelText = "Screen radius" },
                new SettingsSlider<float,MetersSliderBar> { Current = config.GetBindable<float>( XrConfigSetting.ScreenHeight ), LabelText = "Screen height" },

                new SettingsSlider<int,PxSliderBar> { Current = config.GetBindable<int>( XrConfigSetting.ScreenResolutionX ), LabelText = "Screen resolution X" },
                new SettingsSlider<int,PxSliderBar> { Current = config.GetBindable<int>( XrConfigSetting.ScreenResolutionY ), LabelText = "Screen resolution Y" },

                new SettingsCheckbox { Current = config.GetBindable<bool>( XrConfigSetting.RenderToScreen ), LabelText = "Render to screen" },
            };
        }
    }
}
