using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Monitis
{
    public class Authentication : APIObject
    {
        public enum AuthenticationAction
        {
            apikey,
            secretkey,
            userkey,
            authToken
        }

        #region Authentication constructors and methods

        /// <summary>
        /// Constructor wihtout params. Don't forget to call Authenticate to fill apikey, secretkey and authToken
        /// </summary>
        public Authentication()
        {

        }

        /// <summary>
        /// Authentication user by his name and password
        /// </summary>
        /// <param name="userName">Name of user</param>
        /// <param name="password">Password of user</param>
        /// <param name="output">Type of output. If not selected, uses global params (JSON is default). Otherwise, set global param</param>
        public Authentication(string userName, string password, OutputType? output=null)
        {
            OutputGlobal = GetOutput(output);
            Authenticate(userName, password);
        }

        /// <summary>
        /// Authentication by apiKey, secretKey (optional) and authToken (optional)
        /// </summary>
        /// <param name="apiKey">ApiKey of user</param>
        /// <param name="secretKey">SecretKey. If null, it will be get by ApiKey</param>
        /// <param name="authToken">AuthToken. If null, it will be get by ApiKey and SecretKey</param>
        public Authentication(string apiKey, string secretKey = null, string authToken = null)
        {
            this.apiKey = apiKey;
            if (string.IsNullOrEmpty(secretKey))
            {
                this.secretKey = GetSecretKey(OutputGlobal);
            }
            else
            {
                this.secretKey = secretKey;
            }
            if (string.IsNullOrEmpty(authToken))
            {
                this.authToken = GetAuthToken(apiKey, secretKey, OutputGlobal);
            }
            else
            {
                this.authToken = authToken;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="output">Set global output (or use exists - JSON by default)</param>
        public void Authenticate(string userName, string password,OutputType? output=null)
        {
            OutputGlobal = GetOutput(output);
            apiKey = GetApiKey(userName, password, OutputGlobal);
            secretKey = GetSecretKey(OutputGlobal);
            authToken = GetAuthToken(apiKey, secretKey, OutputGlobal);
        }

        #endregion

        private static string GetMD5Hash(string input)
        {
            var x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            var s = new StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            string password = s.ToString();
            return password;
        }

        public virtual string GetApiKey(string userName, string password, OutputType? output=null)
        {
            OutputType outputType = GetOutput(output);
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.userName, userName);
            parameters.Add(Params.password, GetMD5Hash(password));
            var response = MakeGetRequest(AuthenticationAction.apikey, parameters, outputType);
            return Helper.GetValueOfKey(response, AuthenticationAction.apikey, outputType);
        }

        public virtual string GetSecretKey(OutputType? output=null)
        {
            OutputType outputType = GetOutput(output);
            var response = MakeGetRequest(AuthenticationAction.secretkey, output: outputType);
            return Helper.GetValueOfKey(response, AuthenticationAction.secretkey, outputType);
        }

        public virtual string GetAuthToken(string apikey, string secretkey, OutputType? output=null)
        {
            OutputType outputType = GetOutput(output);
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.apikey, apikey);
            parameters.Add(Params.secretkey, secretkey);
            var response = MakeGetRequest(AuthenticationAction.authToken, parameters, outputType);
            return Helper.GetValueOfKey(response, AuthenticationAction.authToken, outputType);
        }

        public virtual string GetUserKey(string userName, string password, OutputType? output=null)
        {
            OutputType outputType = GetOutput(output);
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.userName, userName);
            parameters.Add(Params.password, password);
            var response = MakeGetRequest(AuthenticationAction.userkey, parameters, outputType);
            return Helper.GetValueOfKey(response, AuthenticationAction.userkey, outputType);
        }
    }
}
