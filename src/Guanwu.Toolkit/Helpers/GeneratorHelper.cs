using System;
using Guanwu.Toolkit.Extensions.TimeSpan;

namespace Guanwu.Toolkit.Helpers
{
    public sealed class GeneratorHelper
    {
        public static string RandomId => Guid.NewGuid().ToString("N");
        public static string RandomLongId => BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0).ToString();
        public static long UnixTime => DateTime.UtcNow.ToUnixTime();
    }
}