using osu.XR.Drawables.UserInterface;
using osuTK.Graphics;

namespace osu.XR.Inspector.Editors {
	public class Color4Editor : ValueEditor<Color4> {
		ColorPicker colorPicker;
		public Color4Editor ( Color4 defaultValue = default ) : base( defaultValue ) {
			Add( colorPicker = new ColorPicker { } );
			colorPicker.Current.BindTo( Current );
		}
	}
}
