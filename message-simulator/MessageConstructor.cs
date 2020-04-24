using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CSGOStats.Extensions.Validation;
using CSGOStats.Infrastructure.Extensions;
using CSGOStats.Infrastructure.Messaging.Payload;

namespace CSGOStats.Testing.MessageSimulator
{
    public class MessageConstructor
    {
        private static readonly Random Random = new Random();

        private static readonly char[] CapitalChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] LowercaseChars = "abcdefghijklnmopqrstuvwxyz".ToCharArray();
        private static readonly char[] NumericChars = "0123456789".ToCharArray();
        private static readonly char[] SymbolChars = "#$;%^:&?*()_-+=[]{}/|,.".ToCharArray();

        private readonly ConstructorInfo _constructor;

        public MessageConstructor(ConstructorInfo constructor)
        {
            _constructor = constructor.NotNull(nameof(constructor));
        }

        public IMessage Construct()
        {
            var parameters = _constructor
                .GetParameters()
                .Select(GenerateValue)
                .ToArrayFast();

            return _constructor.Invoke(parameters).OfType<IMessage>();
        }

        private static object GenerateValue(ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(string))
            {
                return RandomizeString();
            }

            if (parameter.ParameterType == typeof(int))
            {
                return RandomizeInt();
            }

            return null;
        }

        private static string RandomizeString() =>
            new string(Enumerable
                .Repeat(0, Random.Next(20, 51))
                .Select(x =>
                    GetRandomItem(
                        GetRandomItem(new[] {CapitalChars, LowercaseChars, NumericChars, SymbolChars})))
                .ToArrayFast());

        private static T GetRandomItem<T>(IReadOnlyList<T> array) => 
            array[Random.Next(array.Count)];

        private static int RandomizeInt() =>
            Random.Next(1000);
    }
}