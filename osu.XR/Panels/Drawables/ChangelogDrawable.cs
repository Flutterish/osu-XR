using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Sprites;
using osu.XR.Drawables.Containers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class ChangelogDrawable : ConfigurationContainer {
		public ChangelogDrawable () {
			Title = "Changelog";
			Description = "check out what's new";
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			Task.Run( () => {
				var raw = File.ReadAllText( @".\Resources\changelog.txt" );
				Schedule( () => loadChangelog( raw ) );
			} );
		}

		void loadChangelog ( string raw ) {
			ClearSections();
			OsuMarkdownContainer createContainer () {
				return new BiggerOsuMarkdownContainer {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y
				};
			}
			OsuMarkdownContainer container = null;
			string text = "";

			void finalizeSection () {
				if ( container is null ) return;

				container.Text = text;

				text = "";
			}
			
			foreach ( var line in raw.Replace( "\r", "" ).Split( "\n", StringSplitOptions.RemoveEmptyEntries ) ) {
				var trimmed = line.Trim();
				if ( trimmed.StartsWith( "[" ) && trimmed.EndsWith( "]" ) ) {
					finalizeSection();

					container = createContainer();

					AddSection( container, name: trimmed.Substring( 1, trimmed.Length - 2 ) );
				}
				else {
					if ( container is null ) {
						container = createContainer();

						AddSection( container, name: "Untitled Section" );
					}

					text += line + "\n";
				}
			}
			finalizeSection();
		}

		private class BiggerOsuMarkdownContainer : OsuMarkdownContainer {
			public override SpriteText CreateSpriteText () => new OsuSpriteText {
				Font = OsuFont.GetFont( Typeface.Inter, size: 18, weight: FontWeight.Regular ),
			};
		}
	}
}
