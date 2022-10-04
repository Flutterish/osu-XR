using osu.Game.Configuration;

namespace osu.XR.Configuration;

public class OsuXrConfigManager : InMemoryConfigManager<OsuXrSetting> {
	protected override void InitialiseDefaults () {
		base.InitialiseDefaults();
		SetDefault( OsuXrSetting.InputMode, InputMode.SinglePointer );
		SetDefault( OsuXrSetting.SinglePointerTouch, false );
		SetDefault( OsuXrSetting.TapOnPress, false );
		SetDefault( OsuXrSetting.Deadzone, 20, 0, 100 );

		SetDefault( OsuXrSetting.ScreenArc, MathF.PI * 0.7f, MathF.PI / 18, MathF.PI * 2 );
		SetDefault( OsuXrSetting.ScreenRadius, 1.6f, 0.4f, 4 );
		SetDefault( OsuXrSetting.ScreenHeight, 1.8f, 0f, 3 );

		SetDefault( OsuXrSetting.ScreenResolutionX, 1920, 500, 7680 );
		SetDefault( OsuXrSetting.ScreenResolutionY, 1080, 400, 4320 );

		SetDefault( OsuXrSetting.DisableTeleport, false );
		SetDefault( OsuXrSetting.DominantHand, Hand.Auto );
		SetDefault( OsuXrSetting.ShadowType, FeetSymbols.None );
		SetDefault( OsuXrSetting.ShowDust, true );
		SetDefault( OsuXrSetting.SkyboxType, SkyBoxType.Solid );
	}
}
