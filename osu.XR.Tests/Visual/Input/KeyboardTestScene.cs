using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;
using osu.XR.Graphics.VirtualReality;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests.Visual.Input;

public partial class KeyboardTestScene : Basic3DTestScene {
	public KeyboardTestScene () {
		Scene.Add( new VrKeyboard() );

		Remove( MouseInteractionSource, disposeImmediately: false );
		AddToggleStep( "Interaction", v => {
			if ( MouseInteractionSource.Parent != null )
				Remove( MouseInteractionSource, disposeImmediately: false );
			else
				Add( MouseInteractionSource );
		} );
	}
}
