using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Monitis.Structures
{
    //[XmlRoot(ElementName = "contacts")] TODO: use attribute instead param in method Helper.Xml.DeserializeObject xmlRoot
    [Serializable]
    [XmlType(TypeName = "contact", Namespace = "")]
    public struct Contact
    {
        public int contactId;
        public int newsletterFlag;
        public string name;
        public Monitis.Contact.ContactType contactType;
        public string contactAccount;
        public int timezone;
        public bool portable;
        public int activeFlag;
        public int textType;
        public int confirmationFlag;
        public string country;
    }

    //[XmlRoot(ElementName = "contactgroups")] TODO: use attribute instead param in method Helper.Xml.DeserializeObject xmlRoot
    [Serializable]
    [XmlType(TypeName = "group", Namespace = "")]
    public struct ContactGroup
    {
        public int id;
        public int activeFlag;
        public string name;
    }

    //[XmlRoot(ElementName = "recentAlerts")] TODO: use attribute instead param in method Helper.Xml.DeserializeObject xmlRoot
    [Serializable]
    [XmlType(TypeName = "alert", Namespace = "")]
    public struct Alert
    {

        /// <summary>
        /// type of the monitor 
        /// </summary>
        public string dataType;

        /// <summary>
        /// time of the recovery 
        /// </summary>
        public DateTime recDate;

        /// <summary>
        /// id of the monitor 
        /// </summary>
        public int dataId;

        /// <summary>
        /// time of the failure 
        /// </summary>
        public DateTime failDate;

        /// <summary>
        /// id of the monitor type 
        /// </summary>
        public int dataTypeId;

        /// <summary>
        /// comma separated contact accounts, that alerts have been sent to 
        /// TODO: separate to array?
        /// </summary>
        public string contacts;

        /// <summary>
        /// name of the monitor 
        /// </summary>
        public string dataName;
    }

    /// <summary>
    /// Contains status field
    /// </summary>
    [Serializable]
    public struct AlertJson
    {
        public string status;
        public Alert[] data;
    }

    public struct AddContactResponce
    {
        /// <summary>
        /// "ok" if no error occurs, else the status will contain the error message
        /// </summary>
        public string status;

        public AddContact data;
    }

     [Serializable]
    public struct AddContact
     {
         /// <summary>
         /// key for confirmation of this contact. Used in confirmContact API action
         /// </summary>
         public string confirmationKey;

         /// <summary>
         ///  id of the created contact
         /// </summary>
         public int contactId;
     }
}
