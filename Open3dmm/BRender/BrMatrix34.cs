namespace Open3dmm.BRender
{
    public struct BrMatrix34
    {
        public BrScalar M00;
        public BrScalar M01;
        public BrScalar M02;
        public BrScalar M10;
        public BrScalar M11;
        public BrScalar M12;
        public BrScalar M20;
        public BrScalar M21;
        public BrScalar M22;
        public BrScalar M30;
        public BrScalar M31;
        public BrScalar M32;

        public static readonly BrMatrix34 Identity = new BrMatrix34
        (
            1f, 0f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f,
            0f, 0f, 0f
        );

        public BrMatrix34(BrScalar m00, BrScalar m01, BrScalar m02, BrScalar m10, BrScalar m11, BrScalar m12, BrScalar m20, BrScalar m21, BrScalar m22, BrScalar m30, BrScalar m31, BrScalar m32)
        {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M10 = m10;
            M11 = m11;
            M12 = m12;
            M20 = m20;
            M21 = m21;
            M22 = m22;
            M30 = m30;
            M31 = m31;
            M32 = m32;
        }

        public override string ToString()
        {
            return $"M00: {M00}, M01: {M01}, M02: {M02}, M10: {M10}, M11: {M11}, M12: {M12}, M20: {M20}, M21: {M21}, M22: {M22}, M30: {M30}, M31: {M31}, M32: {M32}";
        }
    }
}
