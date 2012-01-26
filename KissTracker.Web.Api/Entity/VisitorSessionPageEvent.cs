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
    
    public partial class VisitorSessionPageEvent
    {

        public BsonObjectId _id = new ObjectId();
        public BsonObjectId VisitorSessionPageId = null;
        public System.DateTime VisitorSessionPageEventTimeServer = new System.DateTime(1760, 1, 1);
        public System.String VisitorSessionPageEventTimeElapsed = string.Empty;
        public System.DateTime VisitorSessionPageEventCreateDate = DateTime.Now;

    }

}

