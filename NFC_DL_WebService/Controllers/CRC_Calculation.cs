using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NFC_DL_WebService.Controllers
{
    public class CRC_Calculation : ApiController
    {
        public static int polynomial = 0x1021; // Represents  // x^16+x^12+x^5+1

        int crc;

        public void CRC()
        {
            crc = 0x0000;
        }

        public int getCRC()
        {
            return crc;
        }

        public string getCRCHexString()
        {
            //String crcHexString = Integer.toHexString(crc);
            string crcHexString = Convert.ToString(crc);
            return crcHexString;
        }

        public void resetCRC()
        {
            crc = 0xFFFF;
        }

        public static ushort update(byte[] buffer)
        {
            ushort crc = 0;
            //for (byte b : args)
            foreach (byte b in buffer)
            {
                crc = (ushort)(crc ^ (((b & 0xff) << 8) & 0xffff));
                crc = (ushort)(crc & 0xffff);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0)
                    {
                        crc = (ushort)(((crc << 1) & 0xffff) ^ polynomial);
                    }
                    else
                    {
                        crc = (ushort)((crc & 0xffff) << 1);
                    }
                    crc = (ushort)(crc & 0xffff);
                }

            }
            return crc;
        }
    }
}
