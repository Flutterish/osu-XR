using osu.Framework.Audio.Track;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.XR.Osu;

namespace osu.XR.Timing;

public partial class BeatSyncSource : OsuComponent {
	partial class Source : BeatSyncedContainer {
		protected override void OnNewBeat ( int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes ) {
			base.OnNewBeat( beatIndex, timingPoint, effectPoint, amplitudes );
			BindableBeat.Value = new( beatIndex, timingPoint, effectPoint, amplitudes );
		}

		public readonly Bindable<Beat> BindableBeat = new();
	}

	protected override void OnDependenciesChanged ( OsuDependencies dependencies ) {
		ClearInternal();
		Source source = new();
		AddInternal( source );
		BindableBeat.Current = source.BindableBeat;
	}

	public readonly BindableWithCurrent<Beat> BindableBeat = new();
}

public record struct Beat ( int BeatIndex, TimingControlPoint TimingPoint, EffectControlPoint EffectPoint, ChannelAmplitudes Amplitudes ) {
	public readonly double AverageAmplitude = Amplitudes.Average;
}