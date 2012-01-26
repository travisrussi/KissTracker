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
    
    public partial class VisitorSessionPageEventDataEntry
    {

        public BsonObjectId _id = new ObjectId();
        public BsonObjectId VisitorSessionPageEventId = null;
        public System.String VisitorSessionPageEventDataEntryElementType = string.Empty;
        public System.String VisitorSessionPageEventDataEntryElementName = string.Empty;
        public System.String VisitorSessionPageEventDataEntryElementValue = string.Empty;
        public System.Int32 VisitorSessionPageEventDataEntryElementPositionX = -1;
        public System.Int32 VisitorSessionPageEventDataEntryElementPositionY = -1;
        public System.DateTime VisitorSessionPageEventDataEntryCreateDate = DateTime.Now;

    }

}

