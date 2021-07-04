using osu.Game.Graphics.UserInterface;
using osu.XR.Drawables.Containers;
using osu.XR.Settings.Sections;
using System;

namespace osu.XR.Drawables {
	public class VRConfigDrawable : ConfigurationContainer {
		public VRConfigDrawable () : base() {
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
