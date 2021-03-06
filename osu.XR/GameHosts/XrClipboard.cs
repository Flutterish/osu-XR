using osu.Framework.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.GameHosts {
	public class XrClipboard : Clipboard {
		string copied;
		public override string GetText () {
			return copied;
		}

		public override void SetText ( string selectedText ) {
			copied = selectedText;
		}
	}
}
