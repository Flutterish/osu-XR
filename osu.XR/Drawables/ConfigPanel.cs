using NuGet.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class ConfigPanel : SettingsPanel {
        public string Title => "VR Settings";
        public string Description => "change the way osu!XR behaves";
		public ConfigPanel ( bool showSidebar ) : base( showSidebar ) {

		}

		protected override IEnumerable<SettingsSection> CreateSections () => new SettingsSection[] {
            new InputSettingSection(),
            new GraphicsSettingSection()
        };

        protected override Drawable CreateHeader () => new SettingsHeader( Title, Description );
        protected override Drawable CreateFooter () => new SettingsFooter();
    }

    public class SettingsCheckboxWithTooltip : SettingsCheckbox, IHasTooltip {
        public string TooltipText { get; set; }
    }

    public class SettingsSliderWithTooltip<T,Tslider> : SettingsSlider<T,Tslider>, IHasTooltip 
        where T : struct, IEquatable<T>, IComparable<T>, IConvertible
        where Tslider : OsuSliderBar<T>, new() {
        public string TooltipText { get; set; }
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
