using osu.XR.Graphics;
using osu.XR.Maths;
using osuTK;
using osuTK.Graphics;

namespace osu.XR.Components {
	/// <summary>
	/// White line grid on the floor with a fade.
	/// </summary>
	public class FloorGrid : MeshedXrObject {
		public FloorGrid () {
            MainTexture = Textures.Vertical2SidedGradient( Color4.Transparent, Color4.White, 200 ).TextureGL;

            const int x_segments = 7;
            const float x_length = 16.7f;
            const float x_spread = 1;
            const float x_width = 0.01f;

            const int z_segments = 7;
            const float z_length = 16.7f;
            const float z_spread = 1;
            const float z_width = 0.01f;

            for ( int x = -x_segments; x <= x_segments; x++ ) {
                float xFrom = x * x_spread - x_width / 2;
                float xTo = x * x_spread + x_width / 2;
                float zFrom = x_length * -0.5f;
                float zTo = x_length * 0.5f;
                Mesh.AddQuad( new Quad(
                    new Vector3( xFrom, 0, zFrom ), new Vector3( xFrom, 0, zTo ),
                    new Vector3( xTo, 0, zFrom ), new Vector3( xTo, 0, zTo )
                ), new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 0, 1 ) );
            }

            for ( int z = -z_segments; z <= z_segments; z++ ) {
                float xFrom = z_length * -0.5f;
                float xTo = z_length * 0.5f;
                float zFrom = z * z_spread - z_width / 2;
                float zTo = z * z_spread + z_width / 2;
                Mesh.AddQuad( new Quad(
                    new Vector3( xFrom, 0, zFrom ), new Vector3( xFrom, 0, zTo ),
                    new Vector3( xTo, 0, zFrom ), new Vector3( xTo, 0, zTo )
                ), new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) );
            }
        }
	}
}
