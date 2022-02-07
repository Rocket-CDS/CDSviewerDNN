using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.UserControls;
using Simplisity;
using ToastedMod.Components;

namespace ToastedMod
{
    public partial class Reload: PortalModuleBase
    {
        public int ModId { get; set; }
        public string SystemKey { get; set; }
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                ModId = ModuleId;

                var cmd = LocalUtils.RequestParam(Context, "cmd");
                if (cmd == "clearcache" && LocalUtils.IsAdministrator()) LocalUtils.ClearAllCache();
                if (cmd == "recycleapppool" && LocalUtils.IsSuperUser()) LocalUtils.RecycleApplicationPool();

                Response.Redirect("/", false);
                Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

    }
}