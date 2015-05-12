// ReSharper disable once CheckNamespace
namespace HttpLog
{
    using System;

    /// <summary>
    /// Used to convert .NET specific types to/from Bond types.
    /// </summary>
    public static class BondTypeAliasConverter
    {
        public static long Convert(DateTime value, long unused)
        {
            return value.Ticks;
        }

        public static DateTime Convert(long value, DateTime unused)
        {
            return new DateTime(value);
        }
    }
}