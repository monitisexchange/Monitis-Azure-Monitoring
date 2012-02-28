using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Monitis
{
    /// <summary>
    /// Type of output - xml or json
    /// </summary>
    public enum OutputType { XML, JSON }

    /// <summary>
    /// Helper class
    /// </summary>
    public static class H
    {
        /// <summary>
        /// http://monitis.com by default
        /// </summary>
        private static string UrlServer = "http://monitis.com";
        /// <summary>
        /// http://monitis.com/customMonitorApi by default
        /// </summary>
        public static string UrlCustomMonitorApi = UrlServer + @"/customMonitorApi";
        /// <summary>
        /// http://monitis.com/api by default
        /// </summary>
        public static string UrlApi = UrlServer + @"/api";
        public const string ApiVersion = "2";

        /// <summary>
        /// "error" - for detecting errors
        /// </summary>
        private const string errorResponce = "error";

        public static DateTime DateTimeNowUtc
        {
            get
            {
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.Now);
            }
        }

        /// <summary>
        /// Gets value of key from json or xml string
        /// </summary>
        /// <param name="content">Content of json or xml</param>
        /// <param name="key">Finds value of this key</param>
        /// <param name="outputType">XML of JSON</param>
        /// <returns>Value of key</returns>
        public static string GetValueOfKey(string content, string key, OutputType outputType)
        {
            string value = string.Empty;
            if (outputType == OutputType.JSON)
            {
                value = Json.GetValueOfKey(content, key);
            }
            else if (outputType == OutputType.XML)
            {
                value = Xml.GetValueOfKey(content, key);
            }
            return value;
        }

        /// <summary>
        /// Gets value of key from json or xml string
        /// </summary>
        /// <param name="response">Responce of REST service</param>
        /// <param name="key">Finds value of this key</param>
        /// <param name="outputType">XML of JSON</param>
        /// <returns>Value of key</returns>
        public static string GetValueOfKey(RestResponse response, string key, OutputType outputType)
        {
            return GetValueOfKey(response.Content, key,outputType);
        }

        /// <summary>
        /// Gets value of key from json or xml string
        /// </summary>
        /// <param name="response">Responce of REST service</param>
        /// <param name="key">Finds value of this key</param>
        /// <param name="outputType">XML of JSON</param>
        /// <returns>Value of key</returns>
        public static string GetValueOfKey(RestResponse response, Enum key, OutputType outputType)
        {
            return GetValueOfKey(response.Content, key.ToString(), outputType);
        }

        /// <summary>
        /// Helper for Json
        /// </summary>
        public static class Json
        {
            public static string GetValueOfKey (string content, string key)
            {
               return JObject.Parse(content)[key].ToString();
            }

            public static string GetValueOfKey(RestResponse response, string key)
            {
                return GetValueOfKey(response.Content,key);
            }

            public static string GetValueOfKey(RestResponse response, Enum key)
            {
                return GetValueOfKey(response.Content, key.ToString()); ;
            }
        }

        /// <summary>
        /// Helper for Xml
        /// </summary>
        public static class Xml
        {
            public static string GetValueOfKey(string content, string key)
            {
                string result = string.Empty;
                var doc = XDocument.Parse(content);
                var error = (GetElementWithName(doc.Elements(), errorResponce));
                if (null!=error)
                    throw new Exception(error.Value);
                result = GetElementWithName(doc.Elements(), key).Value;
                return result;
            }

            /// <summary>
            /// Search element with given name recursively 
            /// </summary>
            /// <param name="elements"></param>
            /// <param name="name"></param>
            /// <returns></returns>
            private static XElement GetElementWithName(IEnumerable<XElement> elements, string name)
            {
                //Search in parent node
                foreach (var element in elements)
                {
                    if (element.Name.LocalName == name)
                        return element;
                }
                //Search in child nodes
                foreach (var element in elements)
                {
                    return GetElementWithName(element.Elements(),name);
                }
                return null;
            }

            public static string GetValueOfKey(RestResponse response, string key)
            {
                return GetValueOfKey(response.Content, key);
            }

            public static string GetValueOfKey(RestResponse response, Enum key)
            {
                return GetValueOfKey(response.Content, key.ToString()); ;
            }
        }
    }

    
}
