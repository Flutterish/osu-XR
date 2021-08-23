using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Maths;
using osu.XR.Drawables;
using osu.XR.Graphics;
using osuTK;
using osuTK.Graphics;
using System;
using System.Linq;

namespace osu.XR.Components.Skyboxes {
	/// <summary>
	/// A solid black skybox with neon rectangles orbitting the camera to the beat
	/// </summary>
	public class LightsOutSkyBox : CompositeDrawable3D {
		private Bindable<float> velocity = new( 0 );
		private Container3D clockwise;
		private Container3D counterClockwise;

		public LightsOutSkyBox () {
			AddInternal( clockwise = new Container3D() );
			AddInternal( counterClockwise = new Container3D() );

			recalculateChildren();
			BindableBeat.ValueChanged += v => {
				this.TransformBindableTo( velocity, (float)v.NewValue.AverageAmplitude * (float)v.NewValue.TimingPoint.BPM / 30, 100 );
				foreach ( Model child in clockwise.Concat( counterClockwise ) ) {
					child.FlashColour( Interpolation.ValueAt( 0.4f, child.Tint, Color4.White, 0, 1 ), v.NewValue.TimingPoint.BeatLength * 2 / 3, Easing.Out );
				}
			};
		}

		private void recalculateChildren () {
			this.clockwise.Clear();
			counterClockwise.Clear();
			bool clockwise = true;

			Random random = new Random();
			var radius = 10f;
			for ( float y = 0; y < 4; y += random.NextSingle( 0.1f, 0.3f ) ) {
				var height = random.NextSingle( 0.3f, 0.6f );
				y += height / 2;

				for ( float theta = random.NextSingle( 0, 1.2f ); theta < MathF.Tau - 0.2f; theta += random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ) ) {
					var delta = Math.Min( random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ), MathF.Tau - theta - 0.1f );

					var child = new Model();
					( clockwise ? this.clockwise : counterClockwise ).Add( child );
					child.Mesh.AddArcedPlane( Vector3.UnitY, Quaternion.FromAxisAngle( Vector3.UnitY, theta ).Apply( Vector3.UnitZ ), height, radius, delta, origin: new Vector3( 0, y, 0 ) );
					child.UseGammaCorrection = true;
					child.Tint = NeonColors.NextRandom( random );

					theta += delta;
				}

				clockwise = !clockwise;
				y += height / 2;
			}

			for ( float r = radius; r > 0.9f; r -= random.NextSingle( 0.3f, 0.7f ) * radius / 5 ) {
				var width = random.NextSingle( 0.2f, 0.4f );

				for ( float theta = random.NextSingle( 0, 1.2f ); theta < MathF.Tau - 0.2f; theta += random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ) ) {
					var delta = Math.Min( random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ), MathF.Tau - theta - 0.1f );

					var child = new Model();
					( clockwise ? this.clockwise : counterClockwise ).Add( child );
					child.Mesh.AddCircularArc( Vector3.UnitY, Quaternion.FromAxisAngle( Vector3.UnitY, theta ).Apply( Vector3.UnitZ ), delta, r - width, r, origin: new Vector3( 0, 0.001f, 0 ) );
					child.UseGammaCorrection = true;
					child.Tint = NeonColors.NextRandom( random );

					theta += delta;
				}

				clockwise = !clockwise;
				r -= width;
			}
		}

		protected override void Update () {
			base.Update();
			clockwise.Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, velocity.Value * 0.1f * (float)Time.Elapsed / 1000 ) * clockwise.Rotation;
			counterClockwise.Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, velocity.Value * -0.1f * (float)Time.Elapsed / 1000 ) * counterClockwise.Rotation;
		}

		public readonly Bindable<Beat> BindableBeat = new();
		[BackgroundDependencyLoader]
		private void load ( BeatProvider beat ) {
			BindableBeat.BindTo( beat.BindableBeat );
		}

		//private class CameraTrackingModel : Model {
		//	protected override DrawNode3D CreateDrawNode ()
		//		=> new CameraPositionTrackingDrawNode( this );
		//}
	}
}
