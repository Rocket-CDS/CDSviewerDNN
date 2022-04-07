using Simplisity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using CDSviewerDNN;

namespace CDSviewerDNN.Components
{
    public class ServiceDataLimpet
    {
        private string _guidKey;
        private const string _tableName = "CDSviewer";
        private const string _entityTypeCode = "CDSviewerDNNServices";

        private CDSviewerController _objCtrl;
        /// <summary>
        /// Should be used to create an article, the portalId is required on creation
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="useCache"></param>
        public ServiceDataLimpet(int portalId, bool useCache = true)
        {
            _guidKey = "Services" + portalId;

            Record = new SimplisityRecord();
            Record.ItemID = -1;
            Record.TypeCode = _entityTypeCode;
            Record.ModuleId = -1;
            Record.UserId = -1;
            Record.PortalId = portalId;
            Record.Lang = "";
            Record.GUIDKey = _guidKey;

            Populate();
        }
        private void Populate()
        {
            _objCtrl = new CDSviewerController();

            var rec = _objCtrl.GetRecordByGuidKey(PortalId, -1, _entityTypeCode, _guidKey, "", _tableName); // get existing record.
            if (rec != null) // check if we have a real record.
                Record = rec;
            else
                Record.ItemID = -1; // flags does not exist yet.
        }
        public void Delete()
        {
            if (Record.ItemID > 0)
            {
                _objCtrl.Delete(Record.ItemID, _tableName);
            }
        }
        public void SaveServiceCode(SimplisityInfo postInfo)
        {
            var serviceCode = postInfo.GetXmlProperty("genxml/textbox/servicecode");
            if (serviceCode != "")
            {
                var exists = false;
                foreach (var sRec in GetServices())
                {
                    if (sRec.GetXmlProperty("genxml/textbox/servicecode") == serviceCode) exists = true;
                }
                if (!exists)
                {
                    var srec = new SimplisityRecord();
                    srec.SetXmlProperty("genxml/textbox/servicecode", serviceCode);
                    var sRemote = new SimplisityInfo();
                    sRemote.FromXmlItem(GeneralUtils.Base64Decode(serviceCode));
                    srec.SetXmlProperty("genxml/name", sRemote.GetXmlProperty("genxml/settings/systemkey") + " - " + sRemote.GetXmlProperty("genxml/settings/engineurl"));
                    srec.SetXmlProperty("genxml/config/serviceref", GeneralUtils.GetGuidKey());
                    Record.AddRecordListItem(ServiceListName, srec);
                }
            }

            Record.SetXmlProperty("genxml/textbox/notifyemailcsv", postInfo.GetXmlProperty("genxml/textbox/notifyemailcsv"));
            Record.SetXmlProperty("genxml/textbox/threshold", postInfo.GetXmlProperty("genxml/textbox/threshold"));
            Record.SetXmlProperty("genxml/textbox/errorcount", postInfo.GetXmlProperty("genxml/textbox/errorcount"));
            Record.SetXmlProperty("genxml/textbox/totalerrors", postInfo.GetXmlProperty("genxml/textbox/totalerrors"));

            ValidateAndUpdate();

        }
        public bool ServiceExists(string serviceref)
        {
            if (serviceref == "") return false;
            if (GetService(serviceref) == null) return false;
            return true;
        }
        public SimplisityRecord GetService(int idx)
        {
            return Record.GetRecordListItem(ServiceListName, idx);
        }
        public SimplisityRecord GetService(string serviceref)
        {
            return Record.GetRecordListItem(ServiceListName, "genxml/config/serviceref", serviceref);
        }
        public void RemoveService(SimplisityInfo postInfo)
        {
            var idx = postInfo.GetXmlPropertyInt("genxml/hidden/servicecodeidx");


            Record.RemoveRecordListItem(ServiceListName, idx);
            ValidateAndUpdate();
        }
        public List<SimplisityRecord> GetServices()
        {
            return Record.GetRecordList(ServiceListName);
        }
        public int Update()
        {
            return _objCtrl.Update(Record, _tableName);
        }
        public int ValidateAndUpdate()
        {
            Validate();
            return Update();
        }
        public void Validate()
        {
        }

        #region "properties"

        public SimplisityInfo Info { get { return new SimplisityInfo(Record); } } 
        public SimplisityRecord Record { get; set; }
        public int ModuleId { get { return Record.ModuleId; } set { Record.ModuleId = value; } }
        public int XrefItemId { get { return Record.XrefItemId; } set { Record.XrefItemId = value; } }
        public int ParentItemId { get { return Record.ParentItemId; } set { Record.ParentItemId = value; } }
        public int CategoryId { get { return Record.ItemID; } set { Record.ItemID = value; } }
        public string GUIDKey { get { return Record.GUIDKey; } set { Record.GUIDKey = value; } }
        public int SortOrder { get { return Record.SortOrder; } set { Record.SortOrder = value; } }
        public int PortalId { get { return Record.PortalId; } }
        public bool Exists { get { if (Record.ItemID <= 0) { return false; } else { return true; }; } }
        public string ServiceListName { get { return "servicelist"; } }
        public string AppTheme { get { return Record.GetXmlProperty("genxml/config/apptheme"); } set { Record.SetXmlProperty("genxml/config/apptheme", value); } }
        public string Cmd { get { return Record.GetXmlProperty("genxml/config/cmd"); } set { Record.SetXmlProperty("genxml/config/cmd", value); } }

        public string NotifyEmailCSV { get { return Record.GetXmlProperty("genxml/textbox/notifyemailcsv"); } }
        public int NotifyThreshold { get { return Record.GetXmlPropertyInt("genxml/textbox/threshold"); } }
        public int NotifyErrorCount { get { return Record.GetXmlPropertyInt("genxml/textbox/errorcount"); } set { Record.SetXmlProperty("genxml/textbox/errorcount", value.ToString()); } }
        public int TotalErrors { get { return Record.GetXmlPropertyInt("genxml/textbox/totalerrors"); } set { Record.SetXmlProperty("genxml/textbox/totalerrors", value.ToString()); } }

        #endregion

    }

}
