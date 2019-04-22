namespace Open3dmm.Classes
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct Quad : IEnumerable<char>, IEquatable<Quad>, IComparable, IComparable<Quad>, IComparable<string>
    {
        private string DebuggerDisplay => string.Concat(ToString().Select(c => c.ToString()));

        #region Quads

        public static readonly Quad ACTN = new Quad("ACTN");
        public static readonly Quad ACTR = new Quad("ACTR");
        public static readonly Quad AGPA = new Quad("AGPA");
        public static readonly Quad BKGD = new Quad("BKGD");
        public static readonly Quad BKTH = new Quad("BKTH");
        public static readonly Quad BMDL = new Quad("BMDL");
        public static readonly Quad CAM = new Quad("CAM ");
        public static readonly Quad CATH = new Quad("CATH");
        public static readonly Quad CMTL = new Quad("CMTL");
        public static readonly Quad FILL = new Quad("FILL");
        public static readonly Quad GGAE = new Quad("GGAE");
        public static readonly Quad GGCL = new Quad("GGCL");
        public static readonly Quad GGCM = new Quad("GGCM");
        public static readonly Quad GGFR = new Quad("GGFR");
        public static readonly Quad GGST = new Quad("GGST");
        public static readonly Quad GLBS = new Quad("GLBS");
        public static readonly Quad GLCR = new Quad("GLCR");
        public static readonly Quad GLDC = new Quad("GLDC");
        public static readonly Quad GLLT = new Quad("GLLT");
        public static readonly Quad GLMP = new Quad("GLMP");
        public static readonly Quad GLOP = new Quad("GLOP");
        public static readonly Quad GLPI = new Quad("GLPI");
        public static readonly Quad GLSC = new Quad("GLSC");
        public static readonly Quad GLXF = new Quad("GLXF");
        public static readonly Quad GOKD = new Quad("GOKD");
        public static readonly Quad GST = new Quad("GST ");
        public static readonly Quad HTOP = new Quad("HTOP");
        public static readonly Quad MASK = new Quad("MASK");
        public static readonly Quad MBMP = new Quad("MBMP");
        public static readonly Quad MIDS = new Quad("MIDS");
        public static readonly Quad MTRL = new Quad("MTRL");
        public static readonly Quad MTTH = new Quad("MTTH");
        public static readonly Quad MVIE = new Quad("MVIE");
        public static readonly Quad PATH = new Quad("PATH");
        public static readonly Quad PRTH = new Quad("PRTH");
        public static readonly Quad RTXT = new Quad("RTXT");
        public static readonly Quad SCEN = new Quad("SCEN");
        public static readonly Quad SFTH = new Quad("SFTH");
        public static readonly Quad SMTH = new Quad("SMTH");
        public static readonly Quad SVTH = new Quad("SVTH");
        public static readonly Quad TBTH = new Quad("TBTH");
        public static readonly Quad TCTH = new Quad("TCTH");
        public static readonly Quad TDF = new Quad("TDF ");
        public static readonly Quad TDT = new Quad("TDT ");
        public static readonly Quad TEXT = new Quad("TEXT");
        public static readonly Quad TFTH = new Quad("TFTH");
        public static readonly Quad THUM = new Quad("THUM");
        public static readonly Quad TMAP = new Quad("TMAP");
        public static readonly Quad TMPL = new Quad("TMPL");
        public static readonly Quad TMTH = new Quad("TMTH");
        public static readonly Quad TSTH = new Quad("TSTH");
        public static readonly Quad TXXF = new Quad("TXXF");
        public static readonly Quad TZTH = new Quad("TZTH");
        public static readonly Quad ZBMP = new Quad("ZBMP");

        #endregion

        internal readonly uint rawValue;

        public Quad(string str)
        {
            this = FromString(str);
        }

        private Quad(uint rawValue)
        {
            this.rawValue = rawValue;
        }

        public static Quad FromString(string str)
        {
            str = str?.PadRight(4).Substring(0, 4).ToUpperInvariant() ?? "    ";
            return FromRaw((uint)((byte)str[0] << 24 | (byte)str[1] << 16 | (byte)str[2] << 8 | (byte)str[3] << 0));
        }

        public uint RawValue => rawValue;

        public override int GetHashCode()
        {
            return rawValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Quad other)
                return Equals(other);
            return base.Equals(obj);
        }

        public static Quad FromRaw(uint rawValue)
        {
            return new Quad(rawValue);
        }

        public unsafe override string ToString()
        {
            unsafe
            {
                uint value = RawValue;
                var str = new string((sbyte*)&value, 0, 4);
                return $"{str[3]}{str[2]}{str[1]}{str[0]}";
            }
        }

        public bool Equals(Quad other)
        {
            return other.rawValue == rawValue;
        }

        public int CompareTo(Quad other)
        {
            return rawValue.CompareTo(other.rawValue);
        }

        public int CompareTo(string other)
        {
            return ToString().CompareTo(other);
        }

        int IComparable.CompareTo(object obj)
        {
            if (!(obj is Quad)) throw new ArgumentException("obj is not the same type as this instance.");
            return CompareTo((Quad)obj);
        }

        public IEnumerator<char> GetEnumerator()
        {
            return ToString().Cast<char>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator string(Quad quad)
        {
            return quad.ToString();
        }

        public static explicit operator Quad(string str)
        {
            return new Quad(str);
        }

        public static bool operator ==(Quad a, Quad b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Quad a, Quad b)
        {
            return !a.Equals(b);
        }
    }
}
