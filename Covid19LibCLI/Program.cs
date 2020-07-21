using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Covid19Lib;
using Covid19Lib.Parser;
using Covid19Lib.Parser.TimeSeries;

namespace Covid19LibCLI
{
    class Program
    {
        private static void Main(string[] args)
        {
            //The Project to parse and merge the data is using the settings.cs file in the base path of project Covid19Lib
            //Check if a file exist
            if (!File.Exists(Settings.TimeSeriesConfirmedGlobalFile))
            {
                Console.WriteLine($"File not found: {Settings.TimeSeriesConfirmedGlobalFile}");
                Console.WriteLine("Define the paths in settings.cs in project Covid19Lib");
                return;
            }

            /******************************************
             * Here is the part for the parser samples
             * Uncomment the lines for an output
             ******************************************/

            Console.Title = "Covid-19 global data - Source John Hopkins University";

            ParserShowGlobalData();
            ParserShowUSData();
            //ParserShowUidFipsData();

            /******************************************
             * Here is the part for the merged data
             * Uncomment the lines for an output
             ******************************************/

            //Coming soon

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        private static void ParserShowGlobalData()
        {
            /*
             * Official global data
             */
            var confirmedGlobal = Covid19Global.ParseAsync(Settings.TimeSeriesConfirmedGlobalFile).Result;
            var deathsGlobal = Covid19Global.ParseAsync(Settings.TimeSeriesDeathsGlobalFile).Result;
            var recoveredGlobal = Covid19Global.ParseAsync(Settings.TimeSeriesRecoveredGlobalFile).Result;

            //Global data of confirmed Covid-19 cases in Germany; this line is for demonstration only, without an output
            var germanOnly = confirmedGlobal.Where(x => x.CountryOrRegion == "Germany").ToList();

            //Summary all the most current date in "DateValues". So we will get the summary of all global infected people.
            //With the x.DateValues.Last() function, we will get the most current date.
            var confirmedCases = confirmedGlobal.Where(y => !y.IsHeader).Sum(x => x.DateValues.Last().NumbersComplete);
            var deathCases = deathsGlobal.Where(y => !y.IsHeader).Sum(x => x.DateValues.Last().NumbersComplete);
            var recoveredPeople = recoveredGlobal.Where(y => !y.IsHeader).Sum(x => x.DateValues.Last().NumbersComplete);
            var activeCases = confirmedCases - deathCases - recoveredPeople;

            //This line is good for a discussion; How do you correctly calculate this ratio in an ongoing pandemic?
            //Calculated by deathCases and complete confirmedCases
            var infectedDeathRatio = deathCases * 100d / confirmedCases;
            //The death ratio may need to be calculated based on the 14 days (duration of illness) previously confirmed.
            //The next 3 lines is how to get the confirmed cases 14 days previously
            var lastDateInData = confirmedGlobal.Find(x => !x.IsHeader)?.DateValues.Last().Date;
            var newDate14Prev = lastDateInData - TimeSpan.FromDays(14);
            var confirmedCases14DaysPreviously = confirmedGlobal.Where(y => !y.IsHeader).Sum(x => x.DateValues.Find(z => z.Date == newDate14Prev).NumbersComplete);

            //Top twenty countries @lastDateInData; also only the last day (24h) not the summary of all cases.
            var top20Today = confirmedGlobal.Where(y => !y.IsHeader).OrderByDescending(z => z.Last24Hours).Take(20);

            //This date has the most cases worldwide in the timeline
            //Get all the dates
            var dates = confirmedGlobal.Find(y => !y.IsHeader)?.DateValues;
            DateValue mostCasesAtDay = new DateValue();
            foreach (var date in dates)
            {
                //cases on date
                var numberOfCases = confirmedGlobal.Where(y => !y.IsHeader).Sum(x => x.DateValues.Find(z => z.Date == date.Date).Numbers);
                if (numberOfCases >= mostCasesAtDay.Numbers)
                {
                    mostCasesAtDay = new DateValue { Date = date.Date, Numbers = numberOfCases };
                }
            }

            Console.WriteLine($"Date of the data: {lastDateInData}\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Worldwide infected people: {confirmedCases:N0}");
            Console.WriteLine($"Worldwide last 24 hours: {confirmedGlobal.Sum(x => x.Last24Hours):N0}");
            Console.ResetColor();
            Console.WriteLine($"Worldwide death cases: {deathCases:N0}");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Worldwide recovered people: {recoveredPeople:N0}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Worldwide active cases: {activeCases:N0}");
            Console.ResetColor();
            Console.WriteLine($"Worldwide infection death ratio: {infectedDeathRatio:N3}%");
            Console.WriteLine($"\nThe day with the most new cases worldwide: {mostCasesAtDay?.Date ?? default} with {mostCasesAtDay?.Numbers ?? default:N0} cases");

            Console.WriteLine($"\nTop 20 countries with the most cases in the past 24 hours: {lastDateInData}:");
            var c = 0;
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var top in top20Today)
            {
                c++;
                Console.WriteLine($"{c:00}. {top.CountryOrRegion,-25}: {top.Last24Hours,10:N0}");
            }
            Console.ResetColor();
            Console.WriteLine("=========================================");
            var sumTop20Today = $"{ top20Today.Sum(x => x.Last24Hours):N0}";
            Console.WriteLine($"{"Summary",-29}: {sumTop20Today,10}\n\n");
        }

        private static void ParserShowUSData()
        {
            /*
             * Official global data
             */
            var confirmedUS = Covid19US.ParseConfirmedAsync(Settings.TimeSeriesConfirmedUSFile).Result;
            var deathsUS = Covid19US.ParseDeathAsync(Settings.TimeSeriesDeathsUSFile).Result;
            //var recoveredUS = Covid19Global.ParseAsync(Settings.TimeSeriesRecoveredGlobalFile).Result;

            //Summary all the most current date in "DateValues". So we will get the summary of all global infected people.
            //With the x.DateValues.Last() function, we will get the most current date.
            var confirmedCases = confirmedUS.Where(y => !y.IsHeader).Sum(x => x.DateValues.Last().NumbersComplete);
            var deathCases = deathsUS.Where(y => !y.IsHeader).Sum(x => x.DateValues.Last().NumbersComplete);
            //var recoveredPeople = recoveredUS.Where(y => !y.IsHeader).Sum(x => x.DateValues.Last().NumbersComplete);
            //var activeCases = confirmedCases - deathCases - recoveredPeople;

            //This line is good for a discussion; How do you correctly calculate this ratio in an ongoing pandemic?
            //Calculated by deathCases and complete confirmedCases
            var infectedDeathRatio = deathCases * 100d / confirmedCases;
            //The death ratio may need to be calculated based on the 14 days (duration of illness) previously confirmed.
            //The next 3 lines is how to get the confirmed cases 14 days previously
            var lastDateInData = confirmedUS.Find(x => !x.IsHeader)?.DateValues.Last().Date;
            var newDate14Prev = lastDateInData - TimeSpan.FromDays(14);
            var confirmedCases14DaysPreviously = confirmedUS.Where(y => !y.IsHeader).Sum(x => x.DateValues.Find(z => z.Date == newDate14Prev).NumbersComplete);

            //Top twenty countries @lastDateInData; also only the last day (24h) not the summary of all cases.
            var top20Today = confirmedUS.Where(y => !y.IsHeader).Where(x => int.Parse(x.UID) < 84060000).OrderByDescending(z => z.Last24Hours).Take(20);

            //This date has the most cases worldwide in the timeline
            //Get all the dates
            var dates = confirmedUS.Find(y => !y.IsHeader)?.DateValues;
            DateValue mostCasesAtDay = new DateValue();
            foreach (var date in dates)
            {
                //cases on date
                var numberOfCases = confirmedUS.Where(y => !y.IsHeader).Sum(x => x.DateValues.Find(z => z.Date == date.Date).Numbers);
                if (numberOfCases >= mostCasesAtDay.Numbers)
                {
                    mostCasesAtDay = new DateValue { Date = date.Date, Numbers = numberOfCases };
                }
            }

            Console.WriteLine($"Date of the data: {lastDateInData}\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"US infected people: {confirmedCases:N0}");
            Console.WriteLine($"US last 24 hours: {confirmedUS.Sum(x => x.Last24Hours):N0}");
            Console.ResetColor();
            Console.WriteLine($"US death cases: {deathCases:N0}");
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine($"US recovered people: {recoveredPeople:N0}");
            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.WriteLine($"US active cases: {activeCases:N0}");
            Console.ResetColor();
            Console.WriteLine($"US infection death ratio: {infectedDeathRatio:N3}%");
            Console.WriteLine($"\nThe day with the most new cases in US: {mostCasesAtDay?.Date ?? default} with {mostCasesAtDay?.Numbers ?? default:N0} cases");

            Console.WriteLine($"\nTop 20 provinces or states with the most cases in the past 24 hours: {lastDateInData}:");
            var c = 0;
            Console.ForegroundColor = ConsoleColor.Yellow;
            foreach (var top in top20Today)
            {
                c++;
                var population = deathsUS.Find(x => x.UID == top.UID).Population;
                var percentageOfPopulation = $"{top.Last24Hours * 100d / int.Parse(population):N3}";
                Console.WriteLine($"{c:00}. {top.CombinedKey,-35}: {top.Last24Hours,10:N0}; {percentageOfPopulation}% of population ({population,12:N0})");
            }
            Console.ResetColor();
            Console.WriteLine("===========================================================================");
            Console.WriteLine($"{"Summary",-39}: {top20Today.Sum(x => x.Last24Hours),10:N0}\n\n");
        }

        private static void ParserShowUidFipsData()
        {
            //All the regions and countries. This will be more as 3900 results.
            var fipsList = UidIsoFips.ParseAsync(Settings.UidIsoFipsFile).Result;

            //The 16 federal states in Germany with the new federal state "Unknown". Also in summary 17. :)
            //Remark: In this condition, the Code3 is != UID!
            var fipsListGermany = fipsList.FindAll(x => x.CountryOrRegion == "Germany" && x.Code3 != x.UID).ToList();

            //Group the fipsList to the code3 value. We accept the more as 3200 subgroups in the US (Code3 840).
            //We get more as 220 regions. Maybe Code3 can't be a filter for a single country or region.
            //May you have to adjust more as one filter.
            var fipsListGroups = fipsList.OrderBy(y => y.Code3).GroupBy(x => x.Code3).ToList();

            //A simple loop through this groups
            //Topgroups
            foreach (var topGroups in fipsListGroups)
            {
                var subGroups = topGroups.ToList();
                Console.WriteLine($"======== Code3: {topGroups.Key} ========");
                //Subgroups
                foreach (var group in subGroups)
                {
                    Console.WriteLine($"UID: {group.UID}, Country: {group.CountryOrRegion}, Province or State: {group.ProvinceOrState}");
                    Console.WriteLine($"Population: {group.Population}");
                }
            }
        }
    }
}
