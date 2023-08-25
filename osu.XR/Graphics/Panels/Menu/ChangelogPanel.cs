using osu.Framework.Graphics.Containers.Markdown;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace osu.XR.Graphics.Panels.Menu;

public class ChangelogPanel : SettingsPanel {
	protected override SectionsContainer CreateSectionsContainer () {
		return new Sections( false );
	}

	partial class Sections : SectionsContainer {
		public Sections ( bool showSidebar ) : base( showSidebar ) { }

		protected override IEnumerable<SettingsSection> CreateSections () { // TODO this really shouldnt be hardcoded
			yield return new ChangelogEntry( @"2023.825.0 (Current)", @"
				Hello, World!
				This is the release version for the overhaul of OXR!
				This version features improvements such as significantly less bugs and a few visual changes. We also reworkwed the whole 3D framework.
				There are also new settings, and you can play using your fingers, given your controller can detect them.
				Speaking of settings, you can now create, edit and save setting presets!
			" );

			yield return new ChangelogEntry( @"Upcoming", @"
				### This section contains changes that you can expect in following updates.
				#### QOL
				* Programmatically ensure the game is running in multithreaded and unlimited framerate.
				* Ability to pin and move around elements such as menu panels and the keyboard. You will be able to pin them in space, or relative to another object or set of objects.
				* A small first-time tutorial screen.
				#### Input
				* Ability to select which hand skeleton fingers are active.
				* On-screen keyboard for panels when text input is active.
				* Ability to import props and create ruleset interactions with them.
				* More ruleset binding customisation, including: 3D activation zones and gestures.
				#### Visual
				* Input display when rendering to window.
				* Movable render-to-window camera prop.
				* Player avatar.
				* A non-invasive thank you to contribuitors and ko-fi supporters somewhere.
				#### Audio
				* Spatial sound for enviornment objects.
				#### Milestones
				* VR API for osu! rulesets. This will allow them to display elements in 3D space, create interactible props and much more.
			" );
		}

		protected override Drawable CreateHeader ()
			=> new SettingsHeader( @"Changelog", @"see what's new" );
	}

	partial class ChangelogEntry : SettingsSection {
		static Regex deadspacePattern = new( @"(^\n$)|(^[\t ]+)", RegexOptions.Compiled | RegexOptions.Multiline );
		public ChangelogEntry ( LocalisableString header, string markdown ) {
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
