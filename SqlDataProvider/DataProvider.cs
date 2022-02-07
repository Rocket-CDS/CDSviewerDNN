using System.Collections.Generic;
using System.Data;
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.Providers;
using System.Configuration;
using Simplisity;

namespace CDSviewerDNN
{

	/// -----------------------------------------------------------------------------
	/// <summary>
	/// An abstract class for the data access layer
	/// </summary>
	/// -----------------------------------------------------------------------------
	public abstract class DataProvider
	{

		#region Shared/Static Methods

		private static DataProvider provider;

		// return the provider
		public static DataProvider Instance()
		{
			if (provider == null)
			{
                const string assembly = "CDSviewerDNN.SqlDataProvider,CDSviewerDNN";
				Type objectType = Type.GetType(assembly, true, true);

				provider = (DataProvider)Activator.CreateInstance(objectType);
			}

			return provider;
		}


		#endregion


		#region "Abstract Methods"
		public abstract IDataReader GetList(int portalId, int moduleId, string typeCode, string sqlSearchFilter = "", string lang = "", string sqlOrderBy = "", int returnLimit = 0, int pageNumber = 0, int pageSize = 0, int recordCount = 0, string tableName = "Toasted");
		public abstract int Update(int ItemId, int PortalId, int ModuleId, String TypeCode, String XMLData, String GUIDKey, DateTime ModifiedDate, String TextData, int XrefItemId, int ParentItemId, int UserId, string lang, int sortOrder, string tableName = "CDSviewer");
        public abstract void Delete(int itemId, string tableName = "CDSviewer");
        public abstract IDataReader GetRecord(int itemId, string tableName = "CDSviewer");

        #endregion


    }

}