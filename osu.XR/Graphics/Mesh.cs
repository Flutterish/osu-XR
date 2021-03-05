using NUnit.Framework.Constraints;
using osu.Framework.Bindables;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.XR.Maths;
using osu.XR.Maths;
using osuTK;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Valve.VR;

namespace osu.XR.Graphics {
	public class Mesh {
		public BindableList<Vector3> Vertices { get; } = new();
		public BindableList<Vector2> TextureCoordinates { get; } = new();
		public BindableList<IndexedFace> Tris { get; } = new();

		public readonly ReadonlyIndexer<int, Face> Faces;
		public ulong UpdateVersion { get; private set; } = 1;
		/// <summary>
		/// Whether this mesh is fully loaded and can be edited/used
		/// </summary>
		public bool IsReady = true;
		public Mesh () {
			Faces = new( index => {
				var indices = Tris[ index ];
				return new Face( Vertices[ (int)indices.A ], Vertices[ (int)indices.B ], Vertices[ (int)indices.C ] );
			} );
			Vertices.CollectionChanged += ( _, _ ) => UpdateVersion++;
			Tris.CollectionChanged += ( _, _ ) => UpdateVersion++;
			TextureCoordinates.CollectionChanged += ( _, _ ) => UpdateVersion++;
		}

		private AABox boundgingBox;
		private ulong boundingBoxUpdateVersion;
		/// <summary>
		/// A box bounding this mesh. This is cached.
		/// </summary>
		public AABox BoundingBox {
			get {
				if ( boundingBoxUpdateVersion == UpdateVersion || !IsReady ) return boundgingBox;
				boundingBoxUpdateVersion = UpdateVersion;
				if ( Vertices.Any() ) {
					boundgingBox = new AABox {
						Min = new Vector3(
							Vertices.Min( v => v.X ),
							Vertices.Min( v => v.Y ),
							Vertices.Min( v => v.Z )
						)
					};
					boundgingBox.Size = new Vector3(
						Vertices.Max( v => v.X ),
						Vertices.Max( v => v.Y ),
						Vertices.Max( v => v.Z )
					) - boundgingBox.Min;
				}
				else {
					boundgingBox = new AABox();
				}


				return boundgingBox;
			}
		}

		public static Mesh FromOBJFile ( string path )
			=> FromOBJ( File.ReadAllLines( path ) );
		public static Mesh FromOBJ ( string lines )
			=> FromOBJ( lines.Split( '\n' ) );
		public static Mesh FromOBJ ( IEnumerable<string> lines ) {
			// TODO Merge( IEnumerable<Mesh> ) so we dont repeat here
			return MultipleFromOBJ( lines ).FirstOrDefault() ?? new();
		}

		public static IEnumerable<Mesh> MultipleFromOBJFile ( string path )
			=> MultipleFromOBJ( File.ReadAllLines( path ) );

		public static IEnumerable<Mesh> MultipleFromOBJ ( string lines )
			=> MultipleFromOBJ( lines.Split( '\n' ) );

		public static IEnumerable<Mesh> MultipleFromOBJ ( IEnumerable<string> lines ) {
			Mesh current = new();
			uint indexOffset = 1;
			foreach ( var i in lines ) {
				var line = i.Trim();
				if ( line.StartsWith( "o " ) ) {
					if ( !current.IsEmpty ) {
						yield return current;
						indexOffset += (uint)current.Vertices.Count;
						current = new();
					}
				}
				else if ( line.StartsWith( "v " ) ) {
					var coords = line.Substring( 2 ).Split( " " ).Where( x => x.Length > 0 ).Select( x => float.Parse( x ) ).ToArray();
					current.Vertices.Add( new Vector3( coords[ 0 ], coords[ 1 ], coords[ 2 ] ) );
				}
				else if ( line.StartsWith( "f " ) ) {
					var info = line.Substring( 2 ).Split( " " ).Where( x => x.Length > 0 ).Select( x => x.Split( "/" ).Select( x => uint.Parse( x ) ).ToArray() ).ToArray();
					current.Tris.Add( new IndexedFace( info[ 0 ][ 0 ] - indexOffset, info[ 1 ][ 0 ] - indexOffset, info[ 2 ][ 0 ] - indexOffset ) );
				}
			}

			if ( !current.IsEmpty ) {
				yield return current;
				current = new();
			}
		}

		public bool IsEmpty => !Tris.Any();

		private void FillTextureCoordinates () {
			while ( TextureCoordinates.Count < Vertices.Count ) {
				TextureCoordinates.Add( Vector2.Zero );
			}
		}

		public int UploadToGPU ( int positionLocation, int uvLocation, int attributeBuffer, int elementBuffer, BufferUsageHint hint = BufferUsageHint.StaticDraw ) {
			if ( !IsReady ) {
				throw new InvalidOperationException( "This mesh is not avaialbe" );
			}

			FillTextureCoordinates();
			var vertices = new float[ Vertices.Count * 5 ];
			for ( int i = 0; i < Vertices.Count; i++ ) {
				vertices[ i * 5 ] = Vertices[ i ].X;
				vertices[ i * 5 + 1 ] = Vertices[ i ].Y;
				vertices[ i * 5 + 2 ] = Vertices[ i ].Z;
				vertices[ i * 5 + 3 ] = TextureCoordinates[ i ].X;
				vertices[ i * 5 + 4 ] = TextureCoordinates[ i ].Y;
			}
			var indices = new uint[ Tris.Count * 3 ];
			for ( int i = 0; i < Tris.Count; i++ ) {
				indices[ i * 3 ] = Tris[ i ].A;
				indices[ i * 3 + 1 ] = Tris[ i ].B;
				indices[ i * 3 + 2 ] = Tris[ i ].C;
			}

			GLWrapper.BindBuffer( osuTK.Graphics.ES30.BufferTarget.ArrayBuffer, attributeBuffer );
			GL.BufferData( BufferTarget.ArrayBuffer, vertices.Length * sizeof( float ), vertices, hint );
			GL.VertexAttribPointer( positionLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof( float ), 0 );
			GL.EnableVertexAttribArray( positionLocation );
			GL.VertexAttribPointer( uvLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof( float ), 3 * sizeof( float ) );
			GL.EnableVertexAttribArray( uvLocation );
			GLWrapper.BindBuffer( osuTK.Graphics.ES30.BufferTarget.ElementArrayBuffer, elementBuffer );
			GL.BufferData( BufferTarget.ElementArrayBuffer, indices.Length * sizeof( uint ), indices, hint );

			return indices.Length;
		}

		public void AddQuad ( Quad quad )
			=> AddQuad( quad, new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) );

		public void AddQuad ( Quad quad, Vector2 TL, Vector2 TR, Vector2 BL, Vector2 BR ) {
			FillTextureCoordinates();
			int offset = Vertices.Count;

			Vertices.Add( quad.TL );
			Vertices.Add( quad.TR );
			Vertices.Add( quad.BL );
			Vertices.Add( quad.BR );

			Tris.Add( new( (uint)offset, (uint)offset + 3, (uint)offset + 1 ) );
			Tris.Add( new( (uint)offset, (uint)offset + 3, (uint)offset + 2 ) );
			TextureCoordinates.Add( TL );
			TextureCoordinates.Add( TR );
			TextureCoordinates.Add( BL );
			TextureCoordinates.Add( BR );
		}

		public void AddCircle ( Vector3 origin, Vector3 normal, Vector3 direction, int segments ) {
			FillTextureCoordinates();
			uint offset = (uint)Vertices.Count;

			normal.Normalize();
			Vertices.Add( origin );
			Vertices.Add( origin + direction );
			for ( int i = 1; i < segments; i++ ) {
				var angle = ( (float)i / segments ) * MathF.PI * 2;
				Vertices.Add( origin + ( Quaternion.FromAxisAngle( normal, angle ) * new Vector4( direction, 1 ) ).Xyz );
				Tris.Add( new IndexedFace( offset, offset + (uint)i, offset + (uint)i + 1 ) );
			}
			Tris.Add( new IndexedFace( offset, (uint)(segments + offset), offset + 1 ) );
		}

		public static Mesh UnitCube => FromOBJ(
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

		public static Face operator * ( Matrix4x4 matrix, Face face ) {
			return new Face(
				( matrix * new Vector4( face.A, 1 ) ).Xyz,
				( matrix * new Vector4( face.B, 1 ) ).Xyz,
				( matrix * new Vector4( face.C, 1 ) ).Xyz
			);
		}
	}
}
