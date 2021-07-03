using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Containers.Markdown;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class FileHierarchyViewWithPreview : FileHierarchyView {
		Container preview;
		[Cached]
		OverlayColourProvider overlayColour; // needed for the markdown container
		public FileHierarchyViewWithPreview ( string path = "." ) {
			overlayColour = new OverlayColourProvider( OverlayColourScheme.Purple );
			Add( preview = new Container {
				AutoSizeAxes = Axes.Y,
				Masking = true,
				Margin = new MarginPadding { Top = 5, Horizontal = 15 },
				AutoSizeDuration = 500,
				AutoSizeEasing = Easing.Out,
				CornerRadius = 7
			} );
			preview.OnUpdate += d => d.Width = DrawWidth - 30;

			StepSelected += stepSelected;
		}

		private void stepSelected ( FileHierarchyStep obj ) {
			preview.Clear();
			preview.Add( new Box {
				RelativeSizeAxes = Axes.Both,
				Colour = OsuColour.Gray( 0.1f )
			} );
			if ( obj.IsFile ) {
				var fileInfo = new FileInfo( obj.Value );
				if ( fileInfo.Length > 5120 ) {
					OsuTextFlowContainer text;
					preview.Add( text = new OsuTextFlowContainer {
						AutoSizeAxes = Axes.Both,
						Margin = new MarginPadding( 5 ),
					} );
					text.Colour = Colour4.HotPink;
					text.Text = $"This file is {fileInfo.Length.HumanizeSiBytes()} which is too big to load a preview!\nThe maximum size to display is {5120.HumanizeSiBytes()}";
				}
				else {
					File.ReadAllTextAsync( obj.Value ).ContinueWith( v => Schedule( () => {
						var extension = Path.GetExtension( obj.Value );
						if ( extension == ".md" ) {
							OsuMarkdownContainer markdown;
							preview.Add( markdown = new OsuMarkdownContainer {
								AutoSizeAxes = Axes.Y,
								RelativeSizeAxes = Axes.X,
								Margin = new MarginPadding( 5 ),
								LineSpacing = 5
							} );
							markdown.Text = v.Result;
						}
						else {
							OsuTextFlowContainer text;
							preview.Add( text = new OsuTextFlowContainer {
								AutoSizeAxes = Axes.Both,
								Margin = new MarginPadding( 5 )
							} );
							text.Text = v.Result;
						}
					} ) );
				}
			}
		}

		public static async Task<char[]> ReadCharsAsync ( string filename, int count ) {
			using ( var stream = File.OpenRead( filename ) )
			using ( var reader = new StreamReader( stream, Encoding.UTF8 ) ) {
				char[] buffer = new char[ count ];
				int n = await reader.ReadBlockAsync( buffer, 0, count );

				char[] result = new char[ n ];

				Array.Copy( buffer, result, n );

				return result;
			}
		}
	}

	public class FileHierarchyView : HierarchyView<FileHierarchyStep,string> {
		public FileHierarchyView ( string path = "." ) : base( path == null ? path : Path.GetFullPath( path ) ) { }

		protected override FileHierarchyStep CreateTop ( string value )
			=> new FileHierarchyStep( value );

		protected override FileHierarchyStep CreateParent ( FileHierarchyStep top )
			=> CreateParent( top.Value );

		public static FileHierarchyStep CreateParent ( string current ) {
			if ( current == null ) return null;
			var path = Path.GetFullPath( Path.Combine( current, ".." ) );
			if ( path == current ) {
				if ( current != null ) return new FileHierarchyStep( null );

				return null;
			}
			if ( Directory.Exists( path ) ) {
				return new FileHierarchyStep( path );
			}
			return null;
		}
	}

	public class FileHierarchyStep : HierarchyStep<string> {
		public FileHierarchyStep ( string path ) : base( path == null ? path : Path.GetFullPath( path ) ) { }

		FileSystemWatcher watcher;
		public bool IsFile { get; private set; }
		protected override void LoadComplete () {
			if ( Value == null ) {
				IsFile = false;

				foreach ( var drive in DriveInfo.GetDrives() ) {
					AddChild( drive.Name );
				}

				this.Loop( 1000, d => d.Delay( 0 ).Schedule( () => {
					var drives = DriveInfo.GetDrives().Select( x => x.Name );
					foreach ( var added in drives.Except( Children.Keys ).ToArray() ) {
						AddChild( added );
					}
					foreach ( var removed in Children.Keys.Except( drives ).ToArray() ) {
						RemoveChild( removed );
					}
				} ) );
			}
			else if ( Directory.Exists( Value ) ) {
				IsFile = false;
				watcher = new FileSystemWatcher( Value );
				watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
				watcher.IncludeSubdirectories = false;

				watcher.Created += entryCreated;
				watcher.Deleted += entryDeleted;
				watcher.Renamed += entryRenamed;

				try {
					watcher.EnableRaisingEvents = true;
					foreach ( var entry in Directory.EnumerateFileSystemEntries( Value ) ) {
						AddChild( entry );
					}
				}
				catch ( Exception ) { }
			}
			else {
				IsFile = true;
			}

			setFilterTerms();
			base.LoadComplete();
		}

		private void entryRenamed ( object sender, RenamedEventArgs e ) {
			Schedule( () => {
				if ( e.Name is null ) {
					reloadEntries();
					return;
				}
				ChangeChildValue( Path.GetFullPath( e.OldFullPath ), Path.GetFullPath( e.FullPath ) );
			} );
		}

		private void entryDeleted ( object sender, FileSystemEventArgs e ) {
			Schedule( () => {
				if ( e.Name is null ) {
					reloadEntries();
					return;
				}
				RemoveChild( Path.GetFullPath( e.FullPath ) );
				if ( !Children.Any() ) Icon = CreateIcon();
			} );
		}

		private void entryCreated ( object sender, FileSystemEventArgs e ) {
			Schedule( () => {
				if ( e.Name is null ) {
					reloadEntries();
					return;
				}
				AddChild( Path.GetFullPath( e.FullPath ) );
				if ( Children.Count == 1 ) Icon = CreateIcon();
			} );
		}

		private void reloadEntries () {
			var entries = Directory.EnumerateFileSystemEntries( Value ).Select( Path.GetFullPath );
			foreach ( var removed in Children.Keys.Except( entries ).ToArray() ) {
				RemoveChild( removed );
			}

			foreach ( var added in entries.Except( Children.Keys ).ToArray() ) {
				AddChild( added );
			}
		}

		protected override int Sort ( HierarchyStep<string> a, HierarchyStep<string> b ) {
			var A = a as FileHierarchyStep;
			var B = b as FileHierarchyStep;

			if ( A.IsFile == B.IsFile ) return a.Value.CompareTo( b.Value );
			return A.IsFile ? 1 : -1;
		}

		protected override void ValueChanged ( string old, string @new ) {
			Label.Text = Title;
			Icon = CreateIcon();

			setFilterTerms();
		}

		void setFilterTerms () {
			if ( IsFile )
				filterTerms = new[] { Title, "File", Path.GetExtension( Value ) };
			else
				filterTerms = new[] { Title, "Directory", "Folder" };

			InvokeSearchTermsModified();
		}

		public override Drawable CreateIcon () {
			if ( IsFile ) {
				return new FillFlowContainer() {
					Height = 16,
					AutoSizeAxes = Axes.X,
					Direction = FillDirection.Horizontal,
					Children = new Drawable[] {
						new SpriteIcon() {
							Icon = FontAwesome.Regular.File,
							Height = 16,
							Width = 16
						},
						new OsuSpriteText {
							UseFullGlyphHeight = false,
							Origin = Anchor.CentreLeft,
							Anchor = Anchor.CentreLeft,
							Text = Path.GetExtension( Value ),
							Colour = Colour4.HotPink
						}
					}
				};
			}
			else {
				if ( Value == null ) {
					return new SpriteIcon() {
						Icon = FontAwesome.Brands.Sourcetree,
						Height = 16,
						Width = 16
					};
				}
				else {
					return new SpriteIcon() {
						Icon = Path.GetPathRoot( Value ) == Value
							? FontAwesome.Regular.Hdd
							: Children.Any() 
							? FontAwesome.Regular.Folder 
							: FontAwesome.Regular.FolderOpen,
						Height = 16,
						Width = 16
					};
				}
			}
		}

		protected override FileHierarchyStep CreateChild ( string value )
			=> new FileHierarchyStep( value );
		protected override FileHierarchyStep CreateParent ()
			=> FileHierarchyView.CreateParent( Value );
		public override string Title {
			get {
				if ( Value == null ) return "Drives";
				var val = IsFile ? Path.GetFileNameWithoutExtension( Value ) : Path.GetFileName( Value );
				if ( String.IsNullOrWhiteSpace( val ) && ( !IsFile || String.IsNullOrWhiteSpace( Path.GetExtension( Value ) ) ) ) return Value;
				return val;
			}
		}

		private string[] filterTerms = Array.Empty<string>();
		public override IEnumerable<string> FilterTerms => filterTerms;
	}
}
