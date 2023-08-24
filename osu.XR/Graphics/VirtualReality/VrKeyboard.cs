using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Parsing.Wavefront;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.XR.Allocation;
using osu.XR.Osu;
using osuTK.Graphics;
using osuTK.Input;
using TagLib.IFD;

namespace osu.XR.Graphics.VirtualReality;

public partial class VrKeyboard : CompositeDrawable3D {

	[BackgroundDependencyLoader]
	private void load ( MeshStore meshStore, OverlayColourProvider overlayColours ) {
		base.LoadComplete();
		var kb = meshStore.GetCollection( "keyboard" );
		foreach ( var i in kb.AllObjects ) {
			var mesh = i.MeshParts[0].Mesh.Mesh;
			AddInternal( new Model {
				Mesh = mesh,
				Tint = overlayColours.Background4
			} );

			if ( mesh is not ITriangleMesh tringular )
				continue;

			Panel panel;
			if ( tringular.FindFlatMeshPlane() is Plane plane ) {
				var rotation = plane.Normal.LookRotation();
				var rotationInverse = rotation.Inverted();
				var bb = new AABox( tringular.EnumerateVertices().Select( x => rotationInverse.Apply( x ) ) );
				bool flipped = plane.Normal.Z < 0;
				var forward = flipped ? -plane.Normal : plane.Normal;

				panel = new FlatPanel() {
					Position = rotation.Apply( bb.Center ) + forward * 0.05f,
					Scale = bb.Size / 2,
					Rotation = rotation
				};
				if ( !flipped )
					panel.ScaleX = -panel.ScaleX;
				panel.ContentSize = new( bb.Size.X * 64, bb.Size.Y * 64 );
			}
			else {
				var objMesh = (ObjFile.ObjMesh)mesh;

				panel = new ModelledPanel( objMesh );
				panel.ContentSize = new( tringular.BoundingBox.Size.X * 64, tringular.BoundingBox.Size.Y * 64 );
				panel.Position += Vector3.UnitZ * 0.05f;
			}

			var name = (i.Name ?? string.Empty).ToUpperInvariant();
			if ( name.Contains( '_' ) )
				name = name.Substring( 0, name.IndexOf( '_' ) );

			if ( !KeyboardKey.namedKeys.TryGetValue( name, out var key ) ) {
				throw new InvalidDataException();
			}

			panel.Content.Add( new Key( KeyboardKey.prefabs[key] ) );
			AddInternal( panel );
		}
	}

	partial class ModelledPanel : Panel {
		public ModelledPanel ( ObjFile.ObjMesh mesh ) {
			RenderStage = RenderingStage.Transparent;

			var indices = mesh.ElementBuffer.Indices;
			for ( int i = 0; i < indices.Count / 3; i++ ) {
				var (a, b, c) = (indices[i * 3], indices[i * 3 + 1], indices[i * 3 + 2]);
				Mesh.AddTriangle( new() {
					Position = mesh.Positions.Data[(int)a],
					UV = new() {
						X = mesh.UVs.Data[(int)a].U,
						Y = mesh.UVs.Data[(int)a].V
					}
				}, new() {
					Position = mesh.Positions.Data[(int)b],
					UV = new() {
						X = mesh.UVs.Data[(int)b].U,
						Y = mesh.UVs.Data[(int)b].V
					}
				}, new() {
					Position = mesh.Positions.Data[(int)c],
					UV = new() {
						X = mesh.UVs.Data[(int)c].U,
						Y = mesh.UVs.Data[(int)c].V
					}
				}, computeNormal: true );
			}
		}

		protected override bool ClearMeshOnInvalidate => false;
		protected override void RegenrateMesh () { }

		protected override void Update () {
			base.Update();
		}

		protected override Material GetDefaultMaterial ( MaterialStore materials ) { // TODO disable gamma for this one
			return materials.GetNew( osu.XR.Graphics.Materials.MaterialNames.PanelTransparent );
		}
	}

	partial class Key : CompositeDrawable {
		public readonly KeyboardKey Binding;
		Drawable baseDrawable;
		Drawable? shiftedDrawable;
		public Key ( KeyboardKey binding ) {
			Binding = binding;

			RelativeSizeAxes = Axes.Both;
			AddInternal( baseDrawable = new Container {
				Child = createDrawable( binding.Key, binding.Text ),
				RelativeSizeAxes = Axes.Both,
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );
			if ( binding.Key.ToString().Length != 1 && ( binding.ShiftedKey != null || binding.ShiftedText != null ) ) {
				AddInternal( shiftedDrawable = new Container {
					Child = createDrawable( binding.ShiftedKey ?? binding.Key, binding.ShiftedText ),
					Scale = new( 0.6f ),
					Position = new( 20, -40 ),
					RelativeSizeAxes = Axes.Both,
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre
				} );
			}

			AddInternal( new OsuAnimatedButton() {
				RelativeSizeAxes = Axes.Both,
				Action = () => {
					
				}
			} );
		}

		Drawable createDrawable ( osuTK.Input.Key key, string? text ) {
			if ( icons.TryGetValue( key, out var icon ) ) {
				return new SpriteIcon {
					RelativeSizeAxes = Axes.Both,
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					Icon = icon,
					Scale = new( 0.4f ),
					Position = key == osuTK.Input.Key.Enter ? new( 20, -40 ) : new()
				};
			}

			text = keyNames.GetValueOrDefault( key ) ?? text ?? key.ToString();
			if ( text.Length == 1 ) {
				text = text.ToUpperInvariant();
			}

			if ( text.Contains( ' ' ) ) {
				var textContainer = new OsuTextFlowContainer {
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					TextAnchor = Anchor.Centre,
					Text = text.Replace( ' ', '\n' ),
					Scale = new( 3 ),
					ParagraphSpacing = 0
				};

				return textContainer;
			}
			else {
				return new OsuSpriteText {
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					Text = text,
					Scale = new( 3 ),
					UseFullGlyphHeight = false
				};
			}
		}

		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
			return base.CreateChildDependencies( new MergedDepencencyContainer( parent.Get<OsuDependencies>(), parent ) );
		}

		public static Dictionary<osuTK.Input.Key, string> keyNames = new() {
			[osuTK.Input.Key.Escape] = "Esc",
			[osuTK.Input.Key.ShiftLeft] = "Shift",
			[osuTK.Input.Key.ShiftRight] = "Shift",
			[osuTK.Input.Key.ControlLeft] = "Ctrl",
			[osuTK.Input.Key.ControlRight] = "Ctrl",
			[osuTK.Input.Key.AltLeft] = "Alt",
			[osuTK.Input.Key.AltRight] = "Alt",
			[osuTK.Input.Key.Space] = "",
			[osuTK.Input.Key.PrintScreen] = "Screenshot",
			[osuTK.Input.Key.PageUp] = "Page Up",
			[osuTK.Input.Key.PageDown] = "Page Down",
			[osuTK.Input.Key.Delete] = "Del",
			[osuTK.Input.Key.Insert] = "Ins"
		};

		public static Dictionary<osuTK.Input.Key, IconUsage> icons = new() {
			[osuTK.Input.Key.Up] = FontAwesome.Solid.ArrowUp,
			[osuTK.Input.Key.Down] = FontAwesome.Solid.ArrowDown,
			[osuTK.Input.Key.Left] = FontAwesome.Solid.ArrowLeft,
			[osuTK.Input.Key.Right] = FontAwesome.Solid.ArrowRight,
			[osuTK.Input.Key.BackSpace] = FontAwesome.Solid.ChevronLeft,
			[osuTK.Input.Key.Enter] = OsuIcon.Logo
		};
	}
}

public record KeyboardKey( Key Key ) {
	public string? Text { get; init; }
	public Key? ShiftedKey { get; init; }
	public string? ShiftedText { get; init; }
	public bool IsCapsLockDisabled { get; init; }
	public bool MergeVariant { get; init; }

	static KeyboardKey () {
		foreach ( var i in prefabs.Values ) {
			if ( i.Text != null && i.ShiftedText != null )
				namedKeys.TryAdd( i.Text, i.Key );
		}

		foreach ( var i in Enum.GetValues<Key>() ) {
			namedKeys[i.ToString().ToUpperInvariant()] = i;
			if ( prefabs.ContainsKey( i ) )
				continue;

			var str = i.ToString();
			if ( str.Length == 1 ) {
				prefabs.Add( i, new( i ) { Text = str.ToLowerInvariant(), ShiftedText = str.ToUpperInvariant(), MergeVariant = true } );
			}
			else {
				prefabs.Add( i, new( i ) );
			}
		}
	}

	public static Dictionary<string, Key> namedKeys = new() {
		["ESC"] = Key.Escape,
		["CAPS"] = Key.CapsLock,
		["LSHIFT"] = Key.LShift,
		["RSHIFT"] = Key.RShift,
		["LCTRL"] = Key.LControl,
		["RCTRL"] = Key.RControl,
		["LALT"] = Key.LAlt,
		["RALT"] = Key.RAlt,
	};
	public static Dictionary<Key, KeyboardKey> prefabs = new() {
		[Key.Grave] = new( Key.Grave ) { ShiftedKey = Key.Tilde, Text = "`", ShiftedText = "~", IsCapsLockDisabled = true },
		[Key.Number1] = new( Key.Number1 ) { Text = "1", ShiftedText = "!" },
		[Key.Number2] = new( Key.Number2 ) { Text = "2", ShiftedText = "@" },
		[Key.Number3] = new( Key.Number3 ) { Text = "3", ShiftedText = "#" },
		[Key.Number4] = new( Key.Number4 ) { Text = "4", ShiftedText = "$" },
		[Key.Number5] = new( Key.Number5 ) { Text = "5", ShiftedText = "%" },
		[Key.Number6] = new( Key.Number6 ) { Text = "6", ShiftedText = "^" },
		[Key.Number7] = new( Key.Number7 ) { Text = "7", ShiftedText = "&" },
		[Key.Number8] = new( Key.Number8 ) { Text = "8", ShiftedText = "*" },
		[Key.Number9] = new( Key.Number9 ) { Text = "9", ShiftedText = "(" },
		[Key.Number0] = new( Key.Number0 ) { Text = "0", ShiftedText = ")" },
		[Key.Minus] = new( Key.Minus ) { Text = "-", ShiftedText = "_" },
		[Key.Plus] = new( Key.Plus ) { Text = "=", ShiftedText = "+" },
		[Key.BracketLeft] = new( Key.BracketLeft ) { Text = "[", ShiftedText = "{" },
		[Key.BracketRight] = new( Key.BracketRight ) { Text = "]", ShiftedText = "}" },
		[Key.Semicolon] = new( Key.Semicolon ) { Text = ";", ShiftedText = ":" },
		[Key.Quote] = new( Key.Quote ) { Text = "'", ShiftedText = "\"" },
		[Key.BackSlash] = new( Key.BackSlash ) { Text = "\\", ShiftedText = "|" },
		[Key.Comma] = new( Key.Comma ) { Text = ",", ShiftedText = "<" },
		[Key.Period] = new( Key.Period ) { Text = ".", ShiftedText = ">" },
		[Key.Slash] = new( Key.Slash ) { Text = "/", ShiftedText = "?" },
		[Key.Keypad1] = new( Key.Keypad1 ) { Text = "1" },
		[Key.Keypad2] = new( Key.Keypad2 ) { Text = "2" },
		[Key.Keypad3] = new( Key.Keypad3 ) { Text = "3" },
		[Key.Keypad4] = new( Key.Keypad4 ) { Text = "4" },
		[Key.Keypad5] = new( Key.Keypad5 ) { Text = "5" },
		[Key.Keypad6] = new( Key.Keypad6 ) { Text = "6" },
		[Key.Keypad7] = new( Key.Keypad7 ) { Text = "7" },
		[Key.Keypad8] = new( Key.Keypad8 ) { Text = "8" },
		[Key.Keypad9] = new( Key.Keypad9 ) { Text = "9" },
		[Key.Keypad0] = new( Key.Keypad0 ) { Text = "0" },
		[Key.KeypadDivide] = new( Key.KeypadDivide ) { Text = "/" },
		[Key.KeypadMultiply] = new( Key.KeypadMultiply ) { Text = "*" },
		[Key.KeypadSubtract] = new( Key.KeypadSubtract ) { Text = "-" },
		[Key.KeypadPeriod] = new( Key.KeypadPeriod ) { Text = "." }
	};
}