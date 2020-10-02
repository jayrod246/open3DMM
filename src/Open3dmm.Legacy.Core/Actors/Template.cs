using Open3dmm;
using Open3dmm.Core.Brender;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections.Generic;

namespace Open3dmm.Core.Actors
{
    public class Template : ResolvableObject
    {
        public IList<short> Indices => ResolveReferenceOrDefault<GenericList<short>>(new ReferenceIdentifier(0, Tags.GLPI));
        public GenericGroup<int> Costumes => ResolveReferenceOrDefault<GenericGroup<int>>(new ReferenceIdentifier(0, Tags.GGCM));
        public IList<short> Sets => ResolveReferenceOrDefault<GenericList<short>>(new ReferenceIdentifier(0, Tags.GLBS));

        public BrEuler Rotation { get; set; }
        public ActorFlags Flags { get; set; }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            block.MagicNumber();
            Rotation = block.Read<BrEuler>();
            Flags = block.Read<ActorFlags>();

            //if ((Flags & ActorFlags.IsWord) == 0)
            //{
            //    if (!GetResolve().Scope.TryResolve(GetResolve().Identifier, "GLPI", 0, out indices))
            //        throw new InvalidOperationException("GLPI not found OR parent-child info could not be read!");

            //    if (!GetResolve().Scope.TryResolve(GetResolve().Identifier, "GLBS", 0, out sets))
            //        throw new InvalidOperationException("GLBS not found OR body set info could not be read!");

            //    if (!GetResolve().Scope.TryResolve(GetResolve().Identifier, "GGCM", 0, out costumes))
            //        throw new InvalidOperationException("GGCM not found OR costumes could not be read!");
            //}
            //else
            //{
            //    Indices = new List<short>();
            //    Costumes = new Group<int>();
            //    Sets = new List<short>();
            //}
        }

        public bool TryGetOrCreateActionInfo(int number, out ActionInfo actionInfo)
        {
            return Metadata.Resolver.TryResolve(Metadata.Key, Tags.ACTN, number, out actionInfo);
        }

        public bool TryGetOrCreateModel(int number, out Bmdl model)
        {
            return Metadata.Resolver.TryResolve(Metadata.Key, Tags.BMDL, number, out model);
        }

        public bool TryGetOrCreateCostumeInfo(int number, out CostumeInfo costumeInfo)
        {
            return Metadata.Resolver.TryResolve(Metadata.Key, Tags.CMTL, number, out costumeInfo);
        }
    }
}
