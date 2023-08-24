using osu.Framework.Allocation;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.XR.Graphics.VirtualReality;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests.Visual.Input;

public partial class KeyboardTestScene : Osu3DTestScene {
	[Cached]
	protected OverlayColourProvider ColourProvider { get; } = new OverlayColourProvider( OverlayColourScheme.Purple );

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
