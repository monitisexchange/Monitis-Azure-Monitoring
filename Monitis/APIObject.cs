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

        public enum Validation
        {
            HMACSHA1,
            token
        }

        /// <summary>
        /// Validation type for POST requests 
        /// Default value is HMACSHA1
        /// </summary>
        public Validation CurrentValidation = Validation.HMACSHA1;

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


        public virtual RestResponse MakePostRequest(Enum action, Dictionary<string, string> parameters,
                                                    Validation validation)
        {
            DateTime curTime = H.DateTimeNowUtc;
            string formattedTime = curTime.ToString("yyyy-MM-dd HH:mm:ss");

            var reqParams = new Dictionary<string, string>();
            reqParams.Add(Params.action, action.ToString());
            reqParams.Add(Params.apikey, apiKey);
            reqParams.Add(Params.timestamp, formattedTime);
            reqParams.Add(Params.version, H.ApiVersion);
            reqParams.Add(Params.validation, validation.ToString());
            if (parameters != null)
                PutAll(reqParams, parameters);

            //Order for put request
            reqParams = reqParams.OrderBy(r => r.Key).ToDictionary(r => r.Key, r => r.Value);

            var paramValueStr = new StringBuilder();
            foreach (var reqParam in reqParams)
            {
                paramValueStr.Append(reqParam.Key);
                paramValueStr.Append(reqParam.Value);
            }

            if (Validation.HMACSHA1 == validation)
            {
                //secretkey for checksum
                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new Exceptions.SecretKeyIsNullOrEmptyException();
                }
                string checkSum = CalculateRFC2104HMAC(paramValueStr.ToString(), secretKey);
                reqParams.Add(Params.checksum, checkSum);
            }
            else if (Validation.token == validation)
            {
                if (string.IsNullOrEmpty(authToken))
                {
                    throw new Exceptions.AuthTokenisNullOrEmptyException();
                }
                reqParams.Add(Params.authToken, authToken);
            }

            var restClient = new RestClient(apiUrl);
            var restRequest = new RestRequest(Method.POST);
            foreach (var reqParam in reqParams)
            {
                restRequest.AddParameter(reqParam.Key, reqParam.Value);
            }

            RestResponse responce = restClient.Execute(restRequest);
            return responce;
        }

        public virtual RestResponse MakePostRequest(Enum action, Dictionary<string, string> parameters)
        {
            return MakePostRequest(action, parameters, CurrentValidation);
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

        public virtual RestResponse MakeGetRequest(Enum action, OutputType output = OutputType.JSON,
                                                   Dictionary<string, string> @params = null)
        {
            Dictionary<string, string> reqParams = new Dictionary<string, string>();
            reqParams.Add(Params.action, action.ToString());
            reqParams.Add(Params.apikey, apiKey);
            reqParams.Add(Params.version, H.ApiVersion);
            reqParams.Add(Params.output, output.ToString());
            if (@params != null)
                PutAll(reqParams, @params);

            var restClient = new RestClient(apiUrl);
            var restRequest = new RestRequest(Method.GET);
            foreach (var reqParam in reqParams)
            {
                restRequest.AddParameter(reqParam.Key, reqParam.Value);
            }

            RestResponse responce = restClient.Execute(restRequest);
            return responce;
        }

        public virtual RestResponse MakeGetRequest(string apiKey, Enum action,
                                                   OutputType output = OutputType.JSON,
                                                   Dictionary<string, string> @params = null)
        {
            this.apiKey = apiKey;
            return MakeGetRequest(action, output, @params);
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
