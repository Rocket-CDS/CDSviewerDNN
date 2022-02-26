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
using CDSviewerDNN.Components;
using RocketComm;

namespace CDSviewerDNN
{
    public partial class AppTheme : PortalModuleBase
    {
        public int PageId { get; set; }
        public int ModId { get; set; }
        public string SystemKey { get; set; }

        private bool _doSkinRedirect = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                ModId = ModuleId;
                PageId = TabId;

                //check if we have a skinsrc, if not add it and reload. NOTE: Where just asking for a infinate loop here, but DNN7.2 doesn't leave much option.
                const string skinSrcAdmin = "?SkinSrc=%2fDesktopModules%2fCDSviewerDNN%2fSkins%2fCDSviewer%2fCDSviewerAdmin";
                if (LocalUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    Response.Redirect(EditUrl("AppTheme") + skinSrcAdmin, false);
                    _doSkinRedirect = true;
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                if (Page.IsPostBack == false && _doSkinRedirect == false)
                {
                    var lit = new Literal();
                    lit.Text = EditData();
                    adminpanel.Controls.Add(lit);
                }

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        private string EditData()
        {
            LocalUtils.RemoveCache("editoption" + ModuleId);

            var strOut = "No Service";
            var moduleData = new ModuleDataLimpet(PortalId, ModuleId);
            if (moduleData.Exists)
            {
                var sessionJson = LocalUtils.GetCookieValue("simplisity_sessionparams");  // get session params from cookie, if it exists.
                moduleData.LoadUrlParams(Request.QueryString);
                moduleData.LoadPageUrl(TabId);
                moduleData.LoadSessionParams(sessionJson);
                if (moduleData.ServiceRef == "")
                    strOut = "No Service";
                else
                {
                    // Call to the CDS server.
                    var comm = new CommLimpet(moduleData.Record);
                    var commReturn = comm.CallRedirect("rocketapptheme_getremote", "", "");
                    strOut = commReturn.ViewHtml;
                }
            }
            return strOut;
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
    }
}