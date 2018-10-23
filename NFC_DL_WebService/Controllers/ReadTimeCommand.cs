using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NFC_DL_WebService.Controllers
{
    public class ReadTimeCommand : ApiController
    {
        public static string getReadTimeCmd()
        {
            /*byte[] readTimePack = new byte[22];

            readTimePack[0] = 0xAA;     //AA
            readTimePack[1] = 0xCC;     //CC
            readTimePack[2] = 0x00;     //length 2 bytes
            readTimePack[3] = 0x00;
            readTimePack[4] = 0x83;     //Type id 1 byte
            readTimePack[5] = 0x00;     //Source id 2 bytes
            readTimePack[6] = 0x00;
            readTimePack[7] = 0x00;     //dest id 2 bytes
            readTimePack[8] = 0x00;
            readTimePack[9] = 0x00;     //port no 1 byte
            readTimePack[10] = 0x00;    //seq no 1 byte       
            readTimePack[12] = 0x00;    //data 8 bytes
            readTimePack[13] = 0x00;    
            readTimePack[14] = 0x00;    
            readTimePack[15] = 0x00;
            readTimePack[16] = 0x00;    
            readTimePack[17] = 0x00;
            readTimePack[18] = 0x00;
            readTimePack[19] = 0x00;
            readTimePack[20] = 0x00;    //CRC 2 bytes
            readTimePack[21] = 0x00;*/

            //CRC for 000083000000000000000000000000000000 is E03C
            //string readCmd = "AACC000083000000000000000000000000000000E03C";
            //string readCmd = "AACC00118300000000000000000000000000003C19";
            string readCmd =   "AACC0009830000000000002A72";

            return readCmd;
        }
    }
}
