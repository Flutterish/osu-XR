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
using static osu.XR.Components.XrObject.XrObjectDrawNode;

namespace osu.XR.Components {
	/// <summary>
	/// A 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public abstract class Panel : MeshedXrObject, IHasCollider, IReactsToControllerPointer {
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

        public bool AcceptsInputFrom ( Controller controller )
            => focusedControllerSources.Contains( controller ) || ( !useTouch && inputModeBindable.Value == InputMode.SinglePointer && focusedControllerSources.Contains( VR.MainController ) );

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
            config.BindWith( XrConfigSetting.InputMode, inputModeBindable );
            config.BindWith( XrConfigSetting.SinglePointerTouch, singlePointerTouchBindable );
        }
        Bindable<InputMode> inputModeBindable = new();
        Bindable<bool> singlePointerTouchBindable = new();

        protected override void LoadComplete () {
			base.LoadComplete();

            VR.BindComponentsLoaded( () => {
                var scroll = VR.GetControllerComponent<Controller2DVector>( XrAction.Scroll );
                scroll.BindValueUpdatedDetailed( v => {
                    if ( !AcceptsInputFrom( v.Source ) ) return;

                    EmulatedInput.Scroll += new Vector2( v.NewValue.X, v.NewValue.Y ) * (float)VR.DeltaTime * 80;
                } );

                var mouseLeft = VR.GetControllerComponent<ControllerButton>( XrAction.MouseLeft );
                mouseLeft.BindValueChangedDetailed( v => {
                    if ( !AcceptsInputFrom( v.Source ) ) return;

                    if ( useTouch ) {
                        if ( !touchSources.TryGetValue( v.Source, out var touch ) ) {
                            touchSources.Add( v.Source, touch = new TouchSource { Source = v.Source } );
                        }
                        if ( v.NewValue ) {
                            touch.ActionCount++;
                            if ( touch.ActionCount == 1 ) EmulatedInput.TouchDown( touch, touch.Position );
                        }
                        else {
                            if ( touch.ActionCount == 1 ) EmulatedInput.TouchUp( touch, touch.Position );
                            touch.ActionCount = Math.Max( 0, touch.ActionCount - 1 );
                        }
                    }
                    else {
                        if ( VR.EnabledControllerCount > 1 ) {
                            if ( RequestedInputMode == PanelInputMode.Regular ) EmulatedInput.IsLeftPressed = v.NewValue;
                            else if ( RequestedInputMode == PanelInputMode.Inverted ) EmulatedInput.IsRightPressed = v.NewValue;
                        }
                        else EmulatedInput.IsLeftPressed = v.NewValue;
                    }
                } );

                var mouseRight = VR.GetControllerComponent<ControllerButton>( XrAction.MouseRight );
                mouseRight.BindValueChangedDetailed( v => {
                    if ( !AcceptsInputFrom( v.Source ) ) return;

                    if ( useTouch ) {
                        if ( !touchSources.TryGetValue( v.Source, out var touch ) ) {
                            touchSources.Add( v.Source, touch = new TouchSource { Source = v.Source } );
                        }
                        if ( v.NewValue ) {
                            touch.ActionCount++;
                            if ( touch.ActionCount == 1 ) EmulatedInput.TouchDown( touch, touch.Position );
                        }
                        else {
                            if ( touch.ActionCount == 1 ) EmulatedInput.TouchUp( touch, touch.Position );
                            touch.ActionCount = Math.Max( 0, touch.ActionCount - 1 );
                        }
                    }
                    else {
                        if ( VR.EnabledControllerCount > 1 ) {
                            if ( RequestedInputMode == PanelInputMode.Regular ) EmulatedInput.IsRightPressed = v.NewValue;
                            else if ( RequestedInputMode == PanelInputMode.Inverted ) EmulatedInput.IsLeftPressed = v.NewValue;
                        }
                        else EmulatedInput.IsLeftPressed = v.NewValue;
                    }
                } );
            } );
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
        Dictionary<XrController, Pointer.PointerUpdate> pointerEvents = new();
		public void OnPointerFocusGained ( XrController controller ) {
            focusedControllers.Add( controller );
            Pointer.PointerUpdate @event = ( Raycast.RaycastHit hit ) => {
                onPointerUpdate( controller, hit );
            };
            pointerEvents.Add( controller, @event );
            controller.Pointer.NewHit += @event;

            updateFocus();
		}
        public void OnPointerFocusLost ( XrController controller ) {
            focusedControllers.Remove( controller );
            controller.Pointer.NewHit -= pointerEvents[ controller ];
            pointerEvents.Remove( controller );
            updateFocus();
        }
        void updateFocus () {
            HasFocus = focusedControllers.Any();
            if ( !useTouch ) {
                touchSources.Clear();
                EmulatedInput.ReleaseAllTouch();
			}
        }

        Dictionary<Controller, TouchSource> touchSources = new();
        private void onPointerUpdate ( XrController controller, Raycast.RaycastHit hit ) {
            var position = TexturePositionAt( hit.TrisIndex, hit.Point );
            if ( useTouch ) {
                if ( !touchSources.ContainsKey( controller.Source ) ) {
                    touchSources.Add( controller.Source, new() { Position = position, Source = controller.Source } );
                }
                else {
                    touchSources[ controller.Source ].Position = position;
                    if ( touchSources[ controller.Source ].ActionCount > 0 ) {
                        EmulatedInput.TouchMove( touchSources[ controller.Source ], position );
                    }
                }
			}
            else {
                EmulatedInput.mouseHandler.handleMouseMove( position );
            }
        }

        bool useTouch => focusedControllers.Count > 1 || singlePointerTouchBindable.Value;

        private class TouchSource {
            public Controller Source;
            public Vector2 Position;
            public int ActionCount;
		}
    }

    public enum PanelInputMode {
        Regular,
        Inverted
	}
}
