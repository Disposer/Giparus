using System;
using System.IO;

namespace Giparus.TeltonikaDriver
{
    internal class GenericParser
    {
        public static T Parse<T>(BinaryReader reader) where T : ISelfParser
        {
            var instance = (T)Activator.CreateInstance(typeof(T), true);
            instance.ParseAndFill(reader);

            return instance;
        }
    }
}
