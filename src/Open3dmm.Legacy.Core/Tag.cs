using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Open3dmm
{
    public readonly struct Tag : IComparable<Tag>, IEquatable<Tag>
    {
        private readonly int _value;

        public static readonly Tag Default = new(0);

        public Tag(int value)
            => _value = value;

        #region Overrides/Operators
        public int CompareTo(Tag other)
            => _value.CompareTo(other._value);

        public override bool Equals(object obj)
            => obj is Tag tag && Equals(tag);

        public bool Equals(Tag other)
            => _value == other._value;

        public override int GetHashCode()
            => HashCode.Combine(_value);

        public override string ToString()
        {
            int value = _value;
            var span = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));
            span.Reverse();
            return Encoding.ASCII.GetString(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Tag(in Tags x) => new((int)x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Tags(in Tag x) => (Tags)x._value;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Tag(string s) => Parse(s);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator string(Tag tag) => tag.ToString();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator int(Tag tag) => tag._value;

        public static bool operator ==(Tag left, Tag right) => left.Equals(right);

        public static bool operator !=(Tag left, Tag right) => !(left == right);

        public static bool operator <(Tag left, Tag right) => left.CompareTo(right) < 0;

        public static bool operator <=(Tag left, Tag right) => left.CompareTo(right) <= 0;

        public static bool operator >(Tag left, Tag right) => left.CompareTo(right) > 0;

        public static bool operator >=(Tag left, Tag right) => left.CompareTo(right) >= 0;
        #endregion

        public static Tag Parse(string s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));
            var source = s.AsSpan();
            Span<char> chrs = stackalloc char[4];
            Span<byte> bytes = stackalloc byte[4];

            chrs.Fill(' ');
            if (source.ToUpperInvariant(chrs) == -1)
                source[..4].ToUpperInvariant(chrs);
            Encoding.ASCII.GetBytes(chrs, bytes);
            bytes.Reverse();
            return new(MemoryMarshal.Read<int>(bytes));
        }
    }

    public enum Tags : int
    {
        Default = 0,
        ACTN = 1094931534,
        ACTR = 1094931538,
        AGPA = 1095192641,
        BDS = 1111773984,
        BKGD = 1112229700,
        BKTH = 1112233032,
        BMDL = 1112360012,
        CAM = 1128353056,
        CATH = 1128354888,
        CMTL = 1129141324,
        EDIT = 1162103124,
        FILL = 1179208780,
        GGAE = 1195852101,
        GGCL = 1195852620,
        GGCM = 1195852621,
        GGCR = 1195852626,
        GGFR = 1195853394,
        GGST = 1195856724,
        GLBS = 1196180051,
        GLCR = 1196180306,
        GLLT = 1196182612,
        GLMP = 1196182864,
        GLOP = 1196183376,
        GLPI = 1196183625,
        GLSC = 1196184387,
        GLXF = 1196185670,
        GOKD = 1196378948,
        GST = 1196643360,
        GSTX = 1196643416,
        HTOP = 1213484880,
        MASK = 1296126795,
        MBMP = 1296190800,
        MIDS = 1296647251,
        MSND = 1297305156,
        MTRL = 1297371724,
        MTTH = 1297372232,
        MVIE = 1297500485,
        PATH = 1346458696,
        PRTH = 1347572808,
        RTXT = 1381259348,
        SCEN = 1396917582,
        SFTH = 1397118024,
        SMTH = 1397576776,
        SVTH = 1398166600,
        TBOX = 1413631832,
        TBTH = 1413633096,
        TCTH = 1413698632,
        TDF = 1413760544,
        TDT = 1413764128,
        TEXT = 1413830740,
        TFTH = 1413895240,
        THUM = 1414026573,
        TILE = 1414089797,
        TMAP = 1414349136,
        TMPL = 1414352972,
        TMTH = 1414353992,
        TSTH = 1414747208,
        TXXF = 1415075910,
        TYTH = 1415140424,
        TZTH = 1415205960,
        VIDE = 1447642181,
        WAVE = 1463899717,
        ZBMP = 1514294608,
    }
}
