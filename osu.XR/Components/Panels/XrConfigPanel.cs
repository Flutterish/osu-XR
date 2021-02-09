using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.XR.Components;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using osu.XR.Settings;
using osuTK;
using System;

namespace osu.XR.Components.Panels {
	public class XrConfigPanel : FlatPanel, IHasName, IHasIcon {
		public readonly ConfigPanel Config = new( true ) { AutoSizeAxes = Axes.X, RelativeSizeAxes = Axes.None, Height = 500 };
		public readonly Bindable<bool> IsVisibleBindable = new();

		[Resolved]
		private OsuGameXr Game { get; set; }

		public XrConfigPanel () {
			PanelAutoScaleAxes = Axes.X;
			PanelHeight = 0.5;
			RelativeSizeAxes = Axes.X;
			Height = 500;
			AutosizeX();
			Source.Add( Config );
		}

		protected override void Update () {
			base.Update();
			IsVisible = Config.IsPresent;
			IsVisibleBindable.Value = IsVisible;
		}

		public string DisplayName => "Settings";

		public Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Cog };
	}
}
