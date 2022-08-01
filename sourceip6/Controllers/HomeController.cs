using System.Diagnostics;
using System.Net;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using sourceip6.Models;

namespace sourceip.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            string ipAdd;
            if (!string.IsNullOrEmpty(HttpContext.Request.Headers["X-Forwarded-For"]))
            {
                ipAdd = HttpContext.Request.Headers["X-Forwarded-For"];
            }
            else
            {
                ipAdd = HttpContext.Request.HttpContext.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.ToString();
            }


            var host = HttpContext.Request.Host.Host;
            var value = HttpContext.Request.Host.Value;
            var method = HttpContext.Request.Method;
            var response = HttpContext.Response.StatusCode;
            string remoteIpAddress = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

            if (ipAdd == "::1")
            {
                ipAdd = "174.22.203.118";
            }
            else
            {
                ipAdd = ipAdd.Split(':')[0];
            }

            // Following API key is for https://ipstack.com/quickstart
            // user id is my yahoo email

            string errorResponse;
            string jsonResp;
            string continent;
            string latitude;
            string longitude;
            string region = "";
            string city = "";
            string url;

            try
            {
                string API_KEY = "e22d93bf836af6211aaa0305d4f8f3ba"; //ipstack.com
                // var iplocationURL = "https://api.ipgeolocation.io/ipgeo?apiKey=" + API_KEY + "&ip=" + ipAdd;
                var iplocationURL = "http://api.ipstack.com/" + ipAdd + "?access_key=" + API_KEY;
                url = iplocationURL;

                // Create a request for the URL.
                WebRequest request = WebRequest.Create(iplocationURL);

                // Get the response.
                WebResponse resp = request.GetResponse();
                Stream dataStream = resp.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                jsonResp = responseFromServer;
                errorResponse = "looks good";
                try
                {
                    dynamic iplocation = JsonConvert.DeserializeObject(jsonResp);
                    continent = iplocation.continent_name;
                    latitude = iplocation.latitude;
                    longitude = iplocation.longitude;
                    region = iplocation.region_name;
                    city = iplocation.city;
                    errorResponse = "json convert worked";
                }
                catch
                {
                    errorResponse = "connected to ipstack BUT unable to parse json";
                }
            }
            catch
            {
                errorResponse = "unable to connect to the ipstack.com";
            }

            if (errorResponse != null)
            {
                ViewData["request"] = "Request originating from " + city + ", " + region + " on the host " + host + " from IP address " + ipAdd + " receiving a response " + response;
            }
            else
            {
                ViewData["request"] = errorResponse;
            }

            return View();
        }

        public IActionResult managedIdentity()
        {
            ViewData["Message"] = "Ash's Source IP Site";
            //var credential = new DefaultAzureCredential();
            string _endpoint = System.Environment.GetEnvironmentVariable("APP_CONFIG_ENDPOINT"); //"https://test-app-config-ad.azconfig.io";
            string _label = System.Environment.GetEnvironmentVariable("APP_CONFIG_FILTER");
            var _selector = new SettingSelector { LabelFilter = _label }; // "Production"
            string _client_id = System.Environment.GetEnvironmentVariable("CLIENT_ID");
            ViewData["identity"] = _client_id;
            ViewData["label"] = _label;
            ViewData["endpoint"] = _endpoint;

            var _configPair = new List<string>();
            try
            {
                var miCredentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = _client_id }); // V2.4.2 alternate-sourceip
                // var miCredentials = new DefaultAzureCredential(); // V2.4.1

                // var appConfigClient = new ConfigurationClient(new Uri(endpoint), new DefaultAzureCredential());
                var appConfigClient = new ConfigurationClient(new Uri(_endpoint), miCredentials);
                // 

                foreach (ConfigurationSetting pair in appConfigClient.GetConfigurationSettings(_selector))
                {
                    // Console.WriteLine(set);
                    _configPair.Add(pair.Key + " : " + pair.Value);
                }
                // 
                // string connectionString = "Endpoint=https://test-app-config-ad.azconfig.io;Id=HZrn-l1-s0:zb8fXqh43KB5n3p4Pbp2;Secret=prcFoNKDv90xLCAVFgXLw2dQWDZzRIQGEACXmLVoQ64=";
                // var selector = new SettingSelector { LabelFilter = "Production" };
                // var appConfigClient = new ConfigurationClient(connectionString);
                // // 
                // foreach (ConfigurationSetting pair in appConfigClient.GetConfigurationSettings(selector))
                // {
                //     // Console.WriteLine(set);
                //     _arr.Add(pair.Key + " : " + pair.Value);
                // }
                // ConfigurationSetting setting = client.GetConfigurationSetting("domain");
                // ViewData["config"] = setting.Value;
                //
            }
            catch
            {
                _configPair.Add("Error" + " : " + "Unable to authenticate");
            }

            return View(_configPair);
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

