using osuTK;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace osu.XR.Components {
	public class Mesh {
		public List<Vector3> Vertices { get; } = new();
		public List<IndexedFace> Tris { get; } = new();

		public readonly ReadonlyIndexer<int, Face> Faces;
		public Mesh () {
			Faces = new( index => {
				var indices = Tris[ index ];
				return new Face( Vertices[ (int)indices.A ], Vertices[ (int)indices.B ], Vertices[ (int)indices.C ] );
			} );
		}

		public static Mesh FromOBJFile ( string path )
			=> FromOBJ( File.ReadAllLines( path ) );
		public static Mesh FromOBJ ( string lines )
			=> FromOBJ( lines.Split( '\n' ) );
		public static Mesh FromOBJ ( IEnumerable<string> lines ) {
			Mesh mesh = new();
			foreach ( var i in lines ) {
				var line = i.Trim();
				if ( line.StartsWith( "v " ) ) {
					var coords = line.Substring( 2 ).Split( " " ).Where( x => x.Length > 0 ).Select( x => float.Parse( x ) ).ToArray();
					mesh.Vertices.Add( new Vector3( coords[ 0 ], coords[ 1 ], coords[ 2 ] ) );
				}
				else if ( line.StartsWith( "f " ) ) {
					var info = line.Substring( 2 ).Split( " " ).Where( x => x.Length > 0 ).Select( x => x.Split( "/" ).Select( x => uint.Parse( x ) ).ToArray() ).ToArray();
					mesh.Tris.Add( new IndexedFace( info[ 0 ][ 0 ] - 1, info[ 1 ][ 0 ] - 1, info[ 2 ][ 0 ] - 1 ) );
				}
			}
			return mesh;
		}

		public int UploadToGPU ( int positionLocation, int vertexBuffer, int elementBuffer, BufferUsageHint hint = BufferUsageHint.StaticDraw ) {
			var vertices = new float[ Vertices.Count * 3 ];
			for ( int i = 0; i < Vertices.Count; i++ ) {
				vertices[ i * 3 ] = Vertices[ i ].X;
				vertices[ i * 3 + 1 ] = Vertices[ i ].Y;
				vertices[ i * 3 + 2 ] = Vertices[ i ].Z;
			}
			var indices = new uint[ Tris.Count * 3 ];
			for ( int i = 0; i < Tris.Count; i++ ) {
				indices[ i * 3 ] = Tris[ i ].A;
				indices[ i * 3 + 1 ] = Tris[ i ].B;
				indices[ i * 3 + 2 ] = Tris[ i ].C;
			}

			GL.BindBuffer( BufferTarget.ArrayBuffer, vertexBuffer );
			GL.BufferData( BufferTarget.ArrayBuffer, vertices.Length * sizeof( float ), vertices, hint );
			GL.VertexAttribPointer( positionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof( float ), 0 );
			GL.EnableVertexAttribArray( positionLocation );
			GL.BindBuffer( BufferTarget.ElementArrayBuffer, elementBuffer );
			GL.BufferData( BufferTarget.ElementArrayBuffer, indices.Length * sizeof( uint ), indices, hint );

			return indices.Length;
		}

		public static Mesh UnitCube => Mesh.FromOBJ(
			@"
			v  0.5  0.5  0.5
			v  0.5  0.5 -0.5
			v  0.5 -0.5  0.5
			v  0.5 -0.5 -0.5
			v -0.5  0.5  0.5
			v -0.5  0.5 -0.5
			v -0.5 -0.5  0.5
			v -0.5 -0.5 -0.5

			f 5 8 6
			f 5 8 7
			f 3 2 1
			f 3 2 4
			f 5 3 7
			f 5 3 1
			f 6 4 2
			f 6 4 8
			f 5 2 6
			f 5 2 1
			f 8 3 7
			f 8 3 4
			"
		);
	}

	public struct IndexedFace {
		public uint A;
		public uint B;
		public uint C;

		public IndexedFace ( uint a, uint b, uint c ) {
			A = a;
			B = b;
			C = c;
		}
	}

	public struct Face {
		public Vector3 A;
		public Vector3 B;
		public Vector3 C;

		public Face ( Vector3 a, Vector3 b, Vector3 c ) {
			A = a;
			B = b;
			C = c;
		}
	}
}
