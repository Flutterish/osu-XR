using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
			FillFlowContainer createContainer () {
				return new FillFlowContainer {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Direction = FillDirection.Vertical
				};
			}
			string sectionName = "";
			FillFlowContainer container = null;
			
			foreach ( var line in raw.Replace( "\r", "" ).Split( "\n", StringSplitOptions.RemoveEmptyEntries ) ) {
				var trimmed = line.Trim();
				if ( trimmed.StartsWith( "[" ) && trimmed.EndsWith( "]" ) ) {
					sectionName = trimmed.Substring( 1, trimmed.Length - 2 );
					container = createContainer();

					AddSection( container, name: sectionName );
				}
				else {
					if ( trimmed.StartsWith( "*" ) ) {
						OsuTextFlowContainer text;
						var whitespace = line.Substring( 0, line.Length - line.TrimStart().Length );
						var margin = ( whitespace.Count( x => x == ' ' ) + 4 * whitespace.Count( x => x == '\t' ) ) * 3;
						container.Add( text = new OsuTextFlowContainer {
							AutoSizeAxes = Axes.Y,
							Margin = new MarginPadding { Left = margin + 15, Right = 15, Bottom = 4 }
						} );
						text.OnUpdate += d => d.Width = container.DrawWidth - margin - 30;
						text.AddIcon( FontAwesome.Solid.ChevronRight );
						text.AddText( " " + trimmed.Substring( 1 ).TrimStart() );
					}
					else {
						OsuTextFlowContainer text;
						container.Add( text = new OsuTextFlowContainer {
							AutoSizeAxes = Axes.Y,
							Text = line,
							Margin = new MarginPadding { Horizontal = 15, Bottom = 4 }
						} );
						text.OnUpdate += d => d.Width = container.DrawWidth - 30;
					}
				}
			}
		}
	}
}
