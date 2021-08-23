using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Game.Overlays.Settings;
using osu.XR.Inspector;
using osu.XR.Settings;

namespace osu.XR.Components.Skyboxes {
	public class SkyBox : CompositeDrawable3D, IConfigurableInspectable {
		SolidSkyBox solid = new SolidSkyBox();
		LightsOutSkyBox lightsOut = new LightsOutSkyBox();

		private Bindable<SkyBoxType> typeBindable = new();
		private Bindable<Drawable3D> activeSkybox = new();
		private Bindable<Drawable> _activeSkybox = new();
		public SkyBox () {
			activeSkybox.BindValueChanged( v => {
				v.OldValue?.Hide();
				if ( v.NewValue is null ) {
					ClearInternal( false );
				}
				else {
					InternalChild = v.NewValue;
					v.NewValue.Show();
				}
				_activeSkybox.Value = v.NewValue;
			} );

			typeBindable.BindValueChanged( v => {
				activeSkybox.Value = v.NewValue switch {
					SkyBoxType.Solid => solid,
					SkyBoxType.LightsOut => lightsOut,
					_ => null
				};
			}, true );
		}

		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config ) {
			config.BindWith( XrConfigSetting.SkyboxType, typeBindable );
		}

		public Drawable CreateInspectorSubsection () {
			var section = new InspectorSubsectionWithCurrent {
				Title = "Skybox",
				Icon = FontAwesome.Solid.Image,
				Current = _activeSkybox,
				Children = new Drawable[] {
					new SettingsEnumDropdown<SkyBoxType> { Current = typeBindable, LabelText = "Skybox type" }
				}
			};

			return section;
		}

		public bool AreSettingsPersistent => false;
	}
}
