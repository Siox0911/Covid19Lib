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
    public class Covid19GlobalMerged : Covid19Global
    {
        /// <summary>
        /// The population of a Country or Region
        /// </summary>
        public int Population { get; set; }

        /// <summary>
        /// The infected population in percent.
        /// </summary>
        public double PercentageOfPopulation
        {
            get
            {
                if(DateValues?.Count == 0)
                {
                    return 0;
                }
                return ((DateValues?.Last()?.NumbersComplete) ?? 0) * 100d / Population;
            }
        }

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


            //New Strategy: Nimm erst die UIDFipsListe und nur die Daten, wo die UID == CODE3 ist.
            //Damit erhälst du eine reine Länderliste. Durch diese Liste iterierst du und holst die Gruppen
            //aus der Covid19 Liste. Dann nimmst du die Summe der Infizierten
            //So musst du aber die Basisklasse wegwerfen und die Anzahl der Infizierten hier reinbringen
            //Oder alles so ummodeln, dass die Basisklasse auf Länderebene funktioniert
            //Bedeutet, dass Province or State = "" sein muss. :)

            ////Neue Strategie
            var countryUIDFipsListGrouped = uidIsoFipsList.Where(x => x.Code3 == x.UID).OrderBy(y => y.CountryOrRegion).GroupBy(z => z.CountryOrRegion).ToList();

            //Bisher herausgefunden: Es gibt pro Land noch den CombinedKey, der Province und Country zusammenführt.
            //Hier kommt z.B: China 3x vor. DE nur 1x und Canada auch nur 1x
            //DE kommt meist nur 1x, aber China ist so ein Ding und Canada auch.

            foreach (var countryUIDFipsSubgroup in countryUIDFipsListGrouped)
            {
                foreach (var countryUIDFipsList in countryUIDFipsSubgroup)
                {

                    //Jetzt haben wir hier wieder die Länder
                    //die eigentlich schonmal Einzigartig waren....
                    //Ich könnte kotzen (25.05.2021)
                    var rowResult = new Covid19GlobalMerged();
                    rowResult.CountryOrRegion = countryUIDFipsList.CountryOrRegion;
                    rowResult.IsHeader = countryUIDFipsList.IsHeader;
                    if(!rowResult.IsHeader)
                    {
                        //if (countryOrRegion == "Canada" || countryOrRegion == "Germany" || countryOrRegion == "China")
                        //{
                        //    var countryFound = countryUIDFipsList.Covid19Globals;
                        //    var countryInfected = countryFound.Sum(x => x.DateValues.Last().NumbersComplete);
                        //    Console.WriteLine($"Found: {countryUIDFipsList.CountryOrRegion} ProvinceOrState: {countryUIDFipsList.ProvinceOrState} Infected: {countryInfected} Population: {countryUIDFipsList.Population}");
                        //}
                        var newDateValue = new List<DateValue>();
                        countryUIDFipsList.Covid19Globals.ForEach(x => newDateValue = x.DateValues);
                        rowResult.DateValues = newDateValue;

                        rowResult.Population = int.Parse(countryUIDFipsList.Population);
                    }
                    result.Add(rowResult);
                }
            }


            //Hier noch die alte Strategie

            //foreach (var covid19Global in covid19GlobalList)
            //{
            //    //dynamic convertion of the properties from the baseclass (Covid19Global) to this class (Covid19GlobalMerged)
            //    var rowResult = new Covid19GlobalMerged();

            //    foreach (var prop in covid19Global.GetType().GetProperties())
            //    {
            //        var propInfo = rowResult.GetType().GetProperty(prop.Name);
            //        if (propInfo.CanWrite)
            //        {
            //            var propValue = prop.GetValue(covid19Global, null);
            //            propInfo.SetValue(rowResult, propValue, null);
            //        }
            //    }
            //    var population = countryUIDFipsListGrouped.FindAll(x => x.CountryOrRegion == rowResult.CountryOrRegion && x.Code3 == x.UID).Sum(y => y.Population != "" ? int.Parse(y.Population) : 0);

            //    rowResult.Population = population;

            //    result.Add(rowResult);
            //}

            return result;
        }
    }
}