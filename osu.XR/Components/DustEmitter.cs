using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Logging;
using osu.Framework.Utils;
using osu.Framework.XR.Components;
using osu.Framework.XR.Maths;
using osu.Game.Overlays.Settings;
using osu.XR.Inspector;
using osu.XR.Settings;
using osu.XR.Settings.Sections;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class DustEmitter : ParticleEmitter<DustParticle>, IConfigurableInspectable {
		protected override DustParticle CreateParticle ()
			=> new();

		double emitTimer;
		double emitInterval = 5;
		protected override void Update () {
			base.Update();

			if ( !showDust.Value ) return;

			if ( ActiveParticles < 300 ) {
				emitTimer += Time.Elapsed;
			}
			else emitTimer = 0;
			while ( ActiveParticles < 300 && emitTimer > emitInterval ) {
				Emit();
				emitTimer -= emitInterval;
			}
		}

		Bindable<bool> showDust = new( true );

		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config ) {
			config.BindWith( XrConfigSetting.ShowDust, showDust );
		}

		public Drawable CreateInspectorSubsection () {
			return new SettingsSectionContainer {
				Title = "Dust Particles",
				Icon = FontAwesome.Solid.Star,
				Children = new Drawable[] {
					new SettingsCheckbox { Current = showDust, LabelText = "Show Dust Particles" }
				}
			};
		}
		public bool AreSettingsPersistent => true;
	}

	public class DustParticle : ParticleEmiter.Particle {
		public DustParticle () {
			ShouldBeDepthSorted = true;
			AlwaysPresent = true;
		}

		[Resolved]
		private Player player { get; set; }

		[BackgroundDependencyLoader]
		private void load ( TextureStore textures ) {
			MainTexture = textures.Get( "dust" ).TextureGL;
		}

		protected override void OnApply ( ParticleEmiter emmiter ) {
			base.OnApply( emmiter );

			this.FadeInFromZero( 400, Easing.Out ).Then().FadeOut( 800, Easing.In ).Then().Schedule( () => Release() );
			this.MoveTo( new Vector3( RNG.NextSingle( -5, 5 ), RNG.NextSingle( 0, 6 ), RNG.NextSingle( -5, 5 ) ) + player.GlobalPosition.With( y: 0 ) )
				.MoveToOffset( new Vector3( RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ) ) * 0.1f, 1200 );
		}
	}
}
