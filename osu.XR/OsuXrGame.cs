using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing;
using osu.XR.Graphics.Panels;
using osu.XR.Osu;

namespace osu.XR;

public class OsuXrGame : OsuXrGameBase {
	Scene Scene;
	CurvedPanel osuPanel;
	OsuGameContainer gameContainer;

	public OsuXrGame () {
		Scene = new() {
			RelativeSizeAxes = Axes.Both
		};
		Scene.Camera.Z = -5;
		Scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 )
		} );
		osuPanel.Content.Add( gameContainer = new() );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		Add( Scene );
	}

	#region NonVr Interaction

	public ControlType ControlType = ControlType.Orbit;
	Vector3 CameraOrigin;

	protected override bool OnMouseMove ( MouseMoveEvent e ) {
		e.Target = Scene;
		osuPanel.Content.HasFocus = true;
		if ( tryHit( e.MousePosition, out var pos ) ) {
			if ( useTouch && touchDown )
				osuPanel.Content.TouchMove( this, pos );
			else if ( !useTouch )
				osuPanel.Content.MoveMouse( pos );

			return true;
		}
		else if ( ControlType is ControlType.Fly ) {
			e.Target = Scene;

			var eulerX = Math.Clamp( e.MousePosition.Y / DrawHeight * 180 - 90, -89, 89 );
			var eulerY = e.MousePosition.X / DrawWidth * 720 + 360;

			Scene.Camera.Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, eulerY * MathF.PI / 180 )
				* Quaternion.FromAxisAngle( Vector3.UnitX, eulerX * MathF.PI / 180 );
		}

		return false;
	}

	protected override bool OnDragStart ( DragStartEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MouseDownPosition, out _ ) ) {
			return false;
		}

		return ControlType is ControlType.Orbit;
	}
	protected override void OnDrag ( DragEvent e ) {
		if ( ControlType is ControlType.Orbit ) {
			var dx = e.Delta.X / DrawWidth;
			var dy = e.Delta.Y / DrawHeight;

			if ( e.ShiftPressed ) {
				var m = ( Scene.Camera.Position - CameraOrigin ).Length * 2;
				Scene.Camera.Position -= m * dx * Scene.Camera.Right;
				Scene.Camera.Position += m * dy * Scene.Camera.Up;
				CameraOrigin -= m * dx * Scene.Camera.Right;
				CameraOrigin += m * dy * Scene.Camera.Up;
			}
			else {
				var eulerX = dy * 2 * MathF.PI;
				var eulerY = dx * 4 * MathF.PI;

				var quat = Quaternion.FromAxisAngle( Vector3.UnitY, -eulerY );
				Scene.Camera.Position = CameraOrigin + quat.Inverted().Apply( Scene.Camera.Position - CameraOrigin );
				quat = Quaternion.FromAxisAngle( Scene.Camera.Right, -eulerX );
				Scene.Camera.Position = CameraOrigin + quat.Inverted().Apply( Scene.Camera.Position - CameraOrigin );

				Scene.Camera.Rotation = ( CameraOrigin - Scene.Camera.Position ).LookRotation();
			}
		}
	}

	protected override bool OnScroll ( ScrollEvent e ) {
		e.Target = Scene;
		if ( tryHit( e.MousePosition, out _ ) ) {
			osuPanel.Content.Scroll += e.ScrollDelta;
			return true;
		}
		else if ( ControlType is ControlType.Orbit ) {
			Scene.Camera.Position = CameraOrigin + ( Scene.Camera.Position - CameraOrigin ) * ( 1 + e.ScrollDelta.Y / 10 );
		}

		return base.OnScroll( e );
	}

	protected override void Update () {
		base.Update();

		if ( ControlType is ControlType.Fly ) {
			var camera = Scene.Camera;
			var state = GetContainingInputManager().CurrentState;
			var keyboard = state.Keyboard;

			Vector3 dir = Vector3.Zero;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.W ) )
				dir += camera.Forward;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.S ) )
				dir += camera.Back;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.A ) )
				dir += camera.Left;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.D ) )
				dir += camera.Right;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.Space ) )
				dir += camera.Up;
			if ( keyboard.Keys.IsPressed( osuTK.Input.Key.ControlLeft ) )
				dir += camera.Down;

			if ( dir.Length > 0.1f )
				camera.Position += dir.Normalized() * (float)Time.Elapsed / 300;
		}
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

	#endregion
}
