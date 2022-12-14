using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics;
using osu.Framework.XR;
using osu.XR.Timing;
using osuTK.Graphics;
using osu.XR.Graphics.Meshes;

namespace osu.XR.Graphics.Sceneries;

public partial class LightsOutScenery : Scenery {
	Bindable<float> velocity = new( 0 );
	Container3D clockwise;
	Container3D counterClockwise;

	public LightsOutScenery () {
		AddInternal( clockwise = new Container3D() );
		AddInternal( counterClockwise = new Container3D() );

		bindableBeat.ValueChanged += v => {
			this.TransformBindableTo( velocity, (float)v.NewValue.AverageAmplitude * (float)v.NewValue.TimingPoint.BPM / 30, 100 );
			foreach ( var child in clockwise.Children.Concat( counterClockwise.Children ) ) {
				child.FinishTransforms();
				var tint = (Color4)child.Colour.TopLeft;
				var flash = Interpolation.ValueAt( 0.4f, tint, Color4.White, 0, 1 );
				child.FadeColour( flash ).Then().FadeColour( tint, v.NewValue.TimingPoint.BeatLength * 2 / 3, Easing.Out );
			}
		};
	}

	void recalculateChildren () {
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

				var child = new BasicModel();
				( clockwise ? this.clockwise : counterClockwise ).Add( child );
				child.Mesh.AddArcedPlane( Vector3.UnitY, Quaternion.FromAxisAngle( Vector3.UnitY, theta ).Apply( Vector3.UnitZ ), height, radius, delta, origin: new Vector3( 0, y, 0 ) );
				child.Mesh.CreateFullUnsafeUpload().Enqueue();
				child.OnLoadComplete += _ => child.Material.Set( "useGamma", true );
				child.Tint = NeonColours.NextRandom( random );

				theta += delta;
			}

			clockwise = !clockwise;
			y += height / 2;
		}

		for ( float r = radius; r > 0.9f; r -= random.NextSingle( 0.3f, 0.7f ) * radius / 5 ) {
			var width = random.NextSingle( 0.2f, 0.4f );

			for ( float theta = random.NextSingle( 0, 1.2f ); theta < MathF.Tau - 0.2f; theta += random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ) ) {
				var delta = Math.Min( random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ), MathF.Tau - theta - 0.1f );

				var child = new BasicModel();
				( clockwise ? this.clockwise : counterClockwise ).Add( child );
				child.Mesh.AddCircularArc( Vector3.UnitY, Quaternion.FromAxisAngle( Vector3.UnitY, theta ).Apply( Vector3.UnitZ ), delta, r - width, r, origin: new Vector3( 0, 0.001f, 0 ) );
				child.Mesh.CreateFullUnsafeUpload().Enqueue();
				child.OnLoadComplete += _ => child.Material.Set( "useGamma", true );
				child.Tint = NeonColours.NextRandom( random );

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

	Bindable<Beat> bindableBeat = new();

	[BackgroundDependencyLoader]
	void load ( BeatSyncSource beat ) {
		bindableBeat.BindTo( beat.BindableBeat );
		recalculateChildren();
	}
}
