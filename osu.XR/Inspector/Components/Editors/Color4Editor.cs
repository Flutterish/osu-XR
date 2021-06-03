using osu.XR.Drawables.UserInterface;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Editors {
	public class Color4Editor : ValueEditor<Color4> {
		ColorPicker colorPicker;
		public Color4Editor ( Color4 defaultValue = default ) : base ( defaultValue ) {
			Add( colorPicker = new ColorPicker { } );
			colorPicker.Current.BindTo( Current );
		}
	}
}
