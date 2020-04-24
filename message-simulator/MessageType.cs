using System;
using CSGOStats.Infrastructure.Messaging.Payload;

namespace CSGOStats.Testing.MessageSimulator
{
    public class MessageType
    {
        public string Code { get; }

        public Func<IMessage> Builder { get; }

        public MessageType(string code, Func<IMessage> builder)
        {
            Code = code;
            Builder = builder;
        }
    }
}