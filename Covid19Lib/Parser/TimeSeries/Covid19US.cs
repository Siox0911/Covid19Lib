using Covid19Lib.Parser.Base;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Covid19Lib.Parser.TimeSeries
{
    /// <summary>
    /// Class to parse and present the US Covid-19 data
    /// </summary>
    public class Covid19US : IEquatable<Covid19US>, IComparable<Covid19US>, IComparable, IEqualityComparer<Covid19US>
    {
        /// <summary>
        /// The first row is always a header, all other rows are the values.
        /// </summary>
        public bool IsHeader { get; private set; }

        /// <summary>
        /// Unique Identifier for each row entry. Based on <seealso cref="Code3"/>.
        /// </summary>
        public string UID { get; private set; }

        /// <summary>
        /// Officialy assigned country code identifiers (2 letters)
        /// </summary>
        public string ISO2 { get; private set; }

        /// <summary>
        /// Officialy assigned country code identifiers. (3 letters)
        /// </summary>
        public string ISO3 { get; private set; }

        /// <summary>
        /// <para>
        /// A unique Number for a Country or Region.
        /// </para>
        /// </summary>
        public string Code3 { get; private set; }

        /// <summary>
        /// Federal Information Processing Standards code that uniquely identifies counties within the USA.
        /// </summary>
        public string FIPS { get; private set; }

        /// <summary>
        /// County name. US only.
        /// </summary>
        public string Admin2 { get; private set; }

        /// <summary>
        /// Province, state or dependency name.
        /// </summary>
        public string ProvinceOrState { get; private set; }

        /// <summary>
        /// Country, region or sovereignty name.
        /// The names of locations included on the Website correspond with the official designations used by the U.S. Department of State.
        /// </summary>
        public string CountryOrRegion { get; private set; }

        /// <summary>
        /// Dot locations on the dashboard. All points (except for Australia) shown on the map are based on geographic centroids,
        /// and are not representative of a specific address, building or any location at a spatial scale finer than a province/state.
        /// Australian dots are located at the centroid of the largest city in each state.
        /// </summary>
        public string Latitude { get; private set; }

        /// <summary>
        /// Dot locations on the dashboard. All points (except for Australia) shown on the map are based on geographic centroids,
        /// and are not representative of a specific address, building or any location at a spatial scale finer than a province/state.
        /// Australian dots are located at the centroid of the largest city in each state.
        /// </summary>
        public string Longitude { get; private set; }

        /// <summary>
        /// Summary of information
        /// </summary>
        public string CombinedKey { get; private set; }

        /// <summary>
        /// The population of the <seealso cref="ProvinceOrState"/>
        /// </summary>
        public string Population { get; private set; }

        /// <summary>
        /// The dates if IsHeader = true or the values if IsHeader = false;
        /// </summary>
        public List<DateValue> DateValues { get; private set; }

        /// <summary>
        /// This will calculate the cases of the last 24 hours. The calulation is the summary of current day - the summary of the day before.
        /// </summary>
        public int Last24Hours
        {
            get
            {
                //Current day
                var curDay = DateValues.Find(x => x.Date == DateValues.Last().Date)?.NumbersComplete ?? 0;
                var dayBefore = DateValues.Find(x => x.Date == DateValues.Last().Date - TimeSpan.FromDays(1))?.NumbersComplete ?? 0;
                return curDay - dayBefore;
            }
        }

        /// <summary>
        /// Parses a file from the timeseries and returns a list of <seealso cref="Covid19US"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Task<List<Covid19US>> ParseConfirmedAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var currentRow = 0; //Not zero based row
                try
                {
                    var listOfConfirmedGlobal = new List<Covid19US>();

                    using (var parser = new BaseParser(filePath))
                    {
                        while (!parser.EndOfData)
                        {
                            currentRow++;
                            var row = parser.ReadFields();

                            var newConfirmedGlobal = new Covid19US
                            {
                                IsHeader = currentRow == 1,
                                UID = row[0],
                                ISO2 = row[1],
                                ISO3 = row[2],
                                Code3 = row[3],
                                FIPS = row[4],
                                Admin2 = row[5],
                                ProvinceOrState = row[6],
                                CountryOrRegion = row[7],
                                Latitude = row[8],
                                Longitude = row[9],
                                CombinedKey = row[10],

                                //Here we have a unknown number of DateValues
                                DateValues = new List<DateValue>()
                            };

                            var numbersCurrentDay = 0;
                            //This loop is to adopt the date data with the values.
                            for (int i = 11; i < row.Length; i++)
                            {
                                if (currentRow == 1)
                                {
                                    //In the header row this is the date.
                                    newConfirmedGlobal.DateValues.Add(new DateValue { Date = DateTime.Parse(row[i], System.Globalization.CultureInfo.InvariantCulture) });
                                }
                                else
                                {
                                    //And here we will bring both together
                                    //So the first date is, in the header row, the first zero based entry of DateValues.
                                    var numberOfCasesInRow = int.Parse(row[i] ?? "0");
                                    newConfirmedGlobal.DateValues.Add(new DateValue { Date = listOfConfirmedGlobal[0].DateValues[i - 11].Date, NumbersComplete = numberOfCasesInRow, NumbersLast24Hours = numberOfCasesInRow - numbersCurrentDay });
                                    numbersCurrentDay = numberOfCasesInRow;
                                }
                            }

                            listOfConfirmedGlobal.Add(newConfirmedGlobal);
                        }
                    }
                    return listOfConfirmedGlobal;
                }
                catch (MalformedLineException)
                {
                    throw;
                }
                catch (ArgumentNullException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        /// <summary>
        /// Parses a file from the timeseries and returns a list of <seealso cref="Covid19US"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Task<List<Covid19US>> ParseDeathAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var currentRow = 0; //Not zero based row
                try
                {
                    var listOfConfirmedGlobal = new List<Covid19US>();

                    using (var parser = new BaseParser(filePath))
                    {
                        while (!parser.EndOfData)
                        {
                            currentRow++;
                            var row = parser.ReadFields();

                            var newConfirmedGlobal = new Covid19US
                            {
                                IsHeader = currentRow == 1,
                                UID = row[0],
                                ISO2 = row[1],
                                ISO3 = row[2],
                                Code3 = row[3],
                                FIPS = row[4],
                                Admin2 = row[5],
                                ProvinceOrState = row[6],
                                CountryOrRegion = row[7],
                                Latitude = row[8],
                                Longitude = row[9],
                                CombinedKey = row[10],
                                Population = row[11],

                                //Here we have a unknown number of DateValues
                                DateValues = new List<DateValue>()
                            };

                            var numbersCurrentDay = 0;
                            //This loop is to adopt the date data with the values.
                            for (int i = 12; i < row.Length; i++)
                            {
                                if (currentRow == 1)
                                {
                                    //In the header row are this the dates.
                                    newConfirmedGlobal.DateValues.Add(new DateValue { Date = DateTime.Parse(row[i], System.Globalization.CultureInfo.InvariantCulture) });
                                }
                                else
                                {
                                    //And here we will bring both together
                                    //So the first date is, in the header row, the first zero based entry of DateValues.
                                    var numberOfCasesInRow = int.Parse(row[i] ?? "0");
                                    newConfirmedGlobal.DateValues.Add(new DateValue { Date = listOfConfirmedGlobal[0].DateValues[i - 12].Date, NumbersComplete = numberOfCasesInRow, NumbersLast24Hours = numberOfCasesInRow - numbersCurrentDay });
                                    numbersCurrentDay = numberOfCasesInRow;
                                }
                            }

                            listOfConfirmedGlobal.Add(newConfirmedGlobal);
                        }
                    }
                    return listOfConfirmedGlobal;
                }
                catch (MalformedLineException)
                {
                    throw;
                }
                catch (ArgumentNullException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as Covid19US);
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(Covid19US other)
        {
            return other != null &&
                   IsHeader == other.IsHeader &&
                   ProvinceOrState == other.ProvinceOrState &&
                   CountryOrRegion == other.CountryOrRegion;
        }

        /// <summary>Serves as the default hash function.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(IsHeader, ProvinceOrState, CountryOrRegion);
        }

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

            if (obj is Covid19US x)
            {
                return CompareTo(x);
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
        public int CompareTo([AllowNull] Covid19US other) =>
            string.Equals(ProvinceOrState, other.ProvinceOrState, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(CountryOrRegion, other.CountryOrRegion, StringComparison.OrdinalIgnoreCase)
                ? string.Compare(CountryOrRegion, other.CountryOrRegion, StringComparison.OrdinalIgnoreCase)
                : string.Compare(ProvinceOrState, other.ProvinceOrState, StringComparison.OrdinalIgnoreCase);

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        ///   <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals([AllowNull] Covid19US x, [AllowNull] Covid19US y)
        {
            return x.Equals(y);
        }

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <param name="obj">The <see cref="object" /> for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is <see langword="null" />.</exception>
        public int GetHashCode([DisallowNull] Covid19US obj)
        {
            return obj.GetHashCode();
        }

        public static bool operator ==(Covid19US left, Covid19US right)
        {
            return EqualityComparer<Covid19US>.Default.Equals(left, right);
        }

        public static bool operator !=(Covid19US left, Covid19US right)
        {
            return !(left == right);
        }
    }
}
