using System;
using System.Collections.Generic;

namespace Open3dmm
{
    public class Docb : Component
    {
        public Docb(Docb relative, bool makeSibling) : base(8)
        {
            if (relative == null)
            {
                Next = Application.Current.RootDoc;
                Application.Current.RootDoc = this;
            }
            else if (!makeSibling)
            {
                Parent = relative;
                Next = relative.FirstChild;
                relative.FirstChild = this;
            }
            else
            {
                Parent = relative.Parent;
                Next = relative.Next;
                relative.Next = this;
            }
            Field0x2c = 10;
            ListOfDdg = new List<Ddg>();
        }

        public Docb Parent { get; set; }
        public Docb Next { get; set; }
        public Docb FirstChild { get; set; }
        public int Field0x2c { get; set; }
        public List<Ddg> ListOfDdg { get; }

        public virtual void VirtualFunc5(int param_1) => throw new NotImplementedException();
        public virtual void VirtualFunc6() => throw new NotImplementedException();
        public virtual void VirtualFunc7(int param_1) => throw new NotImplementedException();
        public virtual void VirtualFunc8() => throw new NotImplementedException();
        public virtual void VirtualFunc9() => throw new NotImplementedException();
        public virtual void VirtualFunc10() => throw new NotImplementedException();
        public virtual void VirtualFunc11() => throw new NotImplementedException();
        public virtual bool VirtualFunc12(int flags) => throw new NotImplementedException();
        public virtual void VirtualFunc13() => throw new NotImplementedException();

        public void DeleteViews()
        {
            foreach (var view in ListOfDdg)
            {
                var dmd = view.GetAnscestorOfType<Dmd>();
                if (dmd != null)
                    dmd.Dispose();
                else
                    view.Dispose();
            }
            ListOfDdg.Clear();
        }

        public void SetView(Ddg view)
        {
            int index = ListOfDdg.IndexOf(view);
            if (index > 0)
            {
                ListOfDdg.RemoveAt(index);
                ListOfDdg.Insert(0, view);
            }
        }

        public Ddg GetView()
        {
            var view = GetViewByIndex(0);
            if (view?.Field0x74 == true)
                return view;
            return null;
        }

        private Ddg GetViewByIndex(int index)
        {
            if (index >= 0 && index < ListOfDdg.Count)
                return ListOfDdg[index];
            return null;
        }

        public virtual void VirtualFunc14(int messageId) => throw new NotImplementedException();
        public virtual void VirtualFunc15() => throw new NotImplementedException();
        public virtual void VirtualFunc16() => throw new NotImplementedException();
        public virtual void VirtualFunc17() => throw new NotImplementedException();
        public virtual void VirtualFunc18() => throw new NotImplementedException();
        public virtual void VirtualFunc19() => throw new NotImplementedException();
        public virtual void VirtualFunc20_Undo() => throw new NotImplementedException();
        public virtual void VirtualFunc21_Redo() => throw new NotImplementedException();
        public virtual void VirtualFunc22() => throw new NotImplementedException();
        public virtual void VirtualFunc23() => throw new NotImplementedException();
        public virtual void VirtualFunc24() => throw new NotImplementedException();
        public virtual void VirtualFunc25() => throw new NotImplementedException();
        public virtual void VirtualFunc26() => throw new NotImplementedException();
        public virtual void VirtualFunc27() => throw new NotImplementedException();
        public virtual void VirtualFunc28() => throw new NotImplementedException();
        public virtual void VirtualFunc29() => throw new NotImplementedException();
        public virtual void VirtualFunc30() => throw new NotImplementedException();
    }
}