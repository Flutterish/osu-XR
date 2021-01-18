﻿using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Maths;
using osu.XR.Physics;
using osu.XR.Rendering;
using osuTK;
using System;
using static osu.XR.Components.XrObject.XrObjectDrawNode;

namespace osu.XR.Components {
	/// <summary>
	/// A curved 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public class Panel : MeshedXrObject, IHasCollider {
        public readonly XrInputManager EmulatedInput = new XrInputManager { RelativeSizeAxes = Axes.Both };
        public Container Source => EmulatedInput;
        /// <summary>
        /// Non-stretching scaling applied to the content
        /// </summary>
        public Bindable<Vector2> ContentScale = new( Vector2.One );
        public BufferedCapture SourceCapture { get; } = new BufferedCapture { RelativeSizeAxes = Axes.Both };
        public double Arc { get => ArcBindable.Value; set => ArcBindable.Value = value; }
        public double Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
        public readonly BindableDouble ArcBindable = new( MathF.PI * 0.8f ) { MinValue = MathF.PI / 18, MaxValue = MathF.PI * 2 };
        public readonly BindableDouble RadiusBindable = new( 3 ) { MinValue = 0.1f, MaxValue = 100 };

        private bool isCurveInvalidated = true;
        public Panel () {
            UseGammaCorrection = true;
            ArcBindable.ValueChanged += _ => isCurveInvalidated = true;
            RadiusBindable.ValueChanged += _ => isCurveInvalidated = true;

            // size has to be less than the actual screen because clipping shenigans
            // TODO figure out how to render in any resolution without downgrading quality. might also just modify o!f to not clip.
            ContentScale.ValueChanged += v => {
                if ( v.NewValue.X / v.NewValue.Y > 1 ) {
                    var xScale = v.NewValue.X / v.NewValue.Y;
                    SourceCapture.Size = new Vector2( 1, 1 / xScale );
                    SourceCapture.FrameBufferScale = new Vector2( xScale );
                }
                else {
                    var yScale = v.NewValue.Y / v.NewValue.X;
                    SourceCapture.Size = new Vector2( 1 / yScale, 1 );
                    SourceCapture.FrameBufferScale = new Vector2( yScale );
                }
                isCurveInvalidated = true;
            };

            EmulatedInput.InputPanel = this;
            SourceCapture.Add( EmulatedInput );
            Add( SourceCapture );
        }

		private void recalculateMesh () {
            isCurveInvalidated = false;
            Mesh = new();

            var arc = (float)Arc;
            var radius = (float)Radius;

            var points = 100;
            var arclength = arc * radius;
            var height = arclength / ( (float)MainTexture.Width / MainTexture.Height );
            for ( var i = 0; i < points; i++ ) {
                var start = arc / points * i - arc / 2;
                var end = arc / points * ( i + 1 ) - arc / 2;

                var posA = new Vector2( MathF.Sin( end ), MathF.Cos( end ) ) * radius;
                var posB = new Vector2( MathF.Sin( start ), MathF.Cos( start ) ) * radius;

                Mesh.AddQuad( new Maths.Quad(
                    new Vector3( posB.X, height / 2, posB.Y ), new Vector3( posA.X, height / 2, posA.Y ),
                    new Vector3( posB.X, -height / 2, posB.Y ), new Vector3( posA.X, -height / 2, posA.Y )
                ), new Vector2( (float)i / points, 1 ), new Vector2( (float)(i+1) / points, 1 ), new Vector2( (float)i / points, 0 ), new Vector2( (float)(i+1) / points, 0 ) );
            }
        }

        /// <summary>
        /// The texture position from top left.
        /// </summary>
        public Vector2 TexturePositionAt ( int trisIndex, Vector3 position ) {
            var face = Faces[ trisIndex ];
            var barycentric = Triangles.Barycentric( face, position );
            var tris = Mesh.Tris[ trisIndex ];
            var textureCoord =
                  Mesh.TextureCoordinates[ (int)tris.A ] * barycentric.X
                + Mesh.TextureCoordinates[ (int)tris.B ] * barycentric.Y
                + Mesh.TextureCoordinates[ (int)tris.C ] * barycentric.Z;
            return new Vector2( MainTexture.Width * textureCoord.X, MainTexture.Height * ( 1 - textureCoord.Y ) );
        }

        private Vector2 lastTextureSize;
		public override void BeforeDraw ( DrawSettings settings ) {
            if ( SourceCapture is null ) return;
            if ( SourceCapture.Capture is null ) return;
            MainTexture = SourceCapture.Capture;
            if ( MainTexture.Size != lastTextureSize ) {
                isCurveInvalidated = true;
                lastTextureSize = MainTexture.Size;
			}
            if ( isCurveInvalidated ) {
                recalculateMesh();
			}
		}
	}
}
