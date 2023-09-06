using osu.Game.Overlays.Settings;
using osu.XR.Graphics.Settings;
using osu.XR.Input.Actions;
using osu.XR.Input.Handlers;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class JoystickZoneEditor : FillFlowContainer {
	Container visualsContainer;
	FillFlowContainer actionsContainer;

	JoystickZoneHandler handler;
	JoystickZoneBinding source;

	BindableInt count = new();
	BindableDouble startAngle = new();
	BindableDouble arc = new();
	BindableDouble deadzone = new();

	SettingsSlider<int> countSlider;
	SettingsAngleTextBox startAngleText;
	SettingsAngleTextBox arcText;
	SettingsPercentageTextBox deadzoneText;
	public JoystickZoneEditor ( JoystickZoneBinding source ) {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		Spacing = new( 0, 6 );

		this.source = source;

		AddRange( new Drawable[] {
			handler = source.CreateHandler(),

			visualsContainer = new Container {
				Margin = new MarginPadding { Bottom = 16 },
				Padding = new MarginPadding { Horizontal = 20 },
				RelativeSizeAxes = Axes.X
			}.With( x => x.OnUpdate += x => x.Height = x.DrawWidth - 40 ),

			countSlider = new() { LabelText = @"Count", Current = count, ShowsDefaultIndicator = false },
			startAngleText = new() { LabelText = @"Offset", Current = startAngle },
			arcText = new() { LabelText = @"Arc", Current = arc },
			deadzoneText = new() { LabelText = @"Deadzone", Current = deadzone },

			actionsContainer = new FillFlowContainer {
				Margin = new() { Top = 16 },
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Spacing = new( 0, 6 )
			},

			new DangerousSettingsButton {
				Margin = new() { Top = 6 },
				Text = JoystickStrings.RemoveZone,
				Action = () => source.Parent?.Remove( source )
			}
		} );

		startAngle.BindTo( source.Offset );
		arc.BindTo( source.Arc );
		deadzone.BindTo( source.Deadzone );
		count.BindTo( source.Count );

		count.BindValueChanged( v => {
			visualsContainer.Clear( disposeChildren: true );
			actionsContainer.Clear( disposeChildren: true );
			arc.MaxValue = 360d / v.NewValue; // TODO only limit the visual so it can be reversed

			for ( int i = 0; i < v.NewValue; i++ ) {
				addVisual( i, 360d / v.NewValue * i );
			}
		}, true );
	}

	void addVisual ( int index, double offset ) {
		const string names = "ABCDEFGH";

		var visual = new JoystickZonePiece( drawOutline: index == count.Value - 1 ) {
			RelativeSizeAxes = Axes.Both,
			FillMode = FillMode.Fit,
			Origin = Anchor.TopCentre,
			Anchor = Anchor.TopCentre
		};

		visual.OffsetAngle.Value = offset;
		visual.ZoneStartAngle.BindTo( source.Offset );
		visual.ZoneDeltaAngle.BindTo( source.Arc );
		visual.DeadzonePercentage.BindTo( source.Deadzone );
		visual.JoystickPosition.BindTo( handler.JoystickPosition );

		visualsContainer.Add( visual );

		Container container;
		actionsContainer.Add( container = new Container {
			RelativeSizeAxes = Axes.X,
			AutoSizeAxes = Axes.Y,
			Child = new ActivationIndicator {
				Margin = new(),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre,
				Current = handler.Active[index]
			}
		} );
		if ( count.Value != 1 ) {
			container.Add( new LabelIcon {
				Text = names[index].ToString(),
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre,
				X = -40
			} );
			visual.Label.Text = names[index].ToString();
		}
		RulesetActionDropdown dropdown;
		actionsContainer.Add( dropdown = new RulesetActionDropdown() );
		dropdown.RulesetAction.BindTo( source.Actions[index] );
	}
}
