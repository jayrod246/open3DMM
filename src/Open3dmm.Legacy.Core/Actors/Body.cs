using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Scenes;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Open3dmm.Core.Actors
{
    public class Body : SceneNode
    {
        public Template Template { get; }
        BodyPart[] body;

        public GenericGroup<int> Costumes;
        public Actor Owner { get; }

        public Body(Template template, Actor owner = null)
        {
            Template = template;
            Owner = owner;
            LoadParentInfo(template.Indices);
            LoadBodySetInfo(template.Sets);
            Costumes = template.Costumes;

            LoadAction(0);
            LoadDefaultCostume();
        }

        public void LoadDefaultCostume()
        {
            int bodySet = 0;
            while (LoadCostume(bodySet++, 0)) ;
        }

        public bool LoadAction(int number, int cell = 0, float blend = 0f)
        {
            if (Template.TryGetOrCreateActionInfo(number, out var actionInfo))
            {
                cell %= actionInfo.Cells.Count;
                if (cell < 0)
                    cell += actionInfo.Cells.Count;
                if (blend <= 0f)
                {
                    for (int i = 0; i < body.Length; i++)
                    {
                        if (!actionInfo.TryGetBmdl(cell, i, out var bmdlId) || !Template.TryGetOrCreateModel(bmdlId, out var model) || !SetModel(i, model, model))
                            SetModel(i, null, null);

                        if (!actionInfo.TryGetTransform(cell, i, out var transform) || !SetTransform(i, transform))
                            SetTransform(i, Matrix4x4.Identity);
                    }
                }
                else
                {
                    var nextCell = (cell + 1) % actionInfo.Cells.Count;
                    for (int i = 0; i < body.Length; i++)
                    {
                        if (!actionInfo.TryGetBmdl(cell, i, out var bmdlId) || !Template.TryGetOrCreateModel(bmdlId, out var model)
                            || !actionInfo.TryGetBmdl(nextCell, i, out var nextBmdlId) || !Template.TryGetOrCreateModel(nextBmdlId, out var nextModel) || !SetModel(i, model, nextModel))
                            SetModel(i, null, null);

                        if (!actionInfo.TryGetTransform(cell, i, out var transform1) || !actionInfo.TryGetTransform(nextCell, i, out var transform2) || !SetTransform(i, Matrix4x4.Lerp(transform1, transform2, blend)))
                            SetTransform(i, Matrix4x4.Identity);
                    }
                }
                return true;
            }
            return false;
        }

        public bool LerpActions(int id1, int cell1, int id2, int cell2, float blend)
        {
            if (Template.TryGetOrCreateActionInfo(id1, out var actionInfo1))
            {
                cell1 %= actionInfo1.Cells.Count;
                if (cell1 < 0)
                    cell1 += actionInfo1.Cells.Count;
                if (!Template.TryGetOrCreateActionInfo(id2, out var actionInfo2))
                    return false;
                else
                {
                    cell2 %= actionInfo2.Cells.Count;
                    if (cell2 < 0)
                        cell2 += actionInfo2.Cells.Count;
                    for (int i = 0; i < body.Length; i++)
                    {
                        if (!actionInfo1.TryGetBmdl(cell1, i, out var bmdlId) || !Template.TryGetOrCreateModel(bmdlId, out var model)
                            || !actionInfo2.TryGetBmdl(cell2, i, out var nextBmdlId) || !Template.TryGetOrCreateModel(nextBmdlId, out var nextModel) || !SetModel(i, model, nextModel))
                            SetModel(i, null, null);

                        if (!actionInfo1.TryGetTransform(cell1, i, out var transform1) || !actionInfo2.TryGetTransform(cell2, i, out var transform2) || !SetTransform(i, Matrix4x4.Lerp(transform1, transform2, blend)))
                            SetTransform(i, Matrix4x4.Identity);
                    }
                }
                return true;
            }
            return false;
        }

        private Matrix4x4 RotFixedLerp(Matrix4x4 a, Matrix4x4 b, float amount)
        {
            if (Matrix4x4.Decompose(a, out var s1, out var r1, out var t1) &&
            Matrix4x4.Decompose(b, out var s2, out var r2, out var t2))
            {
                return Matrix4x4.CreateScale(Vector3.Lerp(s1, s2, amount)) *
                Matrix4x4.CreateFromQuaternion(Quaternion.Slerp(r1, r2, amount)) *
                Matrix4x4.CreateTranslation(Vector3.Lerp(t1, t2, amount));
            }
            else
            {
                System.Console.WriteLine("Fail");
                return Matrix4x4.Lerp(a, b, amount);
            }
        }

        public bool LoadCostume(int bodySet, int costume)
        {
            if (bodySet < 0 || bodySet >= Costumes.Count || costume < 0 || costume >= Costumes[bodySet])
                return false;
            var cmtlId = Unsafe.Add(ref Unsafe.As<byte, int>(ref Costumes.GetPayload(bodySet)[0]), costume);
            return LoadCostume(cmtlId);
        }

        public bool LoadCostume(int cmtlId)
        {
            int offset = 0;
            if (!Template.TryGetOrCreateCostumeInfo(cmtlId, out var costumeInfo))
                return false;
            int bodySet = costumeInfo.BodySet;
            for (int i = 0; i < body.Length; i++)
            {
                if (body[i].BodySet == bodySet)
                {
                    if (!costumeInfo.TryGetMaterial(offset, out var material))
                        return false;
                    body[i].Material = material;
                    body[i].ModelOverride = costumeInfo.TryGetModel(offset, out var modelOverride) ? modelOverride : null;
                    offset++;
                }
            }
            return true;
        }

        private bool SetModel(int bodyPart, Bmdl model, Bmdl nextModel)
        {
            body[bodyPart].Model = model;
            body[bodyPart].NextModel = nextModel ?? model;
            return true;
        }

        private bool SetTransform(int bodyPart, Matrix4x4 transform)
        {
            body[bodyPart].Transform = transform;
            return true;
        }

        private void LoadBodySetInfo(IList<short> sets)
        {
            for (int i = 0; i < sets.Count; i++)
                body[i].BodySet = sets[i];
        }

        private void LoadParentInfo(IList<short> indices)
        {
            body = new BodyPart[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                body[i] = new BodyPart(this);
                var parent = indices[i] < 0 ? this as SceneNode : body[indices[i]];
                parent.AddChild(body[i]);
            }
        }
    }
}
