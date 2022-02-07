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
                const string skinSrcAdmin = "?SkinSrc=%2fDesktopModules%2fToasted%2fToastedMod%2fSkins%2fToasted%2fToastedAdmin";
                if (LocalUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    Response.Redirect(EditUrl("Services") + skinSrcAdmin, false);
                    _doSkinRedirect = true;
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }

                if (Page.IsPostBack == false && _doSkinRedirect == false)
                {
                    var razorTemplateFileMapPath = LocalUtils.MapPath("/DesktopModules/Toasted/ToastedMod/Themes/config-w3/1.0/default/Services.cshtml");
                    var razorTemplate = FileSystemUtils.ReadFile(razorTemplateFileMapPath);

                    var serviceData = new ServiceLimpet(PortalId);
                    SystemKey = serviceData.SystemKey;
                    var nbRazor = new SimplisityRazor(serviceData);
                    var remoteparams = new RemoteLimpet(PortalId, TabId, ModuleId);
                    nbRazor.SetDataObject("remoteparams", remoteparams);
                    String razorText = LocalUtils.RazorRender(nbRazor, razorTemplate, true);

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