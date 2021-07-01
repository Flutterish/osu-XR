using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;

namespace osu.XR.Drawables {
	public class ConfigPanel : ConfigurationContainer {
		public ConfigPanel () : base() {
            Title = "VR Settings";
            Description = "change the way osu!XR behaves";

            AddSection( new InputSettingSection() );
            AddSection( new GraphicsSettingSection() );
            AddSection( new PresetsSection() );
        }
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
