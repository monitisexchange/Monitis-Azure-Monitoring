using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Monitis;
using RestSharp;

namespace MonitisTestApp
{
    class Program
    {
        private static string apiKey = "E1ARTRMEDJPMQ0HUVHHMIUOR5";
        private static string sekretKey = "1GJJCOE3PI8IUEFHTEU1SRO6JC";

        static void Main(string[] args)
        {
            //AuthenticationTest("*");
            PostTest();
        }

        public enum Actions
        {
            addSubAccount
        }

        private static void PostTest()
        {
            Authentication authentication=new Authentication();
            APIObject apiObject = new APIObject(apiKey, sekretKey, H.UrlApi);
            apiObject.authToken = authentication.GetAuthToken(apiKey, sekretKey, OutputType.JSON);
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("firstName", "firstName");
            parameters.Add("lastName", "lastName");
            parameters.Add("email", "email");
            parameters.Add("password", "password");
            parameters.Add("group", "group");

            var res= apiObject.MakePostRequest(Actions.addSubAccount, parameters, APIObject.Validation.HMACSHA1);
            Console.WriteLine(res.Content);
            res = apiObject.MakePostRequest(Actions.addSubAccount, parameters, APIObject.Validation.token);
            Console.WriteLine(res.Content);

            Console.WriteLine("END!");
            Console.ReadKey();
        }

        private static void AuthenticationTest(string pass)
        {
            string userName = "stas.n.volkov@gmail.com";
            Authentication authentication = new Authentication();

            var responce = authentication.GetApiKey(userName, pass, OutputType.XML);
            authentication.apiKey = responce;
            Console.WriteLine(responce);
            responce = authentication.GetApiKey(userName, pass, OutputType.JSON);
            Console.WriteLine(responce);

            responce = authentication.GetUserKey(userName, pass, OutputType.XML);
            Console.WriteLine(responce);
            responce = authentication.GetUserKey(userName, pass, OutputType.JSON);
            Console.WriteLine(responce);

            responce = authentication.GetAuthToken(apiKey, sekretKey, OutputType.XML);
            Console.WriteLine(responce);
            responce = authentication.GetAuthToken(apiKey, sekretKey, OutputType.JSON);
            Console.WriteLine(responce);

            responce = authentication.GetSecretKey(OutputType.XML);
            Console.WriteLine(responce);
            responce = authentication.GetAuthToken(apiKey, sekretKey, OutputType.JSON);
            Console.WriteLine(responce);

            Console.WriteLine("END!");
            Console.ReadKey();
        }
    }
}
