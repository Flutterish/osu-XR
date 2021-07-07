using osu.Framework.Allocation;
using osu.Framework.XR.Components;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class Player : CompositeDrawable3D {
		[Resolved]
		private OsuGameXr game { get; set; }
		private Model footprints;

		public Player () {
			Add( footprints = new Model() );
			footprints.Mesh.AddCircle( Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, 32 );
			footprints.Scale = new Vector3( 0.2f );
			footprints.Tint = Color4.Black;
			footprints.Alpha = 0.1f;
		}

		protected override void Update () {
			base.Update();

			Position = game.PlayerPosition.Value;
			footprints.Y = -game.PlayerPosition.Value.Y;
		}
	}
}
