using FFmpeg.AutoGen;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.XR.Inspector.Components.Editors;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Reflections {
	public class ReflectionsInspectorPrimitive : ReflectionsInspectorComponent {
		OsuTextFlowContainer text;
		Circle background;

		FillFlowContainer flow;
		Container container;
		public ReflectionsInspectorPrimitive ( ReflectionsInspector source ) : base( source ) {
			AddInternal( flow = new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,
				Children = new Drawable[] {
					container = new Container {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Children = new Drawable[] {
							background = new Circle {
								RelativeSizeAxes = Axes.Y,
								AlwaysPresent = true,
								Colour = Color4.Transparent,
								Margin = new MarginPadding { Left = 10 }
							},
							text = new OsuTextFlowContainer {
								Margin = new MarginPadding { Horizontal = 15 },
								AutoSizeAxes = Axes.Y
							}
						}
					}
				}
			} );

			if ( source.IsValueEditable ) {
				if ( source.ValueSetter != null && ValueEditor.HasEditorFor( source.TargetType ) ) {
					makeEditable( ValueEditor.GetEditorFor( source.TargetType, source.TargetValue ), v => source.ValueSetter( v ) );
				}
				else {
					var bindableType = source.TargetType.GetBaseType( typeof( Bindable<> ) );
					if ( bindableType is not null && ValueEditor.HasEditorFor( bindableType.GenericTypeArguments[ 0 ] ) ) {
						object value = source.TargetValue.GetProperty<object>( "Value" );
						makeEditable( ValueEditor.GetEditorFor( bindableType.GenericTypeArguments[ 0 ], value ), v => source.TargetValue.SetProperty<object>( "Value", v ) );

						OnUpdate += _ => {
							object newValue = source.TargetValue.GetProperty<object>( "Value" );
							if ( value is null ? newValue is not null : !value.Equals( newValue ) ) {
								value = newValue;
								UpdateValue();
							}
						};
					}
				}
			}

			UpdateValue();
		}

		public static bool CanEdit ( Type t ) {
			if ( ValueEditor.HasEditorFor( t ) ) return true;
			var bindableType = t.GetBaseType( typeof( Bindable<> ) );
			if ( bindableType is not null && ValueEditor.HasEditorFor( bindableType.GenericTypeArguments[ 0 ] ) ) return true;
			return false;
		}

		void makeEditable ( ValueEditor editor, Action<object> action ) {
			flow.Add( editor );
			editor.Current.ValueChanged += v => {
				try {
					action( v.NewValue );
				}
				catch { }
			};

			editor.Alpha = 0;
			editor.Scale = new Vector2( 1, 0 );
			bool editorVisible = false;

			container.Add( new OsuButton {
				Height = 15,
				Width = 60,
				Text = "Edit",
				Anchor = Anchor.CentreRight,
				Origin = Anchor.CentreRight,
				Margin = new MarginPadding { Right = 15 },
				Action = () => {
					editorVisible = !editorVisible;
					if ( editorVisible ) {
						editor.ScaleTo( new Vector2( 1, 1 ), 200 );
						editor.FadeIn( 200 );
					}
					else {
						editor.ScaleTo( new Vector2( 1, 0 ), 200 );
						editor.FadeOut( 200 );
					}
				}
			} );
		}

		protected override void Update () {
			base.Update();

			text.Width = flow.DrawWidth - 30;
		}

		public override void UpdateValue () {
			text.Clear( true );
			text.AddText( $"{Source.TargetName} ", v => v.Colour = Color4.GreenYellow );
			text.AddText( $"[{Source.TargetType.ReadableName()}]: ", v => v.Colour = Color4.LimeGreen );
			if ( Source.TargetValue is Exception ) {
				text.AddText( StringifiedValue, v => v.Colour = Color4.Red );
			}
			else {
				if ( Source.TargetType == typeof( Color4 ) ) {
					text.AddIcon( FontAwesome.Solid.Palette, v => v.Colour = (Color4)Source.TargetValue );
				}
				else {
					text.AddText( StringifiedValue );
				}
			}

			background.FlashColour( Color4.HotPink.Opacity( 0.6f ), 500, Easing.In );
			background.Width = DrawWidth - 20;
		}
	}
}
