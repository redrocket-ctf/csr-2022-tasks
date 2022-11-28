using ezvm.Core.Programming.DTO;
using ezvm.Core.Programming.DTO.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ezvm.Assembler
{
    public class Obfuscator
    {
        private static IEnumerable<OperatorDto> FindOperators(object o)
        {
            if (o == null)
                yield break;
            var _t = typeof(OperatorDto);
            var _o = typeof(ValidatableDto);
            foreach (var prop in o.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (prop.PropertyType == _t)
                    yield return (OperatorDto)prop.GetValue(o);
                else if (prop.PropertyType.IsAssignableTo(_o))
                    foreach (var op in FindOperators(prop.GetValue(o)))
                        yield return op;
            }
        }

        public static void Obfuscate(ProgramDto program, IEnumerator<string> nameGenerator, Random RNG = null)
        {
            if (RNG == null) RNG = new Random(Environment.TickCount);
            
            foreach (var meth in program.Methods)
            {
                //Rename method name
                if (meth.Name != "main")
                {
                    var new_name = nameGenerator.Get();
                    var old_name = meth.Name;

                    meth.Name = new_name;

                    foreach (var m in program.Methods)
                        foreach (var o in m.Operations)
                            if (o.OpCode == Core.Programming.Operations.OperationCode.Call && o.Call == old_name)
                                o.Call = new_name;
                }

                //Rename markers
                var new_markers = new Dictionary<string, int>();
                foreach (var marker in meth.Markers)
                {
                    var new_name = nameGenerator.Get();
                    var old_name = marker.Key;
                    new_markers[new_name] = marker.Value;

                    foreach(var o in meth.Operations)
                    {
                        switch (o.OpCode)
                        {
                            case Core.Programming.Operations.OperationCode.Jump:
                                if (o.Jump == old_name) o.Jump = new_name;
                                break;
                            case Core.Programming.Operations.OperationCode.If:
                                if (o.If.MarkerTrue == old_name) o.If.MarkerTrue = new_name;
                                if (o.If.MarkerFalse == old_name) o.If.MarkerFalse = new_name;
                                break;
                        }
                    }
                }
                meth.Markers = new_markers;

                //Obfuscate strings
                var prints = meth.Operations.Where(o => o.Print != null && o.Print.Source == PrintSource.Constant && o.Print.Constant.Length > 1).ToArray();
                foreach (var print in prints.Reverse())
                {
                    var old_idx = Array.IndexOf(meth.Operations, print);
                    var str = print.Print.Constant;
                    var instrs = str.Select(chr => new BaseOperationDto
                    {
                        OpCode = Core.Programming.Operations.OperationCode.Print,
                        Print = new PrintDto
                        {
                            Type = PrintType.Char,
                            Source = PrintSource.Constant,
                            Constant = chr.ToString()
                        }
                    }).ToArray();
                    if (print.Print.AppendNewLine) instrs.Last().Print.AppendNewLine = true;
                    var ops = meth.Operations.ToList();
                    ops.RemoveAt(old_idx);
                    ops.InsertRange(old_idx, instrs);
                    meth.Operations = ops.ToArray();

                    var newdist = instrs.Length - 1;
                    foreach (var marker in meth.Markers)
                        if (marker.Value > old_idx)
                            meth.Markers[marker.Key] = marker.Value + newdist;
                }

                //Shuffle params + locals
                meth.Locals = meth.Locals.Shuffle(RNG);
                meth.Parameters = meth.Parameters.Shuffle(RNG);
            }
        }
    }
}
