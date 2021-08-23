using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables;
using osu.XR.Drawables.UserInterface;
using osu.XR.Inspector;
using osu.XR.Settings.Sections;
using osuTK;
using osuTK.Graphics;

namespace osu.XR.Components {
	/// <summary>
	/// White line grid on the floor with a fade.
	/// </summary>
	public class FloorGrid : Model, IConfigurableInspectable {
        public readonly Bindable<Color4> TintBindable = new( Color4.White );
        public readonly BindableFloat OpacityBindable = new( 1 ) { MinValue = 0, MaxValue = 1 };
        public readonly BindableInt XSegmentsBindable = new( 7 ) { MinValue = 0, MaxValue = 20, Precision = 1 };
        public readonly BindableInt ZSegmentsBindable = new( 7 ) { MinValue = 0, MaxValue = 20, Precision = 1 };
        public readonly BindableFloat SegmentWidthBindable = new( 0.01f ) { MinValue = 0.001f, MaxValue = 0.05f };
        public readonly BindableFloat SegmentSpreadBindable = new( 1 ) { MinValue = 0.1f, MaxValue = 2 };
        public readonly BindableFloat SegmentLengthBindable = new( 16.7f ) { MinValue = 5, MaxValue = 50 };

        public FloorGrid () {
            MainTexture = Textures.Vertical2SidedGradient( Color4.Transparent, Color4.White, 200 ).TextureGL;

            TintBindable.BindValueChanged( v => Tint = v.NewValue, true );
            OpacityBindable.BindValueChanged( v => Alpha = v.NewValue, true );

            (XSegmentsBindable, ZSegmentsBindable, SegmentWidthBindable, SegmentSpreadBindable, SegmentLengthBindable).BindValuesChanged( recalcualteMesh, true );
        }

        void recalcualteMesh () {
            recalcualteMesh( 
                XSegmentsBindable.Value,
                ZSegmentsBindable.Value,
                SegmentWidthBindable.Value,
                SegmentSpreadBindable.Value,
                SegmentSpreadBindable.Value,
                SegmentLengthBindable.Value,
                SegmentLengthBindable.Value
           );
        }

        void recalcualteMesh ( int x_segments, int z_segments, float width, float x_spread, float z_spread, float x_length, float z_length ) {
            Mesh = new Mesh { IsReady = false };

            for ( int x = -x_segments; x <= x_segments; x++ ) {
                float xFrom = x * x_spread - width / 2;
                float xTo = x * x_spread + width / 2;
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
                float zFrom = z * z_spread - width / 2;
                float zTo = z * z_spread + width / 2;
                Mesh.AddQuad( new Quad(
                    new Vector3( xFrom, 0, zFrom ), new Vector3( xFrom, 0, zTo ),
                    new Vector3( xTo, 0, zFrom ), new Vector3( xTo, 0, zTo )
                ), new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) );
            }

            Mesh.IsReady = true;
		}

		public Drawable CreateInspectorSubsection () {
            return new SettingsSectionContainer {
                Title = "Floor Grid",
                Icon = FontAwesome.Solid.ShoePrints,
                Children = new Drawable[] {
                    new ColorPicker { LabelText = "Tint", Current = TintBindable },
                    new SettingsSlider<float,PercentSliderBar> { LabelText = "Opacity", Current = OpacityBindable },
                    new SettingsSlider<int> { LabelText = "X Segments", Current = XSegmentsBindable },
                    new SettingsSlider<int> { LabelText = "Z Segments", Current = ZSegmentsBindable },
                    new SettingsSlider<float,MetersSliderBar> { LabelText = "Segment Spread", Current = SegmentSpreadBindable },
                    new SettingsSlider<float,MetersSliderBar> { LabelText = "Segment Width", Current = SegmentWidthBindable },
                    new SettingsSlider<float,MetersSliderBar> { LabelText = "Segment Length", Current = SegmentLengthBindable }
                }
            };
        }
        public bool AreSettingsPersistent => false;
	}
}
