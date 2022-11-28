using ezvm.Core.Programming;
using ezvm.Core.Programming.DTO;
using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.Programming.Operations;
using System;
using System.Linq;
using System.Text;

namespace ezvm.Core.VM
{
    public class ErrorEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public ErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }

    public class VirtualMachine
    {
        public enum VMState
        {
            Ok,
            Errored,
            Exited
        }
        public BufferAllocator Buffers { get; private set; }
        public CallFrame Stack { get; private set; }
        public ProgramDto Program { get; private set; }
        public string Args { get; private set; }
        public VMState State { get; private set; }

        public event EventHandler OnExit;
        public event EventHandler<ErrorEventArgs> OnError;

        public VirtualMachine(ProgramDto program, string args = "")
        {
            Args = args;
            Program = program;

            Reset();
        }

        public void Reset()
        {
            Buffers = new BufferAllocator();

            var argHeap = Buffers.Allocate(Encoding.ASCII.GetBytes(Args));
            var argVariable = new Variable("argBuff", argHeap.Id);

            Stack = new CallFrame(null,
                Program.Methods.First(m => m.Name == "main"),
                new Variable[] { argVariable }
                );
            Stack.OnExit += Main_OnExit;
            State = VMState.Ok;
        }

        private void Main_OnExit(object sender, EventArgs e)
        {
            State = VMState.Exited;
            OnExit?.Invoke(this, EventArgs.Empty);
        }

        public void Step(bool ignoreErrors = false)
        {
            //try
            //{
                if (!ignoreErrors && State != VMState.Ok)
                    throw new Exception("VM errored or exited; cannot perform further steps");
                var opData = Stack.Last.GetNextOperation();
                var op = Operations.Instance[opData.OpCode];
                op.Process(this, opData);
            //}
            //catch (Exception ex)
            //{
            //    State = VMState.Errored;
            //    OnError?.Invoke(this, new ErrorEventArgs(ex));
            //}
        }

        public enum ValueSource : byte
        {
            Constant,
            Variable
        }

        public enum HeapSourceSize : byte
        {
            Byte,
            Short,
            Int,
            Long
        }

        public long ResolveValue(OperatorDto op)
        {
            return op.Source switch
            {
                VirtualMachine.ValueSource.Constant => op.Constant.Value,
                VirtualMachine.ValueSource.Variable => Stack.Last[op.Variable].Value,
                _ => throw new Exception("Invalid path"),
            };
        }
    }
}
