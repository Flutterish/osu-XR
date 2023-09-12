using osu.Framework.Localisation;
using osu.Framework.XR;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.XR.Graphics.Settings;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class BeatingCubes : CompositeDrawable3D, IConfigurableSceneryComponent {
	LocalisableString ISceneryComponent.Name => @"Beat Cubes";
	public SceneryComponentSettingsSection CreateSettings () => new BeatingCubesSettingsSection( this );

	public readonly Bindable<string> Seed = new( "" );
	public readonly Bindable<Colour4> TintBindable = new( Colour4.White );

	public BeatingCubes () {
		Seed.BindValueChanged( _ => regenrate(), true );
		TintBindable.BindValueChanged( v => {
			foreach ( BeatingGroup i in InternalChildren ) {
				i.Child.Colour = v.NewValue;
			}
		}, true );
	}

	void regenrate () {
		ClearInternal( disposeChildren: true );

		Random random = new( string.IsNullOrWhiteSpace( Seed.Value ) ? 0 : Seed.Value.GetHashCode() );

		double next () => random.NextDouble( 0.02, 0.1 );
		for ( double theta = next(); theta < Math.PI * 2; theta += next() ) {
			if ( random.NextSingle() <= 0.2f ) {
				double radius = random.NextDouble( 4, 7 );
				double x = Math.Cos( theta ) * radius;
				double y = Math.Sin( theta ) * radius;

				AddInternal( new BeatingGroup {
					Position = new Vector3( (float)x, 0, (float)y ),
					Child = new BasicModel {
						Mesh = BasicMesh.UnitCube,
						IsColliderEnabled = true,
						OriginY = -1f,
						Scale = new Vector3( (float)(random.NextDouble( 0.05, 0.2 ) * radius) ),
						Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, (float)random.NextDouble( 0, Math.PI * 2 ) )
					}
				} );
			}
		}
	}
}

public partial class BeatingCubesSettingsSection : SceneryComponentSettingsSection {
	public BeatingCubesSettingsSection ( BeatingCubes source ) : base( source ) {
		Add( new SettingsColourPicker {
			LabelText = @"Tint",
			Current = source.TintBindable
		} );
		Add( new SettingsCommitTextBox {
			LabelText = @"Seed",
			Current = source.Seed
		} );
	}
}