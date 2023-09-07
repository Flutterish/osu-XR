﻿using osu.Framework.Localisation;
using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.Input.Migration;
using osu.XR.IO;
using osu.XR.Localisation.Bindings;
using System.Text.Json;

namespace osu.XR.Input.Actions;

public class ClapBinding : ActionBinding, IHasBindingType {
	public override LocalisableString Name => TypesStrings.Clap;
	public override bool ShouldBeSaved => Action.ShouldBeSaved;
	public override Drawable CreateEditor () => new ClapEditor( this );
	public override ClapHandler CreateHandler () => new( this );

	public readonly RulesetAction Action = new();
	public readonly Bindable<double> ThresholdABindable = new( 0.325 );
	public readonly Bindable<double> ThresholdBBindable = new( 0.275 );

	public BindingType Type => BindingType.Clap;
	public ClapBinding () {
		TrackSetting( ThresholdABindable );
		TrackSetting( ThresholdBBindable );
		TrackSetting( Action );
	}

	protected override object CreateSaveData ( BindingsSaveContext context ) => new SaveData {
		Type = BindingType.Clap,
		ThresholdA = ThresholdABindable.Value,
		ThresholdB = ThresholdBBindable.Value,
		Action = context.SaveAction( Action )
	};

	public static ClapBinding? Load ( JsonElement data, BindingsSaveContext ctx ) => Load<ClapBinding, SaveData>( data, ctx, static ( save, ctx ) => {
		var clap = new ClapBinding();
		clap.ThresholdABindable.Value = save.ThresholdA;
		clap.ThresholdBBindable.Value = save.ThresholdB;
		ctx.LoadAction( clap.Action, save.Action );
		return clap;
	} );

	[FormatVersion( "" )]
	[FormatVersion( "[Initial]" )]
	public struct SaveData {
		public BindingType Type;
		public double ThresholdA;
		public double ThresholdB;
		public ActionData? Action;
	}
}
