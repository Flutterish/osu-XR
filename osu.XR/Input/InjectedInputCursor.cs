using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.XR.Osu;

namespace osu.XR.Input;

public partial class InjectedInputCursor : CompositeDrawable {
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
	}

	public void MoveTo ( Vector2 position, bool isNormalized = false ) {
		var quad = info.InputManager.ScreenSpaceDrawQuad;

		if ( !isPositionInitialized ) {
			isPositionInitialized = true;
			mousePos = quad.Size / 2;
		}

		if ( isNormalized ) {
			var scale = Math.Min( quad.Width, quad.Height ) / 2;
			position *= scale;
		}

		moveTo( mousePos = position + quad.Size / 2 );
	}

	bool isPositionInitialized = false;
	Vector2 mousePos;
	public void MoveBy ( Vector2 position, bool isNormalized = false ) {
		var quad = info.InputManager.ScreenSpaceDrawQuad;

		if ( !isPositionInitialized ) {
			isPositionInitialized = true;
			mousePos = quad.Size / 2;
		}

		if ( isNormalized ) {
			var scale = Math.Min( quad.Width, quad.Height ) / 2;
			position *= scale;
		}

		mousePos += position;
		mousePos = new Vector2(
			Math.Clamp( mousePos.X, 0, quad.Width ),
			Math.Clamp( mousePos.Y, 0, quad.Height )
		);
		moveTo( mousePos );
	}

	void moveTo ( Vector2 pos ) {
		pos += info.InputManager.ScreenSpaceDrawQuad.TopLeft;
		input.MoveTo( this, pos );
		Position = Parent.ToLocalSpace( pos );
	}
}
