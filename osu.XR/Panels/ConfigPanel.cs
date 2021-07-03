using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Components.Groups;
using osu.XR.Drawables;

namespace osu.XR.Components.Panels {
	public class ConfigPanel : FlatPanel, IHasName, IHasIcon {
		public readonly VRConfigDrawable Config = new() { Height = 500, Width = 400 };
		public readonly Bindable<bool> IsVisibleBindable = new();

		[Resolved]
		private OsuGameXr Game { get; set; }

		public ConfigPanel () {
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
