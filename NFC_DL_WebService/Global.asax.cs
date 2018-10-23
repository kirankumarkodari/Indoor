using NFC_DL_WebService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace NFC_DL_WebService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static string ABC { get; set; }
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
