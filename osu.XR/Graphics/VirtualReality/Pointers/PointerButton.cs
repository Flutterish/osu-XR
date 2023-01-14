namespace osu.XR.Graphics.VirtualReality.Pointers;

/// <summary>
/// An aggregate button whose input can be redirected to another button
/// </summary>
public class PointerButton {
	PointerButton? actuated;
	int inputSourceCount;
	public bool IsDown { get; private set; }

	public void Actuate ( PointerButton? who ) {
		if ( actuated == who )
			return;

		var wasOldDown = actuated?.IsDown;
		var wasTargetDown = who?.IsDown;

		var old = actuated;
		if ( old != null )
			old.inputSourceCount--;
		if ( who != null )
			who.inputSourceCount++;
		actuated = who;

		if ( old != null ) {
			old.IsDown = old.inputSourceCount != 0;
			if ( wasOldDown == true && !old.IsDown )
				old.OnReleased?.Invoke();
		}
		if ( who != null ) {
			who.IsDown = who.inputSourceCount != 0;
			if ( wasTargetDown == false )
				who.OnPressed?.Invoke();
			else {
				who.OnRepeated?.Invoke();
			}
		}
	}

	public event Action? OnPressed;
	public event Action? OnRepeated;
	public event Action? OnReleased;
}
