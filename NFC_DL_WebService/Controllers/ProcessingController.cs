using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.Http;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.SessionState;
using NFC_DL_WebService.Models;
using System.IO;

namespace NFC_DL_WebService.Controllers
{
    //[RoutePrefix("Services/SensorData")]
    public class ProcessingController : ApiController
    {
        public static void writeIntoFile(string msg)
        {
                // Write the string to a file.
                string dbPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;
                System.IO.Directory.CreateDirectory(dbPath + "ProcessingErrors");
                string FilePath = dbPath + "ProcessingErrors//ErrorsOn" + DateTime.Now.ToString("dd") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("yy") + ".txt";
                using (FileStream fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    StreamWriter file = new StreamWriter(fs);
                    file.WriteLine(DateTime.Now.ToString() + ": " + msg);
                    file.WriteLine("*************************************************************");
                    file.Close();
                }
        }

        //variables        
        private int rawDataLoopIndex = 0;
        private int index, index1;
        private string tableName;
        private int validPacketsCount = 0;
        private int numberOfPacketsReceived = 0;
        //private ushort serlNum = 0;

        //private string ackStream;        
        private byte[] tempAckReadPacket = new byte[18];
        private byte[] tempPacket = new byte[20];
        private static DateTime receivedDateTime;
        //MySqlCommand SqlCommand;
        //MySqlConnection ConnectionToEffe;
     

        [HttpPost]
        [Route("postData")]
        //[Route("api/method")]
        public string receiveSensorData()
        {
            //ConnectionToEffe = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ToString());
            var msg = Request.Content.ReadAsStringAsync();
            var res = msg.Result;
            string result = res.ToString();
            
            if (!result.Equals(""))
            {
                try
                {
                    // Write the Packet to a file.
                    string dbPath = System.Web.HttpContext.Current.Request.PhysicalApplicationPath;
                    Directory.CreateDirectory(dbPath+"Packets");
                    string FilePath = dbPath + "Packets//P" + DateTime.Now.ToString("yy") + DateTime.Now.ToString("MM") + DateTime.Now.ToString("dd") + DateTime.Now.ToString("HH") + ".txt";
                    using (FileStream fs = new FileStream(FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
                        file.WriteLine(DateTime.Now.ToString() + ": " + result);
                        file.Close();
                    }

                    /*ConnectionToEffe.Open();
                    SqlCommand = ConnectionToEffe.CreateCommand();
                    //SqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS" + "'" + tableName +"'" ;  
                    SqlCommand.CommandText = "INSERT INTO sample VALUES('" + result + "')";
                    SqlCommand.ExecuteNonQuery();
                    ConnectionToEffe.Close();*/
                    //24Nov start
                    string rawData ;
                    string temprawData = result;
                    string Header = temprawData.Substring(0,4);   /*Bug KKK*/
                    /*if (Header == "AACC")
                    {
                        rawData = temprawData.Substring(4, temprawData.Length-4);
                    }
                    else
                    {*/

                        string temprawData2, NewPkt;
                        int StartPos, EndPos;
                        rawData = "";
                        while (temprawData != null)
                        {
                            StartPos = temprawData.IndexOf("AA66");
                            temprawData2 = temprawData.Substring(StartPos+4, temprawData.Length-4);
                            //EndPos = temprawData2.IndexOf("BB");
                            //NewPkt = temprawData.Substring(StartPos + 2, EndPos+1 );
                            NewPkt = temprawData.Substring(StartPos + 4, 40);
                            rawData = rawData + NewPkt;
                            //temprawData = temprawData2.Substring(EndPos + 2, temprawData2.Length-EndPos-2);
                            if (temprawData.Length == 48)
                            {
                                temprawData = null;
                            }

                            else
                            {
                                temprawData = temprawData2.Substring(44, temprawData2.Length - 44);
                            }

                        }
                    /*}*/

                     //rawData= result;//removed on 24Nov15
                    //24Nov end

                    string tempRawData = "";
                    string tempHexDataWithSpace = "";
                    //byte[] rawData = Encoding.Default.GetBytes(result);

                    //for testing
                    //byte[] rawData = {0x01, 0xaa,0x00, 0x07, 0x00, 0x00, 0x00, 0xca, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0b, 0x76, 0x00, 0x00, 0x00, 0x4f, 0x0a};
                    //byte[] rawData = { 0x01, 0xaa, 0x00, 0x06, 0x00, 0x00, 0x00, 0xc6, 0x00, 0x01, 0x00, 0x01, 0x00, 0x06, 0xb7, 0x00, 0x00, 0x00, 0x4e, 0x34 };

                    //storing request received time
                    receivedDateTime = DateTime.Now;
                    HttpContext.Current.Application["ReqRecTime"] = receivedDateTime;

                    // 05 sep 2015 
                   /* if (rawData.Length == 34)
                    {
                        tempRawData = rawData;
                        //convert tempRawData to hex bytes
                        tempHexDataWithSpace = "";
                        for (int i = 0, k = 0; i < (tempRawData.Length) / 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                tempHexDataWithSpace = tempHexDataWithSpace + tempRawData[k];
                                k = k + 1;
                            }
                            if ((i + 1) < (tempRawData.Length) / 2)
                                tempHexDataWithSpace = tempHexDataWithSpace + " ";
                        }
                        tempAckReadPacket = tempHexDataWithSpace.Split().Select(s => Convert.ToByte(s, 16)).ToArray();
                        Boolean valid2 = AckOfReadTimeCmdProcessing.processReadTimeAckPack(tempAckReadPacket);
                        tempAckReadPacket = new byte[17];
                        if (valid2)
                            return "Command Ok";
                        else
                            return "Command Invalid";
                        
                    }
                    else
                    {*/
                        numberOfPacketsReceived = (rawData.Length) / 40;
                        float f = rawData.Length;
                        numberOfPacketsReceived = (int)Math.Ceiling(f / 40);

                        if ((numberOfPacketsReceived * 40) != rawData.Length)
                            return "Invalid";

                        //dividing into 40 bytes of data
                        index1 = 0;
                        for (index = 0; index < numberOfPacketsReceived; index++)
                        {
                            tempRawData = "";
                            for (rawDataLoopIndex = 0; rawDataLoopIndex < 40; rawDataLoopIndex++)
                            {
                                if (index1 < rawData.Length)
                                {
                                    tempRawData = tempRawData + rawData[index1];
                                    index1 = index1 + 1;
                                }
                                else
                                {
                                    ;
                                }
                            }
                            if (rawDataLoopIndex == 40)
                            {
                                //convert tempRawData to hex bytes
                                tempHexDataWithSpace = "";
                                for (int i = 0, k = 0; i < (tempRawData.Length) / 2; i++)
                                {
                                    for (int j = 0; j < 2; j++)
                                    {
                                        tempHexDataWithSpace = tempHexDataWithSpace + tempRawData[k];
                                        k = k + 1;
                                    }
                                    if ((i + 1) < (tempRawData.Length) / 2)
                                        tempHexDataWithSpace = tempHexDataWithSpace + " ";
                                }
                                tempPacket = tempHexDataWithSpace.Split().Select(s => Convert.ToByte(s, 16)).ToArray();
                                if (packetProcessing(tempPacket) == false)
                                {
                                    //send read time command
                                    return ReadTimeCommand.getReadTimeCmd();
                                }
                                else
                                {
                                    //do nothing
                                }
                            }
                            else
                            {
                                return "Event Invalid";
                            }
                            tempPacket = new byte[20];
                        }
                    /*}*/


                    if (Convert.ToInt16(validPacketsCount) == numberOfPacketsReceived)
                    {
                        return "Event Ok";
                    }
                    else
                    {
                        return "Event Invalid";
                    }
                }
                catch (Exception e)
                {
                    writeIntoFile(e.ToString());
                    return "Exception";
                }
            }
            else
            {
                //no data received
                //return "Invalid";
                return "No data received";
            }
        }

        public Boolean packetProcessing(byte[] packet)
        {
            //application level variables
            string DLPrevReqRecTime,DlPrevResetTime;
            string DLErrorTime;
            try
            {
                byte[] Dlid = new byte[2];
                int DLidValue;
                string strDLidValue;
                Dlid[0] = packet[0];
                Dlid[1] = packet[1];

                //converting idnumber from bytes to int, to get dlid
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(Dlid);
                DLidValue = BitConverter.ToInt16(Dlid, 0);
                strDLidValue = DLidValue.ToString();

                DLPrevReqRecTime = DLidValue.ToString() + "RecTime";
                DlPrevResetTime = DLidValue.ToString() + "PrevReset";
                DLErrorTime = DLidValue.ToString() + "Error";

                //maintaing datetime in application level with respect to DL   
                if (HttpContext.Current.Application != null)
                {
                    //HttpContext.Current.Application[DLPrevReqRecTime] = DateTime.Now;  //for test sandhya
                    //check if session data is null
                    /*if (HttpContext.Current.Application[DLPrevReqRecTime] == null)
                    {
                        //return false to the called function, which indicates that it need to send read time command
                        return false;
                    }
                    else
                    {*/
                        //check last received req time exceeds 5 min
                        //converting error time from ltime to min
                        //if ((int)Math.Floor(Convert.ToDouble(HttpContext.Current.Application[DLErrorTime])/(64*60)) > 5)                        
                        /*if (((Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"])).Subtract
                            (Convert.ToDateTime(HttpContext.Current.Application[DLPrevReqRecTime])).TotalMinutes) > 5)
                             //(Convert.ToDateTime(HttpContext.Current.Application[DLPrevReqRecTime])).TotalMinutes) < 0) //for test sandhya
                        {
                            //return false to the called function, which indicates that it need to send read time command
                            return false;
                        }
                        else
                        {*/
                            //keep DLid and time in session                        
                           /* HttpContext.Current.Application[DLPrevReqRecTime] = HttpContext.Current.Application["ReqRecTime"];*/

                            //based on the type id, send it to respective module to process                    
                            byte typeId = packet[9];
                            switch (typeId)
                            {
                                //80 H indicates reset packet
                                case 128:
                                    /*Boolean valid = ResetPacketProcessing.processResetPack(packet);
                                    if (valid)
                                        validPacketsCount = validPacketsCount + 1;
                                    break;*/
                                    HttpContext.Current.Application[DLPrevReqRecTime] = null;
                                    HttpContext.Current.Application[DlPrevResetTime] = DateTime.Now;
                                    return false;
                                //01 H indicates Analog packet
                                case 1:
                                    Boolean valid1 = AnalogPacketProcessing.processAnalogPack(packet);
                                    if (valid1)
                                        validPacketsCount = validPacketsCount + 1;
                                    break;
                                //05H indicates Health Packet
                                case 5:                                    
                                    validPacketsCount = validPacketsCount + 1;
                                    break;
                                default:
                                    break;
                            }
                        /*}*/
                    /*}*/
                }
                return true;
            }
            catch (Exception e)
            {
                //changed from true to false.. 29 oct 2015
                return false;
            }
        }
        /*public void packetProcessing(byte[] packet)
        {
            string rawPacket = "";
            //bytes of packet
            byte[] idNumber = new byte[2];
            byte[] serNumBytes = new byte[2];
            byte[] packetTime = new byte[4];
            byte packetYear;
            byte[] parameterTypeBytes = new byte[2];
            byte[] dataPart = new byte[4];
            byte CRC1 = packet[18];
            byte CRC2 = packet[19];
            byte[] CRC = { CRC1, CRC2 };
            byte[] crc_buffer = new byte[18];

            //bytes of packet's value
            short idNumberValue;
            ushort serNumValue;
            long pktTime;
            int packetYearValue;
            short parameterType;
            int parameterValue;
            float parameterValueToDB = 0f;
            long recDateTimeToDB;
            //channel numbers
            short temp = 0;
            short humid = 1;


            //byte[] CRC =  { 0x73, 0x66 };
            for (int i = 0; i < 18; i++)
            {
                crc_buffer[i] = packet[i];
            }

            ushort crc_buffer_value = CRC_Calculation.update(crc_buffer);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(CRC);
            ushort crc_value = BitConverter.ToUInt16(CRC, 0);

            if (crc_buffer_value == crc_value)
            {
                validPacketsCount = validPacketsCount + 1;

                //converting received date time to ltime
                recDateTimeToDB = DateToLtime(receivedDateTime);

                StringBuilder hex = new StringBuilder(packet.Length * 2);
                foreach (byte b in packet)
                    hex.AppendFormat("{0:x2}", b);
                rawPacket = hex.ToString();

                //Converting id number bytes to integer
                idNumber[0] = packet[0];
                idNumber[1] = packet[1];
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(idNumber);
                idNumberValue = BitConverter.ToInt16(idNumber, 0);

                tableName = "angdata" + Convert.ToString(idNumberValue);

                try
                {
                    //checking if table exists or not                    
                    ConnectionToEffe.Open();
                    SqlCommand = ConnectionToEffe.CreateCommand();
                    //SqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS" + "'" + tableName +"'" ;  
                    SqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS nfcdatalogger." + tableName +
                        "(SEQNO INT(10) NOT NULL, DLYEAR INT(5) NOT NULL, LTIME INT(10) NOT NULL, ASIGNO INT(10) NOT NULL, AVOLTAGE FLOAT NOT NULL, AFREQ INT(5) NOT NULL,"
                        + "ADLCS INT(5) NOT NULL, ARI INT(5) NOT NULL,CREATEDTIME DATETIME NOT NULL, RECEIVEDTIME INT(10) NOT NULL,RECEIVEDDATE DATETIME NOT NULL , RAWPACKET VARCHAR(50))";
                    SqlCommand.ExecuteNonQuery();
                    ConnectionToEffe.Close();
                }
                catch (MySqlException e)
                {
                    ;
                }

                //reading serial number bytes from packet
                serNumBytes[0] = packet[2];
                serNumBytes[1] = packet[3];
                //converting byte array to int
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(serNumBytes);
                serNumValue = BitConverter.ToUInt16(serNumBytes, 0);

                //packedTime reading and convertion
                packetTime[0] = packet[4];
                packetTime[1] = packet[5];
                packetTime[2] = packet[6];
                packetTime[3] = packet[7];
                //converting bytes to LTime
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(packetTime);
                pktTime = BitConverter.ToInt32(packetTime, 0);
                packetYear = packet[8];
                //if(packetYear != 0)
                packetYearValue = 2000 + Convert.ToInt16(packetYear);

                long tempPktTime = recDateTimeToDB + pktTime;
                string strDateTimeToDB = lTimeToDateString(recDateTimeToDB+pktTime, packetYearValue);

                //reading parameter type bytes from packet
                parameterTypeBytes[0] = packet[10];
                parameterTypeBytes[1] = packet[11];
                //converting byte array to int
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(parameterTypeBytes);
                parameterType = BitConverter.ToInt16(parameterTypeBytes, 0);

                //reading parameter value, for byte array to single convertion it needs 4 bytes thats why 1byte has been taken as 0
                //TODO: Need to complete the convertion
                dataPart[0] = 0;
                dataPart[1] = packet[12];
                dataPart[2] = packet[13];
                dataPart[3] = packet[14];
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(dataPart);
                parameterValue = BitConverter.ToInt16(dataPart, 0);

                float f = Convert.ToSingle(parameterValue); //converting int to float to get fraction value
                if (parameterType == temp) //temperature formula value/100
                    parameterValueToDB = f / 100;
                if (parameterType == humid) //huidity formula (value*140)/4095
                    parameterValueToDB = (f * 140) / 4095;
                
                try
                {
                    //ConnectionToEffe.Open();
                    //SqlCommand = ConnectionToEffe.CreateCommand();
                    //string jan = tableName + "#" + serNumValue + "#" + packetYearValue + "#" + pktTime + "#" + parameterType + "#" +
                    //    parameterValueToDB + "#" + strDateTimeToDB + "#" + recDateTimeToDB + "#" + rawPacket;                    
                    //SqlCommand.CommandText = " INSERT INTO `nfcdatalogger`.`sample` (  temp ) VALUES ('" + jan + "')";
                    //SqlCommand.ExecuteNonQuery();
                    //ConnectionToEffe.Close();

                    //for testing
                    CultureInfo enUS = new CultureInfo("en-US");
                    string strreceivedDate = receivedDateTime.ToString("MM/d/yyyy HH:mm:ss", enUS);

                    ConnectionToEffe.Open();
                    SqlCommand = ConnectionToEffe.CreateCommand();
                    //SqlCommand.CommandText = "INSERT INTO nfcdatalogger." + tableName + "(SEQNO, DLYEAR, LTIME, ASIGNO, AVOLTAGE, AFREQ, ADLCS, ARI, CREATEDTIME, RECEIVEDTIME, RAWPACKET ) " +
                    //" VALUES (" + serNumValue + "," + packetYearValue + "," + pktTime + "," + parameterType + "," + parameterValueToDB + "," + 0 + "," + 0 + "," +
                    //0 + ", STR_TO_DATE('" + strDateTimeToDB + "', '%m/%d/%Y %H:%i:%s'), " + recDateTimeToDB + ",'" + rawPacket + "')";
                    SqlCommand.CommandText = "INSERT INTO nfcdatalogger." + tableName + "(SEQNO, DLYEAR, LTIME, ASIGNO, AVOLTAGE, AFREQ, ADLCS, ARI, CREATEDTIME, RECEIVEDTIME, RECEIVEDDATE, RAWPACKET ) " +
                    " VALUES (" + serNumValue + "," + packetYearValue + "," + (recDateTimeToDB+pktTime) + "," + parameterType + "," + parameterValueToDB + "," + 0 + "," + 0 + "," +
                    0 + ", STR_TO_DATE('" + strDateTimeToDB + "', '%m/%d/%Y %H:%i:%s'), " + recDateTimeToDB + ", STR_TO_DATE('" + strreceivedDate + "', '%m/%d/%Y %H:%i:%s'), '" + rawPacket + "')";
                    SqlCommand.ExecuteNonQuery();
                    ConnectionToEffe.Close();

                    rawPacket = "";                    
                }
                catch (MySqlException e)
                {                    
                    //http://dev.mysql.com/doc/connector-net/en/connector-net-programming-connecting-errors.html
                }
            }
            else
            {                
                //packet is invalid
            }
        }*/

        //procedure to resize 2d array
        private static Array ResizeArray(Array arr, int[] newSizes)
        {
            if (newSizes.Length != arr.Rank)
                throw new ArgumentException("arr must have the same number of dimensions " +
                                            "as there are elements in newSizes", "newSizes");

            var temp = Array.CreateInstance(arr.GetType().GetElementType(), newSizes);
            int length = arr.Length <= temp.Length ? arr.Length : temp.Length;
            Array.ConstrainedCopy(arr, 0, temp, 0, length);
            return temp;
        }       

    }
}