using System;

namespace ezvm.Assembler.Builders
{
    public abstract class BaseBuilder<T> where T : new()
    {
        public T Item { get; private set; }

        protected BaseBuilder()
        {
            Item = new T();
        }
    }

    public abstract class NestedBuilder<T, R> : BaseBuilder<T> where T: new()
    {
        private readonly R parentBuilder;
        private readonly Action<T> callback;

        protected R ParentBuilder => parentBuilder;

        protected NestedBuilder(R parentBuilder, Action<T> callback)
        {
            this.parentBuilder = parentBuilder;
            this.callback = callback;
        }

        public R Close()
        {
            callback(Item);
            return parentBuilder;
        }
    }
}
