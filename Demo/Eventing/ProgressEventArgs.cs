using System;

namespace Eventing
{
    public sealed class ProgressEventArgs<T> : EventArgs
    {
        public ProgressEventArgs(T progress)
            : base()
        {
            _progress = progress;
        }

        private T _progress;

        public T Progress
        {
            get
            {
                return _progress;
            }
        }
    }
}