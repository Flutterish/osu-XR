using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Transforms;
using osu.Game.Graphics.Containers;
using osu.XR.Graphics.Panels.Menu;

namespace osu.XR.Graphics.Panels;

public partial class ToastMessageStack : MenuPanel {
	OsuTextFlowContainer text;
	public ToastMessageStack () {
		Content.Masking = true;
		Content.CornerRadius = 10;
		Content.Add( new Box {
			RelativeSizeAxes = Axes.Both,
			Colour = ColourProvider.Background4
		} );

		Content.Add( text = new() {
			TextAnchor = Anchor.Centre,
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			AutoSizeAxes = Axes.Both,
			Padding = new MarginPadding( 12 )
		} );

		ContentAutoSizeAxes = Axes.Both;
		Content.AutoSizeEasing = Easing.Out;
		Content.AutoSizeDuration = 300;
		Content.AlwaysPresent = true;
		Content.Alpha = 0;

		IsColliderEnabled = false;
		RenderStage = RenderingStage.Transparent;
	}

	protected override Material GetDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( Materials.MaterialNames.PanelTransparent );
	}

	Vector2 contentSize;
	protected override void UpdateAfterChildren () {
		if ( contentSize != Content.DrawSize ) {
			contentSize = Content.DrawSize;
			InvalidateMesh();
		}

		base.UpdateAfterChildren();
	}

	protected override void RegenrateMesh () {
		var h = (int)Content.DrawHeight / PREFFERED_CONTENT_HEIGHT * PANEL_HEIGHT / 2;
		var w = (int)Content.DrawWidth / PREFFERED_CONTENT_WIDTH * PANEL_WIDTH / 2;

		Mesh.AddQuad( new Quad3 {
			TL = new Vector3( -w, h, 0 ),
			TR = new Vector3( w, h, 0 ),
			BL = new Vector3( -w, -h, 0 ),
			BR = new Vector3( w, -h, 0 )
		} );
	}

	Queue<(LocalisableString text, double duration)> messages = new();
	public void PostMessage ( LocalisableString message, double duration = 6000 ) {
		messages.Enqueue( (message, duration) );
	}

	double lastMessageStartTime;
	double lastMessageDuration;
	Visibility visibility = Visibility.Hidden;
	Visibility Visibility {
		get => visibility;
		set {
			if ( visibility == value )
				return;

			visibility = value;
			if ( value == Visibility.Visible )
				PopIn();
			else
				PopOut();
		}
	}

	[Resolved]
	OsuXrGame game { get; set; } = null!;
	protected Vector3 TargetPosition {
		get {
			if ( game.Headset is null )
				return Vector3.Zero;
			return game.Headset.GlobalPosition - Vector3.UnitY * 0.1f + game.Headset.GlobalRotation.Apply( Vector3.UnitZ ) * 0.5f;
		}
	}

	protected Quaternion TargetRotation {
		get {
			if ( game.Headset is null )
				return Quaternion.Identity;
			return game.Headset.GlobalRotation;
		}
	}

	protected override void Update () {
		base.Update();

		var lerp = (float)Interpolation.DampContinuously( 0, 1, 50, Time.Elapsed );
		Position = Position + (TargetPosition - Position) * lerp;
		Rotation = Quaternion.Slerp( Rotation, TargetRotation, lerp );

		if ( lastMessageStartTime + lastMessageDuration < Clock.CurrentTime ) {
			if ( messages.TryDequeue( out var next ) ) {
				text.Text = next.text;
				text.FlashColour( Colour4.White, 100 );
				lastMessageStartTime = Clock.CurrentTime;
				lastMessageDuration = next.duration;

				Visibility = Visibility.Visible;
			}
			else {
				Visibility = Visibility.Hidden;
			}
		}
	}

	void PopIn () {
		Content.FinishTransforms( true );
		Content.FadeIn( 300, Easing.Out );
		this.ScaleTo( new Vector3( 1.2f ) ).Then().ScaleTo( new Vector3( 1f ), 300, Easing.OutBounce );
	}

	void PopOut () {
		Content.FadeOut( 300, Easing.Out );
		this.ScaleTo( new Vector3( 0.8f ), 300, Easing.Out );
	}
}
