using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.XR.Graphics.Bindings.Editors;

// TODO this is copy-pasted from v1, we can probably remove some things
public partial class JoystickZonePiece : JoystickPiece {
	public readonly BindableDouble ZoneStartAngle = new( -30 );
	public readonly BindableDouble ZoneDeltaAngle = new( 60 );
	public readonly BindableDouble DeadzonePercentage = new( 0.4 );

	private BindableDouble animatedZoneStartAngle = new();
	private BindableDouble animatedZoneDeltaAngle = new();
	private BindableDouble animatedDeadzonePercentage = new();

	Box lineA = null!;
	Box lineB = null!;
	DistanceHandle progressOuter = null!;
	CircularProgress progressInner = null!;
	RotationHandle lineC = null!;
	RotationHandle lineD = null!;
	protected override void CreateInteractiveElements () {
		AddInternal( progressOuter = new DistanceHandle( this ) {
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			RelativeSizeAxes = Axes.Both,
			Colour = Colours.Highlight1
		} );
		AddInternal( progressInner = new CircularProgress {
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			RelativeSizeAxes = Axes.Both,
			Colour = Colours.Background5
		} );

		AddInternal( lineA = new Box {
			RelativeSizeAxes = Axes.Both,
			Size = new Vector2( 0.5f * 0.98f, 5f / 320 ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			Colour = Colours.Highlight1,
			EdgeSmoothness = new(1)
		} );
		AddInternal( lineB = new Box {
			RelativeSizeAxes = Axes.Both,
			Size = new Vector2( 0.5f * 0.98f, 5f / 320 ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			Colour = Colours.Highlight1,
			EdgeSmoothness = new( 1 )
		} );
		AddInternal( lineC = new RotationHandle {
			RelativeSizeAxes = Axes.Both,
			Size = new Vector2( 0.5f, 10f / 320 ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			Colour = Colours.Highlight1
		} );
		AddInternal( lineD = new RotationHandle {
			RelativeSizeAxes = Axes.Both,
			Size = new Vector2( 0.5f, 10f / 320 ),
			Origin = Anchor.Centre,
			Anchor = Anchor.Centre,
			Colour = Colours.Highlight1
		} );

		ZoneStartAngle.BindValueChanged( v => this.TransformBindableTo( animatedZoneStartAngle, closestEquivalentAngle( animatedZoneStartAngle.Value, v.NewValue ), 50, Easing.Out ), true );
		ZoneDeltaAngle.BindValueChanged( v => this.TransformBindableTo( animatedZoneDeltaAngle, v.NewValue, 50, Easing.Out ), true );
		DeadzonePercentage.BindValueChanged( v => this.TransformBindableTo( animatedDeadzonePercentage, v.NewValue, 50, Easing.Out ), true );
		animatedZoneStartAngle.BindValueChanged( v => updateZone() );
		animatedZoneDeltaAngle.BindValueChanged( v => updateZone() );
		animatedDeadzonePercentage.BindValueChanged( v => updateZone() );
		updateZone();

		ZoneStartAngle.ValueChanged += v => updateActivation();
		ZoneDeltaAngle.ValueChanged += v => updateActivation();
		DeadzonePercentage.ValueChanged += v => updateActivation();
		JoystickPosition.ValueChanged += v => updateActivation();

		lineC.Angle.Value = ZoneStartAngle.Value;
		lineD.Angle.Value = ZoneStartAngle.Value + ZoneDeltaAngle.Value;
		lineC.Angle.ValueChanged += v => {
			var delta = Math.Clamp( deltaAngle( ZoneStartAngle.Value, v.NewValue ), ZoneDeltaAngle.Value - 360, ZoneDeltaAngle.Value );
			ZoneStartAngle.Value += delta;
			ZoneDeltaAngle.Value -= delta;
		};
		lineD.Angle.ValueChanged += v => {
			var delta = Math.Clamp( deltaAngle( ZoneStartAngle.Value + ZoneDeltaAngle.Value, v.NewValue ), -ZoneDeltaAngle.Value, 360 - ZoneDeltaAngle.Value );
			ZoneDeltaAngle.Value += delta;
		};
		progressOuter.Distance.ValueChanged += v => {
			DeadzonePercentage.Value = v.NewValue;
		};

		IsActive.ValueChanged += v => {
			if ( v.NewValue ) {
				progressInner.FadeColour( Colours.Highlight1.Multiply( 0.75f ) ).FlashColour( Colour4.White, 200 );
			}
			else {
				progressInner.FadeColour(  Colours.Background5, 200 );
			}
		};
		updateActivation();
	}

	double deltaAngle ( double current, double goal ) {
		var diff = ( goal - current ) % 360;
		if ( diff < 0 ) diff += 360;
		if ( diff > 180 ) diff -= 360;

		return diff;
	}
	double closestEquivalentAngle ( double current, double goal ) {
		return current + deltaAngle( current, goal );
	}

	public readonly BindableBool IsActive = new();
	public bool IsNormalizedPointInside ( Vector2 pos ) {
		if ( pos.Length < DeadzonePercentage.Value ) return false;
		if ( pos.Length == 0 ) return true;
		return IsAngleInside( pos );
	}
	public bool IsAngleInside ( Vector2 direction ) {
		var angle = Math.Atan2( direction.Y, direction.X ) / Math.PI * 180;
		angle = deltaAngle( ZoneStartAngle.Value, angle );
		if ( angle < 0 ) angle += 360;
		return angle <= ZoneDeltaAngle.Value;
	}
	void updateActivation () {
		IsActive.Value = IsNormalizedPointInside( JoystickPosition.Value );
	}

	void updateZone () {
		lineC.Width = lineD.Width = (float)( 1 - animatedDeadzonePercentage.Value ) / 2 * 0.98f;
		lineA.Rotation = lineC.Rotation = (float)animatedZoneStartAngle.Value;
		lineB.Rotation = lineD.Rotation = (float)( animatedZoneStartAngle.Value + animatedZoneDeltaAngle.Value );

		progressOuter.Rotation = (float)animatedZoneStartAngle.Value + 90;
		progressOuter.InnerRadius = (float)( 1 - animatedDeadzonePercentage.Value );
		progressOuter.Current.Value = animatedZoneDeltaAngle.Value / 360;

		progressInner.Rotation = (float)animatedZoneStartAngle.Value + 90;
		progressInner.Current.Value = animatedZoneDeltaAngle.Value / 360;
	}

	protected override void Update () {
		base.Update();

		progressInner.InnerRadius = (float)( 1 - animatedDeadzonePercentage.Value ) - ( 20 / DrawSize.X ) * ( DrawSize.X / 320 );

		var angleA = animatedZoneStartAngle.Value / 180 * Math.PI;
		var angleB = ( animatedZoneStartAngle.Value + animatedZoneDeltaAngle.Value ) / 180 * Math.PI;

		var dirA = new Vector2( MathF.Cos( (float)angleA ), MathF.Sin( (float)angleA ) );
		var dirB = new Vector2( MathF.Cos( (float)angleB ), MathF.Sin( (float)angleB ) );

		lineA.Position = DrawSize * dirA / 4 + dirA.PerpendicularRight * 7.5f * DrawSize.X / 320;
		lineB.Position = DrawSize * dirB / 4 + dirB.PerpendicularLeft * 7.5f * DrawSize.X / 320;
		lineC.Position = DrawSize * dirA / 2 * (float)( animatedDeadzonePercentage.Value + ( 1 - animatedDeadzonePercentage.Value ) / 2 * 0.98f ) + dirA.PerpendicularRight * 5 * DrawSize.X / 320;
		lineD.Position = DrawSize * dirB / 2 * (float)( animatedDeadzonePercentage.Value + ( 1 - animatedDeadzonePercentage.Value ) / 2 * 0.98f ) + dirB.PerpendicularLeft * 5 * DrawSize.X / 320;
	}

	partial class RotationHandle : CompositeDrawable {
		public readonly BindableDouble Angle = new();

		public RotationHandle () {
			AddInternal( new Box {
				RelativeSizeAxes = Axes.Both,
				EdgeSmoothness = new(1)
			} );
			// AddInternal( new HoverClockSamples() );
		}

		protected override bool OnDragStart ( DragStartEvent e ) {
			return true;
		}

		protected override void OnDrag ( DragEvent e ) {
			base.OnDrag( e );

			var pos = e.MousePosition - Parent.DrawSize / 2;
			Angle.Value = Math.Atan2( pos.Y, pos.X ) / Math.PI * 180;
		}
	}

	partial class DistanceHandle : CircularProgress {
		public readonly BindableDouble Distance = new();
		JoystickZonePiece parent;

		public DistanceHandle ( JoystickZonePiece parent ) {
			// AddInternal( new HoverClockSamples() );
			this.parent = parent;
		}

		protected override bool OnDragStart ( DragStartEvent e ) {
			var pos = Vector2.Divide( e.MousePosition - Parent.DrawSize / 2, Parent.DrawSize ) * 2;
			return parent.IsAngleInside( pos );
		}

		protected override void OnDrag ( DragEvent e ) {
			base.OnDrag( e );

			var pos = Vector2.Divide( e.MousePosition - Parent.DrawSize / 2, Parent.DrawSize ) * 2;
			if ( parent.IsAngleInside( pos ) )
				Distance.Value = Math.Clamp( pos.Length, 0, 1 );
			else
				Distance.Value = 0;
		}
	}
}
