using System;
using System.Collections.Generic;
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
        private static List<Covid19Global> confirmedGlobal;
        private static List<Covid19Global> deathsGlobal;
        private static List<Covid19Global> recoveredGlobal;

        private static List<Covid19US> confirmedUS;
        private static List<Covid19US> deathsUS;

        private static List<UidIsoFips> fipsList;
        private static List<Covid19GlobalMerged> confirmedGlobalMerged;

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

            /*
             * Official global data
             */
            confirmedGlobal = Covid19Global.ParseAsync(Settings.TimeSeriesConfirmedGlobalFile).Result;
            //Canada / Löschen nach Test
            var confirmedCanada = confirmedGlobal.Where(x => x.CountryOrRegion == "Canada");
            deathsGlobal = Covid19Global.ParseAsync(Settings.TimeSeriesDeathsGlobalFile).Result;
            recoveredGlobal = Covid19Global.ParseAsync(Settings.TimeSeriesRecoveredGlobalFile).Result;

            /*
             * Official us data
             */
            confirmedUS = Covid19US.ParseConfirmedAsync(Settings.TimeSeriesConfirmedUSFile).Result;
            deathsUS = Covid19US.ParseDeathAsync(Settings.TimeSeriesDeathsUSFile).Result;
            //var recoveredUS = Covid19Global.ParseAsync(Settings.TimeSeriesRecoveredGlobalFile).Result;

            //Parsing the UidIsoFips data to collect the complete population of all countrys
            //All the regions and countries. This will be more as 3900 results.
            //Later we count all regions population of a country. This will include "Unknow" regions, but they are often 0.
            fipsList = UidIsoFips.ParseAsync(Settings.UidIsoFipsFile).Result;

            //Merging the Global Data and the UIDFipsData
            confirmedGlobalMerged = Covid19GlobalMerged.CreateCovid19GlobalMergedList(confirmedGlobal, fipsList);

            /******************************************
             * Here is the part for the parser samples
             * Uncomment the lines for an output
             ******************************************/

            Console.Title = "Covid-19 global data - Source John Hopkins University";

            ParserShowGlobalData();
            ParserShowGlobalPercentageOfPopulation();
            //ParserShowUSData();
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
                var numberOfCases = confirmedGlobal.Where(y => !y.IsHeader).Sum(x => x.DateValues.Find(z => z.Date == date.Date).NumbersLast24Hours);
                if (numberOfCases >= mostCasesAtDay.NumbersLast24Hours)
                {
                    mostCasesAtDay = new DateValue { Date = date.Date, NumbersLast24Hours = numberOfCases };
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
            Console.WriteLine($"\nThe day with the most new cases worldwide: {mostCasesAtDay?.Date ?? default} with {mostCasesAtDay?.NumbersLast24Hours ?? default:N0} cases");

            Console.WriteLine($"\nTop 20 countries with the most cases in the past 24 hours: {lastDateInData}:");
            var c = 0;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Nr.|{"Country",-25}| {"Last 24 hours",13} | {"Total",13} | {"Total in percent",11}");
            Console.WriteLine($"   |{"",-25}| {"",13} | {"",13} | {"of country population",11}");
            //Console.WriteLine($"______________________________________________________________");
            Console.WriteLine("--------------------------------------------------------------------------");
            foreach (var top in top20Today)
            {
                c++;
                //Now we will calculate the population of each country
                var population = fipsList.FindAll(x => x.CountryOrRegion == top.CountryOrRegion && x.Code3 == x.UID).Sum(y => y.Population != "" ? int.Parse(y.Population) : 0);
                var top20populationPercentage = $"{top.DateValues.Last().NumbersComplete * 100d / population:N3}";
                Console.WriteLine($"{c:00}.|{top.CountryOrRegion,-25}| {top.Last24Hours,13:N0} | {top.DateValues.Last().NumbersComplete,13:N0} | {top20populationPercentage,11:N0}%");
            }
            Console.ResetColor();
            Console.WriteLine("==========================================================================");
            var sumTop20Today = $"{ top20Today.Sum(x => x.Last24Hours):N0}";
            Console.WriteLine($"{"Summary",-29}| {sumTop20Today,13} | {top20Today.Sum(x => x.DateValues.Last().NumbersComplete),13:N0}\n\n");
            Console.ResetColor();
        }

        private static void ParserShowGlobalPercentageOfPopulation()
        {
            //Hier berechnen wir wieviele Prozent der Bevölkerung aller Länder schon infiziert waren. Eine Mehrfachinfektion einer Person kann aber nicht ausgeschlossen werden.
            var sortedGlobalMergeList = confirmedGlobalMerged.Where(y => !double.IsInfinity(y.PercentageOfPopulation) && y.Population > 0).OrderByDescending(x => x.PercentageOfPopulation).ToList();
            var sortedGlobalMergeListTop20 = sortedGlobalMergeList.Take(20);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Top 20 countries with the highest infection rate including Germany\n\n");
            Console.WriteLine("ATTENTION: This list is not working properly! In progress...\n");
            Console.ResetColor();
            Console.WriteLine($"Date of the data:{sortedGlobalMergeList.Find(x => !x.IsHeader)?.DateValues.Last().Date}\n\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Nr. |{"Country",-25}| {"Population",13} | {"Total",13} | {"Total in percent",11}");
            Console.WriteLine($"    |{"",-25}| {"",13} | {"infected",13} | {"of country population",11}");
            //Console.WriteLine($"______________________________________________________________");
            Console.WriteLine("--------------------------------------------------------------------------");
            int c = 0;
            int sumPopulation = 0;
            int sumInfected = 0;
            foreach (var country in sortedGlobalMergeListTop20)
            {
                c++;
                sumPopulation += country.Population;
                sumInfected += country.DateValues.Last().NumbersComplete;
                Console.WriteLine($"{c:00}. |{country.CountryOrRegion,-25}| {country.Population,13:N0} | {country.DateValues.Last().NumbersComplete,13:N0} | {country.PercentageOfPopulation,11:N2}%");
            }

            Console.WriteLine($"..  |{"",-25}| {"",13:N0} | {"",13:N0} | {"",11:N2}");
            c = 0;
            var lastIndex = sortedGlobalMergeList.Count - 1;
            foreach (var country in sortedGlobalMergeList)
            {
                c++;
                if (country.CountryOrRegion == "Germany")
                {
                    sumPopulation += country.Population;
                    sumInfected += country.DateValues.Last().NumbersComplete;
                    Console.WriteLine($"{c:00}. |{country.CountryOrRegion,-25}| {country.Population,13:N0} | {country.DateValues.Last().NumbersComplete,13:N0} | {country.PercentageOfPopulation,11:N2}%");
                }

                if (c == lastIndex)
                {
                    sumPopulation += country.Population;
                    sumInfected += country.DateValues.Last().NumbersComplete;
                    Console.WriteLine($"..  |{"",-25}| {"",13:N0} | {"",13:N0} | {"",11:N2}");
                    Console.WriteLine($"{c:00}.|{country.CountryOrRegion,-25}| {country.Population,13:N0} | {country.DateValues.Last().NumbersComplete,13:N0} | {country.PercentageOfPopulation,11:N2}%");
                }
            }
            Console.ResetColor();
            Console.WriteLine("==========================================================================");
            Console.WriteLine($"{"Summary",-30}| {sumPopulation,13:N0} | {sumInfected,13:N0}\n\n");
        }

        private static void ParserShowUSData()
        {
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
                var numberOfCases = confirmedUS.Where(y => !y.IsHeader).Sum(x => x.DateValues.Find(z => z.Date == date.Date).NumbersLast24Hours);
                if (numberOfCases >= mostCasesAtDay.NumbersLast24Hours)
                {
                    mostCasesAtDay = new DateValue { Date = date.Date, NumbersLast24Hours = numberOfCases };
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
            Console.WriteLine($"\nThe day with the most new cases in US: {mostCasesAtDay?.Date ?? default} with {mostCasesAtDay?.NumbersLast24Hours ?? default:N0} cases");

            Console.WriteLine($"\nTop 20 provinces or states with the most cases in the past 24 hours: {lastDateInData}:\n");
            var c = 0;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    | {"",-35}| {"",10} | {"Percentage of",12} | {"",12}");
            Console.WriteLine($"Nr. | {"Country",-34} | {"Cases",10} | {"Population",13} | {"Population",12}");
            Console.WriteLine($"-------------------------------------------------------------------------------------");
            foreach (var top in top20Today)
            {
                c++;
                var population = int.Parse(deathsUS.Find(x => x.UID == top.UID).Population);
                var percentageOfPopulation = $"{top.Last24Hours * 100d / population:N3}";
                Console.WriteLine($"{c:00}. |{top.CombinedKey,-35} | {top.Last24Hours,10:N0} | {percentageOfPopulation,12}% | {population,12:N0}");
            }
            Console.ResetColor();
            Console.WriteLine("=====================================================================================");
            Console.WriteLine($"{"Summary",-41}: {top20Today.Sum(x => x.Last24Hours),10:N0}\n\n");
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
            var fipsListGroups = fipsListGermany.OrderBy(y => y.Code3).GroupBy(x => x.Code3).ToList();

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
