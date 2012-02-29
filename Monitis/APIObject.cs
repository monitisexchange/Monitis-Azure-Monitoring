using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

namespace Monitis
{
    public class APIObject
    {
        public string apiKey = "";
        public string secretKey = "";
        public string apiUrl = "";
        public string authToken = "";

        protected OutputType outputType = OutputType.JSON;

        /// <summary>
        /// Validation type for POST requests 
        /// Default value is HMACSHA1
        /// </summary>
        public Validation ValidationCurrent = Validation.HMACSHA1;

        /// <summary>
        /// Default apiUrl is http://monitis.com/api
        /// </summary>
        public APIObject()
        {
            apiUrl = H.UrlApi;
        }

        public APIObject(string apiKey, string secretKey, string apiUrl)
        {
            this.apiKey = apiKey;
            this.secretKey = secretKey;
            this.apiUrl = apiUrl;
        }

        /// <summary>
        /// Post request with specified params
        /// </summary>
        /// <param name="action">Action for post request</param>
        /// <param name="output">Type of output from server</param>
        /// <param name="parameters">Params for post request</param>
        /// <param name="validation">Type of request's validation on server</param>
        /// <returns>Responce from server</returns>
        public virtual RestResponse MakePostRequest(Enum action,OutputType output, Dictionary<string, string> parameters, Validation validation)                                        
        {
            DateTime curTime = H.DateTimeNowUtc;
            string formattedTime = curTime.ToString("yyyy-MM-dd HH:mm:ss");

            var requestParams = new Dictionary<string, string>();
            requestParams.Add(Params.action, action.ToString());
            requestParams.Add(Params.apikey, apiKey);
            requestParams.Add(Params.timestamp, formattedTime);
            requestParams.Add(Params.version, H.ApiVersion);
            requestParams.Add(Params.validation, validation.ToString());
            requestParams.Add(Params.output, output.ToString());
            if (parameters != null)
                PutAll(requestParams, parameters);

            //Order for put request
            requestParams = requestParams.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);

            var paramValueString = new StringBuilder();
            foreach (var reqParam in requestParams)
            {
                paramValueString.Append(reqParam.Key);
                paramValueString.Append(reqParam.Value);
            }

            if (Validation.HMACSHA1 == validation)
            {
                //secretkey for checksum
                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new Exceptions.SecretKeyIsNullOrEmptyException();
                }
                string checkSum = CalculateRFC2104HMAC(paramValueString.ToString(), secretKey);
                requestParams.Add(Params.checksum, checkSum);
            }
            else if (Validation.token == validation)
            {
                if (string.IsNullOrEmpty(authToken))
                {
                    throw new Exceptions.AuthTokenisNullOrEmptyException();
                }
                requestParams.Add(Params.authToken, authToken);
            }

            var restClient = new RestClient(apiUrl);
            var restRequest = new RestRequest(Method.POST);
            foreach (var reqParam in requestParams)
            {
                restRequest.AddParameter(reqParam.Key, reqParam.Value);
            }

            RestResponse responce = restClient.Execute(restRequest);
            return responce;
        }

        /// <summary>
        ///  Post request with current output (JSON by default)
        /// </summary>
        /// <param name="action">Action for post request</param>
        /// <param name="parameters">Params for post request</param>
        /// <param name="validation">Type of request's validation on server</param>
        /// <returns>Responce from server</returns>
        public virtual RestResponse MakePostRequest(Enum action,  Dictionary<string, string> parameters, Validation validation)
        {
            ValidationCurrent = validation;
            return MakePostRequest(action, parameters);
        }

        /// <summary>
        /// Post request with current validation (HMACSHA1 by default) 
        /// </summary>
        /// <param name="action">Action for post request</param>
        /// <param name="parameters">Params for post request</param>
        /// <param name="output">Type of output from server</param>
        /// <returns>Responce from server</returns>
        public virtual RestResponse MakePostRequest(Enum action, Dictionary<string, string> parameters, OutputType output)
        {
            outputType = output;
            return MakePostRequest(action, parameters);
        }

        /// <summary>
        /// Post request with current validation (HMACSHA1 by default) and current output (JSON by default)
        /// </summary>
        /// <param name="action">Action for post request</param>
        /// <param name="parameters">Params for post request</param>
        /// <returns>Responce in XML or JSON format</returns>
        public virtual RestResponse MakePostRequest(Enum action, Dictionary<string, string> parameters)
        {
            return MakePostRequest(action, outputType,parameters, ValidationCurrent);
        }

        private string CalculateRFC2104HMAC(string paramValueString, string secretKey)
        {
            string result = string.Empty;
            var myhmacsha1 = new HMACSHA1();
            myhmacsha1.Key = Encoding.ASCII.GetBytes(secretKey);
            byte[] sigBaseStrByteArr = Encoding.UTF8.GetBytes(paramValueString);
            byte[] hashValue = myhmacsha1.ComputeHash(sigBaseStrByteArr);
            result = Convert.ToBase64String(hashValue);
            return result;
        }

        public virtual RestResponse MakeGetRequest(Enum action, OutputType output, Dictionary<string, string> parameters = null)
        {
            Dictionary<string, string> requestParams = new Dictionary<string, string>();
            requestParams.Add(Params.action, action.ToString());
            requestParams.Add(Params.apikey, apiKey);
            requestParams.Add(Params.version, H.ApiVersion);
            requestParams.Add(Params.output, output.ToString());
            if (parameters != null)
                PutAll(requestParams, parameters);

            var restClient = new RestClient(apiUrl);
            var restRequest = new RestRequest(Method.GET);
            foreach (var reqParam in requestParams)
            {
                restRequest.AddParameter(reqParam.Key, reqParam.Value);
            }

            RestResponse responce = restClient.Execute(restRequest);
            return responce;
        }

        public virtual RestResponse MakeGetRequest(Enum action, Dictionary<string, string> parameters = null)
        {
            return MakeGetRequest(action, outputType, parameters);
        }

        /// <summary>
        /// Replace all items in dictionary1 with items in dictionary2 and adds items dictionary2 to dictionary1
        /// </summary>
        /// <param name="dictionary1"></param>
        /// <param name="dictionary2"></param>
        private static void PutAll(Dictionary<string, string> dictionary1, Dictionary<string, string> dictionary2)
        {
            foreach (KeyValuePair<string, string> keyValuePair in dictionary2)
            {
                if (dictionary1.ContainsKey(keyValuePair.Key))
                {
                    dictionary1.Remove(keyValuePair.Key);
                }
                dictionary1.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }
    }
}
