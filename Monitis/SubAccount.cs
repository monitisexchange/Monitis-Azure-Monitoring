using System;
using System.Collections.Generic;
using RestSharp;

namespace Monitis
{
    public class SubAccount : APIObject
    {

        public enum SubAccountAction
        {
            subAccounts,
            subAccountPages,
            addSubAccount,
            deleteSubAccount,
            addPagesToSubAccount,
            deletePagesFromSubAccount
        }

        public SubAccount()
        {
        }

        public SubAccount(string apiKey, string secretKey): base(apiKey, secretKey)
        {
        }

        public virtual RestResponse GetSubAccounts(OutputType output)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.output, output);
            return MakeGetRequest(SubAccountAction.subAccounts, parameters);
        }

        public virtual RestResponse GetSubAccountPages(OutputType output)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.output, output);
            return MakeGetRequest(SubAccountAction.subAccountPages, parameters);
        }

        public virtual RestResponse AddSubAccount(string firstName, string lastName, string email, string password,
                                                  string group)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.firstName, firstName);
            parameters.Add(Params.lastName, lastName);
            parameters.Add(Params.email, email);
            parameters.Add(Params.password, password);
            parameters.Add(Params.group, group);
            return MakePostRequest(SubAccountAction.addSubAccount, parameters);
        }

        public virtual RestResponse DeleteSubAccount(int userId)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.userId, userId);
            return MakePostRequest(SubAccountAction.deleteSubAccount, parameters);
        }

        public virtual RestResponse AddPagesToSubAccount(int userId, string[] pageNames)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.userId, userId);
            parameters.Add(Params.pageNames, string.Join(Helper.DataSeparator, pageNames));
            return MakePostRequest(SubAccountAction.addPagesToSubAccount, parameters);
        }

        public virtual RestResponse DeletePagesFromSubAccount(int userId, string[] pageNames)
        {
            var parameters = new Dictionary<string, object>();
            parameters.Add(Params.userId, userId);
            parameters.Add(Params.pageNames, string.Join(Helper.DataSeparator, pageNames));
            return MakePostRequest(SubAccountAction.deletePagesFromSubAccount, parameters);
        }
    }
}