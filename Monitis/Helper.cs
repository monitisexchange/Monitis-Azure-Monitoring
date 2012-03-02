using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Monitis
{
    /// <summary>
    /// Type of output - xml or json
    /// </summary>
    public enum OutputType { XML, JSON }

    /// <summary>
    /// Type of server validation requests - HMACSHA1 or token
    /// </summary>
    public enum Validation
    {
        HMACSHA1,
        token
    }

    /// <summary>
    /// Helper class
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// http://monitis.com by default
        /// </summary>
        private static string UrlServer = "http://monitis.com";
        /// <summary>
        /// http://monitis.com/customMonitorApi by default
        /// </summary>
        public static readonly string UrlCustomMonitorApi = UrlServer + @"/customMonitorApi";
        /// <summary>
        /// http://monitis.com/api by default
        /// </summary>
        public static readonly string UrlApi = UrlServer + @"/api";
        /// <summary>
        /// Current version = 2
        /// </summary>
        public static readonly string ApiVersion = "2";
        /// <summary>
        /// ";" - separator
        /// </summary>
        public static readonly string DataSeparator = ";";

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

        public static T DeserializeObject<T>(string content, OutputType outputType, string xmlRoot=null)
        {
            T result = default(T);
            if (OutputType.JSON == outputType)
            {
                result = Json.DeserializeObject<T>(content);
            }
            else if (OutputType.XML == outputType)
            {
                result = Xml.DeserializeObject<T>(content,xmlRoot);
            }
            return result;
        }

        public static T DeserializeObject<T>(RestResponse response, OutputType outputType, string xmlRoot=null)
        {
            return DeserializeObject<T>(response.Content, outputType, xmlRoot);
        }

        /// <summary>
        /// Gets value of key from json or xml string
        /// </summary>
        /// <param name="response">Response of REST service</param>
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
        /// <param name="response">Response of REST service</param>
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
            public static T DeserializeObject<T>(string content)
            {
                return JsonConvert.DeserializeObject<T>(content);
            }

            public static T DeserializeObject<T>(RestResponse response)
            {
                return DeserializeObject<T>(response.Content);
            }

            #region GetValueOfKey
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
            #endregion
        }

        /// <summary>
        /// Helper for Xml
        /// </summary>
        public static class Xml
        {
            public static T DeserializeObject<T>(string content, string xmlRoot)
            {
                XmlSerializer xmlSerializer;
                if (xmlRoot != null)
                    xmlSerializer = new XmlSerializer(typeof(T),new XmlRootAttribute(xmlRoot));
                else
                    xmlSerializer = new XmlSerializer(typeof (T));
                return (T) xmlSerializer.Deserialize(new StringReader(content));
            }

            public static T DeserializeObject<T>(RestResponse response, string xmlRoot)
            {
                return DeserializeObject<T>(response.Content, xmlRoot);
            }

            public static string GetValueOfKey(string content, string key)
            {
                string result = string.Empty;
                var doc = XDocument.Parse(content);
                var error = (GetElementWithName(doc.Elements(), Params.error));
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
