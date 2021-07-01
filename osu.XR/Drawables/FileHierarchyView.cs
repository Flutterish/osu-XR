using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class FileHierarchyView : HierarchyView<FileHierarchyStep,string> {
		public FileHierarchyView ( string path = "." ) : base( path == null ? path : Path.GetFullPath( path ) ) {

		}

		protected override FileHierarchyStep CreateStep ( string value )
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
		bool isFile;
		protected override void LoadComplete () {
			if ( Value == null ) {
				isFile = false;

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
				isFile = false;
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
				catch ( Exception e ) { }
			}
			else {
				isFile = true;
			}

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

			if ( A.isFile == B.isFile ) return a.Value.CompareTo( b.Value );
			return A.isFile ? 1 : -1;
		}

		protected override void ValueChanged ( string old, string @new ) {
			Label.Text = Title;
			Icon = CreateIcon();
		}

		public override Drawable CreateIcon () {
			if ( isFile ) {
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
				var val = isFile ? Path.GetFileNameWithoutExtension( Value ) : Path.GetFileName( Value );
				if ( String.IsNullOrWhiteSpace( val ) ) return Value;
				return val;
			}
		}
	}
}
