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
using Newtonsoft.Json.Linq;
using Simplisity;
using ToastedMod.Components;

namespace ToastedMod
{
    public partial class Edit : PortalModuleBase
    {
        public int PageId { get; set; }
        public int ModId { get; set; }
        public string SystemKey { get; set; }

        private bool _doSkinRedirect = false;
        protected override void OnInit(EventArgs e)
        {

            try
            {
                base.OnInit(e);

                ModId = ModuleId;
                PageId = TabId;

                //check if we have a skinsrc, if not add it and reload. NOTE: Where just asking for a infinate loop here, but DNN7.2 doesn't leave much option.
                const string skinSrcAdmin = "?SkinSrc=%2fDesktopModules%2fToasted%2fToastedMod%2fSkins%2fToasted%2fToastedAdmin";
                if (LocalUtils.RequestParam(Context, "SkinSrc") == "")
                {
                    Response.Redirect(EditUrl() + skinSrcAdmin, false);
                    _doSkinRedirect = true;
                    Context.ApplicationInstance.CompleteRequest(); // do this to stop iis throwing error
                }


            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        protected override void OnPreRender(EventArgs e)
        {
            try
            {
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
            var strOut = "No Service";
            if (LocalUtils.HasModuleEditRights(TabId, ModuleId))
            {
                LocalUtils.SetEditCulture(""); // set to current langauge

                var remoteParam = new RemoteLimpet(PortalId, TabId, ModuleId);
                if (remoteParam.Exists && remoteParam.EngineURL != "")
                {
                    var sessionCookie = LocalUtils.GetCookieValue("simplisity_sessionparams");
                    if (sessionCookie != null && sessionCookie != "")
                    {
                        JToken token = JObject.Parse(sessionCookie);
                        var ccode = (string)token.SelectToken("culturecodeedit");
                        if (ccode != "") remoteParam.CultureCodeEdit = ccode;
                        remoteParam.Update();
                    }

                    var serviceData = new ServiceLimpet(PortalId);
                    if (!serviceData.ServiceExists(remoteParam.ServiceRef))
                    {
                        remoteParam.ServiceRef = "";
                        remoteParam.Update();
                    }
                    if (remoteParam.ServiceRef == "")
                    {
                        strOut = LocalUtils.RenderServiceTemplate(PortalId, ModuleId, TabId);
                    }
                    else
                    {
                        strOut = LocalUtils.RenderRemoteTemplate(PortalId, ModuleId, TabId, "remote_edit");
                    }
                }
            }
            return strOut;
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                LocalUtils.ClearAllCache();
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
    }
}