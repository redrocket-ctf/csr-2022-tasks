using ezvm.Core.Programming.DTO;
using System;
using System.Linq;

namespace ezvm.Assembler.Builders
{
    public class ProgramBuilder : BaseBuilder<ProgramDto>
    {
        public static string InputBufferId => "argBuff";
        public static string InputBufferLength => "argBuffLength";
        
        public ProgramBuilder(string programName) : base()
        {
            Item.Name = programName;
        }

        public MethodBuilder AddMain()
        {
            var main = new MethodBuilder(this,
                method => Item.Methods = Item.Methods.Append(method).ToArray(),
                "main");
            main.AddParameters(InputBufferId)
                .AddLocals(InputBufferLength)
                .OpBufferGetLength(InputBufferLength).SetId().SetVariable(InputBufferId).Close()
                .Close();
            return main;
        }
        public MethodBuilder AddMethod(string name)
        {
            return new MethodBuilder(this,
                method => Item.Methods = Item.Methods.Append(method).ToArray(),
                name
                );
        }

        public ProgramBuilder AddObfuscatedBufferLoadingMethod(byte[] data, string methodName, Random RNG)
        {
            Action<MethodBuilder, string, string, string> writeByte = (builder, dataVar, buffId, offsetVar) => builder
                    .OpBufferWrite(dataVar, buffId, offsetVar)
                    .OpIncrement(offsetVar);

            var ops = new Action<MethodBuilder, byte, string>[]
            {
                    //Add
                    (builder, data, dst)=>
                    {
                        //dst = val1 + val2
                        var val1 = RNG.Next(data);
                        var val2 = data - val1;
                        builder
                            .OpSetVariable(dst, val1)
                            .OpAdd(dst, dst, val2);
                    },
                    //Subtract
                    (builder, data, dst)=>
                    {
                        //dst = val1 - val2
                        var val2 = RNG.Next(int.MaxValue);
                        var val1 = data + val2;
                        builder
                            .OpSetVariable(dst, val1)
                            .OpArithmetic(ArithmeticOperator.Subtract, dst)
                                .SetOperatorA().SetVariable(dst).Close()
                                .SetOperatorB().SetConstant(val2).Close()
                                .Close();
                    },
                    //ShiftLeft
                    (builder, data, dst)=>
                    {
                        var shifts = RNG.Next(56);
                        long randomBits = 0, shiftBits=0;
                        for (var i = 0; i< shifts; i++)
                        {
                            randomBits = randomBits << 1;
                            randomBits |= (RNG.Next(2) == 1) ? 1 : 0;
                            shiftBits = shiftBits << 1;
                            shiftBits |= 1;
                        }
                        var val = ((long)data << shifts) | randomBits;

                        builder
                            .OpSetVariable(dst, val)
                            .OpArithmetic(ArithmeticOperator.ShiftRight, dst)
                                .SetOperatorA().SetVariable(dst).Close()
                                .SetOperatorB().SetConstant(shifts).Close()
                                .Close()
                            .OpArithmetic(ArithmeticOperator.And, dst)
                                .SetOperatorA().SetVariable(dst).Close()
                                .SetOperatorB().SetConstant(0xff).Close()
                                .Close();
                    },
                    //Divide
                    (builder, data, dst) =>
                    {
                        //dst = val1 / val2
                        var val2 = RNG.Next(2, short.MaxValue);
                        var val1 = data * val2;

                        builder
                            .OpSetVariable(dst, val1)
                            .OpArithmetic(ArithmeticOperator.Divide, dst)
                                .SetOperatorA().SetVariable(dst).Close()
                                .SetOperatorB().SetConstant(val2).Close()
                                .Close();
                    }
    };


            var fill_buffer = AddMethod(methodName)
                .AddParameters("dstBuffId")
                .AddLocals("i", "tmp")
                .OpBufferAllocate("dstBuffId").SetLength().SetConstant(data.Length).Close().Close();

            foreach (var i in Enumerable.Range(0, data.Length).ToArray().Shuffle(RNG))
            {
                fill_buffer.OpSetVariable("i", i);
                ops[RNG.Next(ops.Length)](fill_buffer, data[i], "tmp");
                fill_buffer.OpBufferWrite("tmp", "dstBuffId", "i");
            }
            fill_buffer.OpReturn().Close();

            return this;
        }
    }
}
