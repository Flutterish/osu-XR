using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Framework.XR;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Containers;
using osu.XR.Graphics.Settings;
using osu.XR.Timing;
using osuTK.Graphics;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class RaveCylinder : CompositeDrawable3D, IConfigurableSceneryComponent {
	LocalisableString ISceneryComponent.Name => @"Neon Lights";
	public SceneryComponentSettingsSection CreateSettings () => new RaveCylinderSettingsSection( this );

	Bindable<float> velocity = new( 0 );
	Container3D clockwise;
	Container3D counterClockwise;

	public RaveCylinder () {
		AddInternal( clockwise = new Container3D() );
		AddInternal( counterClockwise = new Container3D() );
	}

	public readonly Bindable<string> Seed = new( "" );

	protected override void LoadComplete () {
		base.LoadComplete();

		bindableBeat.ValueChanged += v => {
			this.TransformBindableTo( velocity, (float)v.NewValue.AverageAmplitude * (float)v.NewValue.TimingPoint.BPM / 30, 100 );
			foreach ( var child in clockwise.Children.Concat( counterClockwise.Children ) ) {
				var tint = tints[child];
				var flash = Interpolation.ValueAt( 0.4f, tint, Color4.White, 0, 1 );
				child.FadeColour( flash ).FadeColour( tint, v.NewValue.TimingPoint.BeatLength * 2 / 3, Easing.Out );
			}
		};
	}

	Dictionary<Drawable3D, Color4> tints = new();
	void recalculateChildren () {
		this.clockwise.Clear();
		counterClockwise.Clear();
		tints.Clear();
		bool clockwise = true;

		Random random = new( string.IsNullOrWhiteSpace( Seed.Value ) ? 0 : Seed.Value.GetHashCode() );
		var radius = 10f;
		for ( float y = 0; y < 4; y += random.NextSingle( 0.1f, 0.3f ) ) {
			var height = random.NextSingle( 0.3f, 0.6f );
			y += height / 2;

			for ( float theta = random.NextSingle( 0, 1.2f ); theta < MathF.Tau - 0.2f; theta += random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ) ) {
				var delta = Math.Min( random.NextSingle( 0.1f, random.NextSingle( 0.1f, 1.2f ) ), MathF.Tau - theta - 0.1f );

				var child = new BasicModel();
				(clockwise ? this.clockwise : counterClockwise).Add( child );
				child.Mesh.AddArcedPlane( Vector3.UnitY, Quaternion.FromAxisAngle( Vector3.UnitY, theta ).Apply( Vector3.UnitZ ), height, radius, delta, origin: new Vector3( 0, y, 0 ) );
				child.Mesh.CreateFullUnsafeUpload().Enqueue();
				child.OnLoadComplete += _ => child.Material.Set( "useGamma", true );
				child.Tint = NeonColours.NextRandom( random );
				tints.Add( child, child.Tint );

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
				(clockwise ? this.clockwise : counterClockwise).Add( child );
				child.Mesh.AddCircularArc( Vector3.UnitY, Quaternion.FromAxisAngle( Vector3.UnitY, theta ).Apply( Vector3.UnitZ ), delta, r - width, r, origin: new Vector3( 0, 0.001f, 0 ) );
				child.Mesh.CreateFullUnsafeUpload().Enqueue();
				child.OnLoadComplete += _ => child.Material.Set( "useGamma", true );
				child.Tint = NeonColours.NextRandom( random );
				tints.Add( child, child.Tint );

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
		Seed.BindValueChanged( _ => recalculateChildren(), true );
	}
}

public partial class RaveCylinderSettingsSection : SceneryComponentSettingsSection {
	public RaveCylinderSettingsSection ( RaveCylinder source ) : base( source ) {
		Add( new SettingsCommitTextBox {
			LabelText = @"Seed",
			Current = source.Seed
		} );
	}
}