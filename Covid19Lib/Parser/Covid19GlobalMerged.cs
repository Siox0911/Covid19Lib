using Covid19Lib.Parser.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Covid19Lib.Parser
{
    /// <summary>
    /// This class is merging the GlobalData with the UidIsoFips data.
    /// </summary>
    public class Covid19GlobalMerged: Covid19Global
    {
        /// <summary>
        /// The population of a Country or Region
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// The infected population in percent.
        /// </summary>
        public double PercentageOfPopulation { get { return DateValues.Last().NumbersComplete * 100d / Population; } }

        internal Covid19GlobalMerged() { }

        /// <summary>
        /// This will create a List of all countrys. This is merging the global data with the UidfipsList. At this time, this method works wrong. So keep in patient.
        /// </summary>
        /// <param name="covid19GlobalList"></param>
        /// <param name="uidIsoFipsList"></param>
        /// <returns></returns>
        public static List<Covid19GlobalMerged> CreateCovid19GlobalMergedList(List<Covid19Global> covid19GlobalList, List<UidIsoFips> uidIsoFipsList)
        {
            if (covid19GlobalList == null)
                throw new ArgumentNullException(nameof(covid19GlobalList), "Can't be null");

            if (uidIsoFipsList == null)
                throw new ArgumentNullException(nameof(uidIsoFipsList), "Can't be null");

            var result = new List<Covid19GlobalMerged>();

            foreach(var covid19Global in covid19GlobalList)
            {
                //dynamic convertion of the properties from the baseclass (Covid19Global) to this class (Covid19GlobalMerged)
                var rowResult = new Covid19GlobalMerged();

                foreach(var prop in covid19Global.GetType().GetProperties())
                {
                    var propInfo = rowResult.GetType().GetProperty(prop.Name);
                    if(propInfo.CanWrite)
                    {
                        var propValue = prop.GetValue(covid19Global, null);
                        propInfo.SetValue(rowResult, propValue, null);
                    }
                }

                rowResult.Population = uidIsoFipsList.FindAll(x => x.CountryOrRegion == rowResult.CountryOrRegion && x.Code3 == x.UID).Sum(y => y.Population != "" ? int.Parse(y.Population) : 0);

                if(covid19Global.CountryOrRegion == "Canada")
                {
                    //Here we are, and I have to check the result.. Why the canadian infected people are 0?
                    //Thats the reason why this method works wrong!
                }

                result.Add(rowResult);
            }

            return result;
        }
    }
}
