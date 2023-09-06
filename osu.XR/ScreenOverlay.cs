using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osu.XR.Allocation;
using osu.XR.Graphics;
using osu.XR.Graphics.Settings;
using osu.XR.Osu;
using osuTK.Input;

namespace osu.XR;

public partial class ScreenOverlay : Container {
	public ScreenOverlay () {
		Add( warningContainer = new() {
			RelativeSizeAxes = Axes.Both
		} );
		RelativeSizeAxes = Axes.Both;
	}

	OsuXrScene scene = null!;

	[BackgroundDependencyLoader]
	private void load ( OsuXrGame game ) {
		scene = game.Scene; 
		Add( new HiddenButton {
			Origin = Anchor.Centre,
			Anchor = Anchor.CentreLeft,
			X = 40,
			Action = () => settings.ToggleVisibility()
		} );
		Add( settings );
	}

	Container warningContainer;
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
