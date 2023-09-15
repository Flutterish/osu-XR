using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.XR.Graphics.VirtualReality;
using osu.XR.Graphics.VirtualReality.Pointers;
using osu.XR.Osu;

namespace osu.XR.Input;

public partial class InjectedInputCursor : CompositeDrawable, IControllerRelay {
	PlayerInfo info;
	InjectedInput input;
	public InjectedInputCursor ( InjectedInput input ) {
		info = input.PlayerInfo;
		this.input = input;

		Origin = Anchor.Centre;
		AutoSizeAxes = Axes.Both;

		const float size = 50;
		AddInternal( new Circle {
			Size = new( 0.5f * size ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre
		} );
		CircularProgress outer;
		AddInternal( outer = new CircularProgress {
			InnerRadius = 0.1f,
			Size = new( size ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre
		} );
		outer.Current.Value = 1;

		AlwaysPresent = true;
	}

	protected override void Update () {
		base.Update();
		Alpha = input.PlayerInfo.CursorOpacityFromMods;
	}

	public void MoveTo ( Vector2 position, bool isNormalized = false ) {
		var quad = info.InputManager.ScreenSpaceDrawQuad;

		if ( !isPositionInitialized ) {
			isPositionInitialized = true;
			CursorPosition = quad.Size / 2;
		}

		if ( isNormalized ) {
			var scale = Math.Min( quad.Width, quad.Height ) / 2;
			position *= scale;
		}

		moveTo( CursorPosition = position + quad.Size / 2 );
	}

	bool isPositionInitialized = false;
	public Vector2 CursorPosition;
	public void MoveBy ( Vector2 position, bool isNormalized = false ) {
		var quad = info.InputManager.ScreenSpaceDrawQuad;

		if ( !isPositionInitialized ) {
			isPositionInitialized = true;
			CursorPosition = quad.Size / 2;
		}

		if ( isNormalized ) {
			var scale = Math.Min( quad.Width, quad.Height ) / 2;
			position *= scale;
		}

		CursorPosition += position;
		CursorPosition = new Vector2(
			Math.Clamp( CursorPosition.X, 0, quad.Width ),
			Math.Clamp( CursorPosition.Y, 0, quad.Height )
		);
		moveTo( CursorPosition );
	}

	void moveTo ( Vector2 pos ) {
		pos += info.InputManager.ScreenSpaceDrawQuad.TopLeft;
		input.MoveTo( this, pos );
		Position = Parent.ToLocalSpace( pos );
	}

	public readonly RelayButton LeftButton = new();
	public readonly RelayButton RightButton = new();
	public IEnumerable<RelayButton> GetButtonsFor ( VrController source, VrAction action ) {
		if ( action == VrAction.LeftButton )
			yield return source.Hand == Framework.XR.VirtualReality.Hand.Left ? LeftButton : RightButton;
	}

	public void ScrollBy ( VrController source, Vector2 amount ) { }
}
