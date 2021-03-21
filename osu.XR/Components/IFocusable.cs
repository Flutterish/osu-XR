namespace osu.XR.Components {
	public interface IFocusable {
		void OnControllerFocusGained ( IFocusSource controller );
		void OnControllerFocusLost ( IFocusSource controller );
		bool CanHaveGlobalFocus { get; }
	}

	public interface IFocusSource {

	}
}
