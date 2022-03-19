using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common.Utilities.Internal;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using Newtonsoft.Json.Linq;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using Simplisity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CDSviewerDNN.Components
{
    public class LocalUtils
    {
        public static void IncludeTextInHeader(Page page, string TextToInclude)
        {
            if (TextToInclude != "") page.Header.Controls.Add(new LiteralControl(TextToInclude));
        }
        public static void IncludeTextInHeaderAt(Page page, string TextToInclude, int addAt = 0)
        {
            if (addAt == 0) addAt = page.Header.Controls.Count;
            if (TextToInclude != "") page.Header.Controls.AddAt(addAt, new LiteralControl(TextToInclude));
        }
        public static string GetCookieValue(string name)
        {
            if (HttpContext.Current.Request.Cookies[name] != null)
            {
                return HttpContext.Current.Request.Cookies[name].Value;
            }
            return "";
        }
        public static string GetLocalizeString(string keyName, string resourceFile = "")
        {
            if (resourceFile == "") resourceFile = "/DesktopModules/CDSviewerDNN/App_LocalResources/Settings.ascx.resx";
            return Localization.GetString(keyName, resourceFile);
        }
        public static string NavigateURL(int tabId, string[] param)
        {
            return Globals.NavigateURL(tabId, "", param).ToString();
        }
        public static PortalSettings GetPortalSettings(int portalId)
        {
            var controller = new PortalController();
            var portal = controller.GetPortal(portalId);
            return new PortalSettings(portal);
        }
        public static void Handle404Exception(HttpResponse response, int portalId)
        {
            var portalSetting = GetPortalSettings(portalId);
            if (portalSetting?.ErrorPage404 > Null.NullInteger)
            {
                response.Redirect(Globals.NavigateURL(portalSetting.ErrorPage404, string.Empty, "status=404"));
            }
            else
            {
                response.ClearContent();
                response.TrySkipIisCustomErrors = true;
                response.StatusCode = 404;
                response.Status = "404 Not Found";
                response.Write("404 Not Found");
                response.End();
            }
        }
        public static int GetCurrentPortalId()
        {
            if (PortalSettings.Current == null)
                return -1;
            else
                return PortalSettings.Current.PortalId;
        }
        public static string ReadTemplate(string template)
        {
            var razorTemplateFileMapPath = LocalUtils.MapPath("/DesktopModules/CDSviewerDNN/Themes/config-w3/1.0/default/" + template);
            return FileSystemUtils.ReadFile(razorTemplateFileMapPath);
        }
        public static string MapPath(string relpath)
        {
            if (String.IsNullOrWhiteSpace(relpath)) return "";
            relpath = "/" + relpath.TrimStart('/');
            return System.Web.Hosting.HostingEnvironment.MapPath(relpath);
        }
        public static string RazorRender(SimplisityRazor model, string razorTempl, Boolean debugMode = false)
        {
            var errorPath = "";
            var result = "";
            var errmsg = "";
            try
            {
                if (razorTempl == null || razorTempl == "") return "";
                var hashCacheKey = GeneralUtils.GetMd5Hash(razorTempl);
                if (HttpContext.Current == null) // can be null if ran from scheduler.
                {
                    try
                    {
                        if (razorTempl == null || razorTempl == "") return "";
                        return Engine.Razor.RunCompile(razorTempl, hashCacheKey, null, model);
                    }
                    catch (Exception ex)
                    {
                        return "ERROR in RazorRunCompile : " + ex.ToString();
                    }
                }
                var service = (IRazorEngineService)HttpContext.Current.Application.Get("DNNrocketIRazorEngineService");
                if (service == null)
                {
                    // do razor test
                    var config = new TemplateServiceConfiguration();
                    config.Debug = debugMode;
                    config.BaseTemplateType = typeof(RazorEngineTokens<>);
                    service = RazorEngineService.Create(config);
                    HttpContext.Current.Application.Set("DNNrocketIRazorEngineService", service);
                }
                Engine.Razor = service;
                errorPath += "RunCompile1>";
                result = Engine.Razor.RunCompile(razorTempl, hashCacheKey, null, model);
            }
            catch (Exception ex)
            {
                result = "CANNOT REBUILD TEMPLATE: errorPath=" + errorPath + " - " + ex.ToString() + " -------> " + result + " [" + errmsg + "]";
            }

            return result;
        }
        public static bool HasModuleAdminRights(int moduleId)
        {
            try
            {
                if (moduleId == 0) return false;
                // Data security is not linked ot the tab, only the data.  Unlike DNN.  Search all tabs for any security access granted.
                foreach (ModuleInfo modInfo in ModuleController.Instance.GetAllTabsModulesByModuleID(moduleId))
                {
                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Admin, "MANAGE", modInfo))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
        public static bool HasModuleEditRights(int moduleId)
        {
            try
            {
                if (moduleId == 0) return false;
                // Data security is not linked ot the tab, only the data.  Unlike DNN.  Search all tabs for any security access granted.
                foreach (ModuleInfo modInfo in ModuleController.Instance.GetAllTabsModulesByModuleID(moduleId))
                {
                    if (ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "MANAGE", modInfo))
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }
        public static string RequestParam(HttpContext context, string paramName)
        {
            string result = null;

            if (context.Request.Form.Count != 0)
            {
                result = Convert.ToString(context.Request.Form[paramName]);
            }

            if (result == null)
            {
                if (context.Request.QueryString.Count != 0)
                {
                    result = Convert.ToString(context.Request.QueryString[paramName]);
                }
            }

            return (result == null) ? String.Empty : result.Trim();
        }
        public static bool IsSuperUser()
        {
            if (!IsAuthorised()) return false;
            return UserController.Instance.GetCurrentUserInfo().IsSuperUser;
        }
        public static bool IsAdministrator()
        {
            if (!IsAuthorised()) return false;
            PortalInfo ps = PortalController.Instance.GetPortal(GetCurrentPortalId());
            return ps != null && UserController.Instance.GetCurrentUserInfo().IsInRole(ps.AdministratorRoleName);
        }
        public static bool IsAuthorised()
        {
            return IsAuthorised(PortalSettings.Current.PortalId, UserController.Instance.GetCurrentUserInfo().UserID);
        }
        public static bool IsAuthorised(int portalId, int userId)
        {
            var userInfo = UserController.GetUserById(portalId, userId);
            if (userInfo != null) return userInfo.Membership.Approved;
            return false;
        }
        /// <summary>
        /// Recycles a web site Application Pool (including the current web site).
        /// Requires to reference Microsoft.Web.Administration and System.Web.Hosting.
        /// IMPORTANT: The IIS user requires extended permissions to recycle application pool(s).
        /// </summary>
        /// <param name="siteName">The site name: leave it NULL to recycle the current site's App Pool.</param>
        /// <returns>TRUE if the site's App Pool has been recycled; FALSE if no site has been found with the given name.</returns>
        public static bool RecycleApplicationPool(string siteName = null)
        {
            try
            {
                RetryableAction.Retry5TimesWith2SecondsDelay(() => File.SetLastWriteTime(Globals.ApplicationMapPath + "\\web.config", DateTime.Now), "Touching config file");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static string GetCurrentCulture()
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            return currentCulture.Name;
        }



        public static void SetCache(string cacheKey, object objObject, string cacheGroup = "", int keephours = 4)
        {
            if (cacheGroup != "")
            {
                cacheGroup = "cachegroup:" + cacheGroup;
                var cgDict = (List<string>)GetCache(cacheGroup);
                if (cgDict == null) cgDict = new List<string>();
                cgDict.Add(cacheKey);
                DataCache.SetCache(cacheGroup, cgDict);
            }
            DataCache.SetCache(cacheKey, objObject, DateTime.Now + new TimeSpan(0, keephours, 0, 0));
        }
        public static object GetCache(string cacheKey)
        {
            return DataCache.GetCache(cacheKey);
        }
        public static void RemoveCache(string cacheKey)
        {
            DataCache.RemoveCache(cacheKey);
        }
        public static void ClearAllCache()
        {
            DataCache.ClearCache();
        }
        public static void ClearAllGroupCache(string cacheGroup)
        {
            if (cacheGroup != "")
            {
                cacheGroup = "cachegroup:" + cacheGroup;
                var cgDict = (List<string>)GetCache(cacheGroup);
                if (cgDict == null) cgDict = new List<string>();
                foreach (var cacheKey in cgDict)
                {
                    DataCache.RemoveCache(cacheKey);
                }
            }
        }
        public static List<UserInfo> GetUsers(int portalId, string inRole = "")
        {
            var rtnList = new List<UserInfo>();
            var l = UserController.GetUsers(portalId);
            foreach (UserInfo u in l)
            {
                if (inRole == "" || u.IsInRole(inRole))
                {
                    rtnList.Add(u);
                }
            }
            return rtnList;
        }
        public static List<UserInfo> GetSuperUsers()
        {
            return GetUsers(-1, "SuperUser");
        }
        public static void SendNotifyEmail(ModuleDataLimpet moduleData, ServiceDataLimpet serviceData)
        {
            var suList = LocalUtils.GetSuperUsers();
            if (suList.Count > 0)
            {
                var suInfo = suList.First();
                var emailarray = serviceData.NotifyEmailCSV.Replace(';', ',').Split(',');
                foreach (var email in emailarray)
                {
                    if (!string.IsNullOrEmpty(email.Trim()) && GeneralUtils.IsEmail(suInfo.Email) && GeneralUtils.IsEmail(email.Trim()))
                    {
                        var razorTemplateFileMapPath = LocalUtils.MapPath("/DesktopModules/CDSviewerDNN/Themes/config-w3/1.0/default/NotifyEmail.cshtml");
                        var razorTemplate = FileSystemUtils.ReadFile(razorTemplateFileMapPath);
                        moduleData.Record.SetXmlProperty("genxml/portalurl", PortalSettings.Current.DefaultPortalAlias);
                        var nbRazor = new SimplisityRazor(moduleData);
                        var emailHtml = LocalUtils.RazorRender(nbRazor, razorTemplate, true);                        

                        string[] stringarray = new string[0];
                        DotNetNuke.Services.Mail.Mail.SendMail(
                            suInfo.Email,
                            email.Trim(), "", "", "",
                            DotNetNuke.Services.Mail.MailPriority.Normal,
                            "ERROR: CDSviewer Notification",
                            DotNetNuke.Services.Mail.MailFormat.Html,
                            System.Text.Encoding.UTF8,
                            emailHtml,
                            stringarray, "", "", "", "", false);
                    }
                }
            }


        }

    }
}
