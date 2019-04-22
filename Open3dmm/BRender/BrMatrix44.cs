namespace Open3dmm.BRender {
    public struct BrMatrix44 {
        public BrScalar M00;
        public BrScalar M01;
        public BrScalar M02;
        public BrScalar M03;
        public BrScalar M10;
        public BrScalar M11;
        public BrScalar M12;
        public BrScalar M13;
        public BrScalar M20;
        public BrScalar M21;
        public BrScalar M22;
        public BrScalar M23;
        public BrScalar M30;
        public BrScalar M31;
        public BrScalar M32;
        public BrScalar M33;

        public static readonly BrMatrix44 Identity = new BrMatrix44
        (
            1f, 0f, 0f, 0f,
            0f, 1f, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f
        );

        public BrMatrix44(BrScalar m00, BrScalar m01, BrScalar m02, BrScalar m03, BrScalar m10, BrScalar m11, BrScalar m12, BrScalar m13, BrScalar m20, BrScalar m21, BrScalar m22, BrScalar m23, BrScalar m30, BrScalar m31, BrScalar m32, BrScalar m33) {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M03 = m03;
            M10 = m10;
            M11 = m11;
            M12 = m12;
            M13 = m13;
            M20 = m20;
            M21 = m21;
            M22 = m22;
            M23 = m23;
            M30 = m30;
            M31 = m31;
            M32 = m32;
            M33 = m33;
        }
    }
}
