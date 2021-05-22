using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using osu.XR.Input.Custom;

namespace osu.XR.Components.Panels {
	public class XrRulesetInfoPanel : FlatPanel, IHasName, IHasIcon {
		public readonly RulesetInfoPanel Panel = new() { Height = 500, Width = 400 };
		public readonly Bindable<bool> IsVisibleBindable = new();
		public BindableList<CustomBinding> GetBindingsForVariant ( int variant )
			=> Panel.GetBindingsForVariant( variant );

		[Resolved]
		private OsuGameXr Game { get; set; }

		public XrRulesetInfoPanel () {
			PanelAutoScaleAxes = Axes.X;
			PanelHeight = 0.5;
			RelativeSizeAxes = Axes.X;
			Height = 500;
			AutosizeX();
			Source.Add( Panel );
		}

		protected override void Update () {
			base.Update();
			IsVisible = Panel.IsPresent;
			IsVisibleBindable.Value = IsVisible;
		}

		public string DisplayName => "Ruleset";

		public Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Gamepad };
	}
}
