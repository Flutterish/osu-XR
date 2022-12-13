﻿using osu.Game.Overlays.Settings;
using osu.XR.Input.Actions;
using osu.XR.Localisation.Bindings;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class JoystickZoneEditor : FillFlowContainer {
	JoystickZonePiece visual;
	RulesetActionDropdown dropdown;
	ActivationIndicator indicator;
	public JoystickZoneEditor ( JoystickZoneBinding source ) {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		AddRange( new Drawable[] {
			new Container {
				Child = visual = new JoystickZonePiece {
					RelativeSizeAxes = Axes.Both,
					FillMode = FillMode.Fit,
					Origin = Anchor.TopCentre,
					Anchor = Anchor.TopCentre
				},
				Margin = new MarginPadding { Bottom = 16 },
				Padding = new MarginPadding { Horizontal = 20 },
				RelativeSizeAxes = Axes.X
			}.With( x =>  x.OnUpdate += x => x.Height = x.DrawWidth - 40 ),
			new Container {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Child = indicator = new ActivationIndicator {
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre
				}
			},
			dropdown = new(),
			new DangerousSettingsButton {
				Text = JoystickStrings.RemoveZone,
				Action = () => RemoveRequested?.Invoke( this )
			}
		} );

		visual.ZoneStartAngle.BindTo( source.StartAngle );
		visual.ZoneDeltaAngle.BindTo( source.Arc );
		visual.DeadzonePercentage.BindTo( source.Deadzone );

		dropdown.RulesetAction.BindTo( source.Action );
	}

	public event Action<JoystickZoneEditor>? RemoveRequested;
}
