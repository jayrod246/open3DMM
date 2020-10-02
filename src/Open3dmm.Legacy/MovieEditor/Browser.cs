using ImGuiNET;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace Open3dmm.MovieEditor
{
    public partial class Browser : Gok
    {
        private const int GidBase = 135456;
        public Tag BrowseTag { get; }
        public int ItemsPerPage { get; }
        private int page;
        private int pageCount;
        private GlobalReference? current;
        private int lastSelectedIndex = -1;

        public Gok PageLeft { get; }
        public Gok PageRight { get; }
        public Action<bool, int, GlobalReference> SelectAction { get; }
        public TextGob PageText { get; }
        public bool TextOnly { get; }

        private (TagManager.Product product, IScopedResolver crf, int num)[] items;

        public override void Draw(CommandList commandList, in RectangleF dest)
        {
            var l = ImGui.GetBackgroundDrawList();
            var win = Application.Current.Window;
            var clientRect = new Vector2(win.Width, win.Height);
            l.PushClipRect(Vector2.Zero, clientRect);
            l.AddRectFilled(Vector2.Zero, clientRect, 0xAF685540);
            l.PopClipRect();
            base.Draw(commandList, dest);
        }

        public Browser(IResolver resolver, int number, Tag tag, bool textOnly, Action<bool, int, GlobalReference> selectAction, GlobalReference? parentThum = null, GlobalReference? current = null) : base(GobOptions.NewChild(Kwa.Instance, 131074, unchecked((int)number)), Application.Current.SoundManager)
        {
            Application.Current.Exchange.AddListener(this, 35353535, MessageFlags.Self | MessageFlags.Broadcast | MessageFlags.Other);
            SelectAction = selectAction;
            this.current = current;
            LoadGokd(Kwa.Instance, number, resolver);
            ChangeState(1);
            BrowseTag = tag;
            if (parentThum == null)
            {
                items = (from product in TagManager.Default.Products
                         let module = product.Get3thModule()
                         from df in module.Files
                         from c in df.File.GetChunksOfType(BrowseTag)
                         select (product, module.ScopeOf(c.Identifier), c.Identifier.Number)).ToArray();
            }
            else if (TagManager.Default.TryGetProduct(parentThum.Value.ProductKey, out var product))
            {
                var scope = product.Get3thModule().ScopeOf(parentThum.Value.Identifier);
                items = (from c in scope.File.GetChildrenOfType(parentThum.Value.Identifier, BrowseTag)
                         select (product, scope, c.Identifier.Number)).ToArray();
            }
            else
                items = Array.Empty<(TagManager.Product, IScopedResolver, int)>();

            int i = 0;
            while (Find(GidBase + i) != null) i++;
            ItemsPerPage = i;
            pageCount = (items.Length / ItemsPerPage) + 1;
            if (items.Length % ItemsPerPage == 0)
                pageCount--;
            PageLeft = Find(135185) as Gok;
            PageRight = Find(135184) as Gok;
            if (pageCount > 1)
            {
                PageLeft.ChangeState(2);
                PageRight.ChangeState(2);
            }
            PageText = new TextGob(GobOptions.NewChild(Kwa.Instance, 135190, 0));
            TextOnly = textOnly;
            PageToCurrent();

            // NOTE: When adding new buttons, add to FirstChild
            if (Find(135188) == null)
            {
                var importButton = Kwa.Instance.CreateGokChild(FirstChild, 135188, 135188, Resolver);
                importButton.MoveAbs(104, 368);
                importButton.ChangeState(1);
            }
        }

        private bool UpdatePage()
        {
            bool containsCurrent = false;
            int i = 0;
            foreach (var (product, crf, num) in items.Skip(page * ItemsPerPage))
            {
                if (Find(GidBase + i) is not Gok cell) break;
                while (cell.FirstChild != null)
                    cell.FirstChild.Dispose();
                if (TextOnly)
                {
                    var tgob = new TextGob(GobOptions.NewChild(Kwa.Instance, cell.Id, 0))
                    {
                        Text = crf.File.GetChunk(new ChunkIdentifier(BrowseTag, num)).Label
                    };
                }
                if (crf.TryGetIdentifier(new ChunkIdentifier(BrowseTag, num), Tags.GOKD, 0, out var button))
                {
                    var gok = Kwa.Instance.CreateGokChild(cell, 0, button.Number, crf);
                    var rect = gok.ActualRect;
                    rect.Offset(4, 4);
                    gok.UpdateRectangle(rect, null);
                }
                if (current.HasValue && current.Value.ProductKey == product.Key && current.Value.Identifier.Number == num)
                {
                    containsCurrent = true;
                    cell.ChangeState(4);
                }
                else
                    cell.ChangeState(2);
                i++;
            }

            while (Find(GidBase + i++) is Gok cell)
                cell.ChangeState(1);
            PageText.Text = $"{page + 1}/{pageCount}";
            return containsCurrent;
        }

        private bool HandleImport(Message m)
        {
            return true;
        }

        private bool PageToCurrent()
        {
            page = 0;
            if (current == null)
                return UpdatePage();
            while (!UpdatePage())
            {
                if (++page >= pageCount)
                {
                    page = 0;
                    return UpdatePage();
                }
            }
            return true;
        }

        private bool HandlePageRight(Message m)
        {
            if (++page >= pageCount)
                page = 0;
            UpdatePage();
            return true;
        }

        private bool HandlePageLeft(Message m)
        {
            if (--page < 0)
                page = pageCount - 1;
            UpdatePage();
            return true;
        }

        private bool HandleSelectItem(Message m)
        {
            int cell = Convert.ToInt32(m.ParamA);
            int index = GidToIndex(cell);
            int lastCell = IndexToGid(lastSelectedIndex);
            if (lastCell >= 0 && lastCell != cell)
            {
                var gok = ((Gok)Find(IndexToGid(lastSelectedIndex)));
                if (gok.StateFlags == 4)
                {
                    gok.ChangeState(2);
                }
            }
            lastSelectedIndex = index;
            var (product, _, num) = items[index];
            current = new GlobalReference(product.Key, BrowseTag, num);

            SelectAction(false, lastSelectedIndex, current.Value);

            return true;
        }

        int IndexToGid(int index)
        {
            if (index < 0)
                return -1;
            return (index % ItemsPerPage) + GidBase;
        }

        int GidToIndex(int gid)
        {
            return gid - GidBase + page * ItemsPerPage;
        }

        private bool HandleConfirm(Message m)
        {
            SelectAction(true, lastSelectedIndex, current.Value);
            Dispose();
            return true;
        }

        private bool HandleCloseButton(Message m)
        {
            Dispose();
            return true;
        }
    }
}
