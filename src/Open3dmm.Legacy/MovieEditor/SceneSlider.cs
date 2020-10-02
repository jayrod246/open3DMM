using System;

namespace Open3dmm.MovieEditor
{
    public class SceneSlider
    {
        private Movie _movie;

        public SceneSlider(Movie movie)
        {
            Movie = movie;
        }

        public Movie Movie {
            get => _movie;
            set {
                _movie = value;
                UpdateSliders();
            }
        }

        public int FrameFlag { get; set; }
        public TextGob FrameNumGob { get; set; }
        public TextGob SceneNumGob { get; set; }
        public int AltFrameStart { get; set; }

        public virtual void UpdateSliders()
        {
            if (Movie == null || Movie.CurrentScene == null)
                return;
            var root = Kwa.Instance;
            if (root != null)
            {
                var scene = Movie.CurrentScene;
                UpdateTrack(root, 131209, 131210, scene.StartFrame, scene.EndFrame - scene.StartFrame, scene.CurrentFrame);
                UpdateTrack(root, 131218, 131219, 0, Movie.Scenes.Count, Movie.CurrentSceneIndex);
                int frameStart = FrameFlag == 0 ? scene.StartFrame : AltFrameStart;
                FrameNumGob.Text = (scene.CurrentFrame - frameStart + 1).ToString().PadLeft(4);
                SceneNumGob.Text = (Movie.CurrentSceneIndex + 1).ToString().PadLeft(4);
            }
        }

        private void UpdateTrack(Woks root, int trackId, int sliderId, int start, int count, int current)
        {
            var track = root.Find(trackId);
            var slider = root.Find(sliderId) as Gok;
            if (track != null && slider != null)
            {
                int xpos = 0;
                if (count > 0)
                {
                    xpos = CalcHorizontal(trackId, sliderId);
                    xpos = (current - start) * xpos / count;
                }
                slider.Cell(-1, Gok.EvalCellFlags.NoScript, 0, xpos - slider.ActualRect.Left, 0, 0);
            }
        }

        public static int CalcHorizontal(int trackId, int sliderId)
        {
            var root = Kwa.Instance;
            if (root != null)
            {
                var slider = root.Find(sliderId);
                var track = root.Find(trackId);
                if (slider != null && track != null)
                {
                    int sliderWidth = slider.GetRectangle(CoordinateSpace.None).Width;
                    int trackWidth = track.GetRectangle(CoordinateSpace.None).Width;
                    return trackWidth - sliderWidth;
                }
            }
            return 0;
        }
    }
}