using osu.Framework.Graphics.OpenGL.Buffers;
using osuTK.Graphics.ES30;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Graphics {
	/// <summary>
	/// A <see cref="FrameBuffer"/> with a 32 bit depth component.
	/// </summary>
	public class DepthFrameBuffer : FrameBuffer {
		public DepthFrameBuffer () : base ( new RenderbufferInternalFormat[] { RenderbufferInternalFormat.DepthComponent32f } ) {

		}
	}
}
