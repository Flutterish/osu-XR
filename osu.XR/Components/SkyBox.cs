using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Projection;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables;
using osu.XR.Drawables.UserInterface;
using osu.XR.Inspector;
using osu.XR.Settings.Sections;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;

namespace osu.XR.Components {
	/// <summary>
	/// A hot pink skybox that fits the osu theme.
	/// </summary>
	public class SkyBox : Model, IBehindEverything, IConfigurableInspectable {
		public readonly Bindable<Color4> TintBindable = new( new Color4( 253, 35, 115, 255 ) );
		public readonly BindableFloat OpacityBindable = new( 1 ) { MinValue = 0, MaxValue = 1 };

		public SkyBox () {
			MainTexture = Textures.VerticalGradient( Color4.Black, Color4.White, 100, x => MathF.Pow( x, 2 ) ).TextureGL;
			Mesh.Vertices.AddRange( new[] {
				new Vector3(  1,  1,  1 ) * 300,
				new Vector3(  1,  1, -1 ) * 300,
				new Vector3(  1, -1,  1 ) * 300,
				new Vector3(  1, -1, -1 ) * 300,
				new Vector3( -1,  1,  1 ) * 300,
				new Vector3( -1,  1, -1 ) * 300,
				new Vector3( -1, -1,  1 ) * 300,
				new Vector3( -1, -1, -1 ) * 300,
			} );
			Mesh.TextureCoordinates.AddRange( new[] {
				new Vector2( 0, 0 ),
				new Vector2( 0, 0 ),
				new Vector2( 0, 1 ),
				new Vector2( 0, 1 ),
				new Vector2( 0, 0 ),
				new Vector2( 0, 0 ),
				new Vector2( 0, 1 ),
				new Vector2( 0, 1 ),
				new Vector2( 0, 1 ),
				new Vector2( 1, 1 ),
			} );
			Mesh.Tris.AddRange( new IndexedFace[] {
				new( 4, 7, 5 ),
				new( 4, 7, 6 ),
				new( 4, 2, 6 ),
				new( 4, 2, 0 ),
				new( 5, 3, 7 ),
				new( 5, 3, 1 ),
				new( 6, 3, 7 ),
				new( 6, 3, 2 ),
				new( 0, 2, 1 ),
				new( 1, 2, 3 )
			} );

			UseGammaCorrection = true;
			TintBindable.BindValueChanged( v => Tint = v.NewValue, true );
			OpacityBindable.BindValueChanged( v => Alpha = v.NewValue, true );
		}

		public IEnumerable<Drawable> CreateInspectorSubsections () {
			yield return new SettingsSectionContainer {
				Title = "Skybox",
				Icon = FontAwesome.Solid.Image,
				Children = new Drawable[] {
					new ColorPicker { LabelText = "Tint", Current = TintBindable },
					new SettingsSlider<float,PercentSliderBar> { LabelText = "Opacity", Current = OpacityBindable },
				}
			};
		}
		public bool AreSettingsPersistent => false;

		protected override DrawNode3D CreateDrawNode ()
			=> new SkyboxDrawNode( this );

		class SkyboxDrawNode : ModelDrawNode<SkyBox> {
			public SkyboxDrawNode ( SkyBox source ) : base( source ) {
			}

			private Transform cameraTrackingTransform = new();
			protected override Transform Transform => cameraTrackingTransform;

			public override void Draw ( DrawSettings settings ) {
				cameraTrackingTransform.Position = settings.GlobalCameraPos;
				base.Draw( settings );
			}
		}
	}
}
