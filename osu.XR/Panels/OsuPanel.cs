using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables;
using osu.XR.Drawables.Containers;
using osu.XR.Inspector;
using osu.XR.Settings;
using System.Collections.Generic;

namespace osu.XR.Components.Panels {
	class OsuPanel : CurvedPanel, IConfigurableInspectable {
		public OsuPanel () {
			Y = 1.8f;
		}

		OsuGame osuGame;
		public void SetSource ( OsuGame osuGame ) {
			Source.Add( this.osuGame = osuGame );
			AutosizeBoth();
		}

		Bindable<float> screenHeightBindable = new( 1.8f );

		Bindable<int> screenResX = new( 1920 * 2 );
		Bindable<int> screenResY = new( 1080 );

		[Resolved]
		private XrConfigManager Config { get; set; }

		[BackgroundDependencyLoader]
		private void load () {
			Config.BindWith( XrConfigSetting.ScreenHeight, screenHeightBindable );
			screenHeightBindable.BindValueChanged( v => Y = v.NewValue, true );

			screenResX.BindValueChanged( v => osuGame.Width = v.NewValue, true );
			screenResY.BindValueChanged( v => osuGame.Height = v.NewValue, true );

			Config.BindWith( XrConfigSetting.ScreenRadius, RadiusBindable );
			Config.BindWith( XrConfigSetting.ScreenArc, ArcBindable );

			Config.BindWith( XrConfigSetting.ScreenResolutionX, screenResX );
			Config.BindWith( XrConfigSetting.ScreenResolutionY, screenResY );
		}

		public IEnumerable<Drawable> CreateInspectorSubsections () {
			yield return new NamedContainer {
				DisplayName = "Panel Properties",
				Children = new Drawable[] {
					new SettingsSlider<float,RadToDegreeSliderBar> { Current = Config.GetBindable<float>( XrConfigSetting.ScreenArc ), LabelText = "Screen arc" },
					new SettingsSlider<float,MetersSliderBar> { Current = Config.GetBindable<float>( XrConfigSetting.ScreenRadius ), LabelText = "Screen radius" },
					new SettingsSlider<float,MetersSliderBar> { Current = Config.GetBindable<float>( XrConfigSetting.ScreenHeight ), LabelText = "Screen height" },

					new SettingsSlider<int,PxSliderBar> { Current = Config.GetBindable<int>( XrConfigSetting.ScreenResolutionX ), LabelText = "Screen resolution X" },
					new SettingsSlider<int,PxSliderBar> { Current = Config.GetBindable<int>( XrConfigSetting.ScreenResolutionY ), LabelText = "Screen resolution Y" },
				}
			};
		}

		public bool AreSettingsPersistent => true;
	}
}
