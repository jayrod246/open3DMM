using System;

namespace Open3dmm
{
    public partial class Ddg : Gob
    {
        public Docb Document { get; set; }
        public bool Field0x74;
        public int Field0x78;
        public int DocumentX;
        public Cex Exchange => Application.Current.Exchange;

        public Ddg(Docb docb, GobOptions options) : base(options)
        {
            Document = docb;
            DocumentX = 0;
            Field0x78 = 0;
        }

        public override bool VirtualFunc25(Message m)
        {
            VirtualFunc37(true);
            return true;
        }

        public virtual bool VirtualFunc28()
        {
            Flags_6c |= 512;
            Document.ListOfDdg.Add(this);
            Flags_6c &= ~512;
            return true;
        }

        public virtual void VirtualFunc29(bool value)
        {
            Exchange.RemoveListener(this, 0);
            if (value)
                Exchange.AddListener(this, 0, MessageFlags.Broadcast);
        }

        public virtual bool VirtualFunc30(int value)
            => false;

        public virtual bool VirtualFunc31(int leftTop, int rightBottom, int x, int y)
            => throw new NotImplementedException();

        public virtual bool VirtualFunc32(int leftTop, int rightBottom, int x, int y)
            => VirtualFunc31(leftTop, rightBottom, x, y);

        public virtual void VirtualFunc33(int x, int y)
            => throw new NotImplementedException();

        public virtual bool VirtualFunc34(out Docb docb)
        {
            docb = null;
            return false;
        }

        public virtual void VirtualFunc35()
        { }

        public virtual bool VirtualFunc36(Clipboard clipboard, int a, int message)
            => false;

        public virtual void VirtualFunc37(bool value)
        {
            if (Field0x74 != value)
            {
                if (value)
                {
                    var view = Document.GetView();
                    if (view != null)
                    {
                        view.VirtualFunc29(false);
                        view.Field0x74 = false;
                    }
                    MakeDmdTreeTopmost();
                    Document.SetView(this);
                }

                VirtualFunc29(value);
                Field0x74 = value;
            }
        }

        public virtual bool VirtualFunc38(Message m)
        {
            const int THUMBTRACK = 5;
            const int RIGHT_OR_BOTTOM = 7;
            var a = (int)m.ParamA;
            var b = (int)m.ParamB;

            if (m.Id == (int)KnownMessageValues.ScrollBar)
            {
                if (b != THUMBTRACK)
                {
                    if (a == RIGHT_OR_BOTTOM)
                        VirtualFunc32(0, b, 0, 0);
                    else
                        VirtualFunc32(b, 0, 0, 0);
                }
            }
            else if (m.Id == (int)KnownMessageValues.ScrollBarThumbTrack)
            {
                if (a == RIGHT_OR_BOTTOM)
                    VirtualFunc32(0, 5, 0, b);
                else
                    VirtualFunc32(5, 0, b, 0);
            }

            return true;
        }
        public virtual bool VirtualFunc39(Message m)
        {
            if (Document.VirtualFunc12(m.Id == 117 ? 4 : 0))
                Document.DeleteViews();
            return true;
        }
        public virtual bool VirtualFunc40(Message m)
        {
            Document.VirtualFunc14(m.Id);
            return true;
        }
        public virtual bool VirtualFunc41(Message m)
        {
            switch (m.Id)
            {
                case 118:
                case 119:
                    if (VirtualFunc34(out var docb))
                    {
                        Clipboard.Instance.FUN_0044bbb0(docb, true);
                        docb?.Dispose();

                        if (m.Id == 118)
                            goto case 121;
                    }
                    break;
                case 121:
                    VirtualFunc35();
                    break;
                case 120:
                case 140:
                    if (!Clipboard.Instance.FUN_0044bb40(null)
                        && !Clipboard.Instance.FUN_0044bb40(Document))
                    {
                        VirtualFunc36(Clipboard.Instance, 1, m.Id);
                    }
                    break;
            }
            return true;
        }

        // VirtualFunc42 not required.

        public virtual bool VirtualFunc43(Message m)
        {
            if (m.Id == (int)KnownMessageValues.App_Undo)
                Document.VirtualFunc20_Undo();
            else
                Document.VirtualFunc21_Redo();
            return true;
        }

        private void MakeDmdTreeTopmost()
        {
            var dmd = GetAnscestorOfType<Dmd>();
            if (dmd != null)
            {
                var current = (Gob)this;
                while (current != null && current != dmd)
                {
                    current.MoveToFront();
                    current = current.Parent;
                }
            }
        }
    }

    public class Clipboard
    {
        public static Clipboard Instance { get; } = new();

        public void FUN_0044bbb0(Docb docb, bool val)
        {
            throw new NotImplementedException();
        }
        public bool FUN_0044bb40(Docb docb)
        {
            throw new NotImplementedException();
        }
    }
}