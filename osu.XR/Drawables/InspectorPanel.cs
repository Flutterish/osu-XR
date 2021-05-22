using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.XR.Components;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Components;
using osu.XR.Inspector;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

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
			elements.Add( new SettingsCheckbox { LabelText = "Granular selection", Current = GranularSelectionBindable } );
			elements.Add( elementName = new TextFlowContainer( s => s.Font = OsuFont.GetFont( Typeface.Torus, 20 ) ) {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = 15, Right = 15 }
			} );

			InspectedElementBindable.BindValueChanged( v => setInspected( v.NewValue ), true );
			SelectedElementBindable.BindValueChanged( v => {
				if ( GranularSelectionBindable.Value ) {
					InspectedElementBindable.Value = v.NewValue.GetValidInspectable();
				}
				else {
					InspectedElementBindable.Value = ( v.NewValue?.GetClosestInspectable() as Drawable3D ) ?? v.NewValue?.GetValidInspectable();
				}
			} );
		}

		Selection selection = new();
		Selection helperSelection = new() { Tint = Color4.Yellow };

		protected override void Update () {
			base.Update();
			selection.IsVisible = IsPresent && InspectedElementBindable.Value is not null;
			if ( selection.IsVisible ) {
				selection.Select( InspectedElementBindable.Value );
			}
		}

		string elementLabel {
			set {
				elementName.Text = "Selected: ";
				elementName.AddText( value, s => s.Font = s.Font = OsuFont.GetFont( Typeface.Torus, 20, FontWeight.Bold ) );
			}
		}
		readonly List<SettingsSubsection> subsections = new();
		public readonly Bindable<Drawable3D> SelectedElementBindable = new();
		private void setInspected ( Drawable3D element ) {
			if ( element is not null ) selection.Parent = element.Root;

			elements.RemoveAll( x => subsections.Contains( x ) );
			subsections.Clear();
			
			if ( element is null ) {
				elementLabel = "Nothing";
				return;
			}

			elementLabel = element.GetInspectorName();
			subsections.Add( new TransformInspectorSection( element ) );
			if ( element is IConfigurableInspectable inspectable ) {
				subsections.AddRange( inspectable.CreateInspectorSubsections() );
			}
			subsections.Add( new HierarchyInspectorSubsection( element, v => InspectedElementBindable.Value = v ) {
				DrawableHovered = d => {
					if ( helperSelection.Selected == d ) return;

					helperSelection.Parent = d?.Root;
					helperSelection.Select( d );
				}
			} );
			
			elements.AddRange( subsections );
		}

		public readonly Bindable<Drawable3D> InspectedElementBindable = new();
		public readonly BindableBool IsSelectingBindable = new( false );
		public readonly BindableBool GranularSelectionBindable = new( false );
	}

	public class TransformInspectorSection : SettingsSubsection {
		private Drawable3D drawable;
		public TransformInspectorSection ( Drawable3D drawable ) {
			this.drawable = drawable;
		}
		
		protected override string Header => "Transform (Preview)";

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

		double timer;
		protected override void Update () {
			base.Update();
			timer += Time.Elapsed;
			if ( timer < 500 ) {
				return;
			}

			scaleX.Value = drawable.ScaleX.ToString();
			scaleY.Value = drawable.ScaleY.ToString();
			scaleZ.Value = drawable.ScaleZ.ToString();

			rotX.Value = drawable.EulerRotX.ToString();
			rotY.Value = drawable.EulerRotY.ToString();
			rotZ.Value = drawable.EulerRotZ.ToString();

			posX.Value = drawable.X.ToString();
			posY.Value = drawable.Y.ToString();
			posZ.Value = drawable.Z.ToString();

			timer = 0;
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

	public class HierarchyInspectorSubsection : SettingsSubsection {
		protected override string Header => "Hierarchy";

		Drawable3D drawable;
		FillFlowContainer list;
		Action<Drawable3D> drawableSelected;
		public Action<Drawable3D> DrawableHovered = _ => { };
		public HierarchyInspectorSubsection ( Drawable3D drawable, Action<Drawable3D> drawableSelected ) {
			this.drawableSelected = drawableSelected;
			this.drawable = drawable;
			AutoSizeAxes = Axes.Y;
			RelativeSizeAxes = Axes.X;
			Add( list = new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			} );

			refresh();
			if ( drawable is CompositeDrawable3D comp ) {
				comp.ChildAdded += (_,_) => refresh();
				comp.ChildRemoved += (_,_) => refresh();
			}
		}

		void addButton ( Drawable3D drawable, string name, bool enabled = true, float width = 1 ) {
			OsuButton button;
			list.Add( button = new HoverableOsuButton {
				RelativeSizeAxes = Axes.X,
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre,
				Width = width,
				Height = 15,
				Text = name,
				Action = () => drawableSelected( drawable ),
				Hovered = () => DrawableHovered( drawable ),
				HoverEnd = () => DrawableHovered( null )
			} );
			button.Enabled.Value = enabled;
		}

		private class HoverableOsuButton : OsuButton {
			public Action Hovered;
			public Action HoverEnd;

			protected override void OnHoverLost ( HoverLostEvent e ) {
				HoverEnd?.Invoke();
				base.OnHoverLost( e );
			}

			protected override void Update () {
				base.Update();
				if ( IsHovered ) Hovered?.Invoke();
			}
		}

		void refresh () {
			Drawable3D parent = drawable.Parent?.GetValidInspectable();
			if ( parent is not null ) {
				addButton( parent, $".. ({parent.GetInspectorName()})" );
			}
			addButton( drawable, drawable.GetInspectorName(), enabled: false, width: 0.9f );

			void addRange ( Drawable3D drawable ) {
				if ( drawable is CompositeDrawable3D comp ) {
					foreach ( var i in comp.Children ) {
						if ( i is INotInspectable ) {
							addRange( i );
						}
						else {
							addButton( i, i.GetInspectorName() );
						}
					}
				}
			}

			addRange( drawable );
		}
	}
}
