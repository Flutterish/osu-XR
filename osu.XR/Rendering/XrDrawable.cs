using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.XR.Projection;

namespace osu.XR.Rendering {
	public abstract class XrDrawable : Drawable, IXrDrawable {
        public IShader TextureShader { get; private set; }
        public Camera Camera { get; private set; }
        [BackgroundDependencyLoader]
        private void load ( ShaderManager shaders, Camera camera ) {
            TextureShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
            Camera = camera;
        }

        protected override void Update () {
            base.Update();
            Invalidate( Invalidation.DrawNode );
        }

        protected abstract override DrawNode CreateDrawNode ();
    }
}
