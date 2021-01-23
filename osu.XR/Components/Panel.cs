using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
	/// A 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public abstract class Panel : MeshedXrObject, IHasCollider {
        public PanelInputMode RequestedInputMode { get; protected set; } = PanelInputMode.Regular;
        public readonly XrInputManager EmulatedInput = new XrInputManager { RelativeSizeAxes = Axes.Both };
        public Container Source => EmulatedInput;
        /// <summary>
        /// Non-stretching scaling applied to the content
        /// </summary>
        public Bindable<Vector2> ContentScale = new( Vector2.One );
        public BufferedCapture SourceCapture { get; } = new BufferedCapture { RelativeSizeAxes = Axes.Both };

        protected bool IsMeshInvalidated = true;

        private bool hasFocus;
        new public bool HasFocus {
            get => hasFocus;
            set {
                if ( hasFocus == value ) return;
                hasFocus = value;
                if ( !hasFocus ) {
                    EmulatedInput.IsLeftPressed = false;
                    EmulatedInput.IsRightPressed = false;
				}
			}
		}

        public Panel AutosizeX () {
            EmulatedInput.RelativeSizeAxes = Axes.Y;
            SourceCapture.RelativeSizeAxes = Axes.Y;

            EmulatedInput.AutoSizeAxes = Axes.X;
            SourceCapture.AutoSizeAxes = Axes.X;

            return this;
        }
        public Panel AutosizeY () {
            EmulatedInput.RelativeSizeAxes = Axes.X;
            SourceCapture.RelativeSizeAxes = Axes.X;

            EmulatedInput.AutoSizeAxes = Axes.Y;
            SourceCapture.AutoSizeAxes = Axes.Y;

            return this;
        }
        public Panel AutosizeBoth () {
            EmulatedInput.RelativeSizeAxes = Axes.None;
            SourceCapture.RelativeSizeAxes = Axes.None;

            EmulatedInput.AutoSizeAxes = Axes.Both;
            SourceCapture.AutoSizeAxes = Axes.Both;

            return this;
        }
        public Panel () {
            UseGammaCorrection = true;

            ContentScale.ValueChanged += v => {
                SourceCapture.Size = v.NewValue;
                IsMeshInvalidated = true;
            };

            EmulatedInput.InputPanel = this;
            SourceCapture.Add( EmulatedInput );
            Add( SourceCapture );
        }

        protected abstract void RecalculateMesh ();

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
                IsMeshInvalidated = true;
                lastTextureSize = MainTexture.Size;
			}
            if ( IsMeshInvalidated ) {
                RecalculateMesh();
			}
		}

        public bool IsColliderEnabled => IsVisible;
	}

    public enum PanelInputMode {
        Regular,
        Inverted
	}
}
