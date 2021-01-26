using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class BeatProvider : BeatSyncedContainer {
		protected override void OnNewBeat ( int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes ) {
			base.OnNewBeat( beatIndex, timingPoint, effectPoint, amplitudes );
			BindableBeat.Value = new( beatIndex, timingPoint, effectPoint, amplitudes );
		}

		public readonly Bindable<Beat> BindableBeat = new();
	}

	public record Beat ( int BeatIndex, TimingControlPoint TimingPoint, EffectControlPoint EffectPoint, ChannelAmplitudes Amplitudes ) {
		public readonly double AverageAmplitude = Amplitudes.Average;
	}
}
