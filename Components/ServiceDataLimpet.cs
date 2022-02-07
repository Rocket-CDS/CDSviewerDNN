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

            // chekc if we already have it.
            var exists = false;
            foreach (var sRec in GetServices())
            {
                if (sRec.GetXmlProperty("genxml/textbox/servicecode") == serviceCode) exists = true;
            }
            if (!exists)
            {
                var srec = new SimplisityRecord();
                srec.SetXmlProperty("genxml/textbox/servicecode",serviceCode);
                var sRemote = new SimplisityInfo();
                sRemote.FromXmlItem(GeneralUtils.Base64Decode(serviceCode));
                srec.SetXmlProperty("genxml/name", sRemote.GetXmlProperty("genxml/settings/systemkey") + " - " + sRemote.GetXmlProperty("genxml/settings/engineurl"));
                srec.SetXmlProperty("genxml/config/serviceref", GeneralUtils.GetGuidKey());

                Record.AddRecordListItem(ServiceListName, srec);
                ValidateAndUpdate();
            }
        }
        public bool ServiceExists(string serviceref)
        {
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
        public string SystemKey { get { return Record.GetXmlProperty("genxml/config/systemkey"); } set { Record.SetXmlProperty("genxml/config/systemkey", value); } }
        public string AppTheme { get { return Record.GetXmlProperty("genxml/config/apptheme"); } set { Record.SetXmlProperty("genxml/config/apptheme", value); } }
        public string Cmd { get { return Record.GetXmlProperty("genxml/config/cmd"); } set { Record.SetXmlProperty("genxml/config/cmd", value); } }

        #endregion

    }

}
