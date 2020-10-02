using Open3dmm.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Open3dmm
{
    public class Tagl
    {
        private readonly Group<GlobalReference> list;

        public Tagl()
        {
            list = new Group<GlobalReference>();
        }

        public void Add(in GlobalReference reference, bool update)
        {
            int index = list.BinarySearch(reference);
            if (index < 0)
                list.Insert(~index, reference);
            else if (update)
                list[index] = reference;
        }
    }
}
