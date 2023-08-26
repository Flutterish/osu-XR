using OpenVR.NET.Manifest;

namespace osu.XR.VirtualReality;

public class OsuXrAppManifest : VrManifest {
	public OsuXrAppManifest () {
		AppKey = "osu-xr";
		ActionManifestPath = "ActionManifest.json";
		WindowsPath = Path.Combine( Directory.GetCurrentDirectory(), "osu.XR.Desktop.exe" );
		IsDashboardOverlay = false;
		LocalizedNames = new() {
			["en_us"] = new() {
				Name = "osu!XR",
				Description = "The full osu! experience in VR"
			}
		};
	}
}
