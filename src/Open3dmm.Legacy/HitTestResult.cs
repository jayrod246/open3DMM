namespace Open3dmm
{
    public struct HitTestResult
    {
        public Gob GobHit;
        public PT PointHit;

        public HitTestResult(Gob gobHit, PT pointHit)
        {
            GobHit = gobHit;
            PointHit = pointHit;
        }
    }
}
