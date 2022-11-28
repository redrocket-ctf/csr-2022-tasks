using ezvm.Core.Programming;
using ezvm.Core.Programming.DTO;
using ezvm.Core.Programming.DTO.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ezvm.Core.VM
{
    public class CallFrame
    {
        public IEnumerable<Variable> Locals => locals;
        public IEnumerable<Variable> CallStack => callStack;
        public int OperationIdx { get; set; }
        public MethodDto Method { get; private set; }

        private List<Variable> callStack;
        private readonly Variable[] locals;

        public CallFrame Previous { get; private set; }
        public CallFrame Next { get; private set; }
        public CallFrame First => Previous?.First ?? this;
        public CallFrame Last => Next?.Last ?? this;
        public bool IsFirst => Previous == null;
        public bool IsLast=> Next == null;

        public event EventHandler OnExit;
        public event EventHandler OnEntered;
        public BaseOperationDto LastOperation { get; private set; }
        public BaseOperationDto CurrentOperation { get; private set; }
        public Variable this[string name] => locals.FirstOrDefault(l=>l.Name == name);

        public CallFrame(CallFrame previous, MethodDto method, IEnumerable<Variable> locals)
        {
            Previous = previous;
            Method = method;
            if (Previous != null) Previous.Next = this;
            this.locals = locals.Concat(method.Locals.Select(l=>new Variable(l))).ToArray();
            callStack = new List<Variable>();
        }

        public void Push(Variable var)
        {
            callStack.Add(var);
        }

        public BaseOperationDto GetNextOperation()
        {
            LastOperation = CurrentOperation;
            CurrentOperation = Method.Operations[OperationIdx++];
            return CurrentOperation;
        }

        public CallFrame CallMethod(MethodDto method)
        {
            if (method.Parameters.Any(param => !callStack.Any(var => var.Name == param)))
                throw new Exception($"Incomplete parameters for call to {method.Name}");

            var parameters = callStack.Where(var => method.Parameters.Contains(var.Name)).ToArray();
            callStack = callStack.Except(parameters).ToList();
            return new CallFrame(this, method, parameters);
        }

        public CallFrame ExitMethod()
        {
            if (Previous != null) Previous.Next = null;
            OnExit?.Invoke(this, EventArgs.Empty);
            return Previous;
        }

        public override string ToString()
        {
            return $"{Method} @{OperationIdx}";
        }

        public string Dump(string indent = "", bool recursive=true)
        {
            var b = new StringBuilder();
            b.Append(indent);
            b.AppendLine(ToString());

            b.Append(indent);
            b.AppendLine($" - Locals[{locals.Length}]:");
            foreach (var variable in locals)
            {
                b.Append(indent);
                b.AppendLine($"   - {variable}");
            }

            b.Append(indent);
            b.AppendLine($" - Callstack[{callStack.Count}]:");
            foreach (var variable in callStack)
            {
                b.Append(indent);
                b.AppendLine($"   - {variable}");
            }

            b.Append(indent);
            if (LastOperation != null)
                b.AppendLine($" - Last Operation: [{LastOperation}@{Array.IndexOf(Method.Operations, LastOperation)}]");
            else
                b.AppendLine($" - Last Operation: <none>");

            b.Append(indent);
            if (CurrentOperation != null)
                b.AppendLine($" - Current Operation: [{CurrentOperation}@{Array.IndexOf(Method.Operations, CurrentOperation)}]");
            else
                b.AppendLine($" - Current Operation: <none>");

            if (recursive && Next != null)
                b.Append(Next.Dump(indent + "   "));

            return b.ToString();
        }
    }
}
