using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.XR.Graphics.Bindings;

namespace osu.XR.Tests.Visual.Drawables;

public partial class TestSceneBindings : OsuTestScene {
	[Cached]
	OverlayColourProvider colours = new( OverlayColourScheme.Purple );

	public TestSceneBindings () {
		Add( new Container {
			RelativeSizeAxes = Axes.Y,
			Width = Game.Overlays.SettingsPanel.PANEL_WIDTH,
			Children = new Drawable[] {
				new Box {
					RelativeSizeAxes = Axes.Both,
					Colour = colours.Background4
				},
				new OsuScrollContainer {
					Masking = true,
					RelativeSizeAxes = Axes.Both,
					ScrollbarVisible = false,
					Child = new RulesetBindingsSection() {
						Margin = new() { Bottom = 100 }
					}
				}
			}
		} );
	}

	[BackgroundDependencyLoader]
	private void load ( OsuXrGameBase game ) {
		if ( game.VrInput is VirtualVrInput input )
			AddVrControls( input );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		OsuDependencies.Ruleset.Value = new TestRuleset().RulesetInfo;
	}

	class TestRuleset : Ruleset {
		public override IEnumerable<Mod> GetModsFor ( ModType type ) {
			throw new NotImplementedException();
		}

		public override DrawableRuleset CreateDrawableRulesetWith ( IBeatmap beatmap, IReadOnlyList<Mod>? mods = null ) {
			throw new NotImplementedException();
		}

		public override IBeatmapConverter CreateBeatmapConverter ( IBeatmap beatmap ) {
			throw new NotImplementedException();
		}

		public override DifficultyCalculator CreateDifficultyCalculator ( IWorkingBeatmap beatmap ) {
			throw new NotImplementedException();
		}

		public override string Description => "test!ruleset";
		public override string ShortName => "test";

		public override IEnumerable<KeyBinding> GetDefaultKeyBindings ( int variant = 0 ) {
			return new KeyBinding[] {
				new( InputKey.A, TestRulesetAction.Left ),
				new( InputKey.D, TestRulesetAction.Right ),
				new( InputKey.W, TestRulesetAction.Up ),
				new( InputKey.S, TestRulesetAction.Down ),
				new( InputKey.Space, TestRulesetAction.Middle )
			};
		}

		enum TestRulesetAction {
			Up,
			Down,
			Left,
			Right,
			Middle
		}
	}
}
