using ADgateway.Models;
using ADgateway.Utils;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Text.Json;

namespace ADgateway.Controllers
{
    public class HomeController : Controller
    {
        const string dnsCompany = "kopavogur";
        const string dnsCountry = "is";
        const string adDomain = "VOGUR";

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ShowUser(bool simplify = true, string user = null)
        {

            Dictionary<string, object> userMap = LoadUserMap(simplify, user);
            ViewBag.userJson = JsonSerializer.Serialize(userMap, new JsonSerializerOptions { WriteIndented = true }); ;
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        public Dictionary<string, object> LoadUserMap(bool simplify = true, string user = null)
        {
            string? username = user ?? User.Identity?.Name;
            if (username == null) throw new Exception("No user specified and no user login to fall back on");
            if (!username.Contains("@")) username = username.RemoveStart($"{adDomain}\\") + $"@{dnsCompany}.{dnsCountry}";

            Dictionary<string, object> userMap;
            using (var context = new PrincipalContext(ContextType.Domain, name: adDomain))
            {
                UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(context, username);
                DirectoryEntry userEntry = userPrincipal.GetUnderlyingObject() as DirectoryEntry;
                string managerDN = userEntry.Properties.GetPropertyValueString("manager");
                string company = userEntry.Properties.GetPropertyValueString("company");
                string department = userEntry.Properties.GetPropertyValueString("department");

                DirectoryEntry departmentEntry = new DirectoryEntry($"LDAP://{adDomain}/OU={department},OU={company},DC=kopavogur,DC=is");
                DirectoryEntry companyEntry = new DirectoryEntry($"LDAP://{adDomain}/OU={company},DC=kopavogur,DC=is");
                DirectoryEntry managerEntry = new DirectoryEntry($"LDAP://{adDomain}/{managerDN}");

                userMap = userEntry.Properties.CopyPropertyCollectionToDictionary(simplify);
                Dictionary<string, object> departmentMap = departmentEntry.Properties.CopyPropertyCollectionToDictionary(simplify);
                Dictionary<string, object> companyMap = companyEntry.Properties.CopyPropertyCollectionToDictionary(simplify);
                Dictionary<string, object> managerMap = managerEntry.Properties.CopyPropertyCollectionToDictionary(simplify);
                userMap["manager"] = managerMap;
                userMap["department"] = departmentMap;
                userMap["company"] = companyMap;
            }
            return userMap;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}