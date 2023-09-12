using osu.Framework.Caching;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Localisation;
using osu.Framework.XR;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Game.Overlays.Settings;
using osu.XR.Graphics.Settings;
using osuTK.Graphics;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class FloorGrid : BasicModel, IConfigurableSceneryComponent {
	LocalisableString ISceneryComponent.Name => @"Floor Grid";
	public SceneryComponentSettingsSection CreateSettings () => new FloorGridSettingsSection( this );

	Cached meshCache = new();
	public readonly BindableInt SegmentsCountBindable = new( 7 ) { MinValue = 0, MaxValue = 20, Precision = 1 };
	public readonly BindableFloat SegmentWidthBindable = new( 0.01f ) { MinValue = 0.001f, MaxValue = 0.05f };
	public readonly BindableFloat SegmentSpreadBindable = new( 1 ) { MinValue = 0.1f, MaxValue = 2 };
	public readonly BindableFloat SegmentLengthBindable = new( 16.7f ) { MinValue = 5, MaxValue = 50 };
	public readonly Bindable<Colour4> TintBindable = new( Colour4.White );

	public FloorGrid () {
		RenderStage = RenderingStage.Transparent;
		(SegmentsCountBindable, SegmentWidthBindable, SegmentSpreadBindable, SegmentLengthBindable).BindValuesChanged( () => meshCache.Invalidate(), true );
		TintBindable.BindValueChanged( v => Colour = v.NewValue, true );
	}

	protected override void Update () {
		if ( !meshCache.IsValid ) {
			Mesh.Clear();
			RegenerateMesh();
			Mesh.CreateFullUpload().Enqueue();
		}
		base.Update();
	}

	protected override Material CreateDefaultMaterial ( MaterialStore materials )
		=> materials.GetNew( MaterialNames.Transparent );

	[BackgroundDependencyLoader]
	private void load ( IRenderer renderer ) {
		Material.SetTexture( "tex", TextureGeneration.Vertical2SidedGradient( renderer, Color4.Transparent, Color4.White, 200 ) );
	}

	void RegenerateMesh () {
		var (x_segments, z_segments, width, x_spread, z_spread, x_length, z_length) = (
			SegmentsCountBindable.Value,
			SegmentsCountBindable.Value,
			SegmentWidthBindable.Value,
			SegmentSpreadBindable.Value,
			SegmentSpreadBindable.Value,
			SegmentLengthBindable.Value,
			SegmentLengthBindable.Value
		);

		for ( int x = -x_segments; x <= x_segments; x++ ) {
			float xFrom = x * x_spread - width / 2;
			float xTo = x * x_spread + width / 2;
			float zFrom = x_length * -0.5f;
			float zTo = x_length * 0.5f;
			Mesh.AddQuad( new Quad3(
				new Vector3( xFrom, 0, zFrom ), new Vector3( xFrom, 0, zTo ),
				new Vector3( xTo, 0, zFrom ), new Vector3( xTo, 0, zTo )
			), new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 0, 1 ) );
		}

		for ( int z = -z_segments; z <= z_segments; z++ ) {
			float xFrom = z_length * -0.5f;
			float xTo = z_length * 0.5f;
			float zFrom = z * z_spread - width / 2;
			float zTo = z * z_spread + width / 2;
			Mesh.AddQuad( new Quad3(
				new Vector3( xFrom, 0, zFrom ), new Vector3( xFrom, 0, zTo ),
				new Vector3( xTo, 0, zFrom ), new Vector3( xTo, 0, zTo )
			), new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) );
		}
	}
}

public partial class FloorGridSettingsSection : SceneryComponentSettingsSection {
	public FloorGridSettingsSection ( FloorGrid source ) : base( source ) {
		Add( new SettingsColourPicker {
			LabelText = @"Tint",
			Current = source.TintBindable
		} );
		Add( new SettingsSlider<int> {
			LabelText = @"Line count",
			Current = source.SegmentsCountBindable
		} );
		Add( new SettingsSlider<float, MetersSliderBar> {
			LabelText = "Line width",
			Current = source.SegmentWidthBindable
		} );
		Add( new SettingsSlider<float, MetersSliderBar> {
			LabelText = "Line spread",
			Current = source.SegmentSpreadBindable
		} );
		Add( new SettingsSlider<float, MetersSliderBar> {
			LabelText = "Line length",
			Current = source.SegmentLengthBindable
		} );
	}
}