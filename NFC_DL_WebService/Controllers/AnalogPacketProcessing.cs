using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Globalization;
using System.Web;
using System.Data.OleDb;
using NFC_DL_WebService.Models;
using System.IO;

namespace NFC_DL_WebService.Controllers
{
    public class AnalogPacketProcessing : ApiController
    {
        //by D. Venkata Naresh- private static OleDbCommand OleCommand;
        //by D. Venkata Naresh- private static OleDbConnection OleDBConn;

        public static Boolean processAnalogPack(byte[] packet)
        {
            dbModule dbObj = new dbModule();
            //by D. Venkata Naresh- OleDBConn = new OleDbConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MyOrclConn"].ToString());

            string DLPrevReqRecTime;
            string DLErrorTime;

            string angTableName;
            string digTableName;
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
            double parameterValueToDB = 0f;
            long recDateTimeToDB;
            long ltimeToDB;
            //channel numbers
            const short temp = 0;
            const short humid = 1;
            const short co2 = 2;
            const short voc = 3;
            const short tvoc = 4;
            const short dust_1 = 5;
            const short dust_2 = 6;


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
                //validPacketsCount = validPacketsCount + 1;                

                /* for raw data
                 * StringBuilder hex = new StringBuilder(packet.Length * 2);
                foreach (byte b in packet)
                    hex.AppendFormat("{0:x2}", b);
                rawPacket = hex.ToString();*/

                //Converting id number bytes to integer
                idNumber[0] = packet[0];
                idNumber[1] = packet[1];
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(idNumber);
                idNumberValue = BitConverter.ToInt16(idNumber, 0);

                DLPrevReqRecTime = idNumberValue.ToString() + "RecTime";
                DLErrorTime = idNumberValue.ToString() + "Error";

                angTableName = "angdata" + Convert.ToString(idNumberValue);
                digTableName = "digdata" + Convert.ToString(idNumberValue);

                //by D. Venkata Naresh- 
                /*
                try
                {                    
                    //checking if table exists or not                    
                    OleDBConn.Open();
                    
                        OleCommand = new OleDbCommand("CREATE TABLE " + angTableName + " (SEQNO Number(10) NOT NULL, DLYEAR Number(5) NOT NULL," 
                                                        +"LTIME Number(10) NOT NULL, ASIGNO Number(10) NOT NULL, AVOLTAGE FLOAT NOT NULL, AFREQ Number(5) NOT NULL, " +
                                                        "ADLCS Number(5) NOT NULL, ARI Number(5) NOT NULL,CREATEDTIME date NOT NULL, RECEIVEDTIME Number(10) NOT NULL)", OleDBConn);
                        //OleDBCommand.CommandText = "CREATE TABLE IF NOT EXISTS " + tableName +
                          //  "(SEQNO INT(10) NOT NULL, DLYEAR INT(5) NOT NULL, LTIME INT(10) NOT NULL, ASIGNO INT(10) NOT NULL, AVOLTAGE FLOAT NOT NULL, AFREQ INT(5) NOT NULL,"
                            //+ "ADLCS INT(5) NOT NULL, ARI INT(5) NOT NULL,CREATEDTIME DATETIME NOT NULL, RECEIVEDTIME INT(10) NOT NULL,RECEIVEDDATE DATETIME NOT NULL , RAWPACKET VARCHAR(50))";
                        OleCommand.ExecuteNonQuery();
                    }
                catch (OleDbException e)
                {
                    ;
                }
                try{
                        OleCommand = new OleDbCommand("create table "+ digTableName +"(signo number(10), ddlcs number(10), drigs number(10), seqno number(10)," 
                                                    +" dlyear number(5), ltime number(11), dsigstatus number(5), createdtime date, receivedtime number(11))", OleDBConn);                        
                        OleCommand.ExecuteNonQuery();                   
                }
                catch (OleDbException e)
                {
                    ;
                }*/

                //reading serial number bytes from packet
                serNumBytes[0] = packet[2];
                serNumBytes[1] = packet[3];
                //converting byte array to int
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(serNumBytes);
                serNumValue = BitConverter.ToUInt16(serNumBytes, 0);

                //packedTime reading and convertion
                /* packetTime[0] = packet[4];
                 packetTime[1] = packet[5];
                 packetTime[2] = packet[6];
                 packetTime[3] = packet[7];
                 //converting bytes to LTime
                 if (BitConverter.IsLittleEndian)
                     Array.Reverse(packetTime);
                 pktTime = BitConverter.ToInt32(packetTime, 0);*/
                /*packetYear = packet[8];
                //if(packetYear != 0)
                packetYearValue = 2000 + Convert.ToInt16(packetYear);*/

                //convert current time to ltime without year
                long receiveLtime = DateTimeConversions.DateToLtime(Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]));
                /*if (receiveLtime < pktTime)
                    packetYearValue = DateTime.Now.Year - 1;
                else
                    packetYearValue = DateTime.Now.Year;*/
                packetYearValue = Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]).Year;


                //converting received date time to ltime
                /*recDateTimeToDB = DateTimeConversions.DateToLtime(Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]));*/
                recDateTimeToDB = receiveLtime;

                /*
                 * ltime to store in db i.e error + packet time                
                 * converting error time to ltime and then adding it to the packet ltime*/
                //ltimeToDB = pktTime + (long)Math.Floor(Convert.ToDouble(HttpContext.Current.Application[DLErrorTime]) * 64 * 60);
                /*ltimeToDB = pktTime + Convert.ToInt64(HttpContext.Current.Application[DLErrorTime]);*/
                ltimeToDB = receiveLtime;

                //converting ltimeToDB to datetime, to store in DB as CreatedDateTime                
                /*DateTime createdDateTime = DateTimeConversions.packTimeYearToDateTime(ltimeToDB, packetYearValue);*/
                DateTime createdDateTime = Convert.ToDateTime(HttpContext.Current.Application["ReqRecTime"]);

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
                parameterValue = BitConverter.ToInt32(dataPart, 0);

                float f = Convert.ToSingle(parameterValue); //converting int to float to get fraction value
                /*if (parameterType == temp) //temperature formula value/100
                    parameterValueToDB = f / 100;
                if (parameterType == humid) //humidity formula (value*140)/4095
                    parameterValueToDB = (f * 140) / 4095;*/

                //Applying Formulas
                switch (parameterType)
                {
                    case temp:
                        if (angTableName == "angdata711")
                        {
                            parameterValueToDB = (f / 32767) * 60000;
                        }
                        else
                        {
                            parameterValueToDB = (f / 32767) * 140;
                        }
                        break;
                    case humid:
                        if (angTableName == "angdata711")
                        {
                            parameterValueToDB = (f / 32767) * 60000;
                        }
                        else
                        {
                            parameterValueToDB = (f / 32767) * 100;
                        }
                        break;
                    case co2:
                        if (angTableName == "angdata711")
                        {
                            parameterValueToDB = (f / 32767) * 60000;
                        }
                        else
                        {
                            parameterValueToDB = (f / 32767) * 10000;
                        }
                        break;
                    case voc:
                        if (angTableName == "angdata711")
                        {
                            parameterValueToDB = (f / 32767) * 60000;
                        }
                        else
                        {
                            parameterValueToDB = (f / 32767) * 60000;
                        }
                        break;
                    case tvoc:
                        if (angTableName == "angdata711")
                        {
                            parameterValueToDB = (f / 32767) * 60000;
                        }
                        else
                        {
                            parameterValueToDB = (f / 32767) * 30000;
                        }
                        break;
                    
                    

                    /*case dust_1:
                        parameterValueToDB = (f / 140) * 32767;
                        break;
                    case dust_2:
                        parameterValueToDB = (f / 140) * 32767;
                        break;*/

                    //Energy Auditor
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                    case 26:
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                    case 31:
                    case 32:
                    case 33:
                    case 34:
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                    case 44:
                    case 45:
                    case 46:
                    case 47:
                    case 48:
                    case 49:
                    case 50:
                    case 51:
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                    case 58:
                    case 59:
                    case 60:
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71:
                    case 72:
                        dataPart[3] = packet[12];
                        dataPart[2] = packet[13];
                        dataPart[1] = packet[14];
                        dataPart[0] = packet[15];
                        parameterValueToDB = BitConverter.ToSingle(dataPart, 0);
                        break;
                    
                    //Beacon ID
                    case 90: parameterValueToDB = parameterValue;
                        break;
                }

                //Converting Commented because all Sensors values are going to take backup

                /* int fbASigNo = -1;
                 //Converting Energy Auditor signo to EAC signo (Only for Energy Auditor)
                 switch (parameterType)
                 {
                     case 11: fbASigNo = 3; break;
                     case 12: fbASigNo = 7; break;
                     case 13: fbASigNo = 11; break;
                     /*case 14: fbASigNo = ; break;
                     case 15: fbASigNo = ; break;
                     case 16: fbASigNo = ; break;
                     case 17: fbASigNo = ; break;
                     case 18: fbASigNo = ; break;
                     case 19: fbASigNo = ; break;
                     case 20: fbASigNo = ; break;
                     case 21: fbASigNo = ; break;
                     case 22: fbASigNo = ; break;*/
                /*   case 23: fbASigNo = 1; break;
                   case 24: fbASigNo = 5; break;
                   case 25: fbASigNo = 9; break;
                   case 26: fbASigNo = 16; break;
                   case 27: fbASigNo = 19; break;
                   case 28: fbASigNo = 22; break;
                   case 29: fbASigNo = 2; break;
                   case 30: fbASigNo = 6; break;
                   case 31: fbASigNo = 10; break;
                   /*case 32: fbASigNo = ; break;
                   case 33: fbASigNo = ; break;
                   case 34: fbASigNo = ; break;*/
                /*    case 35: fbASigNo = 4; break;
                    case 36: fbASigNo = 8; break;
                    case 37: fbASigNo = 12; break;
                    //case 38: fbASigNo = ; break;
                    case 39: fbASigNo = 13; break;
                    /*case 40: fbASigNo = ; break;
                    case 41: fbASigNo = ; break;
                    case 42: fbASigNo = ; break;*/
                /*}*/

                try
                {
                    /*if (fbASigNo != -1)
                    {
                        dbObj.postQueryIntoFB("INSERT INTO ANGDATA65 (SEQNO, DLYEAR, LTIME, ASIGNO, AVOLTAGE, AFREQ, ADLCS, ARI, RECEIVEDTIME) VALUES ("
                             + serNumValue + ", " + packetYearValue + "," + ltimeToDB + "," + fbASigNo + ", " + parameterValueToDB + ", 0, 0, 0, " + recDateTimeToDB + ");");
                    }*/
                    dbObj.postQueryIntoFB("INSERT INTO " + angTableName + " (SEQNO, DLYEAR, LTIME, ASIGNO, AVOLTAGE, AFREQ, ADLCS, ARI, RECEIVEDTIME) VALUES ("
                             + serNumValue + ", " + packetYearValue + "," + ltimeToDB + "," + parameterType + ", " + parameterValueToDB + ", 0, 0, 0, " + recDateTimeToDB + ");");
                }
                catch (Exception e)
                {
                    //do nothing
                }
                try
                {
                    /*
                     * //for testing
                    CultureInfo enUS = new CultureInfo("en-US");
                    string strreceivedDate = ProcessingController.receivedDateTime.ToString("MM/d/yyyy HH:mm:ss", enUS);*/

                    //by D. Venkata Naresh- 
                    /*OleCommand = new OleDbCommand("INSERT INTO " + angTableName + "(SEQNO, DLYEAR, LTIME, ASIGNO, AVOLTAGE, AFREQ, ADLCS, ARI, CREATEDTIME, RECEIVEDTIME) " +
                    " VALUES (" + serNumValue + "," + packetYearValue + "," + ltimeToDB + "," + parameterType + "," + parameterValueToDB + "," + 0 + "," + 0 + "," +
                    0 + ", TO_DATE('" + strCreatedDateTime + "', 'MM/DD/YYYY HH24:MI:SS')," + recDateTimeToDB + ")", OleDBConn);*/

                    /*Oracle db - old
                     * OleCommand = new OleDbCommand("INSERT INTO " + tableName + "(SEQNO, DLYEAR, LTIME, ASIGNO, AVOLTAGE, AFREQ, ADLCS, ARI, CREATEDTIME, RECEIVEDTIME, RAWPACKET ) " +
                    " VALUES (" + serNumValue + "," + packetYearValue + "," + ltimeToDB + "," + parameterType + "," + parameterValueToDB + "," + 0 + "," + 0 + "," +
                    0 + ", TO_DATE('" + strCreatedDateTime + "', 'MM/DD/YYYY HH24:MI:SS')," + recDateTimeToDB + ",'" + rawPacket + "')", OleDBConn);*/

                    /*MySql-old
                     * OleCommand.CommandText = "INSERT INTO " + tableName + "(SEQNO, DLYEAR, LTIME, ASIGNO, AVOLTAGE, AFREQ, ADLCS, ARI, CREATEDTIME, RECEIVEDTIME, RAWPACKET ) " +
                    " VALUES (" + serNumValue + "," + packetYearValue + "," + ltimeToDB + "," + parameterType + "," + parameterValueToDB + "," + 0 + "," + 0 + "," +
                    0 + ", STR_TO_DATE('" + strCreatedDateTime + "', '%m/%d/%Y %H:%i:%s'), " + recDateTimeToDB + ",'" + rawPacket + "')";*/

                    //by D. Venkata Naresh- OleCommand.ExecuteNonQuery();
                    //by D. Venkata Naresh- OleDBConn.Close();

                    dbObj.postQuery(new List<string>{"Insert into "+ angTableName +" (date, dlyear, ltime, seqno, asigno, avoltage, createdtime, receivedtime, afreq, adlcs, ari) Values ('" +
                       createdDateTime.ToString("yyyy-MM-dd") + "'," + packetYearValue + "," + ltimeToDB + "," + serNumValue + ", " + parameterType + "," + parameterValueToDB + ", '" + createdDateTime.ToString("yyyy-MM-dd HH:mm:ss") + "'," + recDateTimeToDB + ", 0, 0, 0);"});
                    rawPacket = "";
                    return true;
                }
                catch (Exception e)
                {
                    //http://dev.mysql.com/doc/connector-net/en/connector-net-programming-connecting-errors.html
                    return false;
                }
            }
            else
            {
                //packet is invalid
                return false;
            }
        }

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
    }
}
