namespace Open3dmm.BRender {
    public struct BrMatrix23 {
        public BrScalar M00;
        public BrScalar M01;
        public BrScalar M10;
        public BrScalar M11;
        public BrScalar M20;
        public BrScalar M21;

        public static readonly BrMatrix23 Identity = new BrMatrix23
        (
            1f, 0f,
            0f, 1f,
            0f, 0f
        );

        public BrMatrix23(BrScalar m00, BrScalar m01, BrScalar m10, BrScalar m11, BrScalar m20, BrScalar m21) {
            M00 = m00;
            M01 = m01;
            M10 = m10;
            M11 = m11;
            M20 = m20;
            M21 = m21;
        }
    }
}
