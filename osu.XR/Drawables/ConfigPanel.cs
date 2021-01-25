using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.XR.Components;
using osu.XR.Settings;
using osuTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
    public class XrConfigPanel : FlatPanel {
        public readonly ConfigPanel Config = new( true ) { AutoSizeAxes = Axes.X, RelativeSizeAxes = Axes.None, Height = 500 };
        public readonly Bindable<bool> IsVisibleBindable = new();

        [Resolved]
        private OsuGameXr Game { get; set; }

        public XrConfigPanel () {
            PanelAutoScaleAxes = Axes.X;
            PanelHeight = 0.5;
            AutosizeBoth();
            Source.Add( Config );

            VR.BindComponentsLoaded( () => {
                var toggleMenu = VR.GetControllerComponent<ControllerButton>( XrAction.ToggleMenu );
                toggleMenu.BindValueChangedDetailed( v => {
                    if ( v.NewValue ) {
                        if ( Config.State.Value == Visibility.Visible ) {
                            if ( HoldingController is null || HoldingController.Source == v.Source ) {
                                Config.Hide();
                                holdingController = null;
                            }
                            else {
                                holdingController = openingController = v.Source;
                            }
                        }
                        else {
                            holdingController = openingController = v.Source;
                            this.Position = targetPosition;
                            this.Rotation = targetRotation;
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

            inputModeBindable.BindValueChanged( v => {
                if ( inputModeBindable.Value == InputMode.SinglePointer ) {
                    holdingController = null;
                }
                else if ( inputModeBindable.Value == InputMode.DoublePointer ) {
                    holdingController = openingController;
                }
                else if ( inputModeBindable.Value == InputMode.TouchScreen ) {

                }
            }, true );
        }

        private Controller openingController; // TODO try to simplify this
        private Controller _holdingController;
        private Controller holdingController {
            get => _holdingController;
            set {
                var prev = Game.GetControllerFor( _holdingController );
                if ( prev is not null ) prev.IsHoldingAnything = false;

                _holdingController = value;
                var next = HoldingController;
                if ( next is not null ) next.IsHoldingAnything = true;
			}
		}
        public XrController HoldingController {
            get {
                if ( inputModeBindable.Value == InputMode.SinglePointer || VR.EnabledControllerCount <= 1 ) return null;
                if ( holdingController?.IsEnabled == true ) return Game.GetControllerFor( holdingController );
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
            
            if ( Config.State.Value == Visibility.Visible ) {
                if ( VR.EnabledControllerCount == 0 ) {
                    Config.Hide();
                    holdingController = null;
                }
                this.MoveTo( targetPosition, 100 );
                this.RotateTo( targetRotation, 100 );
                RequestedInputMode = HoldingController == Game.MainController ? PanelInputMode.Inverted : PanelInputMode.Regular;
            }
        }
	}

	public class ConfigPanel : SettingsPanel {

        InputSettingSection inputSettingSection = new InputSettingSection();
        public string Title => "VR Settings";
        public string Description => "change the way osu!XR behaves";
		public ConfigPanel ( bool showSidebar ) : base( showSidebar ) {

		}

        protected override IEnumerable<SettingsSection> CreateSections () => new SettingsSection[] {
            inputSettingSection
        };

        private readonly List<SettingsSubPanel> subPanels = new List<SettingsSubPanel>();

        protected override Drawable CreateHeader () => new SettingsHeader( Title, Description );
        protected override Drawable CreateFooter () => new SettingsFooter();
    }

    public class InputSettingSection : SettingsSection {
        public override string Header => "Input";

        public override Drawable CreateIcon () => new SpriteIcon {
            Icon = FontAwesome.Solid.Keyboard
        };


        [BackgroundDependencyLoader]
        private void load ( XrConfigManager config ) {
            Children = new Drawable[] {
                new SettingsEnumDropdown<InputMode> { LabelText = "Input mode", Current = config.GetBindable<InputMode>( XrConfigSetting.InputMode ) },
                new SettingsCheckboxWithTooltip { LabelText = "Emulate touch with single pointer", Current = config.GetBindable<bool>( XrConfigSetting.SinglePointerTouch ), TooltipText = "In single pointer mode, send position only when holding a button" },
                new SettingsCheckboxWithTooltip { LabelText = "Tap only on press (TBD)", Current = config.GetBindable<bool>( XrConfigSetting.TapOnPress ), TooltipText = "In touchscreen mode, hold a button to touch the screen" },
                new SettingsSliderWithTooltip<int, PxSliderBar> { LabelText = "Deadzone (TBD)", Current = config.GetBindable<int>( XrConfigSetting.Deadzone ), TooltipText = "Pointer deadzone after touching the screen or pressing a button" }
            };
        }
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

    public enum InputMode {
        [Description( "Single Pointer" )]
        SinglePointer,
        [Description( "Two Pointers" )]
        DoublePointer,
        [Description( "Touchscreen (TBD)" )]
        TouchScreen
	}
}
