using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Covid19Lib.Parser.TimeSeries
{
    public class DateValue : IComparable, IComparable<DateValue>
    {
        /// <summary>
        /// Day of the cases, this meens the sum of all cases.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// The number of infected cases in summary.
        /// </summary>
        public int NumbersComplete { get; set; }

        /// <summary>
        /// The number of cases in the past 24 hours
        /// </summary>
        public int Numbers { get; set; }


        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///   Value
        ///   Meaning
        ///   Less than zero
        ///   This instance precedes <paramref name="obj" /> in the sort order.
        ///   Zero
        ///   This instance occurs in the same position in the sort order as <paramref name="obj" />.
        ///   Greater than zero
        ///   This instance follows <paramref name="obj" /> in the sort order.</returns>
        /// <exception cref="ArgumentException">
        ///   <paramref name="obj" /> is not the same type as this instance.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (obj is DateValue x)
            {
                if (x.Date.CompareTo(Date) == 0)
                {
                    return x.NumbersComplete.CompareTo(NumbersComplete);
                }

                return x.Date.CompareTo(Date);
            }

            throw new ArgumentException("", nameof(obj));
        }


        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///   Value
        ///   Meaning
        ///   Less than zero
        ///   This instance precedes <paramref name="other" /> in the sort order.
        ///   Zero
        ///   This instance occurs in the same position in the sort order as <paramref name="other" />.
        ///   Greater than zero
        ///   This instance follows <paramref name="other" /> in the sort order.</returns>
        public int CompareTo([AllowNull] DateValue other)
        {
            return CompareTo((object)other);
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"{Date}: {NumbersComplete}";
        }
    }
}
