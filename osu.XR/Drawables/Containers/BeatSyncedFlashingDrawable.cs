using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;

namespace osu.XR.Drawables.Containers {
	public class BeatSyncedFlashingDrawable : BeatSyncedContainer {
		protected override void OnNewBeat ( int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, ChannelAmplitudes amplitudes ) {
			this.FlashColour( Colour4.White, timingPoint.BeatLength * 0.8f, Easing.Out );
		}
	}
}
