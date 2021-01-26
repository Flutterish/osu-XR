using osu.Framework.Graphics;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public static class TransformExtensions {
        public static TransformSequence<T> MoveTo<T> ( this T drawable, Vector3 position, double duration = 0, Easing easing = Easing.None )
            where T : XrObject
            => drawable.TransformTo( drawable.PopulateTransform( new PositionTransform( position ), default, duration, new DefaultEasingFunction( easing ) ) );

        public static TransformSequence<T> ScaleTo<T> ( this T drawable, Vector3 scale, double duration = 0, Easing easing = Easing.None )
            where T : XrObject
            => drawable.TransformTo( drawable.PopulateTransform( new ScaleTransform( scale ), default, duration, new DefaultEasingFunction( easing ) ) );
        public static TransformSequence<T> ScaleTo<T> ( this TransformSequence<T> seq, Vector3 scale, double duration = 0, Easing easing = Easing.None )
            where T : XrObject
            => seq.Append( o => o.ScaleTo(scale,duration,easing) );

        public static TransformSequence<T> RotateTo<T> ( this T drawable, Quaternion rotation, double duration = 0, Easing easing = Easing.None )
            where T : XrObject
            => drawable.TransformTo( drawable.PopulateTransform( new RotationTransform( rotation ), default, duration, new DefaultEasingFunction( easing ) ) );

        private class PositionTransform : Transform<Vector3, XrObject> {
            private readonly Vector3 target;

            public override string TargetMember => nameof( XrObject.Position );

            public PositionTransform ( Vector3 target ) {
                this.target = target;
            }

            private Vector3 positionAt ( double time ) {
                if ( time < StartTime ) return StartValue;
                if ( time >= EndTime ) return EndValue;

                return StartValue + Interpolation.ValueAt( time, 0f, 1f, StartTime, EndTime, Easing ) * ( EndValue - StartValue );
            }

            protected override void Apply ( XrObject d, double time ) => d.Position = positionAt( time );

            protected override void ReadIntoStartValue ( XrObject d ) {
                StartValue = d.Position;
                EndValue = target;
            }
        }

        private class ScaleTransform : Transform<Vector3, XrObject> {
            private readonly Vector3 target;

            public override string TargetMember => nameof( XrObject.Scale );

            public ScaleTransform ( Vector3 target ) {
                this.target = target;
            }

            private Vector3 scaleAt ( double time ) {
                if ( time < StartTime ) return StartValue;
                if ( time >= EndTime ) return EndValue;

                return StartValue + Interpolation.ValueAt( time, 0f, 1f, StartTime, EndTime, Easing ) * ( EndValue - StartValue );
            }

            protected override void Apply ( XrObject d, double time ) => d.Scale = scaleAt( time );

            protected override void ReadIntoStartValue ( XrObject d ) {
                StartValue = d.Scale;
                EndValue = target;
            }
        }

        private class RotationTransform : Transform<Quaternion, XrObject> {
            private readonly Quaternion target;

            public override string TargetMember => nameof( XrObject.Rotation );

            public RotationTransform ( Quaternion target ) {
                this.target = target;
            }

            private Quaternion rotationAt ( double time ) {
                if ( time < StartTime ) return StartValue;
                if ( time >= EndTime ) return EndValue;

                return Quaternion.Slerp( StartValue, EndValue, Interpolation.ValueAt( time, 0f, 1f, StartTime, EndTime, Easing ) );
            }

            protected override void Apply ( XrObject d, double time ) => d.Rotation = rotationAt( time );

            protected override void ReadIntoStartValue ( XrObject d ) {
                StartValue = d.Rotation;
                EndValue = target;
            }
        }
    }
}
