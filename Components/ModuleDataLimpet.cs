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
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace CDSviewerDNN.Components
{
    public class ModuleDataLimpet
    {
        private string _guidKey;
        private const string _tableName = "CDSviewer";
        private const string _entityTypeCode = "CDSviewerModuleData";

        private CDSviewerController _objCtrl;
        /// <summary>
        /// Used for local API communication, where we do not have the moduleid.
        /// It is only initiated from the local API (apihandler.ashx)
        /// </summary>
        /// <param name="moduleRef">Link to specific module data that has already been created.</param>
        public ModuleDataLimpet(int portalId, string moduleRef)
        {
            _objCtrl = new CDSviewerController(); // Database controller.
            RecordxRef = _objCtrl.GetRecordByGuidKey(portalId, -1, "XREFMOD", moduleRef, "", _tableName); // get existing record.
            _guidKey = "Module" + portalId + "*" + RecordxRef.ModuleId;
            Record = _objCtrl.GetRecordByGuidKey(portalId, RecordxRef.ModuleId, _entityTypeCode, _guidKey, "", _tableName); // get existing record.
        }
        /// <summary>
        /// Load/Create DNN module communication data.
        /// It is only called from the DNN module server-side code.  Where we know the moduleId and tabid
        /// </summary>
        /// <param name="portalId">Link the data to a specific portal in DNN.</param>
        /// <param name="moduleId">Link the data to a specific module in DNN.</param>
        public ModuleDataLimpet(int portalId, int moduleId)
        {
            // on a normal pageload in DNN we know the moduleId, not the CDS moduleref.
            _guidKey = "Module" + portalId + "*" + moduleId;

            _objCtrl = new CDSviewerController(); // Database controller.

            Record = new SimplisityRecord();
            Record.ItemID = -1;
            Record.TypeCode = _entityTypeCode;
            Record.ModuleId = moduleId;
            Record.UserId = -1;
            Record.PortalId = portalId;
            Record.Lang = "";
            Record.GUIDKey = _guidKey;

            // Save the local API url, so we can call it from the client side JS.
            Record.SetXmlProperty("genxml/remote/apiurl", "/Desktopmodules/CDSviewerDNN/apihandler.ashx");

            // Get Data record for this module, using moduleId.
            var rec = _objCtrl.GetRecordByGuidKey(portalId, moduleId, _entityTypeCode, _guidKey, "", _tableName); // get existing record.
            if (rec != null) Record = rec;

            // Create a unique moduleref to be used by the CDS for data storage.
            // (The moduleId may not be unique across systems.)
            var moduleRef = Record.GetXmlProperty("genxml/remote/moduleref");
            var recxref = _objCtrl.GetRecordByGuidKey(PortalId, moduleId, "XREFMOD", moduleRef, "", _tableName); // get existing record.
            if (recxref != null) RecordxRef = recxref;
            if (moduleRef == "" || RecordxRef == null)
            {
                moduleRef = GeneralUtils.GetGuidKey() + moduleId; // create a moduleref for CDS usage.
                Record.SetXmlProperty("genxml/remote/moduleref", moduleRef);
                // create a XREFMOD record so we can use the CDS moduleref to get the moduleID for DNN.
                RecordxRef = new SimplisityRecord();
                RecordxRef.PortalId = portalId;
                RecordxRef.ModuleId = moduleId;
                RecordxRef.GUIDKey = moduleRef;
                RecordxRef.TypeCode = "XREFMOD";
            }
            if (rec == null || recxref == null) Update();
        }
        /// <summary>
        /// Reset the moduleData record to match the CDS connection.
        /// </summary>
        /// <param name="serviceCode">CDS Service Code</param>
        public void LoadServiceSecurityCode(string serviceCode)
        {
            try
            {
                if (serviceCode != "")
                {
                    var sRemote = new SimplisityInfo();
                    sRemote.FromXmlItem(GeneralUtils.Base64Decode(serviceCode));
                    var l = sRemote.ToDictionary();
                    foreach (var d in l)
                    {
                        if (d.Key != "systemkey") // do not save legacy systemkey [this test can be removed in future]
                        {
                            Record.SetXmlProperty("genxml/remote/" + d.Key, d.Value);
                        }
                    }
                    Update();
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }
        public void LoadUrlParams(NameValueCollection queryString)
        {
            if (Record != null)
            {
                Record.RemoveXmlNode("genxml/remote/urlparams");
                foreach (String key in queryString.AllKeys)
                {
                    Record.SetXmlProperty("genxml/remote/urlparams/" + key, queryString[key]);
                }
            }
        }
        public void LoadPageUrl(int tabId)
        {
            Record.SetXmlProperty("genxml/remote/pageurl", LocalUtils.NavigateURL(tabId, new string[0]));
        }
        public void LoadSessionParams(string sessionJson)
        {
            if (!String.IsNullOrEmpty(sessionJson))
            {
                XmlDocument doc = (XmlDocument)JsonConvert.DeserializeXmlNode("{root:" + sessionJson + "}"); // add root so we can transform into xml
                var nodList = doc.SelectNodes("root/*");  // loop on values (this could be json, but I find XML easier)
                foreach (XmlNode nod in nodList)
                {
                    if (nod.Name != "null") // don't use null.  
                    {
                        Record.SetXmlProperty("genxml/remote/" + nod.Name, nod.InnerText);
                    }
                }
            }
        }

        public void Delete()
        {
            var l = _objCtrl.GetList(Record.PortalId, Record.ModuleId, "XREFMOD", "","","",0,0,0,0, _tableName);
            foreach (var r in l)
            {
                _objCtrl.Delete(r.ItemID, _tableName);
            }
            _objCtrl.Delete(Record.ItemID, _tableName);
        }
        public int Update()
        {
            var xrefId = _objCtrl.Update(RecordxRef, _tableName);
            Record.XrefItemId = xrefId;
            return _objCtrl.Update(Record, _tableName);
        }

        public void SaveSelectedService(int portalId, SimplisityInfo postInfo)
        {
            var serviceData = new ServiceDataLimpet(portalId);
            if (serviceData.Exists)
            {
                var idx = postInfo.GetXmlPropertyInt("genxml/hidden/servicecodeidx");
                var service = serviceData.GetService(idx);
                if (service != null && service.GetXmlProperty("genxml/config/serviceref") != "")
                {
                    ServiceRef = service.GetXmlProperty("genxml/config/serviceref");
                    var serviceCode = service.GetXmlProperty("genxml/textbox/servicecode");
                    LoadServiceSecurityCode(serviceCode);
                }
            }
        }

        /// <summary>
        ///  Build cachekey from known values that will not create a duplicate.
        ///  NOTE: In future this may need to be changed for new systems.
        /// </summary>
        /// <returns></returns>
        public string GetCacheKey()
        {
            // build cacheKey.  Unique for each page.
            var cacheKey = CultureCode;
            cacheKey += ModuleRef;
            cacheKey += Record.GetXmlPropertyInt("genxml/remote/pagesize");
            cacheKey += Record.GetXmlPropertyInt("genxml/remote/urlparams/page") + "*" + Record.GetXmlPropertyInt("genxml/remote/urlparams/p");
            cacheKey += Record.GetXmlPropertyInt("genxml/remote/urlparams/catid");
            cacheKey += Record.GetXmlPropertyInt("genxml/remote/urlparams/id") + "*" + Record.GetXmlPropertyInt("genxml/remote/urlparams/articleid") + "*" + Record.GetXmlPropertyInt("genxml/remote/urlparams/productid");
            cacheKey += Record.GetXmlPropertyInt("genxml/remote/orderbyref"); ;
            // add filters
            var nodList = Record.XMLDoc.SelectNodes("genxml/remote/*[starts-with(name(), 'checkboxfilter')]");
            if (nodList != null && nodList.Count > 0)
            {
                foreach (XmlNode nod in nodList)
                {
                    cacheKey += nod.Name + "*" + nod.InnerText;
                }
            }
            nodList = Record.XMLDoc.SelectNodes("genxml/remote/*[starts-with(name(), 'radiofilter')]");
            if (nodList != null && nodList.Count > 0)
            {
                foreach (XmlNode nod in nodList)
                {
                    cacheKey += nod.Name + "*" + nod.InnerText;
                }
            }
            // Add searchtext, so we get a different key when search used.
            // This is not cached, but we need a different key to the cached data.
            cacheKey += Record.GetXmlProperty("genxml/remote/searchtext");
            return GeneralUtils.GetMd5Hash(cacheKey);
        }


        #region "properties"

        public SimplisityInfo Info { get { return new SimplisityInfo(Record); } } 
        public SimplisityRecord Record { get; set; }
        public SimplisityRecord RecordxRef { get; set; }
        public int ModuleId { get { return Record.ModuleId; } set { Record.ModuleId = value; } }
        public int TabId { get { return Record.ParentItemId; } set { Record.ParentItemId = value; } }
        public int XrefItemId { get { return Record.XrefItemId; } set { Record.XrefItemId = value; } }
        public int ParentItemId { get { return Record.ParentItemId; } set { Record.ParentItemId = value; } }
        public int CategoryId { get { return Record.ItemID; } set { Record.ItemID = value; } }
        public string GUIDKey { get { return Record.GUIDKey; } set { Record.GUIDKey = value; } }
        public int SortOrder { get { return Record.SortOrder; } set { Record.SortOrder = value; } }
        public int PortalId { get { return Record.PortalId; } }
        public bool Exists { get { if (Record.ItemID <= 0) { return false; } else { return true; }; } }
        public string CultureCode { get { return Record.GetXmlProperty("genxml/remote/culturecode"); } set { Record.SetXmlProperty("genxml/remote/culturecode", value); } }
        public string CultureCodeEdit { get { return Record.GetXmlProperty("genxml/remote/culturecodeedit"); } set { Record.SetXmlProperty("genxml/remote/culturecodeedit", value); } }
        public string SystemKey { get { return Record.GetXmlProperty("genxml/remote/systemkey"); } set { Record.SetXmlProperty("genxml/remote/systemkey", value); } }
        public string EngineUrl { get { return Record.GetXmlProperty("genxml/remote/engineurl"); } }
        public string ServiceRef { get { return Record.GetXmlProperty("genxml/remote/serviceref"); } set { Record.SetXmlProperty("genxml/remote/serviceref", value); } }
        public string ModuleRef { get { return RecordxRef.GUIDKey; }  }
        #endregion

    }

}
