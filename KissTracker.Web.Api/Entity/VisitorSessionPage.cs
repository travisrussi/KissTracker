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
    
    public partial class VisitorSessionPage
    {

        public BsonObjectId _id = new ObjectId();
        public BsonObjectId VisitorSessionId = null;
        public System.String VisitorSessionPageReferralUrl = string.Empty;
        public System.String VisitorSessionPageRequestUrl = string.Empty;
        public System.String VisitorSessionPageRequestQueryString = string.Empty;
        public System.DateTime VisitorSessionPageRenderTimeStart = new System.DateTime(1760, 1, 1);
        public System.DateTime VisitorSessionPageRenderTimeEnd = new System.DateTime(1760, 1, 1);
        public System.DateTime VisitorSessionPageLoadTimeStart = new System.DateTime(1760, 1, 1);
        public System.DateTime VisitorSessionPageLoadTimeEnd = new System.DateTime(1760, 1, 1);
        public System.DateTime VisitorSessionPageVisitTimeStart = new System.DateTime(1760, 1, 1);
        public System.DateTime VisitorSessionPageVisitTimeEnd = new System.DateTime(1760, 1, 1);
        public System.DateTime VisitorSessionPageCreateDate = DateTime.Now;

    }

}

