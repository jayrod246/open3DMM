using Open3dmm.Core;
using System;
using System.Runtime.InteropServices;

namespace Open3dmm
{
    public class Txtb : Docb
    {
        public Txtb(Docb relative, bool makeSibling) : base(relative, makeSibling)
        {
            Field0x3c = 432;
            Field0x38 = 0x1ffffff;
        }

        public int Field0x38 { get; set; }
        public int Field0x3c { get; set; }
        public int Field0x248 { get; set; }
        public Bsf Bsf { get; set; }
        public string Text { get; set; }

        public virtual bool VirtualFunc31(string filename, Bsf bsf, int encoding)
        {
            if (filename != null)
                throw new NotImplementedException();
            if (bsf != null)
            {
                if (encoding != EncodingTypes.Default)
                    return false;

                Bsf = bsf;
            }
            else
            {
                Bsf = new Bsf();
                if (!VirtualFunc32(encoding))
                    return false;
            }

            return Bsf.FUN_00456130(new byte[] { 0xd }, 1, Bsf.SomePosition, 0);
        }
        public virtual bool VirtualFunc32(int encoding)
        {
            // TODO: Implement VirtualFunc32
            return true;
        }
        public virtual char VirtualFunc33(int charIndex)
        {
            char c = default;
            VirtualFunc38(charIndex, MemoryMarshal.CreateSpan(ref c, 1));
            return c;
        }
        public virtual void VirtualFunc34() => throw new NotImplementedException();
        public virtual void VirtualFunc35() => throw new NotImplementedException();
        public virtual void VirtualFunc36() => throw new NotImplementedException();
        public virtual void VirtualFunc37() => throw new NotImplementedException();
        public virtual void VirtualFunc38(int charIndex, Span<char> span)
        {
            Text.AsSpan(charIndex, span.Length).CopyTo(span);
        }
        public virtual void VirtualFunc39() => throw new NotImplementedException();
        public virtual void VirtualFunc40() => throw new NotImplementedException();
        public virtual void VirtualFunc41() => throw new NotImplementedException();
        public virtual void VirtualFunc42() => throw new NotImplementedException();
        public virtual bool VirtualFunc43(int charIndex, ref AutoText autoText, out Rectangle bounds)
        {
            bounds = default;
            return false;
        }
        public virtual void VirtualFunc44() => throw new NotImplementedException();
        public virtual void VirtualFunc45() => throw new NotImplementedException();
        public virtual void VirtualFunc46() => throw new NotImplementedException();
        public virtual void VirtualFunc47() => throw new NotImplementedException();
        public virtual void VirtualFunc48()
        {
            Field0x248++;
        }
        public virtual void VirtualFunc49() => throw new NotImplementedException();
        public virtual void VirtualFunc50() => throw new NotImplementedException();
        public virtual void VirtualFunc51() => throw new NotImplementedException();
        public virtual void VirtualFunc52() => throw new NotImplementedException();
        public virtual void VirtualFunc53() => throw new NotImplementedException();
        public virtual void VirtualFunc54() => throw new NotImplementedException();

        public bool ShouldReturnCarriage(int charIndex)
        {
            if (charIndex < 1)
                return charIndex == 0;
            if (Text.Length <= charIndex)
                return false;
            var c = VirtualFunc33(charIndex);
            var cflags = AutoText.ParseCharFlags(c);
            if (cflags.HasFlag(CharFlags.NewLine))
                return false;
            while (true)
            {
                if (charIndex < 1)
                    return false;
                c = VirtualFunc33(--charIndex);
                cflags = AutoText.ParseCharFlags(c);
                if (cflags.HasFlag(CharFlags.ReturnCarriage))
                    return true;
                if (!cflags.HasFlag(CharFlags.NewLine))
                    return false;
            }
        }
    }
}