using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.XR.VirtualReality;
using osu.Game.Overlays;
using osu.XR.Allocation;
using osu.XR.Configuration;
using osu.XR.Graphics;
using osu.XR.Graphics.Input;
using osu.XR.Graphics.Settings;
using osu.XR.Osu;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.XR;

public partial class ScreenOverlay : Container {
	public ScreenOverlay () {
		Add( inputDisplayContainer = new() {
			Anchor = Anchor.BottomLeft,
			Origin = Anchor.BottomLeft,
			AutoSizeAxes = Axes.Both
		} );
		Add( warningContainer = new() {
			RelativeSizeAxes = Axes.Both
		} );
		RelativeSizeAxes = Axes.Both;
	}

	OsuXrScene scene = null!;

	[BackgroundDependencyLoader]
	private void load ( OsuXrGame game, OsuXrConfigManager config ) {
		scene = game.Scene; 
		Add( new HiddenButton {
			Origin = Anchor.Centre,
			Anchor = Anchor.CentreLeft,
			X = 40,
			Action = () => settings.ToggleVisibility()
		} );
		Add( settings );

		config.BindWith( OsuXrSetting.ShowInputDisplay, showInputDisplay );
		showInputDisplay.BindValueChanged( v => {
			inputDisplayContainer.Clear( disposeChildren: true );
			if ( v.NewValue ) {
				inputDisplayContainer.Add( new Container {
					RelativeSizeAxes = Axes.Both,
					Masking = true,
					CornerRadius = 10,
					Position = new( -15, 15 ),
					Child = new Box {
						RelativeSizeAxes = Axes.Both,
						Colour = Colour4.Black.Opacity( 0.2f )
					}
				}.WithEffect( new GlowEffect { PadExtent = true, Blending = BlendingParameters.Mixture, Colour = Color4.Black } ) );
				inputDisplayContainer.Add( new FillFlowContainer() {
					Direction = FillDirection.Horizontal,
					AutoSizeAxes = Axes.Both,
					Anchor = Anchor.BottomLeft,
					Origin = Anchor.BottomLeft,
					Spacing = new( 30 ),
					Margin = new() { Top = 50, Bottom = 20, Left = 20, Right = 50 },
					Children = new[] {
						new ControllerInputDisplay( Hand.Left ),
						new ControllerInputDisplay( Hand.Right )
					}
				} );
			}
		}, true );
	}

	Container warningContainer;
	Bindable<bool> showInputDisplay = new();
	Container inputDisplayContainer;
	NotRenderingWarning warning = new();
	protected override void Update () {
		base.Update();

		if ( (warning.Parent == null) != scene.RenderToScreen ) {
			if ( scene.RenderToScreen ) {
				warningContainer.Remove( warning, disposeImmediately: false );
			}
			else {
				warningContainer.Add( warning );
			}
		}
	}

	[Cached]
	protected OverlayColourProvider ColourProvider { get; } = new OverlayColourProvider( OverlayColourScheme.Purple );

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		return new ExcludingDependencyContainer( base.CreateChildDependencies( new MergedDepencencyContainer( parent.Get<OsuDependencies>(), parent ) ), t => t.Name != "IOverlayManager" );
	}

	OverlaySettings settings = new( showSidebar: true );
	protected override bool OnKeyDown ( KeyDownEvent e ) {
		if ( e.Key == Key.F10 ) {
			settings.ToggleVisibility();
			return true;
		}
		return false;
	}
}
