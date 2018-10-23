using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Globalization;

namespace NFC_DL_WebService.Controllers
{
    public class DateTimeConversions : ApiController
    {
        /*
         * after receiving request, 
         * --> need to convert string to DateTime the result is in GMT 
         * --> and then GMT to IST (to get Indian time)4
         * --> and then IST to ltime (for database comparisions)
         * --> ltime to IST (if necessary)         
         * --> IST to GMT to send in response
         * --> or we can directly have gmt from ltime
         */
        public static DateTime strToDateTime(string strgmtTime)
        {
            return DateTime.Now;
        }
        public static DateTime strDateTimetoIST(string strgmtTime)
        {
            DateTime dt = Convert.ToDateTime(strgmtTime);
            return dt;
        }
        public static long ISTtoLTime(DateTime istTime)
        {
            int recDays, recHours, recMinutes, recSec, recMilliSec;
            try
            {
                DateTime dtStart = new DateTime(istTime.Year, 1, 1, 0, 0, 0);
                DateTime dtReceived = new DateTime(istTime.Year, istTime.Month, istTime.Day, istTime.Hour, istTime.Minute, istTime.Second, istTime.Millisecond);
                recDays = Convert.ToInt16(Math.Floor((dtReceived.Date - dtStart).TotalDays));
                if (recDays < 0)
                    recDays = 0;
                recHours = istTime.Hour;
                recMinutes = istTime.Minute;
                recSec = istTime.Second;
                recMilliSec = istTime.Millisecond;
                long tempResult = (recDays * 24 * 60 * 60 * 64) + (recHours * 60 * 60 * 64) + (recMinutes * 60 * 64) + (recSec * 64) + (int)Math.Floor((recMilliSec / 15.625));
                return tempResult;
            }
            catch (Exception e)
            {
                ;
                return 0;
            }            
        }
        public static DateTime LtimetoIST(int lTime, int year)
        {
            int days, hour, min, sec, millisec;
            float Remainder1, Remainder2, Remainder3, Remainder4;
            try
            {
                //Getting days, hours, minutesm seconds from lTime
                days =  lTime / (64 * 60 * 60 * 24);
                Remainder1 = lTime % (64 * 60 * 60 * 24);
                hour = (int)Math.Floor(Remainder1 / (64 * 60 * 60));
                Remainder2 = Remainder1 % (64 * 60 * 60);
                min = (int)Math.Floor(Remainder2 / (64 * 60));
                Remainder3 = Remainder2 % (64 * 60);
                sec = (int)Math.Floor(Remainder3 / (64));
                Remainder4 = Remainder3 % (64);
                millisec = (int)Math.Floor(Remainder4 * 15.625);


                DateTime theDate = new DateTime(year, 1, 1).AddDays(days - 0);
                string dateOfYear = theDate.ToString("dd");   // The date in required format                
                string month = theDate.ToString("MM");

                //(year, month, date, hours, minutes, sec, millisec)
                DateTime dt = new DateTime(year, Convert.ToInt16(month), Convert.ToInt16(dateOfYear), hour, min, sec, millisec);
                //string strDateTime = String.Format("{0:MM/d/yyyy HH:mm:ss}", dt);                
                CultureInfo enUS = new CultureInfo("en-US");
                //string strDateTime = dt.ToString("MM/dd/yyyy hh:mm:ss tt", enUS); //for 12 hours format
                string strDateTime = dt.ToString("MM/d/yyyy HH:mm:ss", enUS);

                return dt;
            }
            catch (Exception e)
            {
                ;
                return DateTime.Now;
            }
        }
        public static string ISTtoGMT(DateTime ist)
        {
            DateTime gmt = ist.Subtract(new TimeSpan(5, 30, 0));
            string strgmt = gmt.ToString("yyyy-MM-dTHH:mm:ss.fffZ");            
            return strgmt;
        }

        //To get List of dates between Two Dates
        public static List<DateTime> GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            List<DateTime> allDates = new List<DateTime>();
            for (DateTime date = endDate.Date; date >= startDate.Date; date = date.AddDays(-1))
                allDates.Add(date);
            return allDates;
        }

        //converting datetime to long
        public static long DateToLtime(DateTime inputDateTime)
        {
            int recDays, recHours, recMinutes, recSec, recMilliSec;
            try
            {
                DateTime dtStart = new DateTime(inputDateTime.Year, 1, 1, 0, 0, 0);
                DateTime dtReceived = new DateTime(inputDateTime.Year, inputDateTime.Month, inputDateTime.Day, inputDateTime.Hour, inputDateTime.Minute, inputDateTime.Second, inputDateTime.Millisecond);
                //without -1, it is showing tomorrows date
                recDays = Convert.ToInt16(Math.Floor((dtReceived - dtStart).TotalDays));
                if (recDays < 0)
                    recDays = 0;
                recHours = inputDateTime.Hour;
                recMinutes = inputDateTime.Minute;
                recSec = inputDateTime.Second;
                recMilliSec = inputDateTime.Millisecond;

                long tempResult = (recDays * 24 * 60 * 60 * 64) + (recHours * 60 * 60 * 64) + (recMinutes * 60 * 64) + (recSec * 64) + (int)Math.Floor(recMilliSec / 15.625);
                return tempResult;
            }
            catch (Exception e)
            {
                ;
                return 0;
            }
        }

        //converstion of lont time to date string
        public static DateTime packTimeYearToDateTimeString(long pktTime, int packetYearValue)
        {
            int days, hour, min, sec, millisec;
            float Remainder1, Remainder2, Remainder3, Remainder4;
            try
            {
                //Getting days, hours, minutesm seconds from LTime
                days = (int)pktTime / (64 * 60 * 60 * 24);
                Remainder1 = pktTime % (64 * 60 * 60 * 24);
                hour = (int)Math.Floor(Remainder1 / (64 * 60 * 60));
                Remainder2 = Remainder1 % (64 * 60 * 60);
                min = (int)Math.Floor(Remainder2 / (64 * 60));
                Remainder3 = Remainder2 % (64 * 60);
                sec = (int)Math.Floor(Remainder3 / (64));
                Remainder4 = Remainder3 % (64);
                millisec = (int)Math.Floor(Remainder4 * 15.265);

                DateTime theDate = new DateTime(packetYearValue, 1, 1).AddDays(days - 0);
                string dateOfYear = theDate.ToString("dd");   // The date in required format                
                string month = theDate.ToString("MM");

                //(year, month, date, hours, minutes, sec, millisec)
                DateTime dt = new DateTime(packetYearValue, Convert.ToInt16(month), Convert.ToInt16(dateOfYear), hour, min, sec, millisec);
                //string strDateTime = String.Format("{0:MM/d/yyyy HH:mm:ss}", dt); 

                //Commented by D. Venkata Naresh (Return type is changed from String to DateTime)
                //CultureInfo enUS = new CultureInfo("en-US");
                //string strDateTime = dt.ToString("MM/d/yyyy HH:mm:ss", enUS);

                //return strDateTime;
                return dt;
            }
            catch (Exception e)
            {
                return new DateTime(0, 0, 0, 0, 0, 0, 0);
            }
        }

        //converstion of lont time to date string
        public static DateTime packTimeYearToDateTime(long pktTime, int packetYearValue)
        {
            int days, hour, min, sec, millisec;
            float Remainder1, Remainder2, Remainder3, Remainder4;
            try
            {
                //Getting days, hours, minutesm seconds from LTime
                days = (int)pktTime / (64 * 60 * 60 * 24);
                Remainder1 = pktTime % (64 * 60 * 60 * 24);
                hour = (int)Math.Floor(Remainder1 / (64 * 60 * 60));
                Remainder2 = Remainder1 % (64 * 60 * 60);
                min = (int)Math.Floor(Remainder2 / (64 * 60));
                Remainder3 = Remainder2 % (64 * 60);
                sec = (int)Math.Floor(Remainder3 / (64));
                Remainder4 = Remainder3 % (64);
                millisec = (int)Math.Floor(Remainder4 * 15.265);

                DateTime theDate = new DateTime(packetYearValue, 1, 1).AddDays(days - 0);
                string dateOfYear = theDate.ToString("dd");   // The date in required format                
                string month = theDate.ToString("MM");

                //(year, month, date, hours, minutes, sec, millisec)
                DateTime dt = new DateTime(packetYearValue, Convert.ToInt16(month), Convert.ToInt16(dateOfYear), hour, min, sec, millisec);

                return dt;
            }
            catch (Exception e)
            {
                return new DateTime(0, 0, 0, 0, 0, 0, 0);
            }

        }
    }
}
