using osu.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.GameHosts {
	public class XrTextInput : ITextInputSource {
		private string pending;

		public void AppendText ( string text ) {
			this.pending += text;
		}

		public string GetPendingText () {
			try {
				return pending;
			}
			finally {
				pending = string.Empty;
			}
		}

		public void Deactivate ( object sender ) { }

		public void Activate ( object sender ) { }

		public bool ImeActive => false;

		public event Action<string> OnNewImeComposition;
		public event Action<string> OnNewImeResult;
	}
}
