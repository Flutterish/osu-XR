using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.XR.Components.Panels;
using osu.XR.Settings;
using osuTK;
using System;
using System.Linq;

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
							Show();
							openingController = v.Source;
							Position = TargetPosition;
							Rotation = TargetRotation;
						}
					}
				} );
			} );
		}
		public bool IsOpen { get; private set; }

		public override void Hide () {
			base.Hide();
			IsOpen = false;
			openingController = null;
		}

		public override void Show () {
			base.Show();
			IsOpen = true;
		}

		private Vector3 retainedPosition;
		protected override Vector3 TargetPosition {
			get {
				if ( !IsOpen ) return retainedPosition;
				if ( HoldingController is null ) {
					return retainedPosition = Game.Camera.Position + Game.Camera.Forward * 0.5f;
				}
				else {
					return retainedPosition = HoldingController.Position + HoldingController.Forward * 0.2f + HoldingController.Up * 0.05f;
				}
			}
		}

		private Quaternion retainedRotation;
		protected override Quaternion TargetRotation {
			get {
				if ( !IsOpen ) return retainedRotation;
				if ( HoldingController is null ) {
					return retainedRotation = Game.Camera.Rotation;
				}
				else {
					return retainedRotation = HoldingController.Rotation * Quaternion.FromAxisAngle( Vector3.UnitX, MathF.PI * 0.25f );
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
