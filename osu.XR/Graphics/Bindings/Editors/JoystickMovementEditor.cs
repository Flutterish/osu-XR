﻿using osu.Game.Overlays.Settings;
using osu.XR.Input.Actions;
using osu.XR.Localisation.Bindings.Joystick;

namespace osu.XR.Graphics.Bindings.Editors;
public partial class JoystickMovementEditor : FillFlowContainer {
	JoystickPiece visual;
	SettingsSlider<double> slider;
	SettingsEnumDropdown<JoystickMovementType> type;

	public JoystickMovementEditor ( JoystickMovementBinding source ) {
		Direction = FillDirection.Vertical;
		RelativeSizeAxes = Axes.X;
		AutoSizeAxes = Axes.Y;
		AddRange( new Drawable[] {
			new Container {
				Child = visual = new JoystickPiece {
					RelativeSizeAxes = Axes.Both,
					FillMode = FillMode.Fit,
					Origin = Anchor.TopCentre,
					Anchor = Anchor.TopCentre
				},
				Margin = new MarginPadding { Bottom = 16 },
				Padding = new MarginPadding { Horizontal = 20 },
				RelativeSizeAxes = Axes.X
			}.With( x =>  x.OnUpdate += x => x.Height = x.DrawWidth - 40 ),
			type = new() {
				Current = source.MovementType
			},
			slider = new() {
				LabelText = MovementStrings.Distance,
				Current = source.Distance
			}
		} );

		type.Current.BindValueChanged( v => {
			if ( v.NewValue is JoystickMovementType.None )
				type.ClearNoticeText();
			else
				type.SetNoticeText( MovementStrings.Warning );
		}, true );
	}
}