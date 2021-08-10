using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osu.Framework.XR.Components;
using osu.Framework.XR.GameHosts;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Physics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.XR.Components;
using osu.XR.Components.Panels;
using osu.XR.Inspector;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace osu.XR.Input { // BUG there is something wrong when inputting text with different pointers. probably sometihing with panels having focus.
	public class XrKeyboard : CompositeDrawable3D, IInspectable {
		public readonly Bindable<KeyboardLayout> LayoutBindable = new( KeyboardLayout.Default );
		private List<XrKey> keys = new();
		Bindable<IFocusable> globalFocusBindable;

		[BackgroundDependencyLoader]
		private void load ( Bindable<IFocusable> globalFocusBindable ) {
			this.globalFocusBindable = globalFocusBindable.GetBoundCopy();
		}

		public XrKeyboard () {
			LayoutBindable.BindValueChanged( _ => remapKeys(), true );
			AutoOffsetAxes = Axes3D.All;
			EulerRotX = -0.1f;

			modifiers.BindCollectionChanged( (a,b) => {
				if ( b.Action == NotifyCollectionChangedAction.Add ) {
					foreach ( Key i in b.NewItems ) {
						( globalFocusBindable.Value as Panel )?.EmulatedInput.HoldKey( i );
						foreach ( var k in keys.Where( x => x.KeyBindalbe.Value.Key == i ) ) {
							k.MarkActive();
						}
					}
				}
				else if ( b.Action is NotifyCollectionChangedAction.Reset or NotifyCollectionChangedAction.Remove ) {
					foreach ( Key i in b.OldItems.Cast<Key>().Except( b.NewItems?.Cast<Key>() ?? Array.Empty<Key>() ) ) {
						( globalFocusBindable.Value as Panel )?.EmulatedInput.ReleaseKey( i );
						foreach ( var k in keys.Where( x => x.KeyBindalbe.Value.Key == i ) ) {
							k.Unmark();
						}
					}
				}

				foreach ( var i in keys ) {
					i.ApplyModifiers( modifiers.ToArray() );
				}
			} );
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			globalFocusBindable.BindValueChanged( v => {
				foreach ( var i in modifiers ) {
					( v.OldValue as Panel )?.EmulatedInput.ReleaseKey( i );
					( v.NewValue as Panel )?.EmulatedInput.HoldKey( i );
				}
			}, true );

			( Host as ExtendedRealityGameHost ).TextInput.IsActiveBindable.BindValueChanged( v => {
				if ( v.NewValue ) {
					this.Position = new Vector3( 0, 1, -0.3f );
				}
				else {
					this.Position = new Vector3( 0, 0, -10 );
				}
			}, true );
		}

		protected override void Update () {
			base.Update();
			//this.MoveTo( Game.Camera.Position + Game.Camera.Forward + Game.Camera.Down * 0.3f, 50 );
			//this.RotateTo( Game.Camera.Rotation * Quaternion.FromAxisAngle( Vector3.UnitX, -50f / 180 * MathF.PI ), 50 );
		}

		public void LoadModel ( string path ) {
			Task.Run( () => {
				var mesh = Mesh.MultipleFromOBJFile( path );
				ScheduleAfterChildren( () => loadMesh( mesh ) );
			} );
		}

		public void LoadModel ( IEnumerable<string> lines )
			=> loadMesh( Mesh.MultipleFromOBJ( lines ) );

		private void loadMesh ( IEnumerable<Mesh> keys ) {
			foreach ( var i in this.keys ) {
				i.Clicked -= onKeyPressed;
				i.Held -= OnKeyHeld;
				i.Released -= OnKeyReleased;
				i.Destroy();
			}
			this.keys.Clear();

			foreach ( var i in keys ) {
				var key = new XrKey { Mesh = i };
				key.Clicked += onKeyPressed;
				key.Held += OnKeyHeld;
				key.Released += OnKeyReleased;
				AddInternal( key );
				this.keys.Add( key );
			}

			// we have the keys sorted top-down left-right so its easy to visually map them in KeyboardLayout. This is going to change.
			this.keys = this.keys.OrderByDescending( k => k.Mesh.BoundingBox.Max.Z ).ThenBy( x => x.Mesh.BoundingBox.Min.X ).ToList();

			remapKeys();
		}

		private void remapKeys () {
			modifiers.Clear();
			foreach ( var (key,i) in keys.Zip( Enumerable.Range(0,keys.Count) ) ) {
				key.KeyBindalbe.Value = LayoutBindable.Value.Keys.ElementAtOrDefault( i );
			}
		}

		[Resolved]
		private GameHost Host { get; set; }
		private void onKeyPressed ( KeyboardKey key ) {
			if ( key.IsToggle ) {
				if ( modifiers.Contains( key.Key.Value ) ) {
					modifiers.Remove( key.Key.Value );
				}
				else {
					modifiers.Add( key.Key.Value );
				}
			}
			else {
				if ( globalFocusBindable.Value is null ) return;

				if ( key.Key is Key k ) {
					( globalFocusBindable.Value as Panel )?.EmulatedInput.PressKey( k );
				}

				if ( !(modifiers.Contains( Key.ControlLeft ) || modifiers.Contains( Key.ControlRight )) ) // workaround really, we need a better way
					( Host as ExtendedRealityGameHost ).TextInput.AppendText( key.GetComboFor( modifiers.ToArray() ) );
			}
		}

		private BindableList<Key> modifiers = new();
		private void OnKeyHeld ( KeyboardKey key ) {
			if ( key.IsToggle ) return;

			if ( key.Key is Key k && key.IsModifier ) {
				modifiers.Add( k );
			}
		}
		private void OnKeyReleased ( KeyboardKey key ) {
			if ( key.IsToggle ) return;

			if ( key.Key is Key k && key.IsModifier ) {
				modifiers.Remove( k );
			}
		}

		private class XrKey : CompositeDrawable3D {
			public readonly Bindable<KeyboardKey> KeyBindalbe = new();
			public Mesh Mesh {
				set => KeyMesh.Mesh = value;
				get => KeyMesh.Mesh;
			}
			Model KeyMesh = new();
			FlatPanel panel = new FlatPanel { CanHaveGlobalFocus = false };
			XrKeyDrawable drawable;

			public event Action<KeyboardKey> Clicked;
			public event Action<KeyboardKey> Held;
			public event Action<KeyboardKey> Released;

			public XrKey () {
				KeyMesh.MainTexture = Textures.Pixel( Color4.Gray ).TextureGL;
				AddInternal( KeyMesh );
				AddInternal( panel );
				panel.AutosizeBoth();

				drawable = new XrKeyDrawable { 
					KeyBindalbe = KeyBindalbe,
					Panel = panel
				};
				panel.Source.Add( drawable );
				drawable.Clicked += v => Clicked?.Invoke( v );
				drawable.Held += v => Held?.Invoke( v );
				drawable.Released += v => Released?.Invoke( v );
			}

			public void MarkActive () {
				drawable.MarkActive();
			}
			public void MarkInactive () {
				drawable.MarkInactive();
			}
			public void Unmark () {
				drawable.Unmark();
			}

			ulong meshver;
			protected override void Update () {
				base.Update();
				if ( meshver != Mesh.UpdateVersion ) {
					meshver = Mesh.UpdateVersion;

					panel.PanelHeight = Mesh.BoundingBox.Size.Z;
					panel.PanelWidth = Mesh.BoundingBox.Size.X;

					drawable.Height = Mesh.BoundingBox.Size.Z * 100;
					drawable.Width = Mesh.BoundingBox.Size.X * 100;

					panel.EulerRotX = MathF.PI / 2;
					panel.Position = KeyMesh.Centre;
					panel.Y += Mesh.BoundingBox.Size.Y / 2 + 0.01f;
				}
			}

			public void ApplyModifiers ( Key[] modifiers ) {
				drawable.ApplyModifiers( modifiers );
			}

			public bool IsColliderEnabled => ( (IHasCollider)panel ).IsColliderEnabled;
		}

		private class XrKeyDrawable : CompositeDrawable {
			OsuTextFlowContainer text;
			Box bg;
			public IBindable<KeyboardKey> KeyBindalbe { get; init; }
			public Panel Panel { get; init; }
			private string displayText = "";

			public XrKeyDrawable () {
				AutoSizeAxes = Axes.None;
				Origin = Anchor.Centre;
				Anchor = Anchor.Centre;

				text = new OsuTextFlowContainer( x => { x.Font = OsuFont.GetFont( Typeface.Torus, displayText.Length > 6 ? 30 : 35 ); } ) {
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre,
					RelativeSizeAxes = Axes.Both,
					TextAnchor = Anchor.Centre
				};
				// for consistant hover box
				AddInternal( new Box {
					AlwaysPresent = true,
					Colour = Color4.Transparent,
					RelativeSizeAxes = Axes.Both,
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre
				} );
				AddInternal( bg = new Box {
					AlwaysPresent = true,
					Colour = Color4.Transparent,
					RelativeSizeAxes = Axes.Both,
					Origin = Anchor.Centre,
					Anchor = Anchor.Centre
				} );
				AddInternal( text );
			}

			public void MarkActive () {
				text.FadeColour( Colour4.HotPink, 100 );
			}
			public void MarkInactive () {
				text.FadeColour( Colour4.Gray, 100 );
			}
			public void Unmark () {
				text.FadeColour( Colour4.White, 100 );
			}

			protected override void LoadComplete () {
				base.LoadComplete();

				KeyBindalbe.BindValueChanged( v => {
					ApplyModifiers( modifiers );
				}, true );
			}

			Key[] modifiers = Array.Empty<Key>();
			public void ApplyModifiers ( Key[] modifiers ) {
				this.modifiers = modifiers;

				applyText( text, modifiers );
			}

			private void applyText ( OsuTextFlowContainer text, IEnumerable<Key> modifiers ) {
				var value = KeyBindalbe.Value?.GetDisplayFor( modifiers.ToArray() ) ?? "";
				displayText = "";
				text.Clear( true );
				text.Scale = Vector2.One;

				if ( value == Key.Left.ToString() ) text.AddIcon( FontAwesome.Solid.ArrowLeft );
				else if ( value == Key.Right.ToString() ) text.AddIcon( FontAwesome.Solid.ArrowRight );
				else if ( value == Key.Up.ToString() ) text.AddIcon( FontAwesome.Solid.ArrowUp );
				else if ( value == Key.Down.ToString() ) text.AddIcon( FontAwesome.Solid.ArrowDown );
				else if ( value == "Host" ) text.AddIcon( OsuIcon.Logo ); // TODO either yeet this key or find usage and make the icon a circle with "XR!"
				else if ( value == "BackSpc" ) text.AddIcon( FontAwesome.Solid.Backspace );
				else if ( value == "Enter" ) {
					foreach ( var i in text.AddIcon( FontAwesome.Solid.LevelDownAlt ) )
						i.Rotation = 90;
				}
				else if ( value == "␣" ) {
					foreach ( var i in text.AddText( "[" ) ) {
						i.Scale = new Vector2( 2 );
						i.Rotation = -90;
					}
				}
				else {
					text.Text = displayText = value;
				}
			}

			protected override bool OnHover ( HoverEvent e ) {
				bg.FadeColour( new Color4( 0, 0, 0, 0.5f ), 200, Easing.Out );
				bg.ScaleTo( 0.85f, 200, Easing.Out );
			
				return true;
			}
			
			protected override void OnHoverLost ( HoverLostEvent e ) {
				bg.FadeColour( Color4.Transparent, 200, Easing.Out );
				bg.ScaleTo( 1, 200, Easing.Out );
			
				base.OnHoverLost( e );
			}

			public event Action<KeyboardKey> Clicked;
			public event Action<KeyboardKey> Held;
			public event Action<KeyboardKey> Released;
			protected override bool OnClick ( ClickEvent e ) {
				if ( KeyBindalbe.Value is not null )
					Clicked?.Invoke( KeyBindalbe.Value );

				bg.FadeColour( Color4.Transparent ).FlashColour( Color4.White, 200, Easing.In );

				return true;
			}

			protected override bool OnMouseDown ( MouseDownEvent e ) {
				if ( KeyBindalbe.Value is not null && KeyBindalbe.Value.IsModifier )
					Held?.Invoke( KeyBindalbe.Value );

				return true;
			}

			protected override void OnMouseUp ( MouseUpEvent e ) {
				if ( KeyBindalbe.Value is not null && KeyBindalbe.Value.IsModifier )
					Released?.Invoke( KeyBindalbe.Value );

				base.OnMouseUp( e );
			}
		}
	}
}
