using NFC_DL_WebService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Data.OleDb;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections;
using System.Globalization;
using Cassandra;


namespace NFC_DL_WebService.Controllers
{
    [RoutePrefix("Services/SensorData")]
    public class SendingJSONDataController : ApiController
    {
        dbModule dbModuleObj = new dbModule();
        CultureInfo enUS = new CultureInfo("en-US");
        ResErrorDTO error = new ResErrorDTO();
        [HttpGet]
        [Route("GetSiteConfig")]
        public string GetSiteConfig(string reqConfig)
        {
            //by D. Venkata Naresh- OleDbDataReader dbReader;
            string query = "";
            ResConfigDTO resConfigData = new ResConfigDTO();            
            JavaScriptSerializer jSearializer = new JavaScriptSerializer();
            jSearializer.MaxJsonLength = Int32.MaxValue; //Added by D. Venkata Naresh to maximize the JSON Length
            ReqConfigDTO reqConfigObj;
            Boolean dataFound = false;
            try
            {
                if ((!reqConfig.Equals("")) || (reqConfig !=null))
                {
                    reqConfigObj = jSearializer.Deserialize<ReqConfigDTO>(reqConfig);
                    if (reqConfigObj != null)
                    //if(reqConfig != null)
                    {
                        //get configuration data of requested site from rhsetup table                                                           
                        //by D. Venkata Naresh- query = "select dlno, dlname from rhsetup where upper(dlstation) = '" + reqConfigObj.siteName.ToString().ToUpper() + "'";
                        query = "select dlno, sitecode, lastupdate from rhsetup where sitename = '" + reqConfigObj.siteName.ToString() + "'";
                        //query = "select dlno, dlname from rhsetup where dlstation = 'Lakheri PMU'";
                        List<Row> rows = dbModuleObj.postQuery(new List<string> { query });
                        if (rows.Count > 0)  
                        //by D. Venkata Naresh- dbReader = dbModule.getDBData(query);
                        //by D. Venkata Naresh- if (dbReader.HasRows == true)
                        {
                            dataFound = true;
                            //by D. Venkata Naresh-
                            /*while (dbReader.Read())
                            {                                
                                resConfigData.dlNo = Convert.ToInt32(dbReader.GetValue(0));
                                resConfigData.siteCode = dbReader.GetString(1);
                                resConfigData.lastUpdate = DateTime.Now.ToString();
                            }*/
                            
                            foreach (var row in rows)
                            {
                                resConfigData.dlNo = row.GetValue<Int32>("dlno");
                                resConfigData.siteCode = row.GetValue<string>("sitecode");  
                                resConfigData.lastUpdate = (row.GetValue<DateTime>("lastupdate")).ToString("MM/d/yyyy HH:mm:ss", enUS);
                            }
                            
                        }
                        else
                        {                            
                            //if here dbreader returns 0 rows, means that there is no site present with this name
                            error.responseCode = 201;
                            error.description = "No Site found";
                            return jSearializer.Serialize(error);
                        }
                        //by D. Venkata Naresh- dbReader.Dispose();

                        //get analog signals information from asignal table
                        //by D. Venkata Naresh- query = "select asigno, asigname, asigtypeno,aminvolt, amaxvolt from asignal" + resConfigData.dlNo + " where asigname not like '%Not%'";
                        query = "select asigno, asigname, asigtypeno,aminvolt, amaxvolt from asignal" + resConfigData.dlNo + ";";
                        //by D. Venkata Naresh- dbReader = dbModule.getDBData(query);
                        ArrayList angKeys = new ArrayList();
                        
                        angKeys.Add("asigNo");
                        angKeys.Add("asigName");
                        angKeys.Add("asigTypeNo");
                        angKeys.Add("aMinVolt");
                        angKeys.Add("aMaxVolt");
                        resConfigData.channels.Analog.Add(angKeys);
                        //by D. Venkata Naresh- if (dbReader.HasRows == true)
                        rows = dbModuleObj.postQuery(new List<string> { query });
                        if (rows.Count > 0) 
                        {
                            //by D. Venkata Naresh-
                            /*while (dbReader.Read())
                            {
                                ArrayList angValues = new ArrayList();
                                angValues.Add(Convert.ToInt32(dbReader.GetValue(0)));
                                angValues.Add(dbReader.GetString(1));
                                angValues.Add(Convert.ToInt32(dbReader.GetValue(2)));
                                angValues.Add(Convert.ToSingle(dbReader.GetValue(3)));
                                angValues.Add(Convert.ToSingle(dbReader.GetValue(4)));
                                resConfigData.channels.Analog.Add(angValues);
                            }*/
                           
                            foreach (var row in rows)
                            {
                                ArrayList angValues = new ArrayList();
                                angValues.Add(row.GetValue<Int32>("asigno"));
                                angValues.Add(row.GetValue<string>("asigname"));
                                angValues.Add(row.GetValue<Int32>("asigtypeno"));
                                angValues.Add(row.GetValue<Single>("aminvolt"));
                                angValues.Add(row.GetValue<Single>("amaxvolt"));
                                resConfigData.channels.Analog.Add(angValues);
                            }
                            
                        }
                        //by D. Venkata Naresh- dbReader.Dispose();

                        //get digital signals information from dsignal table
                        //by D. Venkata Naresh- query = "select signo, signame, sigtypeno from dsignal" + resConfigData.dlNo + " where signame not like '%SPARE%'";
                        query = "select signo, signame, sigtypeno from dsignal" + resConfigData.dlNo + ";";
                        //by D. Venkata Naresh- dbReader = dbModule.getDBData(query);
                        ArrayList digKeys = new ArrayList();
                        digKeys.Add("sigNo");
                        digKeys.Add("sigName");
                        digKeys.Add("sigTypeNo");
                        resConfigData.channels.Digital.Add(digKeys);
                        //by D. Venkata Naresh- if (dbReader.HasRows == true) 
                        rows = dbModuleObj.postQuery(new List<string> { query });
                        if (rows.Count > 0)                       
                        {
                             //by D. Venkata Naresh-
                            /*
                            dataFound = true;
                            while (dbReader.Read())
                            {
                                ArrayList digValues = new ArrayList();
                                digValues.Add(Convert.ToInt32(dbReader.GetValue(0)));
                                digValues.Add(dbReader.GetString(1));
                                digValues.Add(Convert.ToInt32(dbReader.GetValue(2)));
                                resConfigData.channels.Digital.Add(digValues);                                
                            }*/

                            foreach (var row in rows)
                            {
                                ArrayList digValues = new ArrayList();
                                digValues.Add(row.GetValue<Int32>("signo"));
                                digValues.Add(row.GetValue<string>("signame"));
                                digValues.Add(row.GetValue<Int32>("sigtypeno"));
                                resConfigData.channels.Digital.Add(digValues); 
                            }
                            
                        }
                        //by D. Venkata Naresh- dbReader.Dispose();
                    }
                }

                //if data found is true then response code is 200 i.e success; otherwise it is 201 i.e no site found                
                if (dataFound == true)
                {
                    resConfigData.responseCode = 200;
                    //return siteconfig;
                    return jSearializer.Serialize(resConfigData);
                }
                else
                {                            
                    //by D. Venkata Naresh-  resConfigData.responseCode = 202;
                    error.responseCode = 201;
                    error.description = "No Site found";
                    return jSearializer.Serialize(error);
                }     
            }
            catch (Exception e)
            {               
                error.responseCode = 204;
                error.description = e.Message;// "Unable to Process Request";
                return jSearializer.Serialize(error);
            }
        }

        [HttpGet]
        [Route("GetOnlineData")]
        public string GetOnlineData(String reqOnline)
        {
            //by D. Venkata Naresh- OleDbDataReader dbReader;
            string query = "";
            ReqOnlineDataDTO reqOnlineDataObj;
            ResOnlineDataDTO resOnlineData = new ResOnlineDataDTO();
            JavaScriptSerializer jSearializer = new JavaScriptSerializer();
            jSearializer.MaxJsonLength = Int32.MaxValue;
            Boolean dataFound = false;
            long prevSigno = -1;
            long presSigno = -1;  
            try
            {
                if ((!reqOnline.Equals("")) || (reqOnline != null))
                {
                    reqOnlineDataObj = jSearializer.Deserialize<ReqOnlineDataDTO>(reqOnline);
                    if (reqOnlineDataObj != null)
                    {
                        DateTime reqFrom = DateTimeConversions.LtimetoIST(reqOnlineDataObj.fromTime, reqOnlineDataObj.fromYear);
                        DateTime[] queryingDates = DateTimeConversions.GetDatesBetween(reqFrom, DateTime.Now).ToArray();



                        //for the first time, requesting data
                        //if (reqOnlineDataObj.seqNo == -1)
                        //temporarily kept >= -1 condition, bcz query is not ready for seqno base
                        //by D. Venkata Naresh- if (reqOnlineDataObj.seqNo >= -1)
                        //by D. Venkata Naresh- {                   
                            //getting analog data
                        //by D. Venkata Naresh- query = "select avoltage, ltime, dlyear, seqno, asigno from angdata" + reqOnlineDataObj.DLNO + " where ltime >= " + reqOnlineDataObj.fromTime + " and dlyear = " + reqOnlineDataObj.fromYear + " order by asigno, ltime desc";
                        query = "select avoltage, ltime, dlyear, seqno, asigno from angdata" + reqOnlineDataObj.DLNO + " where date = '" + queryingDates[0].ToString("yyyy-MM-dd") + "' and asigno in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime,seqno) > (" + reqOnlineDataObj.fromYear + ", " + reqOnlineDataObj.fromTime + ", " + reqOnlineDataObj.seqNo + ") order by asigno desc, dlyear desc, ltime desc, seqno desc;";
                        List<string> queries = new List<string> { query };
                        for (int dateIndex = 1; dateIndex < queryingDates.Length; dateIndex++)
                            queries.Add("select avoltage, ltime, dlyear, seqno, asigno from angdata" + reqOnlineDataObj.DLNO + " where date = '" + queryingDates[dateIndex].ToString("yyyy-MM-dd") + "' order by asigno desc, dlyear desc, ltime desc, seqno desc;");

                            //for testing
                            //query = "select avoltage, ltime, dlyear, seqno, asigno from angdata65 where rownum < 10 order by asigno, ltime desc";
                        //by D. Venkata Naresh- dbReader = dbModule.getDBData(query);
                            resOnlineData.analog.keys[0] = "aVoltage";
                            resOnlineData.analog.keys[1] = "ltime";
                            resOnlineData.analog.keys[2] = "dlYear";
                            resOnlineData.analog.keys[3] = "seqNo";
                            //by D. Venkata Naresh- angValues tempangValues = new angValues();
                            angValues tempangValues = null;
                            //by D. Venkata Naresh- if (dbReader.HasRows == true)
                            List<Row> rows = dbModuleObj.postQuery(queries);
                            if (rows.Count > 0)
                            {
                                dataFound = true;
                                //by D. Venkata Naresh-
                                /*while (dbReader.Read())
                                {
                                    presSigno = Convert.ToInt64(dbReader.GetValue(4));
                                    if (prevSigno == presSigno)
                                    {
                                        //don't create new angValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                        ;
                                    }
                                    else
                                    {
                                        //create bew angValues object iff signo changes
                                        tempangValues = new angValues();
                                        prevSigno = presSigno;
                                        resOnlineData.analog.values.Add(tempangValues);
                                    }
                                    tempangValues.asigNo = presSigno;
                                    ArrayList tempValues = new ArrayList();
                                    tempValues.Add(Convert.ToSingle(dbReader.GetValue(0)));
                                    tempValues.Add(Convert.ToInt64(dbReader.GetValue(1)));
                                    tempValues.Add(Convert.ToInt32(dbReader.GetValue(2)));
                                    tempValues.Add(Convert.ToInt64(dbReader.GetValue(3)));
                                    tempangValues.values.Add(tempValues);
                                }*/
                                
                                foreach (var row in rows)
                                {
                                    presSigno = row.GetValue<Int32>("asigno");
                                    if (prevSigno == presSigno)
                                    {
                                        //don't create new angValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                        ;
                                    }
                                    else
                                    {
                                        tempangValues = null;
                                        //Search for angValues Obj having signo same as presSigNo;
                                        foreach (var tempObj in resOnlineData.analog.values)
                                        {
                                            if (tempObj.asigNo == presSigno)
                                            {
                                                tempangValues = tempObj;
                                                break;
                                            }
                                        }
                                        if (tempangValues == null)
                                        {
                                            //create new angValues object if object not exist
                                            tempangValues = new angValues();                                            
                                            resOnlineData.analog.values.Add(tempangValues);
                                        }
                                        prevSigno = presSigno;
                                    }                                   


                                    tempangValues.asigNo = presSigno;
                                    ArrayList tempValues = new ArrayList();
                                    tempValues.Add(row.GetValue<Single>("avoltage"));
                                    tempValues.Add(row.GetValue<Int32>("ltime"));
                                    tempValues.Add(row.GetValue<Int32>("dlyear"));
                                    tempValues.Add(row.GetValue<Int32>("seqno"));
                                    tempangValues.values.Add(tempValues);
                                }
                                
                            }
                            prevSigno = -1;
                            presSigno = -1;
                            //by D. Venkata Naresh- dbReader.Dispose();

                            //getting digital data
                            //by D. Venkata Naresh- query = "select dsigstatus, ltime, dlyear,seqno, signo from digdata" + reqOnlineDataObj.DLNO + " where ltime >= " + reqOnlineDataObj.fromTime + " and dlyear = " + reqOnlineDataObj.fromYear + " order by signo, ltime desc";
                            queries.Clear();
                            queries.Add("select dsigstatus, ltime, dlyear, seqno, signo from digdata" + reqOnlineDataObj.DLNO + " where date = '" + queryingDates[0].ToString("yyyy-MM-dd") + "' and signo in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime,seqno) > (" + reqOnlineDataObj.fromYear + ", " + reqOnlineDataObj.fromTime + ", " + reqOnlineDataObj.seqNo + ") order by signo desc, dlyear desc, ltime desc, seqno desc;");
                            for (int dateIndex = 1; dateIndex < queryingDates.Length; dateIndex++)
                                queries.Add("select dsigstatus, ltime, dlyear, seqno, signo from digdata" + reqOnlineDataObj.DLNO + " where date = '" + queryingDates[dateIndex].ToString("yyyy-MM-dd") + "' order by signo desc, dlyear desc, ltime desc, seqno desc;");
                            //for testing
                            //query = "select dsigstatus, ltime, dlyear,seqno, signo from digdata65 where rownum < 10  order by signo, ltime desc";
                            //by D. Venkata Naresh- dbReader = dbModule.getDBData(query);
                            resOnlineData.digital.keys[0] = "dSigStatus";
                            resOnlineData.digital.keys[1] = "ltime";
                            resOnlineData.digital.keys[2] = "dlYear";
                            resOnlineData.digital.keys[3] = "seqNo";
                            //by D. Venkata Naresh- digValues tempdigValues = new digValues();
                            digValues tempdigValues = null;
                            //by D. Venkata Naresh- if (dbReader.HasRows == true)
                            rows = dbModuleObj.postQuery(queries);
                            if (rows.Count > 0)
                            {
                                dataFound = true;
                                /*while (dbReader.Read())
                                {
                                    presSigno = Convert.ToInt64(dbReaders.GetValue(4));
                                    if (prevSigno == presSigno)
                                    {
                                        //don't create new angValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                        ;
                                    }
                                    else
                                    {
                                        //create bew digValues object iff signo changes
                                        tempdigValues = new digValues();
                                        prevSigno = presSigno;
                                        resOnlineData.digital.values.Add(tempdigValues);
                                    }
                                    tempdigValues.sigNo = presSigno;
                                    ArrayList tempValues = new ArrayList();
                                    tempValues.Add(Convert.ToSingle(dbReader.GetValue(0)));
                                    tempValues.Add(Convert.ToInt64(dbReader.GetValue(1)));
                                    tempValues.Add(Convert.ToInt32(dbReader.GetValue(2)));
                                    tempValues.Add(Convert.ToInt64(dbReader.GetValue(3)));
                                    tempdigValues.values.Add(tempValues);
                                }*/
                                
                                foreach (var row in rows)
                                {
                                    presSigno = row.GetValue<Int32>("signo");
                                    if (prevSigno == presSigno)
                                    {
                                        //don't create new angValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                        ;
                                    }
                                    else
                                    {                                        
                                        tempdigValues = null;
                                        //Search for angValues Obj having signo same as presSigNo;
                                        foreach (var tempObj in resOnlineData.digital.values)
                                        {
                                            if (tempObj.sigNo == presSigno)
                                            {
                                                tempdigValues = tempObj;
                                                break;
                                            }
                                        }
                                        if (tempdigValues == null)
                                        {
                                            //create new digValues object if object not exist
                                            tempdigValues = new digValues();                                            
                                            resOnlineData.digital.values.Add(tempdigValues);
                                        }
                                        prevSigno = presSigno;
                                    }
                                    tempdigValues.sigNo = presSigno;
                                    ArrayList tempValues = new ArrayList();
                                    tempValues.Add(row.GetValue<Int32>("dsigstatus"));
                                    tempValues.Add(row.GetValue<Int32>("ltime"));
                                    tempValues.Add(row.GetValue<Int32>("dlyear"));
                                    tempValues.Add(row.GetValue<Int32>("seqno"));
                                    tempdigValues.values.Add(tempValues);
                                }
                                
                            }
                            prevSigno = -1;
                            presSigno = -1;
                        //by D. Venkata Naresh- dbReader.Dispose();
                        //by D. Venkata Naresh- }
                            //by D. Venkata Naresh- else
                        //by D. Venkata Naresh- {

                        //by D. Venkata Naresh- }
                    }
                }

                //if data found is true then response code is 200 i.e success; otherwise it is 202 i.e no data found                
                if (dataFound == true)
                {
                    resOnlineData.responseCode = 200;
                    //return onlineData;
                    return jSearializer.Serialize(resOnlineData);
                }
                else
                {
                    //by D. Venkata Naresh-  resOnlineData.responseCode = 202;
                    error.responseCode = 202;
                    error.description = "No data found";
                    return jSearializer.Serialize(error);
                }                
            }
            catch (Exception e)
            {
                if (e.Message.Contains("unconfigured table angdata") || e.Message.Contains("unconfigured table digdata"))
                {
                    error.responseCode = 203;
                    error.description = "Invalid DLNO";
                }
                else
                {
                    error.responseCode = 204;
                    error.description = e.Message;// "Unable to Process Request";
                }
                return jSearializer.Serialize(error);
            }
        }

        [HttpGet]
        [Route("GetHistoryData")]
        public string GetHistoryData(string reqHistory)
        {
            //by D. Venkata Naresh- OleDbDataReader dbReader;
            string query = "";
            ReqHistoryDataDTO reqHistoryDataObj;
            ResHistoryDataDTO resHistoryData = new ResHistoryDataDTO();
            JavaScriptSerializer jSearializer = new JavaScriptSerializer();
            jSearializer.MaxJsonLength = Int32.MaxValue;
            Boolean dataFound = false;
            long prevSigno = -1;
            long presSigno = -1;               

            try
            {
                if ((!reqHistory.Equals("")) || (reqHistory != null))
                {
                    reqHistoryDataObj = jSearializer.Deserialize<ReqHistoryDataDTO>(reqHistory);
                    if (reqHistoryDataObj != null)
                    {
                        DateTime reqFrom = DateTimeConversions.LtimetoIST(reqHistoryDataObj.fromTime, reqHistoryDataObj.fromYear);
                        DateTime reqTo = DateTimeConversions.LtimetoIST(reqHistoryDataObj.toTime, reqHistoryDataObj.toYear);
                        DateTime[] queryingDates = DateTimeConversions.GetDatesBetween(reqFrom, reqTo).ToArray();
                        //getting analog data
                        //by D. Venkata Naresh- query = "select avoltage, ltime, dlyear from angdata" + reqHistoryDataObj.DLID + " where (ltime >= " + reqHistoryDataObj.fromTime + " and dlyear = " + reqHistoryDataObj.fromYear + ")"
                        //by D. Venkata Naresh-         + " or (ltime <= " + reqHistoryDataObj.toTime + " and dlyear = " + reqHistoryDataObj.toYear + ") order by asigno, ltime desc";
                        List<string> queries = new List<string> {  };
                        if (queryingDates.Length == 1)
                        {
                            queries.Add("select avoltage, ltime, dlyear, asigno from angdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[0].ToString("yyyy-MM-dd") + "' and asigno in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime) >= (" + reqHistoryDataObj.fromYear + ", " + reqHistoryDataObj.fromTime + ") and (dlyear,ltime) <= (" + reqHistoryDataObj.toYear + ", " + reqHistoryDataObj.toTime + ") order by asigno desc, dlyear desc, ltime desc, seqno desc;");
                        }
                        else
                        {
                            queries.Add("select avoltage, ltime, dlyear, asigno from angdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[0].ToString("yyyy-MM-dd") + "' and asigno in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime) >= (" + reqHistoryDataObj.fromYear + ", " + reqHistoryDataObj.fromTime + ") order by asigno desc, dlyear desc, ltime desc, seqno desc;");                            
                            for (int dateIndex = 1; dateIndex < queryingDates.Length - 1; dateIndex++)
                                queries.Add("select avoltage, ltime, dlyear, asigno from angdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[dateIndex].ToString("yyyy-MM-dd") + "' order by asigno desc, dlyear desc, ltime desc, seqno desc;");
                            queries.Add("select avoltage, ltime, dlyear, asigno from angdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[queryingDates.Length - 1].ToString("yyyy-MM-dd") + "' and asigno in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime) <= (" + reqHistoryDataObj.toYear + ", " + reqHistoryDataObj.toTime + ") order by asigno desc, dlyear desc, ltime desc, seqno desc;");                            
                        }
                        //by D. Venkata Naresh- dbReader = dbModule.getDBData(query);
                        resHistoryData.analog.keys[0] = "aVoltage";
                        resHistoryData.analog.keys[1] = "ltime";
                        resHistoryData.analog.keys[2] = "dlYear";
                        //by D. Venkata Naresh- hisangValues tempangValues = new hisangValues();
                        hisangValues tempangValues = null;
                        //by D. Venkata Naresh- if (dbReader.HasRows == true)
                        List<Row> rows = dbModuleObj.postQuery(queries);
                        if (rows.Count > 0)
                        {
                            dataFound = true;
                            //by D. Venkata Naresh- 
                           /* while (dbReader.Read())
                            {
                                presSigno = Convert.ToInt64(dbReader.GetValue(3));
                                if (prevSigno == presSigno)
                                {
                                    //don't create new angValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                    ;
                                }
                                else
                                {
                                    //create bew angValues object iff signo changes
                                    tempangValues = new hisangValues();
                                    prevSigno = presSigno;
                                    resHistoryData.analog.values.Add(tempangValues);
                                }
                                tempangValues.asigNo = presSigno;
                                ArrayList tempValues = new ArrayList();
                                tempValues.Add(Convert.ToSingle(dbReader.GetValue(0)));
                                tempValues.Add(Convert.ToInt64(dbReader.GetValue(1)));
                                tempValues.Add(Convert.ToInt32(dbReader.GetValue(2)));
                                tempangValues.values.Add(tempValues);
                            }*/
                            
                            foreach (var row in rows)
                            {
                                presSigno = row.GetValue<Int32>("asigno");
                                if (prevSigno == presSigno)
                                {
                                    //don't create new angValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                    ;
                                }
                                else
                                {
                                    tempangValues = null;
                                    //Search for angValues Obj having signo same as presSigNo;
                                    foreach (var tempObj in resHistoryData.analog.values)
                                    {
                                        if (tempObj.asigNo == presSigno)
                                        {
                                            tempangValues = tempObj;
                                            break;
                                        }
                                    }
                                    if (tempangValues == null)
                                    {
                                        //create new digValues object if object not exist
                                        tempangValues = new hisangValues();
                                        resHistoryData.analog.values.Add(tempangValues);
                                    }
                                    prevSigno = presSigno;
                                }
                                tempangValues.asigNo = presSigno;
                                ArrayList tempValues = new ArrayList();
                                tempValues.Add(row.GetValue<Single>("avoltage"));
                                tempValues.Add(row.GetValue<Int32>("ltime"));
                                tempValues.Add(row.GetValue<Int32>("dlyear"));
                                tempangValues.values.Add(tempValues);
                            }
                            
                        }
                        presSigno = -1;
                        prevSigno = -1;
                        //by D. Venkata Naresh- dbReader.Dispose();

                        //getting digital data
                        //by D. Venkata Naresh- query = "select dsigstatus, ltime, dlyear from digdata" + reqHistoryDataObj.DLID + " where (ltime >= " + reqHistoryDataObj.fromTime + " and dlyear = " + reqHistoryDataObj.fromYear + ")"
                        //by D. Venkata Naresh-        + " or (ltime <= " + reqHistoryDataObj.toTime + " and dlyear = " + reqHistoryDataObj.toYear + ") order by signo, ltime desc";
                        queries.Clear();
                        if (queryingDates.Length == 1)
                        {
                            queries.Add("select dsigstatus, ltime, dlyear, signo from digdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[0].ToString("yyyy-MM-dd") + "' and signo in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime) >= (" + reqHistoryDataObj.fromYear + ", " + reqHistoryDataObj.fromTime + ") and (dlyear,ltime) <= (" + reqHistoryDataObj.toYear + ", " + reqHistoryDataObj.toTime + ") order by signo desc, dlyear desc, ltime desc, seqno desc;");
                        }
                        else
                        {
                            queries.Add("select dsigstatus, ltime, dlyear, signo from digdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[0].ToString("yyyy-MM-dd") + "' and signo in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime) >= (" + reqHistoryDataObj.fromYear + ", " + reqHistoryDataObj.fromTime + ") order by signo desc, dlyear desc, ltime desc, seqno desc;");
                            for (int dateIndex = 1; dateIndex < queryingDates.Length - 1; dateIndex++)
                                queries.Add("select dsigstatus, ltime, dlyear, signo from digdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[dateIndex].ToString("yyyy-MM-dd") + "' order by signo desc, dlyear desc, ltime desc, seqno desc;");
                            queries.Add("select dsigstatus, ltime, dlyear, signo from digdata" + reqHistoryDataObj.DLNO + " where date = '" + queryingDates[queryingDates.Length - 1].ToString("yyyy-MM-dd") + "' and signo in (0,1,2,3,4,5,6,7,8,9,10) and (dlyear,ltime) <= (" + reqHistoryDataObj.toYear + ", " + reqHistoryDataObj.toTime + ") order by signo desc, dlyear desc, ltime desc, seqno desc;");
                        }
                        //by D. Venkata Naresh- dbReader = dbModule.getDBData(query);
                        resHistoryData.digital.keys[0] = "dSigStatus";
                        resHistoryData.digital.keys[1] = "ltime";
                        resHistoryData.digital.keys[2] = "dlYear";
                        //by D. Venkata Naresh- hisdigValues tempdigValues = new hisdigValues();
                        hisdigValues tempdigValues = null;
                        //by D. Venkata Naresh- if (dbReader.HasRows == true)
                        rows = dbModuleObj.postQuery(queries);
                        if (rows.Count > 0)
                        {
                            dataFound = true;
                            //by D. Venkata Naresh- 
                            /*while (dbReader.Read())
                            {
                                presSigno = Convert.ToInt64(dbReader.GetValue(3));
                                if (prevSigno == presSigno)
                                {
                                    //don't create new digValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                    ;
                                }
                                else
                                {
                                    //create bew angValues object iff signo changes
                                    tempdigValues = new hisdigValues();
                                    prevSigno = presSigno;
                                    resHistoryData.digital.values.Add(tempdigValues);
                                }
                                tempdigValues.sigNo = presSigno;
                                ArrayList tempValues = new ArrayList();
                                tempValues.Add(Convert.ToSingle(dbReader.GetValue(0)));
                                tempValues.Add(Convert.ToInt64(dbReader.GetValue(1)));
                                tempValues.Add(Convert.ToInt32(dbReader.GetValue(2)));
                                tempdigValues.values.Add(tempValues);
                            }*/
                            
                            foreach (var row in rows)
                            {
                                presSigno = row.GetValue<Int32>("signo");
                                if (prevSigno == presSigno)
                                {
                                    //don't create new digValues object if sigNo doesn't changes, append values to already existing object with sigNo
                                    ;
                                }
                                else
                                {                                    
                                    tempdigValues = null;
                                    //Search for angValues Obj having signo same as presSigNo;
                                    foreach (var tempObj in resHistoryData.digital.values)
                                    {
                                        if (tempObj.sigNo == presSigno)
                                        {
                                            tempdigValues = tempObj;
                                            break;
                                        }
                                    }
                                    if (tempdigValues == null)
                                    {
                                        //create new digValues object if object not exist
                                        tempdigValues = new hisdigValues();
                                        resHistoryData.digital.values.Add(tempdigValues);
                                    }
                                    prevSigno = presSigno;
                                }
                                tempdigValues.sigNo = presSigno;
                                ArrayList tempValues = new ArrayList();
                                tempValues.Add(row.GetValue<Int32>("dsigstatus"));
                                tempValues.Add(row.GetValue<Int32>("ltime"));
                                tempValues.Add(row.GetValue<Int32>("dlyear"));
                                tempdigValues.values.Add(tempValues);
                            }
                            
                        }
                        presSigno = -1;
                        prevSigno = -1;
                        //by D. Venkata Naresh- dbReader.Dispose();
                    }
                }                

                //if data found is true then response code is 200 i.e success; otherwise it is 202 i.e no data found
                if (dataFound == true)
                {
                    resHistoryData.responseCode = 200;
                    //return historyData;
                    return jSearializer.Serialize(resHistoryData);                    
                }
                else
                {
                    //by D. Venkata Naresh-  resHistoryData.responseCode = 202;
                    error.responseCode = 202;
                    error.description = "No data found";
                    return jSearializer.Serialize(error);
                }                
                
            }
            catch (Exception e)
            {                
                if (e.Message.Contains("unconfigured table angdata") || e.Message.Contains("unconfigured table digdata"))
                {
                    error.responseCode = 203;
                    error.description = "Invalid DLNO";                    
                }
                else
                {
                    error.responseCode = 204;
                    error.description = e.Message;// "Unable to Process Request";
                }
                return jSearializer.Serialize(error);
            }
        }

        /*MySqlCommand SqlCommand;
        MySqlConnection ConnectionToEffe;
        MySqlDataReader DataFromDB;

        private float prevTemp;
        private float prevHumid;
        private float presentTemp;
        private float presentHumid;

        [HttpGet]
        [Route("nfc/data")]
        public string Get(string req)
        {
            if (!req.Equals(""))
            {
                try
                {
                   ConnectionToEffe = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MySqlConnection"].ToString());

                    //converting time present in client request into IST Datetime format and then into ltime
                    DateTime req_IST_DateTime = DateTimeConversions.strDateTimetoIST(req);
                    long req_ltime = DateTimeConversions.ISTtoLTime(req_IST_DateTime);

                    //fetching temperature value which is before requested time
                    ConnectionToEffe.Open();
                    SqlCommand = ConnectionToEffe.CreateCommand();
                    SqlCommand.CommandText = "SELECT AVOLTAGE FROM angdata426 WHERE LTIME = (SELECT MAX(LTIME) FROM angdata426 WHERE LTIME <= " + req_ltime + " AND ASIGNO = 0)";
                    DataFromDB = SqlCommand.ExecuteReader();

                    while (DataFromDB.Read())
                    {
                        prevTemp = DataFromDB.GetFloat(0);
                    }
                    DataFromDB.Close();
                    DataFromDB.Dispose();
                    ConnectionToEffe.Close();

                    //fetching humidity value which is before requested time
                    ConnectionToEffe.Open();
                    SqlCommand = ConnectionToEffe.CreateCommand();
                    SqlCommand.CommandText = "SELECT AVOLTAGE FROM angdata426 WHERE LTIME = (SELECT MAX(LTIME) FROM angdata426 WHERE LTIME <= " + req_ltime + " AND ASIGNO = 1)";
                    DataFromDB = SqlCommand.ExecuteReader();

                    while (DataFromDB.Read())
                    {
                        prevHumid = DataFromDB.GetFloat(0);
                    }
                    DataFromDB.Close();
                    DataFromDB.Dispose();
                    ConnectionToEffe.Close();

                    //fetching records from database with client requested criteria
                    ConnectionToEffe.Open();
                    SqlCommand = ConnectionToEffe.CreateCommand();
                    SqlCommand.CommandText = "SELECT DLYEAR, LTIME, ASIGNO, AVOLTAGE FROM angdata426 WHERE LTIME >" + req_ltime + " ORDER BY LTIME";
                    DataFromDB = SqlCommand.ExecuteReader();

                    List<Parameters> paramsList = new List<Parameters>();

                    //reading each record in fetched result
                    while (DataFromDB.Read())
                    {
                        //converting ltiem to IST and then GMT to send in response to the client
                        DateTime ltime_IST = DateTimeConversions.LtimetoIST(DataFromDB.GetInt32(1), DataFromDB.GetInt16(0));
                        string time_gmt = DateTimeConversions.ISTtoGMT(ltime_IST);

                        //if the current reading record is having Temperature then assign it as presentTemp else assign prevTemp
                        if (DataFromDB.GetInt16(2) == 0)
                            presentTemp = DataFromDB.GetFloat(3);
                        else
                            presentTemp = prevTemp;

                        //if the current reading record is having Humidity then assign it as presentHumid else assign prevHumid
                        if (DataFromDB.GetInt16(2) == 1)
                            presentHumid = DataFromDB.GetFloat(3);
                        else
                            presentHumid = prevHumid;

                        //adding object by framing it with the fetched record values
                        paramsList.Add(new Parameters()
                        {
                            humidity = Convert.ToString(presentHumid),
                            temperature = Convert.ToString(presentTemp),
                            timestamp = time_gmt
                        });

                        //assigning present parameters as previous parameters, for the purpose of next recrod
                        prevTemp = presentTemp;
                        prevHumid = presentHumid;
                    }
                    ConnectionToEffe.Close();
               
                    DataFromDB.Dispose();
                    ConnectionToEffe.Close();

                    string csv = "";                    
                    csv = csv + string.Join(",", "hunidity","temperature","timestamp");                    
                    csv = csv + Environment.NewLine;
                    foreach(Parameters objItem in paramsList)
                    {
                        csv = csv + string.Join(",", objItem.humidity, objItem.temperature, objItem.timestamp);
                        csv = csv + Environment.NewLine;
                        //csv.Replace(Environment.NewLine, "<br/>");
                        //csv = csv + "<br />";
                    }

         *          //from here to 
                    JavaScriptSerializer jSearializer = new JavaScriptSerializer();
                    return jSearializer.Serialize(paramsList);
                    
                    string csv = "";
                    //csv = string.Join(",", paramsList);
                    csv = string.Join(",", "Temperature ");
                    csv = csv + string.Join(",", "Hunidity ");
                    csv = csv + string.Join(",", "Timestamp ");
                    csv = csv + string.Join(", ",from item in paramsList select item.temperature);                    
                    csv = csv + string.Join(", ", from item in paramsList select item.humidity);                    
                    csv = csv + string.Join(", ", from item in paramsList select item.timestamp);
         * //to here
                    return csv;
                    
                }catch(Exception e)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            
        }*/
    }
}
