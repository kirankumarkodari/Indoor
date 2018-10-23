using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;

namespace NFC_DL_WebService.Models
{
    public class RequestAndResponse
    {
    }
    //configuration data request and response DTOs   
    public class ReqConfigDTO
    {
        public string siteName { get; set; }
    }
    public class ResConfigDTO
    {
        public int responseCode { get; set; }
        public string siteCode { get; set; }
        public int dlNo { get; set; }
        public string lastUpdate { get; set; }
        public Channels channels = new Channels();
    }
    public class Channels
    {
        public List<ArrayList> Analog = new List<ArrayList>();
        public List<ArrayList> Digital = new List<ArrayList>();
    }    

    //request and response objects for online data
    public class ReqOnlineDataDTO
    {
        public int fromTime { get; set; }
        public int fromYear { get; set; }
        public int DLNO { get; set; }
        public int seqNo { get; set; }
    }
    public class ResOnlineDataDTO
    {
        public int responseCode { get; set; }        
        public onlineAngChannels analog = new onlineAngChannels();
        public onlineDigChannels digital = new onlineDigChannels();
    }
    public class onlineAngChannels
    {
        public string[] keys = new string[4];        
        public List<angValues> values = new List<angValues>();              
    }
    public class angValues
    {
        public long asigNo { get; set; }
        public List<ArrayList> values = new List<ArrayList>();
    }
    public class onlineDigChannels
    {
        public string[] keys = new string[4];        
        public List<digValues> values = new List<digValues>();              
    }
    public class digValues
    {
        public long sigNo { get; set; }
        public List<ArrayList> values = new List<ArrayList>();
    }

    //request and response objects for history data
    public class ReqHistoryDataDTO
    {
        public int fromTime { get; set; }
        public int fromYear { get; set; }
        public int toTime { get; set; }
        public int toYear { get; set; }
        public int DLNO { get; set; }
    }
    public class ResHistoryDataDTO
    {
        public int responseCode { get; set; }
        public historyAngChannels analog = new historyAngChannels();
        public historyDigChannels digital = new historyDigChannels();
    }
    public class historyAngChannels
    {
        public string[] keys = new string[3];
        public List<hisangValues> values = new List<hisangValues>();
    }
    public class hisangValues
    {
        public long asigNo { get; set; }
        public List<ArrayList> values = new List<ArrayList>();
    }
    public class historyDigChannels
    {
        public string[] keys = new string[3];
        public List<hisdigValues> values = new List<hisdigValues>();
    }
    public class hisdigValues
    {
        public long sigNo { get; set; }
        public List<ArrayList> values = new List<ArrayList>();
    }

    //response object for Error
    public class ResErrorDTO
    {
        public int responseCode { get; set; }
        public string description { get; set; }
    }

    //for testing
    public class response
    {
        public string response_code {get;set;}
        public List<analog> ang = new List<analog>();
        public List<digital> dig = new List<digital>();        
        public keys keysObj = new keys();

    }
    public class keys
    {
        public string Name = "Name";
        public string ID = "ID";
    }
    public class analog
    {        
        public int id;
        public string name;
    }
    public class digital
    {
        public int id { get; set; }
        public string name { get; set; }        
    }

}