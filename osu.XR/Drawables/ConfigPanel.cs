using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.XR.Components;
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
        public Bindable<InputMode> InputModeBindable => Config.InputModeBindable;
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
                toggleMenu.BindValueChanged( v => {
                    if ( v ) {
                        Config.ToggleVisibility();
                    }
                } );
            } );
        }

		protected override void Update () {
			base.Update();
            IsVisible = Config.IsPresent;
            IsVisibleBindable.Value = IsVisible;
            var main = Game.MainController;
            var secondary = Game.SecondaryController;
            if ( main is null ) {
                Config.Hide();
            }
            else if ( secondary is null ) {
                this.MoveTo( Game.Camera.Position + Game.Camera.Forward * 0.5f, 100 );
                this.RotateTo( Game.Camera.Rotation, 100 );
                RequestedInputMode = PanelInputMode.Regular;
            }
            else {
                Position = main.Position + ( main.Controller.Role == Valve.VR.ETrackedControllerRole.LeftHand ? main.Right : main.Left ) * 0.3f;
                Rotation = main.Rotation * Quaternion.FromAxisAngle( Vector3.UnitX, MathF.PI / 2 );
                RequestedInputMode = PanelInputMode.Inverted;
            }
        }
	}

	public class ConfigPanel : SettingsPanel {
        public Bindable<InputMode> InputModeBindable => inputSettingSection.InputModeBindable;

        InputSettingSection inputSettingSection;
        public string Title => "VR Settings";
        public string Description => "change the way osu!XR behaves";
		public ConfigPanel ( bool showSidebar ) : base( showSidebar ) {

		}

        protected override IEnumerable<SettingsSection> CreateSections () => new SettingsSection[] {
            inputSettingSection = new InputSettingSection()
        };

        private readonly List<SettingsSubPanel> subPanels = new List<SettingsSubPanel>();

        protected override Drawable CreateHeader () => new SettingsHeader( Title, Description );
        protected override Drawable CreateFooter () => new SettingsFooter();
    }

    public class InputSettingSection : SettingsSection {
        public readonly Bindable<InputMode> InputModeBindable = new Bindable<InputMode>( InputMode.SinglePointer );
        public readonly BindableBool EmulateTouchWithPointersBindable = new();
        public readonly BindableBool TouchOnPressBindable = new();
        public readonly BindableInt DeadzoneBindable = new( 20 ) { MinValue = 0, MaxValue = 100 };
        public override string Header => "Input";

        public override Drawable CreateIcon () => new SpriteIcon {
            Icon = FontAwesome.Solid.Keyboard
        };

        public InputSettingSection () {
            Children = new Drawable[] {
                new SettingsEnumDropdown<InputMode> { LabelText = "Input mode", Current = InputModeBindable },
                new SettingsCheckboxWithTooltip { LabelText = "Emulate touch with single pointer", Current = EmulateTouchWithPointersBindable, TooltipText = "In single pointer mode, send position only when holding a button" },
                new SettingsCheckboxWithTooltip { LabelText = "Tap only on press", Current = TouchOnPressBindable, TooltipText = "In touchscreen mode, hold a button to touch the screen" },
                new SettingsSliderWithTooltip<int, PxSliderBar> { LabelText = "Deadzone", Current = DeadzoneBindable, TooltipText = "Pointer deadzone after touching the screen or pressing a button" }
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
        [Description( "Touchscreen" )]
        TouchScreen
	}
}
