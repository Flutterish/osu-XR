using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;

namespace osu.XR.Drawables {
	public class ConfigPanel : SettingsPanel {
        public string Title => "VR Settings";
        public string Description => "change the way osu!XR behaves";
		public ConfigPanel ( bool showSidebar ) : base( showSidebar ) {

		}

		protected override IEnumerable<SettingsSection> CreateSections () => new SettingsSection[] {
            new InputSettingSection(),
            new GraphicsSettingSection(),
            new PresetsSection()
        };

        protected override Drawable CreateHeader () => new SettingsHeader( Title, Description );
        protected override Drawable CreateFooter () => new SettingsFooter();
    }

    public class PxSliderBar : OsuSliderBar<int> {
        public override string TooltipText => $"{Current.Value}px";
    }

    public class RadToDegreeSliderBar : OsuSliderBar<float> {
        public override string TooltipText => $"{Current.Value / Math.PI * 180:N0}°";
    }

    public class MetersSliderBar : OsuSliderBar<float> {
        public override string TooltipText => $"{Current.Value:N2}m";
    }
}
