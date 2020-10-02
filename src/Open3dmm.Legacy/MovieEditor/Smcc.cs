using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open3dmm.MovieEditor
{
    public class Smcc
    {
        public Smcc(int width, int height, int id, object unused, Studio studio)
        {
            Width = width;
            Height = height;
            SomeId = id;
            Unused = unused;
            Studio = studio;
        }

        public virtual int Width { get; }
        public virtual int Height { get; }
        public virtual int SomeId { get; }
        public object Unused { get; }
        public Studio Studio { get; }
        public SceneSlider SceneSlider { get; set; }

        public virtual void ChangeArrowCursor(int id)
        {
            Studio.ChangeArrowCursor(id);
        }

        public virtual void PlayClickSound(int id, int flags)
        {
            Studio.PlayClickSound(id, flags);
        }

        public string GetAppString(int key)
        {
            (Application.Current as App).MiscAppStrings.TryGetString(key, out var str);
            return str;
        }

        public virtual void VirtualFunc30(string filename)
        {
            throw new NotImplementedException();
        }

        public static void FUN_004101d0(int type)
        {
            int id = type switch
            {
                17 => 131080,
                16 => 131077,
                _ => 131078,
            };

            if (Kwa.Instance.Find(id) is Gok gok && gok.StateFlags != 3)
                gok.ChangeState(3);

            switch (type)
            {
                case 1:
                    id = 131095;
                    break;
                case 16:
                    if (Kwa.Instance.Find(131088) is Gok gok2)
                        gok2.RunScript(65570, default, out _, out _);
                    return;
                case 17:
                    id = 131120;
                    break;
                case 38:
                    id = 131171;
                    break;
                default:
                    return;
            }
            if (Kwa.Instance.Find(id) is Gok gok3 && gok3.StateFlags != 2)
                gok3.ChangeState(2);
        }
    }
}
