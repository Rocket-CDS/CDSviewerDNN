using System;
using System.Web.UI.WebControls;
using System.Xml;
using RocketComm;
using CDSviewerDNN.Components;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using Newtonsoft.Json;

namespace CDSviewerDNN
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ViewNBrightGen class displays the content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : PortalModuleBase, IActionable
    {
        #region Event Handlers

        private ModuleDataLimpet _moduleData;
        private bool _hasEditAccess;
        private CommData _commReturn;

        protected override void OnInit(EventArgs e)
        {
            try
            {

                base.OnInit(e);

                _hasEditAccess = false;
                if (UserId > 0) _hasEditAccess = DotNetNuke.Security.Permissions.ModulePermissionController.CanEditModuleContent(this.ModuleConfiguration);

                _moduleData = new ModuleDataLimpet(PortalId, ModuleId);
                _moduleData.LoadUrlParams(Request.QueryString);
                _moduleData.LoadPageUrl(TabId);

                var sessionJson = LocalUtils.GetCookieValue("simplisity_sessionparams");  // get session params from cookie, if it exists.
                _moduleData.LoadSessionParams(sessionJson);
                // Load the current language,  the sessionJson might be wrong for the view load. 
                _moduleData.CultureCode = LocalUtils.GetCurrentCulture();

                // Call to the CDS server.
                var comm = new CommLimpet(_moduleData.Record);
                _commReturn = comm.CallRedirect("remote_publicview", "", "");

                var strHeader1 = _commReturn.FirstHeader;
                LocalUtils.IncludeTextInHeaderAt(Page, strHeader1, 1); // injected begining of header

                var strHeader = _commReturn.LastHeader;
                LocalUtils.IncludeTextInHeaderAt(Page, strHeader, 0); // injected at end for jQuery Compatibility

            }
            catch (Exception ex)
            {
                Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {

                base.OnLoad(e);

                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void PageLoad()
        {
            var basePage = (DotNetNuke.Framework.CDefault)this.Page;
            var metaSEO = _commReturn.SeoHeader;
            if (metaSEO.Title != "") basePage.Title = metaSEO.Title;
            if (metaSEO.Description != "") basePage.Description = metaSEO.Description;
            if (metaSEO.KeyWords != "") basePage.KeyWords = metaSEO.KeyWords;
        }
        protected override void OnPreRender(EventArgs e)
        {
            var strOut = _commReturn.ViewHtml;
            if (_commReturn.StatusCode == "404") LocalUtils.Handle404Exception(Response, PortalId);
            if (_hasEditAccess && _commReturn.StatusCode != "00") strOut = _commReturn.ErrorMsg;

            var lit = new Literal();
            lit.Text = strOut;
            phData.Controls.Add(lit);

        }


        #endregion


        #region Optional Interfaces

        /// <summary>
        /// The ModuleActions builds the module menu, for actions available.
        /// </summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();

                var editoption = (string)LocalUtils.GetCache("editoption" + ModuleId);
                if (editoption == null)
                {
                    // Call to the CDS server.
                    var comm = new CommLimpet(_moduleData.Record);
                    var commOptReturn = comm.CallRedirect("remote_editoption", "", "");
                    editoption = commOptReturn.ViewHtml;
                    LocalUtils.SetCache("editoption" + ModuleId, editoption);
                }
                Boolean parsedValue;
                Boolean optionValue;
                if (Boolean.TryParse(editoption, out parsedValue))
                {
                    if (parsedValue)
                        optionValue = true;
                    else
                        optionValue = false;
                }
                else
                    optionValue = false;

                if (optionValue)
                {
                    actions.Add(GetNextActionID(), LocalUtils.GetLocalizeString("edit", this.LocalResourceFile), "", "", "", EditUrl(), false, SecurityAccessLevel.Edit, true, false);
                }
                else
                {
                    actions.Add(GetNextActionID(), LocalUtils.GetLocalizeString("editnewwindow", this.LocalResourceFile), "", "", "", _moduleData.EngineUrl + "/" + _moduleData.SystemKey, false, SecurityAccessLevel.Edit, true, true);
                }

                actions.Add(GetNextActionID(), LocalUtils.GetLocalizeString("setup", this.LocalResourceFile), "", "", "", EditUrl("Setup"), false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), LocalUtils.GetLocalizeString("services", this.LocalResourceFile), "", "", "", EditUrl("Services"), false, SecurityAccessLevel.Admin, true, false);
                actions.Add(GetNextActionID(), LocalUtils.GetLocalizeString("apptheme", this.LocalResourceFile), "", "", "", EditUrl("AppTheme"), false, SecurityAccessLevel.Admin, true, false);

                //actions.Add(GetNextActionID(), LocalUtils.GetLocalizeString("clearcache", this.LocalResourceFile), "", "", "", EditUrl("cmd","clearcache","Reload"), false, SecurityAccessLevel.Host, true, false);
                actions.Add(GetNextActionID(), LocalUtils.GetLocalizeString("recycleapppool", this.LocalResourceFile), "", "", "", EditUrl("cmd", "recycleapppool", "Reload"), false, SecurityAccessLevel.Host, true, false);

                return actions;
            }
        }

        #endregion



    }

}
