using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Input.Actions.Gestures;

namespace osu.XR.Input.Handlers;

public partial class ClapHandler : ActionBindingHandler {
	Controller? left;
	Controller? right;
	ClapBinding source;
	public ClapHandler ( ClapBinding source ) : base( source ) {
		this.source = source;
		GetController( Hand.Left, c => left = c );
		GetController( Hand.Right, c => right = c );

		float maxDistance = 0.5f;
		Distance.BindValueChanged( v => Progress.Value = Math.Clamp( v.NewValue / maxDistance, 0, 1 ) );
		RegisterAction( source.Action, Active );
	}

	public readonly BindableBool Active = new();
	public readonly BindableFloat Distance = new();
	public readonly BindableFloat Progress = new();

	protected override void Update () {
		base.Update();

		if ( left != null && right != null )
			Distance.Value = ( right.GlobalPosition - left.GlobalPosition).Length;
		else
			Distance.Value = 0;

		var lowThreshold = Math.Min( source.ThresholdABindable.Value, source.ThresholdBBindable.Value );
		var highThreshold = Math.Max( source.ThresholdABindable.Value, source.ThresholdBBindable.Value );
		if ( Active.Value )
			Active.Value = Progress.Value < highThreshold;
		else
			Active.Value = Progress.Value < lowThreshold;
	}
}
