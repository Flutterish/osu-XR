using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.XR.Input.Custom.Components;
using osu.XR.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public class JoystickInput : CustomInput {
		public Hand Hand { get; init; } = Hand.Auto;
		public override string Name => $"{Hand} Joystick";

		event System.Action onDispose;
		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );

			onDispose?.Invoke();
		}

		protected override Drawable CreateSettingDrawable () {
			var joystick = new JoystickZoneVisual { Size = new osuTK.Vector2( 200 ) };

			void lookForValidController ( Controller controller ) {
				if ( controller.Role != OsuGameXr.RoleForHand( Hand ) ) return;

				var comp = VR.GetControllerComponent<Controller2DVector>( XrAction.Scroll, controller );
				System.Action<ValueUpdatedEvent<System.Numerics.Vector2>> action = v => {
					joystick.JoystickPosition.Value = new osuTK.Vector2( v.NewValue.X, -v.NewValue.Y );
				};
				comp.BindValueChangedDetailed( action, true );
				onDispose += () => comp.ValueChanged -= action;
				VR.NewControllerAdded -= lookForValidController;
			}

			VR.BindComponentsLoaded( () => {
				VR.BindNewControllerAdded( lookForValidController, true );
			} );

			return joystick;
		}
	}
}
