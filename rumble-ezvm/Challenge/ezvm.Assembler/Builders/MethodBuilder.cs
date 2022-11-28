using ezvm.Core.Programming.DTO;
using ezvm.Core.Programming.DTO.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ezvm.Assembler.Builders
{
    public class MethodBuilder : NestedBuilder<MethodDto, ProgramBuilder>
    {
        public MethodBuilder(ProgramBuilder parentBuilder, Action<MethodDto> callback, string name) : base(parentBuilder, callback)
        {
            Item.Name = name;
        }

        public MethodBuilder AddParameters(params string[] parameters)
        {
            Item.Parameters = Item.Parameters.Concat(parameters).ToArray();
            return this;
        }

        public MethodBuilder AddLocals(params string[] locals)
        {
            Item.Locals = Item.Locals.Concat(locals).ToArray();
            return this;
        }

        private void AddOperation(BaseOperationDto op)
        {
            Item.Operations = Item.Operations.Append(op).ToArray();
        }

        public OpSetVariableBuilder OpSetVariable(string name)
        {
            return new OpSetVariableBuilder(this,
                op => AddOperation(new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.SetVariable,
                    SetVar = op
                }),
                name);
        }
        public MethodBuilder OpSetVariable(string name, long constA)
        {
            return OpSetVariable(name).SetValue().SetConstant(constA).Close().Close();
        }
        public MethodBuilder OpSetVariable(string name, string varA)
        {
            return OpSetVariable(name).SetValue().SetVariable(varA).Close().Close();
        }

        public OpMapValueBuilder OpMapValue(string dstName)
        {
            return new OpMapValueBuilder(this,
                op => AddOperation(new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.MapValue,
                    MapValue = op
                }),
                dstName);
        }

        public MethodBuilder OpCall(string methodName)
        {
            AddOperation(new BaseOperationDto()
            {
                OpCode = Core.Programming.Operations.OperationCode.Call,
                Call = methodName
            });
            return this;
        }

        public MethodBuilder OpDebug(DebugScope scope)
        {
            AddOperation(new BaseOperationDto()
            {
                OpCode = Core.Programming.Operations.OperationCode.DebugDump,
                DebugScope = scope
            });
            return this;
        }

        public MethodBuilder OpDebugBreak()
        {
            AddOperation(new BaseOperationDto()
            {
                OpCode = Core.Programming.Operations.OperationCode.DebugBreak
            });
            return this;
        }

        public MethodBuilder OpReturn()
        {
            AddOperation(new BaseOperationDto()
            {
                OpCode = Core.Programming.Operations.OperationCode.Return
            });
            return this;
        }

        public MethodBuilder OpJump(string marker)
        {
            AddOperation(new BaseOperationDto()
            {
                OpCode = Core.Programming.Operations.OperationCode.Jump,
                Jump = marker
            });
            return this;
        }

        public OpPrintBuilder OpPrint(PrintType type)
        {
            return new OpPrintBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.Print,
                    Print = op
                }), type);
        }

        public OpArithmeticBuilder OpArithmetic(ArithmeticOperator type, string destVar)
        {
            return new OpArithmeticBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = (Core.Programming.Operations.OperationCode)type,
                    Arithmetic = op
                }),
                destVar);
        }

        public MethodBuilder OpArithmetic(ArithmeticOperator type, string dstVar, string varA, string varB)
        {
            return OpArithmetic(type, dstVar)
                .SetOperatorA().SetVariable(varA).Close()
                .SetOperatorB().SetVariable(varB).Close()
                .Close();
        }
        public MethodBuilder OpArithmetic(ArithmeticOperator type, string dstVar, string varA, long constB)
        {
            return OpArithmetic(type, dstVar)
                .SetOperatorA().SetVariable(varA).Close()
                .SetOperatorB().SetConstant(constB).Close()
                .Close();
        }

        public MethodBuilder OpAdd(string dstVar, string varA, string varB)
        {
            return OpArithmetic(ArithmeticOperator.Add, dstVar, varA, varB);
        }
        public MethodBuilder OpAdd(string dstVar, string varA, long constB)
        {
            return OpArithmetic(ArithmeticOperator.Add, dstVar, varA, constB);
        }

        public MethodBuilder OpIncrement(string dstVar)
        {
            return OpArithmetic(ArithmeticOperator.Add, dstVar, dstVar, 1);
        }

        public MethodBuilder OpDecrement(string dstVar)
        {
            return OpArithmetic(ArithmeticOperator.Subtract, dstVar, dstVar, 1);
        }

        public MethodBuilder OpModulo(string dstVar, string varA, string varB)
        {
            return OpArithmetic(ArithmeticOperator.Modulo, dstVar, varA, varB);
        }
        public MethodBuilder OpModulo(string dstVar, string varA, long constB)
        {
            return OpArithmetic(ArithmeticOperator.Modulo, dstVar, varA, constB);
        }

        public MethodBuilder OpJumpCmp(string cmp, Comparator comp, string varA, string varB, string markerTrue, string markerFalse)
        {
            if (!Item.HasLocalOrParameter(cmp))
                AddLocals(cmp);
            return OpCompare(cmp, comp)
                .SetOperatorA().SetVariable(varA).Close()
                .SetOperatorB().SetVariable(varB).Close()
                .Close()
            .OpIf(cmp)
                .SetMarkerFalse(markerFalse)
                .SetMarkerTrue(markerTrue)
                .Close();
        }
        public MethodBuilder OpJumpCmp(string cmp, Comparator comp, string varA, long constB, string markerTrue, string markerFalse)
        {
            if (!Item.HasLocalOrParameter(cmp))
                AddLocals(cmp);
            return OpCompare(cmp, comp)
                .SetOperatorA().SetVariable(varA).Close()
                .SetOperatorB().SetConstant(constB).Close()
                .Close()
            .OpIf(cmp)
                .SetMarkerFalse(markerFalse)
                .SetMarkerTrue(markerTrue)
                .Close();
        }

        public MethodBuilder OpJumpIfAltB(string cmp, string varA, string varB, string markerTrue, string markerFalse)
        {
            return OpJumpCmp(cmp, Comparator.Lesser, varA, varB, markerTrue, markerFalse);
        }
        public MethodBuilder OpJumpIfAltB(string cmp, string varA, long constB, string markerTrue, string markerFalse)
        {
            return OpJumpCmp(cmp, Comparator.Lesser, varA, constB, markerTrue, markerFalse);
        }
        public MethodBuilder OpJumpIfAeqB(string cmp, string varA, string varB, string markerTrue, string markerFalse)
        {
            return OpJumpCmp(cmp, Comparator.Equal, varA, varB, markerTrue, markerFalse);
        }
        public MethodBuilder OpJumpIfAeqB(string cmp, string varA, long constB, string markerTrue, string markerFalse)
        {
            return OpJumpCmp(cmp, Comparator.Equal, varA, constB, markerTrue, markerFalse);
        }

        public delegate void IfTrueAction(MethodBuilder builder, string markerEnd);
        public delegate void IfFalseAction(MethodBuilder builder, string markerEnd);
        public MethodBuilder OpIf(string cmp, IfTrueAction ifTrue = null, IfFalseAction ifFalse = null)
        {
            var markerTrue = $"jmp_true_{Guid.NewGuid()}";
            var markerFalse = $"jmp_false_{Guid.NewGuid()}";
            var markerEnd = $"jmp_end_{Guid.NewGuid()}";
            OpIf(cmp)
                .SetMarkerTrue(markerTrue)
                .SetMarkerFalse(markerFalse)
                .Close();
            AddLabel(markerTrue);
            if (ifTrue != null)
            {
                ifTrue(this, markerEnd);
                OpJump(markerEnd).Close();
            }
            AddLabel(markerFalse);
            ifFalse?.Invoke(this, markerEnd);
            AddLabel(markerEnd);
            return this;
        }

        public MethodBuilder AddLabel(string label)
        {
            Item.Markers[label] = Item.Operations.Length;
            return this;
        }

        public OpCompareBuilder OpCompare(string destVar, Comparator cmp)
        {
            return new OpCompareBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.Compare,
                    Compare = op
                }),
                destVar, cmp);
        }

        public OpPushBuilder OpPush(PushType type, string parameter)
        {
            return new OpPushBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.Push,
                    Push = op
                }),
                type, parameter);
        }

        public OpIfBuilder OpIf(string testVar)
        {
            return new OpIfBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.If,
                    If = op
                }),
                testVar);
        }

        public OpBufferFreeBuilder OpBufferFree()
        {
            return new OpBufferFreeBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.BufferFree,
                    BufferInfo = op
                }));
        }

        public OpBufferAllocateBuilder OpBufferAllocate(string dstName)
        {
            return new OpBufferAllocateBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.BufferAllocate,
                    BufferAllocate = op
                }), dstName);
        }

        public OpBufferReadBuilder OpBufferRead(string dstName)
        {
            return new OpBufferReadBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.BufferRead,
                    BufferRead = op
                }), dstName);
        }

        public MethodBuilder OpBufferRead(string dstName, string bufferId, string offset, Core.VM.Buffer.Size size = Core.VM.Buffer.Size.i8)
        {
            return OpBufferRead(dstName)
                .SetId().SetVariable(bufferId).Close()
                .SetOffset().SetVariable(offset).Close()
                .SetSize(size)
                .Close();
        }

        public OpBufferWriteBuilder OpBufferWrite()
        {
            return new OpBufferWriteBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.BufferWrite,
                    BufferWrite = op
                }));
        }

        public MethodBuilder OpBufferWrite(string value, string bufferId, string offset, Core.VM.Buffer.Size size = Core.VM.Buffer.Size.i8)
        {
            return OpBufferWrite()
                .SetValue().SetVariable(value).Close()
                .SetId().SetVariable(bufferId).Close()
                .SetOffset().SetVariable(offset).Close()
                .SetSize(size)
                .Close();
        }

        public MethodBuilder OpBufferWrite(long value, string bufferId, string offset, Core.VM.Buffer.Size size = Core.VM.Buffer.Size.i8)
        {
            return OpBufferWrite()
                .SetValue().SetConstant(value).Close()
                .SetId().SetVariable(bufferId).Close()
                .SetOffset().SetVariable(offset).Close()
                .SetSize(size)
                .Close();
        }

        public OpBufferGetLengthBuilder OpBufferGetLength(string destination)
        {
            return new OpBufferGetLengthBuilder(this, (op) => AddOperation(
                new BaseOperationDto()
                {
                    OpCode = Core.Programming.Operations.OperationCode.BufferGetLength,
                    BufferGetLength = op
                }), destination);
        }

        public MethodBuilder AddJunkSpin(int spinCnt, IEnumerator<string> nameGenerator, Random RNG)
        {
#if !DEBUG
            var name_method = nameGenerator.Get();
            var name_num = nameGenerator.Get();
            var name_i = nameGenerator.Get();
            var name_cmp = nameGenerator.Get();
            var name_loop = nameGenerator.Get();
            var name_cnt = nameGenerator.Get();
            var name_end = nameGenerator.Get();
            var hasParam = RNG.GetBool();

            if (hasParam)
            {
                ParentBuilder.AddMethod(name_method)
                        .AddParameters(name_num)
                        .AddLocals(name_i, name_cmp)
                        .AddLabel(name_loop)
                        .OpJumpIfAltB(name_cmp, name_i, name_num, name_cnt, name_end)
                        .AddLabel(name_cnt)
                        .OpIncrement(name_i)
                        .OpJump(name_loop)
                        .AddLabel(name_end)
                        .OpReturn()
                        .Close();
                OpPush(PushType.ByVal, name_num).SetValue().SetConstant(spinCnt).Close().Close();
                OpCall(name_method);
            }
            else
            {
                ParentBuilder.AddMethod(name_method)
                    .AddLocals(name_i, name_cmp, name_num)
                    .OpSetVariable(name_num, spinCnt)
                    .AddLabel(name_loop)
                    .OpJumpIfAltB(name_cmp, name_i, name_num, name_cnt, name_end)
                    .AddLabel(name_cnt)
                    .OpIncrement(name_i)
                    .OpJump(name_loop)
                    .AddLabel(name_end)
                    .OpReturn()
                    .Close();
                OpCall(name_method);
            }
#endif
            return this;
        }

        public MethodBuilder AddJunkOps(int num, IEnumerator<string> nameGenerator, Random RNG)
        {
#if !DEBUG
            var jmp_name = nameGenerator.Get();
            OpJump(jmp_name);
            var ops = new Action[]
            {
                () => { 
                    //Generic v + c
                    var n = nameGenerator.Get();
                    AddLocals(n);
                    var op = (ArithmeticOperator)RNG.Next((int)ArithmeticOperator.And, (int)ArithmeticOperator.ShiftRight + 1);
                    OpArithmetic(op, n)
                        .SetOperatorA().SetVariable(n).Close()
                        .SetOperatorB().SetConstant(RNG.Next(1, 256)).Close()
                        .Close();
                },
                () => { 
                    //Generic c + c
                    var n = nameGenerator.Get();
                    AddLocals(n);
                    var op = (ArithmeticOperator)RNG.Next((int)ArithmeticOperator.And, (int)ArithmeticOperator.ShiftRight + 1);
                    OpArithmetic(op, n)
                        .SetOperatorA().SetConstant(RNG.Next(1, 256)).Close()
                        .SetOperatorB().SetConstant(RNG.Next(1, 256)).Close()
                        .Close();
                },
                () => { 
                    //Generic v + v
                    var n = nameGenerator.Get();
                    var m = nameGenerator.Get();
                    var x = nameGenerator.Get();
                    OpSetVariable(n).SetValue().SetConstant(RNG.Next(1,255)).Close().Close();
                    OpSetVariable(m).SetValue().SetConstant(RNG.Next(1,255)).Close().Close();
                    OpSetVariable(x).SetValue().SetConstant(RNG.Next(1,255)).Close().Close();
                    AddLocals(n, m, x);
                    var op = (ArithmeticOperator)RNG.Next((int)ArithmeticOperator.And, (int)ArithmeticOperator.ShiftRight + 1);
                    OpArithmetic(op, n)
                        .SetOperatorA().SetVariable(m).Close()
                        .SetOperatorB().SetVariable(x).Close()
                        .Close();
                }
            };
            for (int i = 0; i < num; i++) ops[RNG.Next(ops.Length)]();
            AddLabel(jmp_name);
#endif
            return this;
        }
    }
}
