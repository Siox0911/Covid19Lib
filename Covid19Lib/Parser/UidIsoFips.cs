using Covid19Lib.Parser.Base;
using Covid19Lib.Parser.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Covid19Lib.Parser
{
    public class UidIsoFips
    {
        /// <summary>
        /// The first row is always a header, all other rows are the values.
        /// </summary>
        public bool IsHeader { get; set; }

        /// <summary>
        /// Unique Identifier for each row entry. Based on <seealso cref="Code3"/>.
        /// </summary>
        public string UID { get; set; }

        /// <summary>
        /// Officialy assigned country code identifiers (2 letters)
        /// </summary>
        public string ISO2 { get; set; }

        /// <summary>
        /// Officialy assigned country code identifiers. (3 letters)
        /// </summary>
        public string ISO3 { get; set; }

        /// <summary>
        /// <para>
        /// A unique Number for a Country or Region.
        /// </para>
        /// <para>
        /// Germany: alphabetically ordered all admin1 regions (including Unknown), and their UIDs are from 27601 to 27617. Germany itself Code3 is 276.
        /// </para>
        /// </summary>
        public string Code3 { get; set; }

        /// <summary>
        /// Federal Information Processing Standards code that uniquely identifies counties within the USA. US only
        /// </summary>
        public string FIPS { get; set; }

        /// <summary>
        /// County name. US only.
        /// </summary>
        public string Admin2 { get; set; }

        /// <summary>
        /// Province, state or dependency name. Example Moscow Oblast (Russia) or Bayern (Germany) or Virginia (US)
        /// </summary>
        public string ProvinceOrState { get; set; }

        /// <summary>
        /// Country, region or sovereignty name.
        /// The names of locations included on the Website correspond with the official designations used by the U.S. Department of State.
        /// </summary>
        public string CountryOrRegion { get; set; }

        /// <summary>
        /// Dot locations on the dashboard. All points (except for Australia) shown on the map are based on geographic centroids,
        /// and are not representative of a specific address, building or any location at a spatial scale finer than a province/state.
        /// Australian dots are located at the centroid of the largest city in each state.
        /// </summary>
        public string Latitude { get; set; }

        /// <summary>
        /// Dot locations on the dashboard. All points (except for Australia) shown on the map are based on geographic centroids,
        /// and are not representative of a specific address, building or any location at a spatial scale finer than a province/state.
        /// Australian dots are located at the centroid of the largest city in each state.
        /// </summary>
        public string Longitude { get; set; }

        /// <summary>
        /// Summary of information
        /// </summary>
        public string CombinedKey { get; set; }

        /// <summary>
        /// Population of the <seealso cref="ProvinceOrState"/> or if <seealso cref="ProvinceOrState"/> is null, the population of <seealso cref="CountryOrRegion"/>
        /// </summary>
        public string Population { get; set; }

        /// <summary>
        /// Gives back a timeseries for this province or state. If <seealso cref="ProvinceOrState"/> is null or empty, gives back the values of <seealso cref="CountryOrRegion"/>.
        /// </summary>
        public List<Covid19Global> Covid19Globals { get { return GetTimeSeriesCovid19GlobalsAsync().Result; } }

        public List<Covid19US> Covid19Us { get { return GetTimeSeriesCovid19USAsync().Result; } }

        /// <summary>
        /// Parses the UID_ISO_FIPS_LookUp_Table.csv file and returns a list of <seealso cref="UidIsoFips"/>.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Task<List<UidIsoFips>> ParseAsync(string filePath)
        {
            return Task.Run(() =>
            {
                var currentRow = 0;
                try
                {
                    var listOfUidIsoFips = new List<UidIsoFips>();

                    using (var parser = new BaseParser(filePath))
                    {
                        while (!parser.EndOfData)
                        {
                            currentRow++;
                            var row = parser.ReadFields();

                            var uidIsoFips = new UidIsoFips
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
                                Population = row[11]
                            };

                            listOfUidIsoFips.Add(uidIsoFips);
                        }
                    }

                    return listOfUidIsoFips;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }

        /// <summary>
        /// Gives back a timeseries for this province or state. If <seealso cref="ProvinceOrState"/> is null or empty, gives back the values of <seealso cref="CountryOrRegion"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Covid19Global>> GetTimeSeriesCovid19GlobalsAsync()
        {
            try
            {
                var allResult = await Covid19Global.ParseAsync(Settings.TimeSeriesConfirmedGlobalFile).ConfigureAwait(false);
                return allResult.Where(x => string.IsNullOrEmpty(x.CountryOrRegion) ? x.ProvinceOrState == ProvinceOrState : x.CountryOrRegion == CountryOrRegion).ToList();
            }
            catch (AggregateException)
            {
                throw;
            }
        }

        /// <summary>
        /// Gives back a timeseries for this <seealso cref="CountryOrRegion"/>. Includes all <seealso cref="ProvinceOrState"/>. US only
        /// </summary>
        /// <returns></returns>
        public async Task<List<Covid19US>> GetTimeSeriesCovid19USAsync()
        {
            try
            {
                var allResult = await Covid19US.ParseConfirmedAsync(Settings.TimeSeriesConfirmedUSFile).ConfigureAwait(false);
                return allResult.Where(x => x.CountryOrRegion == CountryOrRegion).ToList();
            }
            catch (AggregateException)
            {
                throw;
            }
        }

        public override string ToString()
        {
            return CountryOrRegion;
        }
    }
}
