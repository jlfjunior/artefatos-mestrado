using FluentValidation.Results;
using System;

namespace Core.Bus
{
    public abstract class Message
    {
        public string MessageType { get; set; }
        public ValidationResult ValidationResult { get; set; }

        protected Message()
        {
            MessageType = GetType().Name;
        }

        public virtual bool IsValid()
        {
            throw new NotImplementedException();
        }
    }
}
