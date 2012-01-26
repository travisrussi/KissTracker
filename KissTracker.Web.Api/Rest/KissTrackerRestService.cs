using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Net;
using System.IO;
using System.Configuration;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.Http;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using KissTracker.Entity;
using MongoDB.Driver.Builders;


namespace KissTracker.Web.Api
{

    [ServiceContract]
    public interface IKissTrackerRest
    {

        [OperationContract]
        [WebInvoke(Method="*",RequestFormat=WebMessageFormat.Json, ResponseFormat=WebMessageFormat.Json)]
        string TrackEvent(string jsonString);
    }

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class KissTrackerRestService : IKissTrackerRest
    {

        public string TrackEvent(string jsonString)
        {
            string sRet = string.Empty;

            if (HttpContext.Current == null ||
                HttpContext.Current.Request == null ||
                HttpContext.Current.Request.QueryString == null ||
                string.IsNullOrEmpty(HttpContext.Current.Request.QueryString.ToString()))
            {
                return string.Empty;
            }
                
            if (string.IsNullOrEmpty(jsonString))
                jsonString = HttpContext.Current.Request.QueryString["jsonString"];

            if (string.IsNullOrEmpty(jsonString))
                return "Error: no data";

            string sessionId = string.Empty;
            if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["sessionId"]))
                sessionId = HttpContext.Current.Request.QueryString["sessionId"];
            else
                return "Error: no sessionId";

            sRet = TrackEvent(sessionId, jsonString);
            
            return sRet;
        }

        private static string TrackEvent(string sessionId, string eventData)
        {
            string sRet = string.Empty;

            string connectionString = "mongodb://localhost";
            MongoServer server = MongoServer.Create(connectionString);

            MongoDatabase kissTrackerDb = null;

            try
            {
                kissTrackerDb = server.GetDatabase("KissTracker");
            }
            catch (Exception ex)
            {
                return "Error: unable to connect to database";
            }

            VisitorSession vs = null;

            try
            {
                var vsCol = kissTrackerDb.GetCollection<VisitorSession>("VisitorSession");
                var vsQuery = Query.EQ("VisitorSessionAspSessionId", sessionId);
                vs = vsCol.FindOne(vsQuery);
                if (vs == null)
                {
                    vs = new VisitorSession();
                    vs.VisitorSessionAspSessionId = sessionId;
                    vsCol.Insert(vs);
                }
            }
            catch (Exception ex)
            {
                return "Error: unable to create visitor session";
            }

            MongoCollection<VisitorSessionPage> vspCol = null;
            VisitorSessionPage vsp = null;
            try
            {
                vspCol = kissTrackerDb.GetCollection<VisitorSessionPage>("VisitorSessionPage");
                var vspQuery = Query.EQ("VisitorSessionId", vs._id);
                vsp = vspCol.FindOne(vspQuery);
                if (vsp == null)
                {
                    vsp = new VisitorSessionPage();
                    vsp.VisitorSessionId = vs._id;
                    vspCol.Insert(vsp);
                }
            }
            catch (Exception ex)
            {
                return "Error: unable to create visitor session page";
            }

            string[] qs = null;
            string decodedEventData = DecodeBase64(eventData);

            if (string.IsNullOrEmpty(decodedEventData))
                return "Error: no data";

            qs = decodedEventData.Split('|');

            List<TrackEvent> teList = new List<TrackEvent>();
            TrackEvent te = null;

            //t:[timeElapsedInMilliseconds];a:[EventAction];e:[ElementType];l:[ElementName];c:[CtaName];p:[PositionX]-[PositionY];v:[ElementValue]
            Regex reTime = new Regex("t:([^;]*)");
            Regex reAction = new Regex(";a:([^;]*)");
            Regex reElementType = new Regex(";e:([^;]*)");
            Regex reElementName = new Regex(";l:([^;]*)");
            Regex reCtaName = new Regex(";c:([^;]*)");
            Regex reElementPosition = new Regex(";p:([^;]*)");
            Regex reDirectionName = new Regex(";d:([^;]*)");
            Regex reElementValue = new Regex(";v:(.*)");
                
            for (int i = 0; i < qs.Length; i++)
            {
                if (string.IsNullOrEmpty(qs[i]))
                    continue;

                te = new TrackEvent();

                if (reTime.IsMatch(qs[i]))
                    te.Timestamp = reTime.Match(qs[i]).Groups[1].Value;

                if (reAction.IsMatch(qs[i]))
                    te.EventAction = reAction.Match(qs[i]).Groups[1].Value;

                if (reElementType.IsMatch(qs[i]))
                    te.ElementType = reElementType.Match(qs[i]).Groups[1].Value;

                if (reElementName.IsMatch(qs[i]))
                    te.ElementName = reElementName.Match(qs[i]).Groups[1].Value;

                if (reCtaName.IsMatch(qs[i]))
                    te.CtaName = reCtaName.Match(qs[i]).Groups[1].Value;

                if (reElementPosition.IsMatch(qs[i]))
                {
                    string elPos = reElementPosition.Match(qs[i]).Groups[1].Value;
                    if (!string.IsNullOrEmpty(elPos) &&
                        elPos.Contains("-"))
                    {
                        te.ElementPositionX = elPos.Split('-')[0];
                        te.ElementPositionY = elPos.Split('-')[1];
                    }
                }

                if (reDirectionName.IsMatch(qs[i]))
                    te.DirectionName = reDirectionName.Match(qs[i]).Groups[1].Value;

                if (reElementValue.IsMatch(qs[i]))
                    te.ElementValue = reElementValue.Match(qs[i]).Groups[1].Value;

                if (te != null &&
                    !string.IsNullOrEmpty(te.Timestamp))
                {
                    teList.Add(te);
                }
            }

            try
            {
                VisitorSessionPageEvent vspe = null;
                VisitorSessionPageEventClick vspec = null;
                VisitorSessionPageEventScroll vspes = null;
                VisitorSessionPageEventDataEntry vspede = null;
                VisitorSessionPageEventError vspee = null;
                VisitorSessionPageEventMessage vspem = null;

                var vspeCol = kissTrackerDb.GetCollection<VisitorSessionPageEvent>("VisitorSessionPageEvent");
                var vspecCol = kissTrackerDb.GetCollection<VisitorSessionPageEventClick>("VisitorSessionPageEventClick");
                var vspedeCol = kissTrackerDb.GetCollection<VisitorSessionPageEventDataEntry>("VisitorSessionPageEventDataEntry");
                var vspeeCol = kissTrackerDb.GetCollection<VisitorSessionPageEventError>("VisitorSessionPageEventError");
                var vspemCol = kissTrackerDb.GetCollection<VisitorSessionPageEventMessage>("VisitorSessionPageEventMessage");
                var vspesCol = kissTrackerDb.GetCollection<VisitorSessionPageEventScroll>("VisitorSessionPageEventScroll");

                foreach (TrackEvent e in teList)
                {
                    vspe = new VisitorSessionPageEvent();
                    vspe.VisitorSessionPageId = vsp._id;
                    vspe.VisitorSessionPageEventTimeElapsed = e.Timestamp;
                    vspeCol.Insert(vspe);

                    int iPosX = 0;
                    int iPosY = 0;
                    int.TryParse(e.ElementPositionX, out iPosX);
                    int.TryParse(e.ElementPositionY, out iPosY);

                    switch (e.EventAction)
                    {
                        case "loadstart":
                            vsp.VisitorSessionPageLoadTimeStart = ConvertJsEpochDateToDate(e.ElementValue);
                            break;

                        case "loadend":
                            vsp.VisitorSessionPageLoadTimeEnd = ConvertJsEpochDateToDate(e.ElementValue);
                            break;

                        case "unload":
                            vsp.VisitorSessionPageVisitTimeEnd = ConvertJsEpochDateToDate(e.ElementValue);
                            break;

                        case "linkclick":
                        case "mouseclick":
                        case "submit":

                            vspec = new VisitorSessionPageEventClick();
                            vspec.VisitorSessionPageEventId = vspe._id;
                            vspec.VisitorSessionPageEventClickElementName = e.ElementName;
                            vspec.VisitorSessionPageEventClickElementText = e.ElementValue;
                            vspec.VisitorSessionPageEventClickElementPositionX = iPosX;
                            vspec.VisitorSessionPageEventClickElementPositionY = iPosY;

                            if (e.EventAction == "linkclick")
                                vspec.VisitorSessionPageEventClickElementIsLink = true;
                            else if (e.EventAction == "submit")
                                vspec.VisitorSessionPageEventClickElementIsSubmit = true;

                            if (e.ElementType == "image")
                                vspec.VisitorSessionPageEventClickElementIsImage = true;
                            else if (e.ElementType == "button")
                                vspec.VisitorSessionPageEventClickElementIsButton = true;

                            vspecCol.Insert(vspec);
                            break;

                        case "scroll":

                            vspes = new VisitorSessionPageEventScroll();
                            vspes.VisitorSessionPageEventId = vspe._id;
                            vspes.VisitorSessionPageEventScrollDepth = e.ElementValue;
                            vspes.VisitorSessionPageEventScrollDirection = e.DirectionName;
                            vspesCol.Insert(vspes);

                            break;

                        case "textchange":

                            vspede = new VisitorSessionPageEventDataEntry();
                            vspede.VisitorSessionPageEventId = vspe._id;
                            vspede.VisitorSessionPageEventDataEntryElementName = e.ElementName;
                            vspede.VisitorSessionPageEventDataEntryElementType = e.ElementType;
                            vspede.VisitorSessionPageEventDataEntryElementValue = e.ElementValue;
                            vspede.VisitorSessionPageEventDataEntryElementPositionX = iPosX;
                            vspede.VisitorSessionPageEventDataEntryElementPositionY = iPosY;
                            vspedeCol.Insert(vspede);
                            break;

                        case "error":

                            vspee = new VisitorSessionPageEventError();
                            vspee.VisitorSessionPageEventId = vspe._id;
                            vspee.VisitorSessionPageEventErrorMessage = e.ElementValue;
                            vspeeCol.Insert(vspee);
                            break;

                        case "message":

                            vspem = new VisitorSessionPageEventMessage();
                            vspem.VisitorSessionPageEventId = vspe._id;
                            vspem.VisitorSessionPageEventMessageValue = e.ElementValue;
                            vspemCol.Insert(vspem);
                            break;
                    }
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                vspCol.Save(vsp);
            }

            return sRet;
        }

        private static string DecodeBase64(string data)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                string result = new String(decoded_char);
                return result;
            }
            catch (Exception e)
            {
                //throw new Exception("Error in base64Decode" + e.Message);
            }
            return string.Empty;
        }

        private static DateTime ConvertJsEpochDateToDate(string epochDate)
        {
            DateTime dt = DateTime.MinValue;

            try
            {
                double epochDateInSeconds = 0;
                double.TryParse(epochDate, out epochDateInSeconds);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dt = epoch.AddSeconds(epochDateInSeconds);
            }
            catch (Exception ex)
            { }

            return dt;
        }

    }

    public class TrackEvent
    {
        public enum TrackEventActionEnum
        {
            none,
            mousehover,
            mouseclick,
            mousedoubleclick,
            linkclick,
            scroll,
            textchange,
            focus,
            blur,
            submit,
            unload,
            loaded,
            rendertime,
            transfertime
        }

        public string EventAction = "";
        public string Timestamp = "";
        public string ElementType = "";
        public string ElementName = "";
        public string CtaName = "";
        public string ElementPositionX = "";
        public string ElementPositionY = "";
        public string ElementValue = "";
        public string DirectionName = "";

        public override string ToString()
        {
            return "t:" + this.Timestamp + ", a:" + this.EventAction + ", e:" + this.ElementType + ", l:" + this.ElementName + ", c:" + this.CtaName + ", p:(" + this.ElementPositionX + "," + this.ElementPositionY + "), d:" + this.DirectionName + ", v:" + this.ElementValue;
        }
    }
}