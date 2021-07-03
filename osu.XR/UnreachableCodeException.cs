using System;
using System.Runtime.Serialization;

namespace osu.XR {
	[Serializable]
	internal class UnreachableCodeException : Exception {
		public UnreachableCodeException () {
		}
	}
}