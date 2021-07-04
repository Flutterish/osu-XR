using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.XR.Drawables;
using osu.XR.Input.Custom;
using osu.XR.Panels;

namespace osu.XR.Components.Panels {
	public class RulesetInfoPanel : HandheldPanel<RulesetInfoDrawable> {
		public BindableList<CustomBinding> GetBindingsForVariant ( int variant )
			=> Content.GetBindingsForVariant( variant );

		protected override RulesetInfoDrawable CreateContent ()
			=> new();

		public override string DisplayName => "Ruleset";
		public override Drawable CreateIcon ()
			=> new SpriteIcon { Icon = FontAwesome.Solid.Gamepad };
	}
}
