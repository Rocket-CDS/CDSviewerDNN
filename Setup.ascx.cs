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
    public partial class Setup : PortalModuleBase
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
                    Response.Redirect(EditUrl("Setup") + skinSrcAdmin, false);
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
            LocalUtils.RemoveCache("editoption" + ModuleId); // flag to know if we want to display the edit option on the module menu.

            var strOut = "No Service";
            var moduleData = new ModuleDataLimpet(PortalId, ModuleId);
            if (moduleData.Exists)
            {
                // clear cache for edit change.
                LocalUtils.ClearAllGroupCache(moduleData.ModuleRef);

                var serviceData = new ServiceDataLimpet(PortalId);
                if (!serviceData.ServiceExists(moduleData.ServiceRef))
                {
                    moduleData.ServiceRef = "";
                    moduleData.Update();

                    var razorTemplateFileMapPath = LocalUtils.MapPath("/DesktopModules/CDSviewerDNN/Themes/config-w3/1.0/default/Services.cshtml");
                    var razorTemplate = FileSystemUtils.ReadFile(razorTemplateFileMapPath);

                    SystemKey = moduleData.SystemKey;
                    var nbRazor = new SimplisityRazor(serviceData);
                    nbRazor.SetDataObject("moduledata", moduleData);
                    strOut = LocalUtils.RazorRender(nbRazor, razorTemplate, true);
                }
                else if(moduleData.SystemKey == "") 
                {
                    strOut = "Get your systemkey";
                }
                else
                {
                    // Load the current language,  the sessionJson might be wrong for the page. 
                    moduleData.CultureCode = LocalUtils.GetCurrentCulture();
                    // Call to the CDS server.
                    var comm = new CommLimpet(moduleData.Record);
                    var commReturn = comm.CallRedirect("remote_settings", "", "");
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
        protected void cmdReset_Click(object sender, EventArgs e)
        {
            try
            {
                var moduleData = new ModuleDataLimpet(PortalId, ModuleId);
                if (moduleData.Exists)
                {
                    moduleData.Delete();
                }
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
    }
}