// ReSharper disable once CheckNamespace
namespace EventLog
{
    using System;

    /// <summary>
    /// Used to convert .NET specific types to/from Bond types.
    /// </summary>
    public static class BondTypeAliasConverter
    {
        public static long Convert(DateTime? value, long unused)
        {
            if (!value.HasValue)
            {
                return -1;
            }

            return value.Value.Ticks;
        }

        public static DateTime? Convert(long value, DateTime? unused)
        {
            if (value == -1)
            {
                return null;
            }

            return new DateTime(value);
        }
    }
}