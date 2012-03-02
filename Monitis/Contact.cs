using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Monitis
{
    public class Contact : APIObject
    {
        /// <summary>
        /// Types of contact (from doc)
        /// </summary>
        public enum ContactType
        {
            Email = 1,
            SMS = 2,
            ICQ = 3,
            Google = 7,
            Twitter = 8,
            PhoneCall = 9,
            SMSCall = 10,
            URL = 11
        }

        /// <summary>
        ///  Action names (from doc)
        /// </summary>
        public enum ContactAction
        {
            contactsList,
            contactGroupList,
            recentAlerts,
            addContact,
            editContact,
            deleteContact,
            confirmContact,
            contactActivate,
            contactDeactivate
        }

        public Contact(string apiKey, string secretKey)
            : base(apiKey, secretKey)
        {

        }

        /// <summary>
        /// Constructor without params. Dont forget to set tokens!
        /// </summary>
        public Contact()
        {

        }

        /// <summary>
        /// This action is used to get user's all contacts. 
        /// </summary>
        /// <param name="output">Type of output - XML or JSON</param>
        /// <returns>Contacts of current user</returns>
        public virtual Structures.Contact[] GetContacts(OutputType? output=null)
        {
            OutputType outputType = GetOutput(output);
            Structures.Contact[] contacts = null;
            RestResponse response = MakeGetRequest(ContactAction.contactsList,output: outputType);
            contacts = Helper.DeserializeObject<Structures.Contact[]>(response, outputType, Params.contacts);
            return contacts;
        }


        /// <summary>
        /// This action is used to get all groups of contacts of the user. 
        /// </summary>
        /// <param name="output">Type of output - XML or JSON</param>
        /// <returns>List of contact groups</returns>
        public virtual Structures.ContactGroup[] GetContactGroups(OutputType? output=null)
        {
            OutputType outputType = GetOutput(output);
            Structures.ContactGroup[] contactGroups = null;
            var parameters = new Dictionary<string, object>();
            RestResponse response = MakeGetRequest(ContactAction.contactGroupList, parameters, outputType);
            contactGroups = Helper.DeserializeObject<Structures.ContactGroup[]>(response, outputType, Params.contactgroups);
            return contactGroups;
        }

        /// <summary>
        /// This action is used to get recent alerts history. 
        /// TODO: add alerts and test
        /// </summary>
        /// <param name="output"></param>
        /// <param name="timezone">offset relative to GMT, used to retrieve results in the given timezone. The default value is 0</param>
        /// <param name="startDate">start date to get results for </param>
        /// <param name="endDate">last date to get results for </param>
        /// <param name="limit">number of alerts to get</param>
        /// <returns></returns>
        public virtual Structures.Alert[] GetRecentAlerts(int? timezone = null, long? startDate = null,
                                                          long? endDate = null, int? limit = null, OutputType? output = null)
        {
            OutputType outputType = GetOutput(output);
            Structures.Alert[] alerts = null;
            var parameters = new Dictionary<string, object>();
            AddIfNotNull(parameters, Params.timezone, timezone);
            AddIfNotNull(parameters, Params.startDate, startDate);
            AddIfNotNull(parameters, Params.endDate, endDate);
            AddIfNotNull(parameters, Params.limit, limit);
            RestResponse response = MakeGetRequest(ContactAction.recentAlerts, parameters, outputType);
            if (output == OutputType.XML)
                alerts = Helper.Xml.DeserializeObject<Structures.Alert[]>(response, Params.recentAlerts);
            else if (output == OutputType.JSON)
            {
                Structures.AlertJson alertsJson = Helper.Json.DeserializeObject<Structures.AlertJson>(response);
                //TODO: Check alertsJson.status and throw exception
                alerts = alertsJson.data;
            }
            return alerts;
        }

        /// <summary>
        /// This action is used to add a new contact. 
        /// </summary>
        /// <param name="firstName">first name of the contact </param>
        /// <param name="lastName">last name of the contact</param>
        /// <param name="account">account information</param>
        /// <param name="group">the group to which contact will be added.
        ///  A new group will be created if a group with such name doesn’t exist. </param>
        /// <param name="country">full name, 2 or 3 letter codes for the country. E.g. United States, US or USA</param>
        /// <param name="contactType">Types of contact</param>
        /// <param name="timezone">timezone offset from GMT in minute</param>
        /// <param name="textType">could be "true" to get plain text alerts or "false" to get HTML formatted alerts.</param>
        /// <param name="portable">is available only for "SMS" and "SMS and Call" contact types.
        ///  "true" if mobile number was moved from one operator to another under the 'number portability' system.</param>
        /// <param name="sendDailyReport">set to "true" to enable daily reports</param>
        /// <param name="sendWeeklyReport">	set to "true" to enable weekly reports</param>
        /// <param name="sendMonthlyReport">set to "true" to enable monthly reports</param>
        /// <returns>Recent alerts</returns>
        public virtual Structures.AddContact AddContact(string firstName, string lastName, string account,
                                               ContactType contactType, int timezone=0, string country = null,
                                               bool? textType = null, bool? portable = null, string group = null,
                                               bool? sendDailyReport = null, bool? sendWeeklyReport = null,
                                               bool? sendMonthlyReport = null, OutputType? output=null)
        {
            OutputType outputType = GetOutput(output);
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.firstName, firstName);
            parameters.Add(Params.lastName, lastName);
            parameters.Add(Params.account, account);
            parameters.Add(Params.contactType, (int)contactType); /*int type required*/
            parameters.Add(Params.timezone, timezone);
            AddIfNotNull(parameters, Params.group, group);
            AddIfNotNull(parameters, Params.country, country);
            AddIfNotNull(parameters, Params.textType, textType);
            AddIfNotNull(parameters, Params.portable, portable);
            RestResponse response = MakePostRequest(ContactAction.addContact, parameters, outputType);
            Structures.AddContactResponce addContactResponce =
                Helper.DeserializeObject<Structures.AddContactResponce>(response, outputType, Params.result);
            if (addContactResponce.status!=Params.ok)
                throw new Exception(addContactResponce.status);
            return addContactResponce.data;
        }

        public virtual RestResponse EditContact(int contactId, string firstName=null, string lastName=null, string account=null,
                                                string group = null, string country = null, ContactType? contactType = null, int? timezone = null,
                                                bool? textType = null, bool? portable = null, string code = null, OutputType? output = null)
        {
            OutputType outputType = GetOutput(output);
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.contactId, contactId);
            AddIfNotNull(parameters, Params.firstName, firstName);
            AddIfNotNull(parameters, Params.lastName, lastName);
            AddIfNotNull(parameters, Params.account, account);
            if (contactType!=null)
                AddIfNotNull(parameters, Params.contactType, (int)contactType); /*int type required*/
            AddIfNotNull(parameters, Params.group, group);
            AddIfNotNull(parameters, Params.timezone, timezone);
            AddIfNotNull(parameters, Params.country, country);
            AddIfNotNull(parameters, Params.textType, textType);
            AddIfNotNull(parameters, Params.portable, portable);
            AddIfNotNull(parameters, Params.code, code);
            RestResponse response= MakePostRequest(ContactAction.editContact, parameters, outputType);
            return response;
        }

        public virtual RestResponse DeleteContact(int? contactId, int? contactType, string account = null)
        {
            var parameters = new Dictionary<string, object>();
            AddIfNotNull(parameters, Params.contactId, contactId);
            AddIfNotNull(parameters, Params.contactType, contactType);
            AddIfNotNull(parameters, Params.account, account);
            return MakePostRequest(ContactAction.deleteContact, parameters);
        }


        public virtual RestResponse ConfirmContact(int contactId, string confirmationKey)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.contactId, contactId);
            parameters.Add(Params.confirmationKey, confirmationKey);
            return MakePostRequest(ContactAction.confirmContact, parameters);
        }

        public virtual RestResponse ActivateContact(int contactId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.contactId, contactId);
            return MakePostRequest(ContactAction.contactActivate, parameters);
        }

        public virtual RestResponse DeActivateContact(int contactId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.contactId, contactId);
            return MakePostRequest(ContactAction.contactDeactivate, parameters);
        }
    }
}