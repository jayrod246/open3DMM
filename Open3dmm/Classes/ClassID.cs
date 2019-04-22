using System.Runtime.InteropServices;

namespace Open3dmm.Classes
{
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public readonly struct ClassID
    {
        public readonly int Value;

        public ClassID(int id)
        {
            this.Value = id;
        }

        public ClassID(string id)
        {
            if (string.IsNullOrEmpty(id))
                Value = 0;
            else
            {
                switch (id.Length)
                {
                    case 1:
                        Value = id[0] & 0xFF;
                        break;
                    case 2:
                        Value = (id[0] & 0xFF) << 8
                          | (id[1] & 0xFF);
                        break;
                    case 3:
                        Value = (id[0] & 0xFF) << 16
                          | (id[1] & 0xFF) << 8
                          | (id[2] & 0xFF);
                        break;
                    default:
                        Value = (id[0] & 0xFF) << 24
                          | (id[1] & 0xFF) << 16
                          | (id[2] & 0xFF) << 8
                          | (id[3] & 0xFF);
                        break;
                }
            }
        }

        public override string ToString()
        {
            unsafe
            {
                int value = Value;
                var str = new string((sbyte*)&value, 0, 4);
                switch (str.IndexOf('\0'))
                {
                    case 0:
                        return string.Empty;
                    case 1:
                        return str[0].ToString();
                    case 2:
                        return str[1].ToString() + str[0];
                    case 3:
                        return str[2].ToString() + str[1] + str[0];
                    case 4:
                    default:
                        return str[3].ToString() + str[2] + str[1] + str[0];
                }
            }
        }

        public static implicit operator ClassID(int v)
        {
            return new ClassID(v);
        }
    }
}
