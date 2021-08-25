using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR;
using osu.Framework.XR.Allocation;
using osu.Framework.XR.Components;
using osu.Framework.XR.Parsing;
using osu.Framework.XR.Parsing.Blender;
using osu.Framework.XR.Parsing.WaveFront;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables.Containers;
using osu.XR.Editor;
using osu.XR.Inspector;
using osu.XR.Panels.Overlays;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static osu.XR.Components.BeatingScenery;

namespace osu.XR.Panels.Drawables {
	public class SceneManagerDrawable : ConfigurationContainer {
		[Resolved]
		private PanelOverlayContainer overlays { get; set; }

		OsuTextFlowContainer raport;

		public readonly Bindable<SceneContainer> SceneContainerBindable = new();
		public SceneContainer SceneContainer {
			get => SceneContainerBindable.Value;
			set => SceneContainerBindable.Value = value;
		}

		private void addRaportMessage ( string message, ParsingErrorSeverity severity = ParsingErrorSeverity.Success ) {
			Color4 color = Color4.White;
			if ( severity.HasFlagFast( ParsingErrorSeverity.Error ) ) {
				color = Color4.MediumVioletRed;
			}
			else if ( severity.HasFlagFast( ParsingErrorSeverity.NotImplemented ) || severity.HasFlagFast( ParsingErrorSeverity.Issue ) ) {
				color = Colour4.GreenYellow;
			}

			if ( severity.HasFlagFast( ParsingErrorSeverity.NotImplemented ) ) {
				Schedule( () => raport.AddParagraph( $"{message} Some things might not display properly.", s => s.Colour = color ) );
			}
			else {
				Schedule( () => raport.AddParagraph( message, s => s.Colour = color ) );
			}
		}

		SerialTaskScheduler importTasks = new();
		LoadingSpinner loadingSpinner;
		/// <summary>
		/// Schedules importing props to run in the background
		/// </summary>
		public void ScheduleImport ( IEnumerable<string> files ) {
			importTasks.Schedule( () => {
				importProps( files );
			} );
		}

		public SceneManagerDrawable () { // TODO save the scenery
			Title = "Scene Manager";
			Description = "change up the scenery";

			AddSection( new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,
				Children = new Drawable[] {
					new SettingsButton {
						Text = "Browse",
						Action = () => {
							overlays.RequestOverlay<FileSelectionOverlay>().Confirmed += files => {
								ScheduleImport( files );
							};
						},
						TooltipText = "Import .obj and .blend files"
					},
					loadingSpinner = new LoadingSpinner( true ) {
						Origin = Anchor.TopCentre,
						Anchor = Anchor.TopCentre,
						Margin = new MarginPadding { Top = 8 }
					},
					new ExpandableSection {
						Margin = new MarginPadding { Horizontal = 15, Top = 10 },
						Child = raport = new OsuTextFlowContainer {
							AutoSizeAxes = Axes.Y,
							RelativeSizeAxes = Axes.X,
							Anchor = Anchor.TopCentre,
							Origin = Anchor.TopCentre,
							Width = 0.9f,
							Text = "Nothing to show!",
							Margin = new MarginPadding { Bottom = 4 }
						},
						Title = "Raport"
					}
				}
			}, name: "Import Props" );

			SceneContainerBindable.BindValueChanged( v => {
				if ( v.OldValue is not null ) {
					v.OldValue.ChildAdded -= childAdded;
					v.OldValue.ChildRemoved -= childRemoved;

					foreach ( var s in sections.Values.SelectMany( x => x ) ) {
						RemoveSection( s );
					}
					sections.Clear();
				}

				if ( v.NewValue is null ) return;

				v.NewValue.BindLocalHierarchyChange( childAdded, childRemoved, true );
			} );

			importTasks.TaskSequenceStarted += () => {
				Schedule( () => raport.Clear() );
				Schedule( () => loadingSpinner.Show() );
			};

			importTasks.TaskSequenceFinished += () => {
				Schedule( () => loadingSpinner.Hide() );
			};
		}

		public static readonly IReadOnlyList<string> SupportedFileFormats = new List<string> {
			".obj", ".blend"
		}.AsReadOnly();

		private void importProps ( IEnumerable<string> files ) {
			using var importedFiles = ListPool<IModelFile>.Shared.Rent();
			foreach ( var i in files ) {
				bool ok = true;
				addRaportMessage( $"Parsing: {Path.GetFileName( i )}" );
				try {
					if ( Directory.Exists( i ) ) {
						addRaportMessage( "This is a directory, not a model file.", ParsingErrorSeverity.Error );
						addRaportMessage( "" );
						continue;
					}
					var type = Path.GetExtension( i );
					if ( !SupportedFileFormats.Contains( type ) ) {
						addRaportMessage( $"This a {type} file, which is not supported. Supported file formats: {string.Join(", ", SupportedFileFormats)}", ParsingErrorSeverity.Error );
						addRaportMessage( "" );
						continue;
					}
					if ( type == ".obj" ) {
						var obj = OBJFile.FromFile( i );
						importedFiles.Add( obj );
						foreach ( var n in obj.ParsingErrors ) {
							addRaportMessage( n.Message, n.Severity );
						}
						addRaportMessage( $"Stats: {obj.Data.Vertices.Count.Pluralize( "Vertice" )}, {obj.Data.Faces.Count.Pluralize( "Face" )}, {obj.Objects.Count.Pluralize( "Object" )} and {obj.Groups.Count.Pluralize( "Group" )} (+{obj.SmoothingGroups.Count.Pluralize( "Smoothing Group" )} and {obj.MergingGroups.Count.Pluralize( "Merging Group" )})" );
						foreach ( var n in obj.Data.MTLFiles ) {
							addRaportMessage( $"Parsing: {Path.GetFileName( i )}+{Path.GetFileName( n.Path )}" );
							try {
								n.Load( Path.Combine( i, ".." ) );
								foreach ( var k in n.Source.ParsingErrors ) {
									addRaportMessage( k.Message, k.Severity );
								}
								addRaportMessage( $"Stats: {n.Source.Materials.Count.Pluralize( "Material" )}" );
							}
							catch ( Exception e ) {
								addRaportMessage( $"Unhandled exception while parsing file {Path.GetFileName( n.Path )}: {e.Message}", ParsingErrorSeverity.Error );
							}
						}
					}
					else if ( type == ".blend" ) {
						var blend = BlendFile.FromFile( i );
						importedFiles.Add( blend );
						addRaportMessage( $"Stats: {blend.Blocks.Count.Pluralize("Data block")}, {blend.GetAllOfType("Mesh").Count().Pluralize("Mesh")}" );
					}
				}
				catch ( Exception e ) {
					addRaportMessage( $"Unhandled exception while parsing file {Path.GetFileName( i )}: {e.Message}", ParsingErrorSeverity.Error );
					ok = false;
				}
				addRaportMessage( $"Finished parsing {Path.GetFileName( i )}: {( ok ? "Success" : "Error" )}", ok ? ParsingErrorSeverity.Success : ParsingErrorSeverity.Error );
				addRaportMessage( "" );
			}
			addRaportMessage( $"Finished parsing files. Loaded {importedFiles.Count.Pluralize( "file" )}." );
			addRaportMessage( "" );

			foreach ( var i in importedFiles ) {
				var group = i.CreateModelGroup();
				using var groups = StackPool<ImportedModelGroup>.Shared.Rent();
				groups.Push( group );

				while ( groups.Count != 0 ) {
					var top = groups.Pop();

					foreach ( var k in top.SubGroups ) 
						groups.Push( k );

					foreach ( var k in top.Models ) {
						foreach ( var (mesh,mat) in k.Elements ) {
							Schedule( () => SceneContainer.Add( new PropContainer( new GripableCollider { Mesh = mesh }, k.Name ) ) );
						}
					}
				}
			}
		}

		Dictionary<IConfigurableInspectable, Drawable[]> sections = new();
		private void childAdded ( Drawable3D parent, Drawable3D child ) {
			if ( child is IConfigurableInspectable c ) {
				addSubsections( c );
			}
		}

		private void childRemoved ( Drawable3D parent, Drawable3D child ) {
			if ( child is IConfigurableInspectable c ) {
				sections.Remove( c, out var removed );
				foreach ( var i in removed ) {
					RemoveSection( i, true );
				}
			}
		}

		// TODO later on scenery components will be able to selectively appear besed on ruleset

		void addSubsections ( IConfigurableInspectable configurable ) {
			var section = configurable.CreateInspectorSubsection();
			if ( configurable.CreateWarnings() is Drawable warning ) {
				AddSection( warning );
				AddSection( section );

				sections.Add( configurable, new Drawable[] { warning, section } );
			}
			else {
				AddSection( section );

				sections.Add( configurable, new Drawable[] { section } );
			}
		}
	}
}
