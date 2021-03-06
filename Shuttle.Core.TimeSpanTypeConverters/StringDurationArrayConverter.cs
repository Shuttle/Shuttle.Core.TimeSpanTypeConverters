using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Shuttle.Core.Contract;

namespace Shuttle.Core.TimeSpanTypeConverters
{
    public class StringDurationArrayConverter : TypeConverter
    {
        private static readonly Regex Expression = new Regex(@"(?<duration>\d+)(?<type>ms|[smhd])(\*(?<repeat>\d+))*",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            Guard.AgainstNull(value, nameof(value));

            if (value?.GetType() != typeof(string))
            {
                return new[] {TimeSpan.FromMinutes(30)};
            }

            var values = ((string) value).Split(new[] {';', ','}, StringSplitOptions.RemoveEmptyEntries);

            var result = new List<TimeSpan>();

            foreach (var durationValue in values)
            {
                var match = Expression.Match(durationValue);
                int repeat;

                if (!match.Success || !int.TryParse(match.Groups["duration"].Value, out var duration))
                {
                    throw new InvalidOperationException(
                        string.Format(Resources.StringDurationFormatError, value));
                }

                var repeatValue = match.Groups["repeat"].Value;

                if (!string.IsNullOrEmpty(repeatValue))
                {
                    if (!int.TryParse(repeatValue, out repeat))
                    {
                        throw new InvalidOperationException(
                            string.Format(Resources.StringDurationFormatError, value));
                    }
                }
                else
                {
                    repeat = 1;
                }

                var timeSpan = new TimeSpan();

                switch (match.Groups["type"].Value.ToLower())
                {
                    case "ms":
                    {
                        timeSpan = TimeSpan.FromMilliseconds(duration);

                        break;
                    }
                    case "s":
                    {
                        timeSpan = TimeSpan.FromSeconds(duration);

                        break;
                    }
                    case "m":
                    {
                        timeSpan = TimeSpan.FromMinutes(duration);

                        break;
                    }
                    case "h":
                    {
                        timeSpan = TimeSpan.FromHours(duration);

                        break;
                    }
                    case "d":
                    {
                        timeSpan = TimeSpan.FromDays(duration);

                        break;
                    }
                }

                for (var entry = 0; entry < repeat; entry++)
                {
                    result.Add(timeSpan);
                }
            }

            return result.ToArray();
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            throw new NotImplementedException();
        }
    }
}