using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using MongoDB.Bson;


namespace KissTracker.Entity
{
    
    public partial class VisitorSession
    {

        public BsonObjectId _id = new ObjectId();
        public System.String VisitorSessionAspSessionId = string.Empty;
        public System.String VisitorSessionReferralUrl = string.Empty;
        public System.String VisitorSessionRequestUrl = string.Empty;
        public System.String VisitorSessionRequestUrlQueryString = string.Empty;
        public System.DateTime VisitorSessionCreateDate = DateTime.Now;

    }

}

