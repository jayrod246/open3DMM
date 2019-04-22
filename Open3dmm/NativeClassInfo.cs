using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Open3dmm
{
    public class NativeClassInfo
    {
        private NativeClassInfo(int vtable, int size, Dictionary<int, FieldData> fields)
        {
            Vtable = vtable;
            Size = size;
            Fields = fields;
        }
        private static Dictionary<string, NativeClassInfo> classes = new Dictionary<string, NativeClassInfo>();
        public static IReadOnlyDictionary<string, NativeClassInfo> Classes => classes;

        public int Vtable { get; set; }
        public int Size { get; set; }

        public Dictionary<int, FieldData> Fields { get; }

        public string ClassName {
            get {
                unsafe
                {
                    var vtable = Vtable;
                    return ((BASE*)&vtable)->ClassID;
                }
            }
        }

        public static NativeClassInfo GetOrCreateClassInfo(string id, int vtable, int size)
        {
            if (!classes.TryGetValue(id, out var info))
            {
                Dictionary<int, FieldData> fields = null;

                if (vtable != 0)
                {
                    unsafe
                    {
                        var inherits = ((BASE*)&vtable)->Inheritance;
                        foreach (var p in inherits.Reverse())
                        {
                            fields = GetOrCreateClassInfo(p, vtable, size).Fields;
                        }
                    }
                }
                
                classes[id] = info = new NativeClassInfo(vtable, size, fields ?? new Dictionary<int, FieldData>());
            }
            return info;
        }

        public override string ToString()
        {
            var b = new StringBuilder(ClassName);
            if (Fields.Count > 0)
            {
                b.Append(" { ");
                foreach (var f in Fields)
                    b.AppendFormat("[{0}]{1}, ", f.Key, f.Value.GuessedClassName);
                b.Remove(b.Length - 2, 2);
                b.Append(" }");
            }
            return b.ToString();
        }

        internal string ToClassDefinition(out string baseId)
        {
            var b = new StringBuilder();
            b.AppendLine("using System.Runtime.InteropServices;");
            b.AppendLine();
            b.AppendLine("namespace Open3dmm.Classes");
            b.AppendLine("\t{");
            b.AppendFormat("\t[StructLayout(LayoutKind.Explicit, Size = {0})]", Size);
            b.AppendLine();
            b.AppendFormat("\tpublic unsafe struct Native{0}", ClassName);
            b.AppendLine();

            b.AppendLine("{");
            unsafe
            {
                var vtable = Vtable;
                baseId = ((BASE*)&vtable)->BaseClassID;
            }
            b.AppendLine("\t\t[FieldOffset(0x0000)]");
            b.AppendFormat("\t\tpublic Native{0} Base;", baseId);
            if (Fields.Count > 0)
            {
                foreach (var f in Fields)
                {
                    b.AppendLine();
                    b.AppendLine();
                    b.AppendFormat("\t\t[FieldOffset(0x{0:X4})]", f.Key);
                    b.AppendLine();
                    b.AppendFormat("\t\tpublic Native{0}* Field{1:X4};", f.Value.GuessedClassName, f.Key);
                    b.AppendLine();
                }
            }
            b.AppendLine("\t}");
            b.AppendLine("}");
            return b.ToString();
        }
    }

    public struct FieldData
    {
        public int Vtable;
        public int Level;
        public string GuessedClassName {
            get {
                unsafe
                {
                    var vtable = Vtable;
                    return ((BASE*)&vtable)->Inheritance[Level];
                }
            }
        }
    }
}
