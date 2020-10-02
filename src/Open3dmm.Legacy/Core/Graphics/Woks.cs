using Open3dmm.Core;
using Open3dmm.Core.GUI;
using Open3dmm.Core.Resolvers;
using System.Collections.Generic;
using System.IO;

namespace Open3dmm
{
    public class Woks : Gob
    {
        public Woks(GobOptions options, IDictionary<int, string> strg = null) : base(options)
        {
            Strg = strg ?? new Dictionary<int, string>();
            Clock1 = new Clok(0, ClockFlags.None);
            Clock2 = new Clok(0, ClockFlags.NoSlip);
            Clock3 = new Clok(0, ClockFlags.None);
            Clock4 = new Clok(0, ClockFlags.ResetOnInput);

            Clock1.Start(0);
            Clock2.Start(0);
            Clock3.Start(0);
            Clock4.Start(0);
        }

        public int ModifierState { get; set; }
        public IDictionary<int, string> Strg { get; set; }
        public Clok Clock1 { get; set; }
        public Clok Clock2 { get; set; }
        public Clok Clock3 { get; set; }
        public Clok Clock4 { get; set; }

        // 00459b90
        public Gok CreateGokChild(Gob parent, int id, int number, IResolver resolver)
        {
            Gob item = this;
            if (parent != null && !IsAnscestorOf(item = parent))
                return null;
            if (id == 0)
                id = NewIds(1);
            else
            {
                if (FindComponent(id) != null)
                    return null; // Already exists
            }

            if (resolver.TryResolve<Gokd>(new ChunkIdentifier(Tags.GOKD, number), out var gokd))
            {
                var gok = new Gok(new GobOptions(id, item, false, InvalidateOptions.Region), Application.Current.SoundManager);
                if (gok.LoadGokd(this, gokd, resolver) && gok.ChangeState(1))
                    return gok;
                gok.Dispose();
            }

            return null;
        }

        public virtual Hbal CreateHbalChild(Gob parent, IResolver resolver, int number, HtopValues? values)
        {
            Gob item = this;
            if (parent != null && !IsAnscestorOf(item = parent))
                return null;

            // WOKS::FUN_0045b730
            {
                var scope = resolver.ScopeOf(new ChunkIdentifier(Tags.HTOP, number));
                if (scope != null)
                {
                    var txhd = new Txhd(resolver, (scope as DataFile).File, Tags.HTOP, number, Strg, 2);
                    txhd.Field0x3c4 = 1;
                    // WOKS::FUN_0045b7b0
                    {
                        HtopValues myValues;
                        if (!values.HasValue)
                            myValues = txhd.Values;
                        else
                        {
                            myValues = values.Value;
                            if (myValues.CnoGraphics == -1)
                                myValues.CnoGraphics = txhd.Values.CnoGraphics;
                            if (myValues.Id == 0)
                                myValues.Id = txhd.Values.Id;
                            if (myValues.RelativeId == 0)
                                myValues.RelativeId = txhd.Values.RelativeId;
                            if (myValues.CnoScript == -1)
                                myValues.CnoScript = txhd.Values.CnoScript;

                            myValues.OffsetX += txhd.Values.OffsetX;
                            myValues.OffsetY += txhd.Values.OffsetY;

                            if (myValues.CnoSound == -1 || myValues.CtgSound == 0)
                            {
                                myValues.CnoSound = txhd.Values.CnoSound;
                                myValues.CtgSound = txhd.Values.CtgSound;
                            }
                        }

                        if (myValues.Id == 0)
                            myValues.Id = NewIds(1);
                        else
                        {
                            var find = FindComponent(myValues.Id);
                            if (find != null && !(find is Hbal))
                                return null;
                            find?.Dispose();
                        }

                        var hbal = new Hbal(new GobOptions(myValues.Id, item, false, InvalidateOptions.Region));
                        var globalId = hbal.RuntimeId;
                        if (hbal.VirtualFunc58(this, txhd, myValues, resolver) && hbal.ChangeState(1))
                        {
                            hbal.Txhg.VirtualFunc68(0, -1, 0, 0);
                            if (FindByRuntimeId(globalId) == hbal)
                                return hbal;
                        }
                        hbal.Dispose();
                    }
                }
            }

            return null;
        }

        public Hbtn CreateHbtnChild(Gob parent, int id, int number, IResolver resolver, int childId, int unk, int offsetX, int offsetY)
        {
            if (FindComponent(id) == null)
            {
                var hbtn = new Hbtn(new GobOptions(id, parent, false, InvalidateOptions.Region));
                hbtn.ChildId = childId;
                hbtn.Unk = unk;
                if (hbtn.LoadGokd(this, number, resolver) && hbtn.ChangeState(1))
                {
                    hbtn.Offset(offsetX, offsetY);
                    return hbtn;
                }

                hbtn.Dispose();
            }

            return null;
        }

        public bool ShowError(IResolver resolver, int number, int param)
        {
            if (!ComponentGraph.TryGetExchange(out var cex))
                return false;

            using var scope = new Woks(new(0, this, false, InvalidateOptions.Inherit, anchor: new(0, 0, 1, 1)), Strg);

            scope.CreateHbalChild(scope, resolver, number, null);

            return ScopeToComponentAndRunMessageLoop(scope, param);
        }

        private bool ScopeToComponentAndRunMessageLoop(Component scope, int param)
        {
            ComponentGraph.BeginExchange();
            ComponentGraph.TryGetExchange(out var cex);
            try
            {
                cex.Scope = scope;
                Application.Current.PumpMessages();
                return true;
            }
            finally
            {
                ComponentGraph.EndExchange();
            }
        }

        public virtual ScriptEngine CreateScriptEngine(IResolver resolver, Gob target)
        {
            return new ScriptEngine(this, resolver, target);
        }

        public virtual Component FindComponent(int cid)
        {
            if (cid == 0)
                return null;
            if (cid == 0x2711)
                return Clock3;
            if (cid == 0x2712)
                return Clock4;

            var component = ComponentGraph.GetComponent(cid);

            if (component is Gob gob && !this.IsAnscestorOf(gob))
                return null;

            return component;
        }

        public int GetState()
        {
            return GetStateWithKeys(AppWindow.GetInputState());
        }

        public int GetStateWithKeys(InputState keys)
        {
            return (ModifierState & ~0xff) | ((int)keys & 0xff);
        }
    }
}
