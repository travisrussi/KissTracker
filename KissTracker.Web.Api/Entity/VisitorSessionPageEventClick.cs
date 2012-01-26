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
    
    public partial class VisitorSessionPageEventClick
    {

        public BsonObjectId _id = new ObjectId();
        public BsonObjectId VisitorSessionPageEventId = null;
        public System.String VisitorSessionPageEventClickElementName = string.Empty;
        public System.String VisitorSessionPageEventClickElementText = string.Empty;
        public System.Int32 VisitorSessionPageEventClickElementPositionX = -1;
        public System.Int32 VisitorSessionPageEventClickElementPositionY = -1;
        public System.Boolean VisitorSessionPageEventClickElementIsButton = false;
        public System.Boolean VisitorSessionPageEventClickElementIsLink = false;
        public System.Boolean VisitorSessionPageEventClickElementIsSubmit = false;
        public System.Boolean VisitorSessionPageEventClickElementIsImage = false;
        public System.DateTime VisitorSessionPageEventClickCreateDate = DateTime.Now;

    }

}

