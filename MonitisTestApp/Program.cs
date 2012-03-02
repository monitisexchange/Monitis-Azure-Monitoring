using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Monitis;
using RestSharp;

namespace MonitisTestApp
{
    class Program
    {
        private static string apiKey = "72VR17E1R8NVUL0DR0D2O4TF49";
        private static string sekretKey = "275SP14E12PLO0RRB12QF301CT ";
        private static string userName = "qwejake@mailforspam.com";
        private static string pass = "qwe1234`";

        static void Main(string[] args)
        {
            //AuthenticationTest();
            //PostTest();
            ContactTest();

            //Monitis.Structures.Contact[] contacts = new Monitis.Structures.Contact[]
            //{ new Monitis.Structures.Contact() { contactType = Contact.ContactType.ICQ } };
            //XmlSerializer serializer = new XmlSerializer(typeof(Monitis.Structures.Contact[]));

            //StringBuilder stringBuilder=new StringBuilder();
            //StringWriter stringWriter=new StringWriter(stringBuilder);
            //serializer.Serialize(stringWriter, contacts);
            //Console.WriteLine(stringBuilder.ToString());

            Console.WriteLine("END! Press any key to exit");
            Console.ReadKey();
        }

        private static void ContactTest()
        {
            Authentication authentication = AuthenticationTest();
            Contact contact = new Contact();
            contact.SetAuthenticationParams(authentication);
            contact.OutputGlobal = OutputType.JSON;
            contact.ValidationGlobal=Validation.token;
            RestResponse response;

            int contactIdToEdit = 0;

            var getContacts = contact.GetContacts(OutputType.XML);
            Console.WriteLine("GetContacts XML length: " + getContacts.Length);
            if (getContacts.Length > 0)
                contactIdToEdit = getContacts[0].contactId;
            getContacts = contact.GetContacts(OutputType.JSON);
            Console.WriteLine("GetContacts JSON length: " + getContacts.Length);

            var getContactGroups = contact.GetContactGroups(OutputType.JSON);
            Console.WriteLine("GetContactGroups JSON length: " + getContactGroups.Length);
            getContactGroups = contact.GetContactGroups(OutputType.XML);
            Console.WriteLine("GetContactGroups XML length: " + getContactGroups.Length);

            var getRecentAlerts = contact.GetRecentAlerts(output:OutputType.JSON);
            Console.WriteLine("GetRecentAlerts JSON length:" + getRecentAlerts.Length);
            getRecentAlerts = contact.GetRecentAlerts(output: OutputType.XML);
            Console.WriteLine("GetRecentAlerts XML length:" + getRecentAlerts.Length);

            //var AddContact = contact.AddContact("temp2", "lastName", "accf43123342@acc.ru", Contact.ContactType.Email,output: OutputType.JSON);
            //Console.WriteLine("AddContact JSON confirmationKey, status:" + AddContact.confirmationKey);
            //AddContact = contact.AddContact("temp3", "lastName", "accf4423123331@acc.ru", Contact.ContactType.Email, output: OutputType.XML);
            //Console.WriteLine("AddContact XML confirmationKey, status:" + AddContact.confirmationKey);

            var EditContact = contact.EditContact(contactIdToEdit,contactType:Contact.ContactType.Email);
            Console.WriteLine("EditContact "+EditContact.Content);
        }

        //TODO: test subAccounts
        //private static void Main(string[] args)
        //{
        //    try
        //    {

        //        SubAccount subaccount = new SubAccount();
        //        Response resp;
        //        resp = subaccount.addSubAccount("narika", "gasp", "narika@gmailik.com", "qqqqqq", "group1");
        //        Console.WriteLine(resp);
        //        //TODO: fix test
        //        //int? subaccountId = new JSONObject(resp.ResponseText).getJSONObject("data").getInt("userId");
        //        //resp = subaccount.getSubAccounts(OutputType.JSON);
        //        //Console.WriteLine(resp);
        //        //resp = subaccount.getSubAccountPages(OutputType.JSON);
        //        //Console.WriteLine(resp);
        //        //resp = subaccount.deleteSubAccount(subaccountId);
        //        //Console.WriteLine(resp);
        //        //resp = subaccount.addPagesToSubAccount(subaccountId, new string[] {"Util"});
        //        //Console.WriteLine(resp);
        //        //resp = subaccount.deletePagesFromSubAccount(subaccountId, new string[] {"Mysql"});
        //        //Console.WriteLine(resp);
        //    }
        //    catch (Exception e)
        //    {
        //        //e.printStackTrace();
        //    }
        //}

        public enum Actions
        {
            addSubAccount
        }

        private static void PostTest()
        {
            Authentication authentication=new Authentication();
            APIObject apiObject = new APIObject(apiKey, sekretKey);
            apiObject.authToken = authentication.GetAuthToken(apiKey, sekretKey, OutputType.JSON);
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("firstName", "firstName");
            parameters.Add("lastName", "lastName");
            parameters.Add("email", "email");
            parameters.Add("password", "password");
            parameters.Add("group", "group");

            var res= apiObject.MakePostRequest(Actions.addSubAccount, parameters,validation: Validation.HMACSHA1);
            Console.WriteLine(res.Content);
            res = apiObject.MakePostRequest(Actions.addSubAccount, parameters, validation: Validation.token);
            Console.WriteLine(res.Content);

            Console.WriteLine("END!");
            Console.ReadKey();
        }

        private static Authentication AuthenticationTest()
        {
           
            Authentication authentication = new Authentication();
            authentication.Authenticate(userName,pass,OutputType.XML);

            Console.WriteLine("XML apikey:"+authentication.apiKey);
            Console.WriteLine("XML secretKey:" + authentication.secretKey);
            Console.WriteLine("XML authToken:" + authentication.authToken);
            Console.WriteLine("XML userKey:" + authentication.GetUserKey(userName, pass));

            Console.WriteLine("JSON Test commented!");
            //authentication.Authenticate(userName, pass, OutputType.JSON);

            //Console.WriteLine("JSON apikey:" + authentication.apiKey);
            //Console.WriteLine("JSON secretKey:" + authentication.secretKey);
            //Console.WriteLine("JSON authToken:" + authentication.authToken);
            //Console.WriteLine("JSON userKey:" + authentication.GetUserKey(userName, pass));

            return authentication;
        }
    }
}
