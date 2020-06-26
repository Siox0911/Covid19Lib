using System;
using System.Collections.Generic;
using System.Text;

namespace Covid19Lib
{
    public static class Settings
    {
        //This path is relativ to the application startup path
        public static string BasePath = @"..\..\..\..\..\COVID-19\csse_covid_19_data\";

        //Or use the fullpath to your Covid-19 Repository from the Johns Hopkins University https://github.com/CSSEGISandData/COVID-19
        //public static string BasePath = @"C:\Users\your username\Source\repos\COVID-19\csse_covid_19_data\";

        //Subdirectory to the time series
        public static string TimeSeries = @"csse_covid_19_time_series\";

        //Subdirectory to the daily reports
        public static string DailyReports = @"csse_covid_19_daily_reports\";

        //Subdirectory to the daily reports US only - started 12th April 2020; 04-12-2020
        public static string DailyReportsUS = @"csse_covid_19_daily_reports_us\";

        /*
         * Time Series Global
         */
        public static string TimeSeriesConfirmedGlobalFile = $"{BasePath}{TimeSeries}time_series_covid19_confirmed_global.csv";
        public static string TimeSeriesDeathsGlobalFile = $"{BasePath}{TimeSeries}time_series_covid19_deaths_global.csv";
        public static string TimeSeriesRecoveredGlobalFile = $"{BasePath}{TimeSeries}time_series_covid19_recovered_global.csv";

        /*
         * Time Series US
         */
        public static string TimeSeriesConfirmedUS = $"{BasePath}{TimeSeries}time_series_covid19_confirmed_us.csv";
        public static string TimeSeriesDeathsUS = $"{BasePath}{TimeSeries}time_series_covid19_deaths_us.csv";
        //public static string TimeSeriesRecoveredUS = $"{basePath}{timeSeries}time_series_covid19_recovered_us.csv";

        /*
         * UID_ISO_FIPS_LookUp_Table
         */
        public static string UidIsoFipsFile = $"{BasePath}UID_ISO_FIPS_LookUp_Table.csv";
    }
}
