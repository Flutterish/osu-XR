using osuTK;

namespace osu.XR.Projection {
	public struct Quad
    {
        public Vector3 TL;
        public Vector3 TR;
        public Vector3 BL;
        public Vector3 BR;

        public Quad (Vector3 tL, Vector3 tR, Vector3 bL, Vector3 bR)
        {
            TL = tL;
            TR = tR;
            BL = bL;
            BR = bR;
        }

        public static Quad operator + (Quad quad, Vector3 offset)
            => new Quad( quad.TL + offset, quad.TR + offset, quad.BL + offset, quad.BR + offset );
    }
}
