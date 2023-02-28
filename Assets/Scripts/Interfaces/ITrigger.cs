
    using System;

    public interface ITrigger
    {
        public bool Value { get; }

        public void Set();

        public void Reset();

        public event Action SetEvent;
        public event Action ResetEvent;
    }
