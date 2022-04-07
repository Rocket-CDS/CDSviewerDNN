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

namespace CDSviewerDNN
{
    public partial class Services : PortalModuleBase
    {
        public int ModId { get; set; }
        public string SystemKey { get; set; }

        private bool _doSkinRedirect = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                ModId = ModuleId;

                // Clear cache foe "Edit" Menu Option 
                LocalUtils.RemoveCache("editoption" + ModuleId);

                //check if we have a skinsrc, if not add it and reload. NOTE: Where just asking for a infinate loop here, but DNN7.2 doesn't leave much option.
                const string skinSrcAdmin = "?SkinSrc=%2fDesktopModules%2fCDSviewerDNN%2fSkins%2fCDSviewer%2fCDSviewerAdmin";
                if (LocalUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    Response.Redirect(EditUrl("Services") + skinSrcAdmin, false);
                    _doSkinRedirect = true;
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                if (Page.IsPostBack == false && _doSkinRedirect == false)
                {
                    var razorTemplateFileMapPath = LocalUtils.MapPath("/DesktopModules/CDSviewerDNN/Themes/config-w3/1.0/default/Services.cshtml");
                    var razorTemplate = FileSystemUtils.ReadFile(razorTemplateFileMapPath);

                    var serviceData = new ServiceDataLimpet(PortalId);
                    var nbRazor = new SimplisityRazor(serviceData);

                    var moduleData = new ModuleDataLimpet(PortalId, ModuleId);
                    moduleData.TabId = TabId;
                    SystemKey = moduleData.SystemKey;
                    nbRazor.SetDataObject("moduledata", moduleData);
                    String razorText = LocalUtils.RazorRender(nbRazor, razorTemplate, true);

                    // clear cache for edit change.
                    LocalUtils.ClearAllGroupCache(moduleData.ModuleRef);

                    var lit = new Literal();
                    lit.Text = razorText;
                    adminpanelheader.Controls.Add(lit);
                }

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
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