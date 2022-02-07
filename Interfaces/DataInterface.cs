using Simplisity;
using System.Collections.Generic;

namespace CDSviewerDNN.Components
{

    public abstract class CDSviewerCtrlInterface
    {
        public abstract List<SimplisityInfo> GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string tableName = "Toasted");
        public abstract SimplisityRecord GetRecord(int itemId, string tableName = "CDSviewer");
        public abstract void Delete(int itemId, string tableName = "CDSviewer");
    }
}
