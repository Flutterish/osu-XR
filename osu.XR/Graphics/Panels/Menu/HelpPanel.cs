using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Overlays.Settings;
using System.Text.RegularExpressions;

namespace osu.XR.Graphics.Panels.Menu;

public partial class HelpPanel : SettingsPanel {
	protected override SectionsContainer CreateSectionsContainer () {
		return new Sections( false );
	}

	partial class Sections : SectionsContainer {
		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		protected override IEnumerable<SettingsSection> CreateSections () {
			const string RulesetSection = "`\uf11b Ruleset`"; // TODO make these clickable
			const string SettingsSection = "`\uf013 Settings`";

			yield return new HelpEntry( @"Input Modes", @$"
				There are a few ways to play OXR:
				* With a single laser pointer.
				* With two laser pointers.
				* With a touchscreen where each controller is a touch source.
				* With a touchscreen where each finger is a touch source.
				* Additionally, you can bind controller inputs and gestures to ruleset actions in the {RulesetSection} section.
			" );

			yield return new HelpEntry( @"Controllers", @"
				Your controllers have 2 main buttons:
				* Left click - this one will usually be a trigger.
				* Right click - this one might be the A or B button.

				While a controller is not active (not pointing at a panel, or not touching it in touchscreen mode), its input will be forwarded to the other controller.
				* This means you can alternate the left and right controller presses to make playing easier.

				There is also a menu button, which brings up this menu. This will usually be the B or A button.
			" );

			yield return new HelpEntry( @"Touchscreen", @"
				In touchscreen mode, controller buttons *still work*.
				* You can glide the screen and press the main buttons instead of tapping every time.
			" );

			yield return new HelpEntry( @"Strumming", @"
				There is an option to enable ""strumming"" in touch mode. This will make it so:
				* Tapping works as usual.
				* When you press a button, it will quickly release and tap again as usual.
				* When you release that button, it will quickly release and tap. Without this option, releasing a button does nothing.

				This is intended to help with fast streams of circles, and should be an intuitive option for guitar hero players.
			" );

			yield return new HelpEntry( @"Teleporting", @"
				Teleporting allows you to move around in the world, and adjust your positioning.

				Teleporting can be disabled temporarily in the options, and is automatically disabled while playing.

				The teleport button might not exist on your controller. In such case, you can assign it in the Vr Limbo. 
				If you do, please send us the binding files so future players will have it by default.
			" );

			yield return new HelpEntry( @"Ruleset Bindings", @$"
				In the {RulesetSection} section of this menu, you can assign controller inputs and gestures to ruleset actions.

				This menu will automatically show you the bindings for the ruleset you have currently selected.

				Currently, there are 3 kinds of bindings you can make:
				* Button bindings: 
				* * You can map a button on your controller to a ruleset action.
				* Joystick bindings:  
				* * You can map a zone of the joystick range to a ruleset action.
				* * You can map the joystick position to the cursor movement. This will however disable regular cursor movement and block the ability to click normally.
				* Gesture bindinbgs:
				* * You can map a ""clap"" gesture to a ruleset action. The gesture is detected based on distance between controllers.
			" );

			yield return new HelpEntry( @"Setting Presets", @$"
				You can load, edit and create setting presets at the bottom of the {SettingsSection} menu.

				The presets can contain all settings, or just a subset.

				While editing a preset, you will see the changes applied in real time, and when you stop, the previous settings will be restored.
			" );
		}

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( @"Help", @"how does this all work?" );
	}

	partial class HelpEntry : SettingsSection {
		static Regex deadspacePattern = new( @"^[\t ]+", RegexOptions.Compiled | RegexOptions.Multiline );
		public HelpEntry ( LocalisableString header, string markdown ) {
			Header = header;
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Add( new OsuMarkdownContainer {
				Text = deadspacePattern.Replace( markdown, "" ),
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			} );
		}

		public override Drawable CreateIcon () {
			return new SpriteIcon() { Icon = FontAwesome.Solid.Cat };
		}

		public override LocalisableString Header { get; }
	}
}
