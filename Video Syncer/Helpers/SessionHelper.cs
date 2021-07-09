using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncd.Helpers
{
    public class SessionHelper
    {
        private static string _key = "gdka9g8kda8g";
        private static string _value = "test";

        public static void SetupSession(HttpContext context)
        {
            if(!SessionHasKey(context))
            {
                context.Session.SetString(_key, _value);
            }
        }

        private static bool SessionHasKey(HttpContext context)
        {
            if(context.Session.GetString(_key) == null)
            {
                return false;
            }
            return true;
        }
    }
}
