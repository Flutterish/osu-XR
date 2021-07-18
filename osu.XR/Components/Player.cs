using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.XR.Settings;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class Player : XrPlayer { // TODO move controllers here too
		private Container3D invariantContainer = new();
		private Model shadow;
		private Foot footLeft;
		private Foot footRight;

		private Mesh paw;
		private Mesh shoe;

		public readonly Bindable<FeetSymbols> FeetSymbols = new( Components.FeetSymbols.None );
		public Player () {
			Add( shadow = new Model() );
			invariantContainer.Add( footRight = new Foot { Scale = new Vector3( -0.1f, 0.1f, 0.1f ), X = 0.1f, Z = 0.03f, EulerRotY = 14 / 180f * MathF.PI } );
			invariantContainer.Add( footLeft = new Foot { Scale = new Vector3( 0.1f, 0.1f, 0.1f ), X = -0.1f, EulerRotY = -10 / 180f * MathF.PI } );

			shadow.Mesh.AddCircle( Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, 32 );
			shadow.Scale = new Vector3( 0.2f );
			shadow.Tint = footRight.Tint = footLeft.Tint = Color4.Black;
			shadow.Alpha = footRight.Alpha = footLeft.Alpha = 0.1f;
		}

		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config ) {
			config.BindWith( XrConfigSetting.ShadowType, FeetSymbols );
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			Root.Add( invariantContainer );

			FeetSymbols.BindValueChanged( v => {
				footLeft.IsVisible = true;
				footRight.IsVisible = true;
				shadow.Scale = new Vector3( 0.012f );
				shadow.Alpha = 0.8f;
				shadow.Tint = Color4.White;
				if ( v.NewValue == Components.FeetSymbols.Shoes ) {
					if ( shoe is null ) shoe = Mesh.FromOBJFile( @".\Resources\shoe.obj" );
					footLeft.Mesh = shoe;
					footRight.Mesh = shoe;

					footLeft.Scale = new Vector3( 0.1f, 0.1f, 0.1f ) * 1.7f;
					footRight.Scale = new Vector3( -0.1f, 0.1f, 0.1f ) * 1.7f;
				}
				else if ( v.NewValue == Components.FeetSymbols.Paws ) {
					if ( paw is null ) paw = Mesh.FromOBJFile( @".\Resources\paw.obj" );
					footLeft.Mesh = paw;
					footRight.Mesh = paw;

					footLeft.Scale = new Vector3( -0.1f, 0.1f, 0.1f );
					footRight.Scale = new Vector3( 0.1f, 0.1f, 0.1f );
				}
				else {
					footLeft.IsVisible = false;
					footRight.IsVisible = false;
					shadow.Scale = new Vector3( 0.2f );
					shadow.Alpha = 0.1f;
					shadow.Tint = Color4.Black;
				}
			}, true );
		}

		protected override void Update () {
			base.Update();

			shadow.Y = -Y;
			footLeft.TargetPosition.Value = new Vector3( X, 0, Z ) + Left * 0.1f;
			footRight.TargetPosition.Value = new Vector3( X, 0, Z ) + Right * 0.1f;
		}
	}

	public enum FeetSymbols {
		None,
		Shoes,
		Paws
	}
}
