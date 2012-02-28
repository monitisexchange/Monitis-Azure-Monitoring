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

        private OutputType outputType = OutputType.JSON;

        #region Constructors

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
        /// <param name="output">Type of output. JSON is default</param>
        public Authentication(string userName, string password, OutputType output = OutputType.JSON)
        {
            outputType = output;
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
                this.secretKey = GetSecretKey(outputType);
            }
            else
            {
                this.secretKey = secretKey;
            }
            if (string.IsNullOrEmpty(authToken))
            {
                this.authToken = GetAuthToken(apiKey, secretKey, outputType);
            }
            else
            {
                this.authToken = authToken;
            }
        }

        #endregion

        #region Helper methods

        public void Authenticate(string userName, string password)
        {
            apiKey = GetApiKey(userName, password, outputType);
            secretKey = GetSecretKey(outputType);
            authToken = GetAuthToken(apiKey, secretKey, outputType);
        }

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

        #endregion

        #region GetApiKey

        public virtual string GetApiKey(string userName, string password, OutputType output)
        {
            Dictionary<string, string> @params = new Dictionary<string, string>();
            @params.Add(Params.userName, userName);
            @params.Add(Params.password, GetMD5Hash(password));
            var response = MakeGetRequest(AuthenticationAction.apikey, output, @params);
            return H.GetValueOfKey(response, AuthenticationAction.apikey, output);
        }

        public virtual string GetApiKey(string userName, string password)
        {
            return GetApiKey(userName, password, outputType);
        }

        #endregion

        #region GetSecretKey

        public virtual string GetSecretKey(OutputType output)
        {
            var response = MakeGetRequest(AuthenticationAction.secretkey, output);
            return H.GetValueOfKey(response, AuthenticationAction.secretkey, output);
        }

        public virtual string GetSecretKey()
        {
            return GetSecretKey(outputType);
        }

        #endregion

        #region GetAuthToken

        public virtual string GetAuthToken(string apikey, string secretkey, OutputType output)
        {
            Dictionary<string, string> @params = new Dictionary<string, string>();
            @params.Add(Params.apikey, apikey);
            @params.Add(Params.secretkey, secretkey);
            var response = MakeGetRequest(AuthenticationAction.authToken, output, @params);
            return H.GetValueOfKey(response, AuthenticationAction.authToken, output);
        }

        public virtual string GetAuthToken(string apikey, string secretkey)
        {
            return GetAuthToken(apikey, secretkey, outputType);
        }

        #endregion

        #region GetUserKey

        public virtual string GetUserKey(string userName, string password, OutputType output)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add(Params.userName, userName);
            parameters.Add(Params.password, password);
            var responce = MakeGetRequest(AuthenticationAction.userkey, output, parameters);
            return H.GetValueOfKey(responce, AuthenticationAction.userkey, output);
        }

        public virtual string GetUserKey(string userName, string password)
        {
            return GetUserKey(userName, password, outputType);
        }

        #endregion
    }
}
