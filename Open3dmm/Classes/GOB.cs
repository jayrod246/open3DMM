using System;

namespace Open3dmm.Classes
{
    public class GOB : CMH
    {
        public int Field000C {
            get => GetField<int>(0x000C);
            set => GetField<int>(0x000C) = value;
        }
        public GPT GPT {
            get => GetReference<GPT>(0x0010);
            set => SetReference(value, 0x0010);
        }

        public RECTANGLE Rect {
            get => GetField<RECTANGLE>(0x0018);
            set => GetField<RECTANGLE>(0x0018) = value;
        }

        public int Flags {
            get => GetField<int>(0x0058);
            set => GetField<int>(0x0058) = value;
        }

        public GOB Unk0058 {
            get => GetReference<GOB>(0x0058);
            set => SetReference(value, 0x0058);
        }
        public GOB Unk0060 {
            get => GetReference<GOB>(0x0060);
            set => SetReference(value, 0x0060);
        }

        internal void Method004241B0(ref RECTANGLE _a4, int v2)
        {
            _a4 = this.Rect;
            Method004243A0(out var point, v2);
            _a4.Translate(point.X - Rect.X1, point.Y - Rect.Y1);
        }

        public int Method004243A0(out POINT pt, int v2)
        {
            int result = 0;
            switch (v2 - 1)
            {
                default:
                    pt = POINT.Zero;
                    break;
                case 0:
                    pt = Rect.TopLeft;
                    break;
                case 1:
                    pt = POINT.Zero;
                    throw new NotImplementedException();
                case 2:
                case 3:
                    pt = POINT.Zero;
                    var gob = this;
                    while (gob != null && gob.Field000C == 0)
                    {
                        pt.X += gob.Rect.X1;
                        pt.Y += gob.Rect.Y1;
                        gob = gob.Unk0058;
                    }

                    if (gob != null)
                        result = gob.Field000C;

                    if (v2 == 4 && result != 0)
                    {

                    }
                    break;
            }
            return result;
        }
    }
}
