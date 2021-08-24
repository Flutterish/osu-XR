using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Components;
using osu.Framework.XR.Parsing;
using osu.Framework.XR.Parsing.WaveFront;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.XR.Components;
using osu.XR.Components.Skyboxes;
using osu.XR.Drawables.Containers;
using osu.XR.Editor;
using osu.XR.Inspector;
using osu.XR.Panels.Overlays;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
				raport.AddParagraph( $"{message} Some things might not display properly.", s => s.Colour = color );
			}
			else {
				raport.AddParagraph( message, s => s.Colour = color );
			}
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
							overlays.RequestOverlay<FileSelectionOverlay>().Confirmed += ImportProps;
						},
						TooltipText = "Import .obj files"
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

				// TODO these will not be instantiated here
				var skybox = new SkyBox();
				var floorgrid = new FloorGrid();
				var dust = new DustEmitter();

				v.NewValue.Add( skybox );
				v.NewValue.Add( floorgrid );
				v.NewValue.Add( dust );
				//SceneContainer.Add( new BeatingScenery.GripableCollider { Mesh = Mesh.UnitCube, Scale = new osuTK.Vector3( 0.3f ), Y = 1 } );
			} );
		}

		public static readonly IReadOnlyList<string> SupportedFileFormats = new List<string> {
			".obj"
		}.AsReadOnly();
		public void ImportProps ( IEnumerable<string> files ) {
			raport.Clear();
			List<OBJFile> importedFiles = new(); // TODO pooled lists
			foreach ( var i in files ) {
				bool ok = true;
				addRaportMessage( $"Parsing: {Path.GetFileName( i )}" );
				try {
					if ( Directory.Exists( i ) ) {
						addRaportMessage( "This is a directory, not a model file.", ParsingErrorSeverity.Error );
						raport.AddParagraph( "" );
						continue;
					}
					var type = Path.GetExtension( i );
					if ( !SupportedFileFormats.Contains( type ) ) {
						addRaportMessage( $"This a {type} file, which is not supported. Supported file formats: {string.Join(", ", SupportedFileFormats)}", ParsingErrorSeverity.Error );
						raport.AddParagraph( "" );
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
				}
				catch ( Exception e ) {
					addRaportMessage( $"Unhandled exception while parsing file {Path.GetFileName( i )}: {e.Message}", ParsingErrorSeverity.Error );
					ok = false;
				}
				addRaportMessage( $"Finished parsing {Path.GetFileName( i )}: {( ok ? "Success" : "Error" )}", ok ? ParsingErrorSeverity.Success : ParsingErrorSeverity.Error );
				raport.AddParagraph( "" );
			}
			addRaportMessage( $"Finished parsing files. Loaded {importedFiles.Count.Pluralize( "file" )}." );
			// TODO actually import them
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
