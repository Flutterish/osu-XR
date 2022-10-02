using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Physics;
using osu.XR.Graphics.Panels;

namespace osu.XR.Tests.Visual.Osu;

public class TestSceneOsuGame : Basic3DTestScene {
	OsuPanel osuPanel;

	public TestSceneOsuGame () {
		Scene.Camera.Z = -5;
		Scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 )
		} );

		osuPanel.Content.HasFocus = true;

		AddToggleStep( "Use Touch", v => {
			osuPanel.Content.ReleaseAllInput();
			useTouch = v;
			touchDown = false;
		} );
	}

	bool useTouch;
	bool tryHit ( Vector2 e, out Vector2 pos ) {
		if ( Raycast.TryHit( Scene.Camera.Position, Scene.Camera.DirectionOf( e, Scene.DrawWidth, Scene.DrawHeight ), osuPanel, out var hit ) ) {
			pos = osuPanel.ContentPositionAt( hit.TrisIndex, hit.Point );

			return true;
		}
		pos = default;
		return false;
	}

	bool touchDown = false;
	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out var pos ) ) {
			if ( useTouch && touchDown )
				osuPanel.Content.TouchMove( this, pos );
			else if ( !useTouch )
				osuPanel.Content.MoveMouse( pos );

			return true;
		}

		return base.OnMouseMove( e );
	}
	protected override bool OnDragStart ( DragStartEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MouseDownPosition, out _ ) ) {
			return false;
		}

		return base.OnDragStart( e );
	}

	protected override bool OnMouseDown ( MouseDownEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MouseDownPosition, out var pos ) ) {
			if ( useTouch ) {
				touchDown = true;
				osuPanel.Content.TouchDown( this, pos );
			}
			else {
				osuPanel.Content.MoveMouse( pos );
				osuPanel.Content.Press( e.Button );
			}

			return true;
		}
		else {
			osuPanel.Content.ReleaseAllInput();
		}

		return base.OnMouseDown( e );
	}

	protected override void OnMouseUp ( MouseUpEvent e ) {
		if ( useTouch ) {
			touchDown = false;
			osuPanel.Content.TouchUp( this );
		}
		else
			osuPanel.Content.Release( e.Button );

		if ( !tryHit( e.MouseDownPosition, out _ ) ) {
			osuPanel.Content.ReleaseAllInput();
		}
	}

	protected override bool OnScroll ( ScrollEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out _ ) ) {
			osuPanel.Content.Scroll += e.ScrollDelta;
			return true;
		}

		return base.OnScroll( e );
	}

	protected override bool OnKeyDown ( KeyDownEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out _ ) ) {
			osuPanel.Content.Press( e.Key );
			return true;
		}

		return base.OnKeyDown( e );
	}

	protected override void OnKeyUp ( KeyUpEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out _ ) ) {
			osuPanel.Content.Release( e.Key );
			return;
		}

		base.OnKeyUp( e );
	}
}
