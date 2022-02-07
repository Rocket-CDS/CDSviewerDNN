using System;
using System.Collections.Generic;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using Simplisity;
using CDSviewerDNN.Components;

namespace CDSviewerDNN
{

    public class CDSviewerController : CDSviewerCtrlInterface
    {
        public override List<SimplisityInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string tableName = "Toasted")
        {
            return CBO.FillCollection<SimplisityInfo>(DataProvider.Instance().GetList(portalId, moduleId, typeCode, sqlSearchFilter, lang, sqlOrderBy, returnLimit, pageNumber, pageSize, recordCount, tableName));
        }
        public override void Delete(int itemId, string tableName = "CDSviewer")
        {
            DataProvider.Instance().Delete(itemId, tableName);
        }
        public override SimplisityRecord GetRecord(int itemId, string tableName = "CDSviewer")
        {
            return CBO.FillObject<SimplisityRecord>(DataProvider.Instance().GetRecord(itemId, tableName));
        }
        public int Update(SimplisityRecord objInfo, string tableName = "CDSviewer")
        {
            // save data
            objInfo.ModifiedDate = DateTime.Now;
            return DataProvider.Instance().Update(objInfo.ItemID, objInfo.PortalId, objInfo.ModuleId, objInfo.TypeCode, objInfo.XMLData, objInfo.GUIDKey, objInfo.ModifiedDate, objInfo.TextData, objInfo.XrefItemId, objInfo.ParentItemId, objInfo.UserId, objInfo.Lang, objInfo.SortOrder, tableName);
        }
        public SimplisityRecord GetRecordByGuidKey(int portalId, int moduleId, string entityTypeCode, string guidKey, string selUserId = "", string tableName = "CDSviewer")
        {
            var strFilter = " and R1.GUIDKey = '" + guidKey + "' ";
            if (selUserId != "")
            {
                strFilter += " and R1.UserId = " + selUserId + " ";
            }

            var l = GetList(portalId, moduleId, entityTypeCode, strFilter, "", "", 1,0,0,0, tableName);
            if (l.Count == 0) return null;
            if (l.Count > 1)
            {
                for (int i = 1; i < l.Count; i++)
                {
                    // remove invalid DB entries
                    Delete(l[i].ItemID, tableName);
                }
            }
            if (l[0] == null) return null;
            var rtn = GetRecord(l[0].ItemID, tableName);
            return rtn;
        }
    }

}
