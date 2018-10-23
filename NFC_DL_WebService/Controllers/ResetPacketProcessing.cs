using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace NFC_DL_WebService.Controllers
{
    public class ResetPacketProcessing : ApiController
    {
        public static Boolean processResetPack(byte[] resetPack)
        {
            string DLPrevReqRecTime;
            string DLErrorTime;

            byte[] Dlid = new byte[2];
            int DLidValue;
            string strDLidValue;
            byte[] crc = new byte[2];
            byte[] crcBuffer = new byte[18];
            byte[] resetTime = new byte[4];
            long resetTimeValue;            
            byte resetYear;
            int resetYearValue;

            try
            {
                crc[0] = resetPack[18];
                crc[1] = resetPack[19];

                for (int i = 0; i < 18; i++)
                {
                    crcBuffer[i] = resetPack[i];
                }

                ushort crc_buffer_value = CRC_Calculation.update(crcBuffer);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(crc);
                ushort crc_value = BitConverter.ToUInt16(crc, 0);

                //crc is valid
                if (crc_buffer_value == crc_value)
                {
                    Dlid[0] = resetPack[0];
                    Dlid[1] = resetPack[1];

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
                    resetTime[0] = resetPack[4];
                    resetTime[1] = resetPack[5];
                    resetTime[2] = resetPack[6];
                    resetTime[3] = resetPack[7];
                    //converting packet time from bytes to long
                    if (BitConverter.IsLittleEndian)
                        Array.Reverse(resetTime);
                    resetTimeValue = BitConverter.ToInt32(resetTime, 0);

                    //converting resettimevalue to ltime

                    //convert current time to ltime without year
                    long receiveLtime = DateTimeConversions.DateToLtime(Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]));
                    if (receiveLtime < resetTimeValue)
                        resetYearValue = DateTime.Now.Year - 1;
                    else
                        resetYearValue = DateTime.Now.Year;

                    /*//reading packet year byte
                    resetYear = resetPack[8];
                    //converting packet year from byte to int
                    resetYearValue = 2000 + Convert.ToInt16(resetYear);*/

                    DateTime packDateTime = DateTimeConversions.packTimeYearToDateTime(resetTimeValue, resetYearValue);
                    long packLtimeLong = DateTimeConversions.DateToLtime(packDateTime);

                    //HttpContext.Current.Application[DLErrorTime] = (Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]) - packDateTime).TotalMinutes;
                    HttpContext.Current.Application[DLErrorTime] = receiveLtime - resetTimeValue;

                    return true;
                }
                else
                {
                    //crc is invalid
                    return false;
                }                
            }
            catch
            {
                return false;
            }
        }
    }
}
