using Microsoft.EntityFrameworkCore.Internal;
using NUnit.Framework;
using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Drawables;
using osu.XR.Maths;
using osu.XR.Physics;
using osu.XR.Rendering;
using osu.XR.Settings;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using static osu.XR.Components.XrObject.XrObjectDrawNode;
using static osu.XR.Physics.Raycast;

namespace osu.XR.Components {
	/// <summary>
	/// A 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public abstract class Panel : MeshedXrObject, IHasCollider, IReactsToController {
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
                    EmulatedInput.ReleaseAllTouch();
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

            SourceCapture.Add( EmulatedInput );
            Add( SourceCapture );
        }

        [BackgroundDependencyLoader]
        private void load ( XrConfigManager config ) {
            config.BindWith( XrConfigSetting.Deadzone, deadzoneBindable );
        }
        BindableInt deadzoneBindable = new( 20 );

        bool inDeadzone = false;
        Vector2 deadzoneCenter;
        Vector2 pointerPosition;
        
        void handleButton ( bool isLeft, bool isDown ) {
            if ( VR.EnabledControllerCount > 1 ) {
                if ( RequestedInputMode == PanelInputMode.Regular == isLeft ) EmulatedInput.IsLeftPressed = isDown;
                else if ( RequestedInputMode == PanelInputMode.Inverted == isLeft ) EmulatedInput.IsRightPressed = isDown;
            }
            else EmulatedInput.IsLeftPressed = isDown;
        
            if ( isDown ) {
                inDeadzone = true;
                deadzoneCenter = pointerPosition;
            }
            else inDeadzone = false;
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
		}

		protected override void Update () {
			base.Update();
            if ( IsMeshInvalidated ) {
                RecalculateMesh();
            }
        }

		public virtual bool IsColliderEnabled => IsVisible;

        List<XrController> focusedControllers = new();
        IEnumerable<Controller> focusedControllerSources => focusedControllers.Select( x => x.Source );
        Dictionary<XrController, System.Action> eventUnsubs = new();
		public void OnControllerFocusGained ( XrController controller ) {
            System.Action<ValueChangedEvent<Vector2>> onScroll = v => { EmulatedInput.Scroll += v.NewValue - v.OldValue; };
            System.Action<ValueChangedEvent<bool>> onLeft = v => { handleButton( isLeft: true, isDown: v.NewValue ); };
            System.Action<ValueChangedEvent<bool>> onRight = v => { handleButton( isLeft: false, isDown: v.NewValue ); };
            System.Action<RaycastHit> onMove = hit => { onPointerMove( controller, hit ); };
            System.Action<RaycastHit> onDown = hit => { onTouchDown( controller, hit ); };
            System.Action onUp = () => { onTouchUp( controller ); };
            controller.PointerMove += onMove;
            controller.PointerDown += onDown;
            controller.PointerUp += onUp;
            controller.ScrollBindable.ValueChanged += onScroll;
            controller.LeftButtonBindable.ValueChanged += onLeft;
            controller.RightButtonBindable.ValueChanged += onRight;
            eventUnsubs.Add( controller, () => {
                controller.PointerMove -= onMove;
                controller.PointerDown -= onDown;
                controller.PointerUp -= onUp;
                controller.ScrollBindable.ValueChanged -= onScroll;
                controller.LeftButtonBindable.ValueChanged -= onLeft;
                controller.RightButtonBindable.ValueChanged -= onRight;
            } );
            focusedControllers.Add( controller );

            updateFocus();
        }
        public void OnControllerFocusLost ( XrController controller ) {
            eventUnsubs[ controller ].Invoke();
            eventUnsubs.Remove( controller );
            focusedControllers.Remove( controller );
            
            updateFocus();
        }
        void updateFocus () {
            HasFocus = focusedControllers.Any();
        }

        private void onPointerMove ( XrController controller, Raycast.RaycastHit hit ) {
            var position = TexturePositionAt( hit.TrisIndex, hit.Point );
            if ( controller.EmulatesTouch ) {
                EmulatedInput.TouchMove( controller, position );
            }
            else {
                pointerPosition = position;
                if ( ( pointerPosition - deadzoneCenter ).Length > deadzoneBindable.Value ) inDeadzone = false;
                if ( !inDeadzone ) EmulatedInput.mouseHandler.handleMouseMove( position );
            }
        }

        private void onTouchDown ( XrController controller, Raycast.RaycastHit hit ) {
            var position = TexturePositionAt( hit.TrisIndex, hit.Point );
            EmulatedInput.TouchDown( controller, position );
        }

        private void onTouchUp ( XrController controller ) {
            EmulatedInput.TouchUp( controller );
		}
    }

    public enum PanelInputMode {
        Regular,
        Inverted
	}
}
