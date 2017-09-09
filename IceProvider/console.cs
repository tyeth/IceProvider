using System;

namespace IceProvider
{
    // ReSharper disable once InconsistentNaming
    public static class console
    {
        // ReSharper disable once InconsistentNaming
        public static void log (object value) => Console.WriteLine(value.ToString());
    }

}