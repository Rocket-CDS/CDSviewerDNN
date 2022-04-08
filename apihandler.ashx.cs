using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Simplisity;
using CDSviewerDNN.Components;
using RocketComm;

namespace CDSviewerDNN
{
    public class ApiHandler : IHttpHandler
    {
        private ModuleDataLimpet _moduleData;
        private bool _hasEditAccess;
        private CommData _commReturn;
        private int _moduleid;
        private string _moduleref;
        private int _tabid;
        private SimplisityInfo _paramInfo;
        private SimplisityInfo _postInfo;

        public void ProcessRequest(HttpContext context)
        {
            String strOut = "";

            var paramCmd = context.Request.QueryString["cmd"];
            var paramJson = context.Request.Form["paramjson"];
            var postJson = context.Request.Form["inputjson"];
            if (paramCmd != "" && paramJson != null && postJson != null)
            {
                _paramInfo = SimplisityJson.GetSimplisityInfoFromJson(HttpUtility.UrlDecode(paramJson), "");
                _moduleref = _paramInfo.GetXmlProperty("genxml/hidden/moduleref");
                _moduleData = new ModuleDataLimpet(LocalUtils.GetCurrentPortalId(), _moduleref);
                // simplisity puts any session fields in a cookie string as json
                // We need to pass these values to the data service.
                var sessionJson = LocalUtils.GetCookieValue("simplisity_sessionparams");  // get session params from cookie, if it exists.
                _moduleData.LoadSessionParams(sessionJson);

                _postInfo = SimplisityJson.GetSimplisityInfoFromJson(HttpUtility.UrlDecode(postJson), "");
                if (!paramCmd.StartsWith("services_"))
                {
                    context.Response.ContentType = "text/plain";

                    // Call to the CDS server. NO CACHE on API, it will not work correctly.
                    var comm = new CommLimpet(_moduleData.Record);
                    _commReturn = comm.CallRedirect(paramCmd, postJson, paramJson);
                    if (_commReturn.StatusCode != "00")
                    {
                        var sRazor = new SimplisityRazor();
                        var t = LocalUtils.ReadTemplate("Reload.cshtml");
                        t += "<div>" + _commReturn.ErrorMsg + "</div>";
                        strOut = LocalUtils.RazorRender(sRazor, t, true);
                    }
                    else
                    {
                        strOut = _commReturn.ViewHtml;
                    }
                }
                else
                {
                    switch (paramCmd)
                    {
                        case "services_resetmodule":
                            strOut = DeleteModuleSettings();
                            break;
                        case "services_selectsystem":
                            strOut = SaveSystemKey();
                            break;
                        case "services_savedata":
                            strOut = SaveService();
                            break;
                        case "services_delete":
                            strOut = DeleteService();
                            break;
                        case "services_selectservice":
                            strOut = SelectService();
                            break;
                        case "services_sendtestnotifyemail":
                            var serviceData = new ServiceDataLimpet(_moduleData.PortalId);
                            _moduleData.TabId = _paramInfo.GetXmlPropertyInt("genxml/hidden/tabid");
                            LocalUtils.SendNotifyEmail(_moduleData, serviceData);
                            strOut = "";
                            break;

                        default:
                            strOut = "INVALID CMD";
                            break;
                    }
                }
            }

            #region "return results"

            // we return the result through the context.Response.            
            context.Response.Clear();
            //context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            //context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");

            // in this case we always return a string (Html), but any typ eof string like Json and Xml can also be returned.
            context.Response.ContentType = "text/plain";
            context.Response.Write(strOut);
            context.Response.End();
            #endregion

        }

        private string DeleteModuleSettings()
        {
            if (LocalUtils.HasModuleAdminRights(_moduleData.ModuleId))
            {
                _moduleData.Reset();
            }
            return ""; // reload page
        }
        private string SaveSystemKey()
        {
            if (LocalUtils.HasModuleAdminRights(_moduleData.ModuleId))
            {
                _moduleData.SystemKey  = _paramInfo.GetXmlProperty("genxml/hidden/systemkey");
                _moduleData.Update();
            }
            return ""; // reload page
        }
        private string SaveService()
        {
            if (LocalUtils.HasModuleAdminRights(_moduleData.ModuleId))
            {
                _postInfo.PortalId = PortalSettings.Current.PortalId;
                var serviceData = new ServiceDataLimpet(PortalSettings.Current.PortalId);
                serviceData.SaveServiceCode(_postInfo);
                if (serviceData.GetServices().Count == 1)
                {
                    // set default to the only service
                    var service = serviceData.GetService(0);
                    if (service != null)
                    {
                        _moduleData.SaveSelectedService(PortalSettings.Current.PortalId, new SimplisityInfo(service));
                    }
                }
                // Register the data client with the CDS
                var comm = new CommLimpet(_moduleData.Record);
                _commReturn = comm.CallRedirect("dataclients_register", "", "", "rocketportal");
            }
            return ""; // reload page
        }
        private string DeleteService()
        {
            if (LocalUtils.HasModuleAdminRights(_moduleData.ModuleId))
            {
                var serviceData = new ServiceDataLimpet(PortalSettings.Current.PortalId);
                if (serviceData.Exists)
                {
                    var idx = _paramInfo.GetXmlPropertyInt("genxml/hidden/servicecodeidx");
                    var service = serviceData.GetService(idx);
                    if (_moduleData.ServiceRef == service.GetXmlProperty("genxml/config/serviceref"))
                    {
                        _moduleData.ServiceRef = "";
                        _moduleData.Update();
                    }
                    serviceData.RemoveService(_paramInfo);
                }
            }
            return ""; // reload page
        }
        private string SelectService()
        {
            if (LocalUtils.HasModuleAdminRights(_moduleData.ModuleId))
            {
                _moduleData.SaveSelectedService(PortalSettings.Current.PortalId, _postInfo);
            }
            return ""; // reload page
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
