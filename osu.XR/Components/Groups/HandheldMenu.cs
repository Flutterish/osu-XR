using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.XR.Components.Panels;
using osu.XR.Settings;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components.Groups {
	public class HandheldMenu : PanelStack {
		Bindable<InputMode> inputModeBindable = new();
		[Resolved]
		private OsuGameXr Game { get; set; }

		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config ) {
			config.BindWith( XrConfigSetting.InputMode, inputModeBindable );
		}

		private Controller openingController;
		private XrController previousHoldingController;
		public XrController HoldingController {
			get {
				if ( inputModeBindable.Value == InputMode.SinglePointer || VR.EnabledControllerCount <= 1 ) return null;
				if ( openingController?.IsEnabled == true ) return Game.GetControllerFor( openingController );
				else return null;
			}
		}

		public HandheldMenu () {
			VR.BindComponentsLoaded( () => {
				var toggleMenu = VR.GetControllerComponent<ControllerButton>( XrAction.ToggleMenu );
				toggleMenu.BindValueChangedDetailed( v => {
					if ( v.NewValue ) {
						if ( Panels.Any( x => x.IsColliderEnabled ) ) {
							if ( HoldingController is null || HoldingController.Source == v.Source ) {
								Hide();
							}
							else {
								openingController = v.Source;
							}
						}
						else {
							openingController = v.Source;
							Position = TargetPosition;
							Rotation = TargetRotation;
							Show();
						}
					}
				} );
			} );
		}

		public override void Hide () {
			base.Hide();
			openingController = null;
		}

		protected override Vector3 TargetPosition {
			get {
				if ( HoldingController is null ) {
					return Game.Camera.Position + Game.Camera.Forward * 0.5f;
				}
				else {
					return HoldingController.Position + HoldingController.Forward * 0.2f + HoldingController.Up * 0.05f;
				}
			}
		}
		protected override Quaternion TargetRotation {
			get {
				if ( HoldingController is null ) {
					return Game.Camera.Rotation;
				}
				else {
					return HoldingController.Rotation * Quaternion.FromAxisAngle( Vector3.UnitX, MathF.PI * 0.25f );
				}
			}
		}

		protected override void Update () {
			base.Update();

			if ( HoldingController != previousHoldingController ) {
				if ( previousHoldingController is not null ) previousHoldingController.IsHoldingAnything = false;
				previousHoldingController = HoldingController;
				if ( previousHoldingController is not null ) previousHoldingController.IsHoldingAnything = true;
			}
			if ( Panels.Any( x => x.IsColliderEnabled ) ) {
				if ( VR.EnabledControllerCount == 0 ) {
					Hide();
				}
				foreach ( var i in Panels ) {
					i.RequestedInputMode = HoldingController == Game.MainController ? PanelInputMode.Inverted : PanelInputMode.Regular;
				}
			}
		}
	}
}
