using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.VirtualReality;
using osu.XR.Configuration;
using osuTK.Graphics;

namespace osu.XR.Graphics.Player;

public partial class OsuXrPlayer : VrPlayer {
	Container3D invariantContainer = new();
	Foot footLeft;
	Foot footRight;

	BasicModel shadow;

	public readonly Bindable<FeetSymbols> FeetSymbols = new( Configuration.FeetSymbols.None );
	public OsuXrPlayer () {
		AddInternal( shadow = new BasicModel() );
		invariantContainer.Add( footRight = new Foot { Scale = new Vector3( -0.1f, 0.1f, 0.1f ), X = 0.1f, Z = 0.03f, Y = -0.001f, EulerY = 14 / 180f * MathF.PI } );
		invariantContainer.Add( footLeft = new Foot { Scale = new Vector3( 0.1f, 0.1f, 0.1f ), X = -0.1f, Y = -0.001f, EulerY = -10 / 180f * MathF.PI } );

		shadow.Mesh.AddCircle( Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, 32 );
		shadow.Mesh.CreateFullUnsafeUpload().Enqueue();
		shadow.Scale = new Vector3( 0.012f );
		footRight.Tint = footLeft.Tint = Color4.AliceBlue;
		footRight.Alpha = footLeft.Alpha = 0.2f;
		shadow.Tint = Color4.White;
		shadow.Alpha = 0.8f;
	}

	[Resolved]
	MeshStore meshes { get; set; } = null!;

	[BackgroundDependencyLoader(permitNulls: true)]
	private void load ( OsuXrConfigManager config ) {
		config?.BindWith( OsuXrSetting.ShadowType, FeetSymbols );
	}

	Dictionary<string, Mesh> meshCache = new();
	bool setMesh ( string name ) {
		if ( !meshCache.TryGetValue( name, out var mesh ) ) {
			meshes.GetAsync( name ).ContinueWith( r => { meshCache[name] = r.Result; Schedule( FeetSymbols.TriggerChange ); } );
			return false;
		}
		else {
			footLeft.Mesh = mesh;
			footRight.Mesh = mesh;
			return true;
		}
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		(Root as Container3D)!.Add( invariantContainer );

		FeetSymbols.BindValueChanged( v => {
			if ( v.NewValue == Configuration.FeetSymbols.Shoes ) {
				if ( !setMesh( "shoe" ) )
					return;

				footLeft.IsVisible = true;
				footRight.IsVisible = true;

				footLeft.Scale = new Vector3( -0.1f, 0.1f, 0.1f ) * 1.7f;
				footRight.Scale = new Vector3( 0.1f, 0.1f, 0.1f ) * 1.7f;
			}
			else if ( v.NewValue == Configuration.FeetSymbols.Paws ) {
				if ( !setMesh( "paw" ) )
					return;

				footLeft.IsVisible = true;
				footRight.IsVisible = true;

				footLeft.Scale = new Vector3( 0.1f, 0.1f, 0.1f );
				footRight.Scale = new Vector3( -0.1f, 0.1f, 0.1f );
			}
			else {
				footLeft.IsVisible = false;
				footRight.IsVisible = false;
			}
		}, true );
	}

	protected override void Update () {
		base.Update();

		shadow.Y = -Y;
		footLeft.TargetPosition.Value = new Vector3( X, -0.001f, Z ) + Left * 0.1f;
		footRight.TargetPosition.Value = new Vector3( X, -0.001f, Z ) + Right * 0.1f;

		footLeft.TargetRotation.Value = Quaternion.FromAxisAngle( Vector3.UnitY, ( -10 / 180f + 1 ) * MathF.PI ) * Rotation;
		footRight.TargetRotation.Value = Quaternion.FromAxisAngle( Vector3.UnitY, ( 14 / 180f + 1 ) * MathF.PI ) * Rotation;
	}
}
