using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CSGOStats.Infrastructure.Messaging.Payload;
using CSGOStats.Infrastructure.Messaging.Transport;
using CSGOStats.Services.HistoryParse.Objects;

namespace CSGOStats.Testing.MessageSimulator
{
    internal static class Program
    {
        private static readonly ICollection<MessageType> Messages = new List<MessageType>();

        private static Task Main()
        {
            RegisterTypes();
            return AwaitForCommandsAsync();
        }

        private static void RegisterTypes()
        {
            RegisterMessagesFromAssembly(typeof(HistoricalMatchParsed).Assembly);
        }

        private static void RegisterMessagesFromAssembly(Assembly assembly)
        {
            foreach (var item in assembly
                .GetTypes()
                .Where(typeof(IMessage).IsAssignableFrom)
                .Select(ConstructMessageType))
            {
                Messages.Add(item);
            }
        }

        private static MessageType ConstructMessageType(Type type) =>
            new MessageType(
                type.Name,
                type
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => new Func<IMessage>(new MessageConstructor(x).Construct))
                    .Single());

        private static async Task AwaitForCommandsAsync()
        {
            await PrintAvailableMessagesAsync();
            await Console.Out.WriteLineAsync(">> Listening for commands.");

            while (true)
            {
                var command = await GetCommandAsync();
                switch (command)
                {
                    case "quit":
                        return;
                    default:
                        await FindMessageAndSendAsync(command);
                        break;
                }
            }
        }

        private static async Task PrintAvailableMessagesAsync()
        {
            foreach (var message in Messages)
            {
                await Console.Out.WriteLineAsync($"-> {message.Code}");
            }
        }

        private static Task<string> GetCommandAsync() =>
            Console.In.ReadLineAsync();

        private static async Task FindMessageAndSendAsync(string messageType)
        {
            var message = Messages.SingleOrDefault(x => x.Code == messageType);
            if (message == null)
            {
                await Console.Out.WriteLineAsync("?? Can't find message");
                await PrintAvailableMessagesAsync();

                return;
            }

            await new RabbitMqEventBus(new RabbitMqConnectionConfiguration(
                "localhost",
                5672,
                "guest",
                "guest",
                1000)).PublishAsync(message.Builder.Invoke());
            await Console.Out.WriteLineAsync(">> Message instance has been sent.");
        }
    }
}
