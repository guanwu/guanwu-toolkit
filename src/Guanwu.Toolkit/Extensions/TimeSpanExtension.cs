using System;

namespace Guanwu.Toolkit.Extensions.TimeSpan
{
    public static class TimeSpanExtension
    {
        public static long ToUnixTime(this DateTime input) => (input.Ticks - 621355968000000000) / 10000;
    }
}