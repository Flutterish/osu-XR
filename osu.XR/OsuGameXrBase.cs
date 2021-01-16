using osu.Framework.Allocation;
using osu.Game;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.XR.Projection;
using osuTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR {
	public class OsuGameXrBase : OsuGameBase {
		[Cached]
		public readonly Camera Camera = new() { Position = new Vector3( 0, 0, 0 ) };

		[Cached]
		private readonly DifficultyRecommender difficultyRecommender = new DifficultyRecommender();
		[Cached]
		private readonly ScreenshotManager screenshotManager = new ScreenshotManager();
	}
}
