using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitis
{
    public static class Params
    {
        public static readonly string action = "action";
        public static readonly string apikey = "apikey";
        public static readonly string timestamp = "timestamp";
        public static readonly string version = "version";
        public static readonly string checksum = "checksum";
        public static readonly string authToken = "authToken";
        public static readonly string output = "output";
        public static readonly string userName = "userName";
        public static readonly string password = "password";
        public static readonly string secretkey = "secretkey";
        public static readonly string validation = "validation";
        /// <summary>
        /// "error" responces from server - for detecting errors
        /// </summary>
        public static readonly string error = "error";
    }
}
