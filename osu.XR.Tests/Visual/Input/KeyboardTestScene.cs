using osu.XR.Graphics.VirtualReality;

namespace osu.XR.Tests.Visual.Input;

public partial class KeyboardTestScene : Osu3DTestScene {
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
