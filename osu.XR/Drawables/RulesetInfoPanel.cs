using Microsoft.CodeAnalysis;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.XR.Input.Custom;
using osu.XR.Settings;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class RulesetInfoPanel : CompositeDrawable {
		FillFlowContainer container;
		public RulesetInfoPanel () {
			AddInternal( new Box {
				RelativeSizeAxes = Axes.Both,
				Colour = OsuColour.Gray( 0.05f )
			} );
			AddInternal( new OsuScrollContainer {
				RelativeSizeAxes = Axes.Both,
				Child = container = new FillFlowContainer {
					Direction = FillDirection.Vertical,
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y
				}
			} );

			TextFlowContainer text = new( s => s.Font = OsuFont.GetFont( Typeface.Torus, 40 ) ) {
				Padding = new MarginPadding { Left = 15, Right = 15, Bottom = 25, Top = 15 },
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			};
			container.Add( text );
			text.AddText( "Ruleset" );
			text.AddParagraph( "adjust how you play the ruleset in XR", s => { s.Font = OsuFont.GetFont( Typeface.Torus, 18 ); s.Colour = Colour4.HotPink; } );

			container.Add( rulesetName = new TextFlowContainer( s => s.Font = OsuFont.GetFont( Typeface.Torus, 20 ) ) {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = 15, Right = 15 }
			} );
		}
		TextFlowContainer rulesetName;

		[Resolved]
		private IBindable<RulesetInfo> ruleset { get; set; }
		List<Drawable> sections = new();
		public BindableList<CustomBinding> CurrentBindings => settings[ ruleset.Value ].ActiveInputs;

		Dictionary<RulesetInfo, RulesetXrBindingsSubsection> settings = new();
		protected override void LoadComplete () {
			base.LoadComplete();

			ruleset.BindValueChanged( v => {
				if ( v.NewValue is null ) return;

				container.RemoveAll( x => sections.Contains( x ) );
				sections.Clear();

				rulesetName.Text = "Ruleset: ";
				rulesetName.AddText( v.NewValue.Name, s => s.Font = s.Font = OsuFont.GetFont( Typeface.Torus, 20, FontWeight.Bold ) );

				if ( !settings.ContainsKey( v.NewValue ) ) {
					var ruleset = v.NewValue.CreateInstance();
					settings.Add( v.NewValue, new RulesetXrBindingsSubsection( ruleset ) );
				}
				sections.Add( settings[ v.NewValue ] );

				container.AddRange( sections );
			}, true );
		}
	}

	public class RulesetXrBindingsSubsection : SettingsSubsection {
		protected override string Header => "Bindings (Not saveable)";

		Ruleset ruleset;
		[Cached]
		BindableList<object> sharedSettings = new(); 
		[Cached]
		List<object> rulesetActions = new();

		static Dictionary<string, Func<CustomBinding>> avaiableInputs = new() {
			[ "Clap" ] = () => new ClapBinding(),
			[ "Left Joystick" ] = () => new JoystickBinding( Hand.Left ),
			[ "Right Joystick" ] = () => new JoystickBinding( Hand.Right ),
			[ "Left Buttons" ] = () => new ButtonBinding( Hand.Left ),
			[ "Right Buttons" ] = () => new ButtonBinding( Hand.Right )
		};

		Dictionary<string, CustomBinding> removedInputs = new();
		public readonly BindableList<CustomBinding> ActiveInputs = new();
		Dictionary<string, CustomBinding> selectedInputs = new();
		Dictionary<string, Drawable> inputDrawables = new();
		FillFlowContainer container;
		OsuButton addButton;
		SettingsDropdown<string> dropdown;

		public RulesetXrBindingsSubsection ( Ruleset ruleset ) {
			this.ruleset = ruleset;
			rulesetActions = ruleset.GetDefaultKeyBindings().Select( x => x.Action ).Distinct().ToList();

			Add( container = new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical
			} );

			container.Add( addButton = new OsuButton {
				Height = 25,
				Width = 120,
				Margin = new MarginPadding { Left = 16 },
				Text = "Add",
				Action = () => {
					addCustomInput( dropdown.Current.Value );
				}
			} );

			container.Add( dropdown = new SettingsDropdown<string> {
				Current = new Bindable<string>( "Select type" )
			} );

			dropdown.Current.BindValueChanged( v => {
				addButton.Enabled.Value = v.NewValue != dropdown.Current.Default;
			}, true );

			updateDropdown();
		}

		protected override void Update () {
			foreach ( var i in inputDrawables.Values ) {
				i.Width = container.DrawWidth - 32;
			}
			base.Update();
		}

		void updateDropdown () {
			dropdown.Items = avaiableInputs.Keys.Except( selectedInputs.Keys ).Prepend( dropdown.Current.Default );
			dropdown.Current.SetDefault();
		}

		void addCustomInput ( string ID ) {
			CustomBinding input;
			if ( removedInputs.ContainsKey( ID ) ) {
				removedInputs.Remove( ID, out input );
				selectedInputs.Add( ID, input );
				ActiveInputs.Add( input );

				container.Add( inputDrawables[ ID ] );
				updateDropdown();
				return;
			}
			
			input = avaiableInputs[ ID ]();

			selectedInputs.Add( ID, input );
			ActiveInputs.Add( input );

			var handler = input.CreateHandler();
			inputDrawables.Add( ID, new Container {
				Masking = true,
				CornerRadius = 5,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = 16, Right = 16, Bottom = 4 },
				Children = new Drawable[] {
					new Box {
						RelativeSizeAxes = Axes.Both,
						Colour = OsuColour.Gray( 0.1f )
					},
					new FillFlowContainer {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Direction = FillDirection.Vertical,
						Children = new Drawable[] {
							new Container {
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y,
								Margin = new MarginPadding { Bottom = 8 },
								Children = new Drawable[] {
									new OsuTextFlowContainer( x => x.Font = OsuFont.GetFont( size: 24 ) ) {
										Text = ID,
										Margin = new MarginPadding { Bottom = 4, Left = 6, Top = 4 },
										RelativeSizeAxes = Axes.X,
										AutoSizeAxes = Axes.Y
									},
									new OsuButton {
										Anchor = Anchor.CentreRight,
										Origin = Anchor.CentreRight,
										Text = "X",
										BackgroundColour = Color4.HotPink,
										Action = () => removeCustomInput( ID ),
										Width = 25,
										Height = 25
									}
								}
							},
							new Container {
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y,
								Children = new Drawable[] {
									handler,
									handler.CreateSettingsDrawable()
								}
							}
						}
					}
				}
			} );
			container.Add( inputDrawables[ ID ] );
			updateDropdown();
		}

		void removeCustomInput ( string ID ) {
			container.Remove( inputDrawables[ ID ] );
			selectedInputs.Remove( ID, out var input );
			ActiveInputs.Remove( input );
			removedInputs.Add( ID, input );
			updateDropdown();
		}
	}
}
