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
            AuthenticationTest("qwe1234`");
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

            var res= apiObject.MakePostRequest(Actions.addSubAccount, parameters, Validation.HMACSHA1);
            Console.WriteLine(res.Content);
            res = apiObject.MakePostRequest(Actions.addSubAccount, parameters, Validation.token);
            Console.WriteLine(res.Content);

            Console.WriteLine("END!");
            Console.ReadKey();
        }

        private static void AuthenticationTest(string pass)
        {
            string userName = "stas.n.volkov@gmail.com";
            Authentication authentication = new Authentication();
            authentication.Authenticate(userName,pass,OutputType.XML);

            Console.WriteLine(authentication.apiKey);
            Console.WriteLine(authentication.secretKey);
            Console.WriteLine(authentication.authToken);
            Console.WriteLine(authentication.GetUserKey(userName,pass));

            authentication.Authenticate(userName, pass, OutputType.JSON);

            Console.WriteLine(authentication.apiKey);
            Console.WriteLine(authentication.secretKey);
            Console.WriteLine(authentication.authToken);
            Console.WriteLine(authentication.GetUserKey(userName, pass));

            Console.WriteLine("END!");
            Console.ReadKey();
        }
    }
}
