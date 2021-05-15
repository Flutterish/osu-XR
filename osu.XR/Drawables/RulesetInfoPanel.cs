using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.XR.Input.Custom;
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

		protected override void LoadComplete () {
			base.LoadComplete();

			ruleset.BindValueChanged( v => {
				if ( v.NewValue is null ) return;

				container.RemoveAll( x => sections.Contains( x ) );
				sections.Clear();

				rulesetName.Text = "Ruleset: ";
				rulesetName.AddText( v.NewValue.Name, s => s.Font = s.Font = OsuFont.GetFont( Typeface.Torus, 20, FontWeight.Bold ) );

				var ruleset = v.NewValue.CreateInstance();
				sections.Add( new RulesetXrBindingsSubsection( ruleset ) );

				container.AddRange( sections );
			}, true );
		}
	}

	public class RulesetXrBindingsSubsection : SettingsSubsection {
		protected override string Header => "Bindings (Not functional)";

		Ruleset ruleset;
		Dictionary<object,CustomInput> rulesetActions = new();
		public RulesetXrBindingsSubsection ( Ruleset ruleset ) {
			this.ruleset = ruleset;
			foreach ( var i in ruleset.GetDefaultKeyBindings().Select( x => x.Action ).Distinct() ) {
				rulesetActions.Add( i, null );
			}

			foreach ( var (i,_) in rulesetActions ) {
				List<CustomInput> customInputs = new() {
					new ClapInput(),
					new JoystickInput { Hand = Settings.Hand.Left },
					new JoystickInput { Hand = Settings.Hand.Right },
					new ButtonInput { Hand = Settings.Hand.Left },
					new ButtonInput { Hand = Settings.Hand.Right }
				};

				SettingsDropdown<string> dropdown;
				Add( dropdown = new SettingsDropdown<string> {
					LabelText = i.ToString(),
					Current = new Bindable<string>( "Default" ),
					Items = customInputs.Select( x => x.Name ).Prepend( "Default" )
				} );
				Container setttingContainer;
				Add( setttingContainer = new Container {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y
				} );

				dropdown.Current.ValueChanged += v => {
					if ( v.NewValue == "Default" ) {
						rulesetActions[ i ] = null;
						setttingContainer.Clear( false );
					}
					else {
						rulesetActions[ i ] = customInputs.First( x => x.Name == v.NewValue );
						setttingContainer.Clear( false );
						setttingContainer.Add( rulesetActions[ i ].SettingDrawable );
					}
				};
			}
		}
	}
}
