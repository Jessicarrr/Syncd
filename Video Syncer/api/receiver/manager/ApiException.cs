using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Video_Syncer.api.receiver.manager
{
    public class ApiException : Exception
    {
        public int statusCode { get; set; }
        public string content { get; set; }

        public ApiException(int statusCode, string content)
        {
            this.statusCode = statusCode;
            this.content = content;
        }
    }
}
