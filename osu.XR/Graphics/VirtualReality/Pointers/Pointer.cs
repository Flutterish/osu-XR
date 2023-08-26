using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osuTK.Input;

namespace osu.XR.Graphics.VirtualReality.Pointers;

/// <summary>
/// A wrapper for an <see cref="IPointer"/> which manages focus, pointer position and input
/// </summary>
public class Pointer {
	public readonly IPointer Source;
	public readonly PanelInteractionSystem.Source InputSource;
	public IHasCollider? HoveredCollider { get; private set; }

	public readonly BindableBool TapStrum = new();

	public Pointer ( IPointer source, PanelInteractionSystem interactionSystem ) {
		Source = source;
		InputSource = interactionSystem.GetSource( source );

		LeftButton.OnPressed += () => onButtonStateChanged( true, MouseButton.Left );
		LeftButton.OnRepeated += () => onButtonStateChanged( true, MouseButton.Left, isRepeated: true );
		LeftButton.OnReleased += () => onButtonStateChanged( false, MouseButton.Left );
		RightButton.OnPressed += () => onButtonStateChanged( true, MouseButton.Right );
		RightButton.OnRepeated += () => onButtonStateChanged( true, MouseButton.Right, isRepeated: true );
		RightButton.OnReleased += () => onButtonStateChanged( false, MouseButton.Right );
	}

	public bool ForceTouch;
	public bool UsesTouch => ForceTouch || Source.IsTouchSource;
	public Vector2 CurrentPosition { get; private set; }
	public bool IsTouchDown { get; private set; }
	public void Blur () {
		InputSource.FocusedPanel = null;
		HoveredCollider = null;
		if ( IsTouchDown ) {
			IsTouchDown = false;
			TouchDownStateChanged?.Invoke( false );
		}
	}

	public PointerHit? Update ( Vector3 playerPosition, Vector3 position, Quaternion rotation ) {
		var maybeHit = Source.UpdatePointer( playerPosition, position, rotation );

		if ( maybeHit is PointerHit hit ) {
			HoveredCollider = hit.Collider;
			if ( hit.Collider is Panel panel ) {
				CurrentPosition = panel.GlobalSpaceContentPositionAt( hit.TrisIndex, hit.Point );

				if ( Source.IsTouchSource && !IsTouchDown ) {
					onButtonStateChanged( true, MouseButton.Left, isFromTouch: true );
				}

				if ( IsTouchDown )
					InputSource.TouchMove( CurrentPosition );
				else if ( !UsesTouch )
					panel.Content.MoveMouse( CurrentPosition );
			}
		}
		else {
			if ( UsesTouch && IsTouchDown ) {
				onButtonStateChanged( false, MouseButton.Left, isFromTouch: true );
			}

			HoveredCollider = null;
		}

		return maybeHit;
	}

	void onButtonStateChanged ( bool value, MouseButton button, bool isFromTouch = false, bool isRepeated = false ) {
		if ( HoveredCollider is Panel panel ) {
			InputSource.FocusedPanel = panel;
		}
		else if ( InputSource.FocusedPanel is null || value ) return;

		if ( UsesTouch )
			handleTouch( value, button, isFromTouch );
		else
			handleMouse( value, button, isRepeated );
	}

	void handleTouch ( bool value, MouseButton button, bool isFromTouch ) {
		if ( button is not MouseButton.Left )
			return;

		if ( IsTouchDown ) {
			if ( value ) {
				InputSource.TouchUp();
				InputSource.TouchDown( CurrentPosition );
			}
			else if ( !Source.IsTouchSource || isFromTouch ) {
				IsTouchDown = false;
				TouchDownStateChanged?.Invoke( false );
				InputSource.TouchUp();
			}
			else if ( TapStrum.Value ) {
				InputSource.TouchUp();
				InputSource.TouchDown( CurrentPosition );
			}
		}
		else if ( value ) {
			IsTouchDown = true;
			TouchDownStateChanged?.Invoke( true );
			InputSource.TouchDown( CurrentPosition );
		}
	}

	void handleMouse ( bool value, MouseButton button, bool isRepeated ) {
		if ( value ) {
			if ( isRepeated )
				InputSource.Release( button );

			InputSource.Press( button );
		}
		else {
			InputSource.Release( button );
		}
	}

	public event Action<bool>? TouchDownStateChanged;

	public readonly PointerButton LeftButton = new();
	public readonly PointerButton RightButton = new();
}
