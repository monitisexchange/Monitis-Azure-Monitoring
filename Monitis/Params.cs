using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monitis
{
    internal static class Params
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
        /// "error" responses from server - for detecting errors
        /// </summary>
        public static readonly string error = "error";

        public static readonly string confirmationKey = "confirmationKey";
        public static readonly string contactId = "contactId";
        public static readonly string account = "account";
        public static readonly string contactType = "contactType";
        public static readonly string code = "code";
        public static readonly string portable = "portable";
        public static readonly string textType = "textType";
        public static readonly string country = "country";
        public static readonly string timezone = "timezone";
        public static readonly string @group = "group";
        public static readonly string lastName = "lastName";
        public static readonly string firstName = "firstName";
        public static readonly string limit = "limit";
        public static readonly string endDate = "endDate";
        public static readonly string startDate = "startDate";
        public static readonly string email = "email";
        public static readonly string userId = "userId";
        public static readonly string pageNames = "pageNames";
        /// <summary>
        /// Root element in xml response Contact.GetContacts method
        /// </summary>
        public static readonly string contacts = "contacts";
        /// <summary>
        /// Root element in xml response Contact.GetContactGroups method
        /// </summary>
        public static readonly string contactgroups = "contactgroups";
        /// <summary>
        /// Root element in xml response Contact.GetRecentAlerts method
        /// </summary>
        public static readonly string recentAlerts = "recentAlerts";

        public static readonly string result = "result";
        public static readonly string ok = "ok";
    }
}
