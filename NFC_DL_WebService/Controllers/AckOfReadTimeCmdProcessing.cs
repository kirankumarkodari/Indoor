using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace NFC_DL_WebService.Controllers
{
    public class AckOfReadTimeCmdProcessing : ApiController
    {
        public static Boolean processReadTimeAckPack(byte[] ackPacket)
        {
            string DLPrevReqRecTime;
            string DLErrorTime;

            byte[] Dlid = new byte[2];
            int DLidValue;
            string strDLidValue;

            byte[] crc = new byte[2];
            byte[] packTime = new byte[4];
            long packTimeValue;
            byte[] packYear = new byte[2];
            int packYearValue;
            byte[] crc_buffer = new byte[15];

            for (int i = 0; i < 15; i++)
            {
                crc_buffer[i] = ackPacket[i];
            }

            //calculation crc for Block0 and Block1
            ushort crc_buffer_value = CRC_Calculation.update(crc_buffer);

            crc[0] = ackPacket[15];
            crc[1] = ackPacket[16];

            if (BitConverter.IsLittleEndian)
                Array.Reverse(crc);
            ushort crc_value = BitConverter.ToUInt16(crc, 0);

            //crc is valid
            if (crc_buffer_value == crc_value)
            {
                Dlid[0] = ackPacket[3];
                Dlid[1] = ackPacket[4];

                //converting idnumber from bytes to int, to get dlid
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(Dlid);
                DLidValue = BitConverter.ToInt16(Dlid, 0);
                strDLidValue = DLidValue.ToString();

                DLPrevReqRecTime = DLidValue.ToString() + "RecTime";
                DLErrorTime = DLidValue.ToString() + "Error";

                //setting current req received time as previous request received time
                HttpContext.Current.Application[DLPrevReqRecTime] = HttpContext.Current.Application["ReqRecTime"];

                //reading packet time
                packTime[0] = ackPacket[9];
                packTime[1] = ackPacket[10];
                packTime[2] = ackPacket[11];
                packTime[3] = ackPacket[12];

                //converting time from bytes to int i.e ltime
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(packTime);
                packTimeValue = BitConverter.ToInt32(packTime, 0);

                /*//reading packet year
                packYear[0] = ackPacket[13];
                packYear[1] = ackPacket[14];

                //converting year from bytes to int
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(packYear);
                packYearValue = BitConverter.ToUInt16(packYear, 0);*/

                //convert current time to ltime without year
                long receiveLtime = DateTimeConversions.DateToLtime(Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]));
                if (receiveLtime < packTimeValue)
                    packYearValue = DateTime.Now.Year - 1;
                else
                    packYearValue = DateTime.Now.Year;

                //converting packet time and year to datetime
                DateTime packDateTime = DateTimeConversions.packTimeYearToDateTime(packTimeValue, packYearValue);
                //converting datetime to ltime (long)
                long packTimeYearLong = DateTimeConversions.DateToLtime(packDateTime);

                //HttpContext.Current.Application[DLErrorTime] = (Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]) - packDateTime).TotalMinutes;
                HttpContext.Current.Application[DLErrorTime] = receiveLtime - packTimeValue;

                return true;
            }
            else
            {
                //crc is invalid
                return false;
            }
        }
    }
}
