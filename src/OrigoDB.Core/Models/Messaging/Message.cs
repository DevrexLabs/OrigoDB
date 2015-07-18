using System;

namespace OrigoDB.Core.Models
{
    /// <summary>
    /// All messages handled by MessageBroker derive from Message
    /// </summary>
    [Serializable]
    public abstract class Message
    {
        /// <summary>
        /// Unique id of the message, assigned when the message is created
        /// </summary>
        public readonly Guid Id;

        /// <summary>
        /// Subject is an optional header describing the message
        /// </summary>
        public readonly String Subject;

        /// <summary>
        /// Timestamp when message was created
        /// </summary>
        public readonly DateTime Created;

        protected Message()
        {
            Id = Guid.NewGuid();
            Created = DateTime.Now;
        }

        protected Message(String subject) : this()
        {
            Subject = subject;
        }
    }
}