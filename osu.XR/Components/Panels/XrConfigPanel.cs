using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Components;
using osu.XR.Drawables;
using osu.XR.Settings;
using osuTK;
using System;

namespace osu.XR.Components.Panels {
	public class XrConfigPanel : FlatPanel {
		public readonly ConfigPanel Config = new( true ) { AutoSizeAxes = Axes.X, RelativeSizeAxes = Axes.None, Height = 500 };
		public readonly Bindable<bool> IsVisibleBindable = new();

		[Resolved]
		private OsuGameXr Game { get; set; }

		public XrConfigPanel () {
			PanelAutoScaleAxes = Axes.X;
			PanelHeight = 0.5;
			RelativeSizeAxes = Axes.X;
			Height = 500;
			AutosizeX();
			Source.Add( Config );

			VR.BindComponentsLoaded( () => {
				var toggleMenu = VR.GetControllerComponent<ControllerButton>( XrAction.ToggleMenu );
				toggleMenu.BindValueChangedDetailed( v => {
					if ( v.NewValue ) {
						if ( Config.State.Value == Visibility.Visible ) {
							if ( HoldingController is null || HoldingController.Source == v.Source ) {
								Config.Hide();
								openingController = null;
							}
							else {
								openingController = v.Source;
							}
						}
						else {
							openingController = v.Source;
							Position = targetPosition;
							Rotation = targetRotation;
							Config.Show();
						}
					}
				} );
			} );
		}
		Bindable<InputMode> inputModeBindable = new();

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
		public override bool IsColliderEnabled => Config.State.Value == Visibility.Visible;

		Vector3 targetPosition {
			get {
				if ( HoldingController is null ) {
					return Game.Camera.Position + Game.Camera.Forward * 0.5f;
				}
				else {
					return HoldingController.Position + HoldingController.Forward * 0.2f + HoldingController.Up * 0.05f;
				}
			}
		}

		Quaternion targetRotation {
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
			IsVisible = Config.IsPresent;
			IsVisibleBindable.Value = IsVisible;

			if ( HoldingController != previousHoldingController ) {
				if ( previousHoldingController is not null ) previousHoldingController.IsHoldingAnything = false;
				previousHoldingController = HoldingController;
				if ( previousHoldingController is not null ) previousHoldingController.IsHoldingAnything = true;
			}
			if ( Config.State.Value == Visibility.Visible ) {
				if ( VR.EnabledControllerCount == 0 ) {
					Config.Hide();
					openingController = null;
				}
				this.MoveTo( targetPosition, 100 );
				this.RotateTo( targetRotation, 100 );
				RequestedInputMode = HoldingController == Game.MainController ? PanelInputMode.Inverted : PanelInputMode.Regular;
			}
		}
	}
}
