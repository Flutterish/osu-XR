using Microsoft.EntityFrameworkCore.Metadata.Internal;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.News;
using osu.Game.Overlays.Settings;
using osu.XR.Components;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class InspectorPanel : CompositeDrawable {
		FillFlowContainer elements;
		TextFlowContainer elementName;
		public InspectorPanel () {
			AddInternal( new Box {
				RelativeSizeAxes = Axes.Both,
				Colour = OsuColour.Gray( 0.05f )
			} );
			AddInternal( new OsuScrollContainer {
				RelativeSizeAxes = Axes.Both,
				Child = elements = new FillFlowContainer {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Direction = FillDirection.Vertical
				}
			} );

			TextFlowContainer text = new( s => s.Font = OsuFont.GetFont( Typeface.Torus, 40 ) ) {
				Padding = new MarginPadding { Left = 15, Right = 15, Bottom = 25, Top = 15 },
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			};
			elements.Add( text );
			text.AddText( "Inspector" );
			text.AddParagraph( "inspect and modify properties", s => { s.Font = OsuFont.GetFont( Typeface.Torus, 18 ); s.Colour = Colour4.HotPink; } );
			text.AddParagraph( "these settings are not persistent", s => { s.Font = OsuFont.GetFont( Typeface.Torus, 18 ); s.Colour = Colour4.HotPink; } );

			elements.Add( new SettingsCheckbox { LabelText = "Select element to inspect", Current = IsSelectingBindable } );
			elements.Add( elementName = new TextFlowContainer( s => s.Font = OsuFont.GetFont( Typeface.Torus, 20 ) ) {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = 15, Right = 15 }
			} );

			SelectedElementBindable.BindValueChanged( v => {
				setSelected( v.NewValue );
			}, true );
		}

		string elementLabel {
			set {
				elementName.Text = "Selected: ";
				elementName.AddText( value, s => s.Font = s.Font = OsuFont.GetFont( Typeface.Torus, 20, FontWeight.Bold ) );
			}
		}
		readonly List<SettingsSubsection> subsections = new();
		public readonly Bindable<Drawable3D> SelectedElementBindable = new();
		private void setSelected ( Drawable3D element ) {
			elements.RemoveAll( x => subsections.Contains( x ) );
			subsections.Clear();
			
			if ( element is null ) {
				elementLabel = "Nothing";
				return;
			}

			elementLabel = string.IsNullOrWhiteSpace( element.Name ) ? element.GetType().Name : element.Name;
			subsections.Add( new TransformInspectorSection( element ) );
			if ( element is IInspectable inspectable ) {
				subsections.AddRange( inspectable.CreateInspectorSubsections() );
			}
			
			elements.AddRange( subsections );
		}

		public readonly BindableBool IsSelectingBindable = new( false );
	}

	public class TransformInspectorSection : SettingsSubsection {
		private Drawable3D drawable;
		public TransformInspectorSection ( Drawable3D drawable ) {
			this.drawable = drawable;
			scaleX.Value = drawable.ScaleX.ToString();
			scaleY.Value = drawable.ScaleY.ToString();
			scaleZ.Value = drawable.ScaleZ.ToString();

			rotX.Value = drawable.EulerRotX.ToString();
			rotY.Value = drawable.EulerRotY.ToString();
			rotZ.Value = drawable.EulerRotZ.ToString();

			posX.Value = drawable.X.ToString();
			posY.Value = drawable.Y.ToString();
			posZ.Value = drawable.Z.ToString();
		}
		
		protected override string Header => "Transform";

		private readonly Bindable<string> scaleX = new();
		private readonly Bindable<string> scaleY = new();
		private readonly Bindable<string> scaleZ = new();

		private readonly Bindable<string> rotX = new();
		private readonly Bindable<string> rotY = new();
		private readonly Bindable<string> rotZ = new();

		private readonly Bindable<string> posX = new();
		private readonly Bindable<string> posY = new();
		private readonly Bindable<string> posZ = new();
		protected override void LoadComplete () {
			base.LoadComplete();

			Add( new TripleSettingsTextBox {
				LabelText = "Scale",
				CurrentA = scaleX,
				CurrentB = scaleY,
				CurrentC = scaleZ
			} );

			Add( new TripleSettingsTextBox {
				LabelText = "Rotation",
				CurrentA = rotX,
				CurrentB = rotY,
				CurrentC = rotZ
			} );

			Add( new TripleSettingsTextBox {
				LabelText = "Position",
				CurrentA = posX,
				CurrentB = posY,
				CurrentC = posZ
			} );
		}
	}

	public class SettingsTextBox : CompositeDrawable {
		SpriteText label;
		OsuTextBox textBox;
		public SettingsTextBox () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Margin = new MarginPadding { Left = 15 };

			AddInternal( new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					label = new SpriteText { 
						Font = OsuFont.Default,
						Height = 16,
						RelativeSizeAxes = Axes.X
					},
					textBox = new OsuTextBox {
						RelativeSizeAxes = Axes.X,
						Height = 30
					}
				}
			} );

			textBox.Current = current;
		}

		BindableWithCurrent<string> current = new();
		public Bindable<string> Current {
			set => current.Current = value;
		}

		public string LabelText {
			set => label.Text = value;
		}
	}

	public class TripleSettingsTextBox : CompositeDrawable {
		SpriteText label;
		OsuTextBox textBoxA;
		OsuTextBox textBoxB;
		OsuTextBox textBoxC;

		public TripleSettingsTextBox () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Margin = new MarginPadding { Left = 15 };

			AddInternal( new FillFlowContainer {
				Direction = FillDirection.Vertical,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					label = new SpriteText {
						Font = OsuFont.Default,
						Height = 16,
						RelativeSizeAxes = Axes.X
					},
					new Container {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Children = new Drawable[] {
							textBoxA = new OsuTextBox {
								RelativeSizeAxes = Axes.X,
								Height = 30,
								Width = 0.3f,
								Origin = Anchor.CentreLeft,
								Anchor = Anchor.CentreLeft
							},
							textBoxB = new OsuTextBox {
								RelativeSizeAxes = Axes.X,
								Height = 30,
								Width = 0.3f,
								Origin = Anchor.Centre,
								Anchor = Anchor.Centre
							},
							textBoxC = new OsuTextBox {
								RelativeSizeAxes = Axes.X,
								Height = 30,
								Width = 0.3f,
								Origin = Anchor.CentreRight,
								Anchor = Anchor.CentreRight
							}
						}
					}
				}
			} );

			textBoxA.Current = currentA;
			textBoxB.Current = currentB;
			textBoxC.Current = currentC;
		}

		protected override void Update () {
			base.Update();
			InternalChild.Width = DrawWidth - 30;
		}

		BindableWithCurrent<string> currentA = new();
		public Bindable<string> CurrentA {
			set => currentA.Current = value;
		}
		BindableWithCurrent<string> currentB = new();
		public Bindable<string> CurrentB {
			set => currentB.Current = value;
		}
		BindableWithCurrent<string> currentC = new();
		public Bindable<string> CurrentC {
			set => currentC.Current = value;
		}
		public string LabelText {
			set => label.Text = value;
		}
	}
}
