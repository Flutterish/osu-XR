using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.XR.Components;
using osu.XR.Graphics;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input {
	public class XrKeyboard : XrObject {
		public readonly Bindable<KeyboardLayout> LayoutBindable = new( KeyboardLayout.Default );
		private List<XrKey> keys = new();
		[Resolved]
		private OsuGameXr Game { get; set; }

		public XrKeyboard () {
			LayoutBindable.BindValueChanged( _ => remapKeys(), true );
			AutoOffsetAxes = Axes3D.All;
		}

		protected override void Update () {
			base.Update();
			this.MoveTo( Game.Camera.Position + Game.Camera.Forward + Game.Camera.Down * 0.3f, 50 );
			this.RotateTo( Game.Camera.Rotation, 50 );
		}

		public void LoadModel ( string path )
			=> loadMesh( Mesh.MultipleFromOBJFile( path ) );

		public void LoadModel ( IEnumerable<string> lines )
			=> loadMesh( Mesh.MultipleFromOBJ( lines ) );

		private void loadMesh ( IEnumerable<Mesh> keys ) {
			foreach ( var i in this.keys ) i.Destroy();
			this.keys.Clear();

			foreach ( var i in keys ) {
				var key = new XrKey { Mesh = i };
				Add( key );
				this.keys.Add( key );
			}

			// we have the keys sorted top-down left-right so its easy to visually map them in KeyboardLayout. This is going to change.
			this.keys.Sort( (a,b) =>
				Math.Sign( a.Mesh.BoundingBox.Max.Z - b.Mesh.BoundingBox.Max.Z ) * 2 + Math.Sign( b.Mesh.BoundingBox.Min.X - a.Mesh.BoundingBox.Min.X )
			);

			remapKeys();
		}

		private void remapKeys () {
			foreach ( var (key,i) in keys.Zip( Enumerable.Range(0,keys.Count) ) ) {
				key.KeyBindalbe.Value = LayoutBindable.Value.Keys.ElementAtOrDefault( i );
			}
		}

		private class XrKey : MeshedXrObject {
			public readonly Bindable<KeyboardKey> KeyBindalbe = new();

			public XrKey () {
				MainTexture = Textures.Pixel( Color4.Gray ).TextureGL;
				KeyBindalbe.BindValueChanged( _ => {
					
				} );
			}

			public bool IsDown { get; private set; }
		}
	}
}
