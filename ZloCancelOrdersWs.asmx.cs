using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Configuration;
using System.Web.Services;
using System.Xml.Serialization;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using X5.Packing.Portal.Models;
using X5.Packing.Portal.Services;
using X5.Packing.Portal.ZloCancelOrders;

namespace X5.Packing.Portal
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ZloCancelOrdersWs : System.Web.Services.WebService
    {
        /// <remarks/>
        [WebMethod]
        public ZloCancelOrdersResponse ZloCancelOrders([System.Xml.Serialization.XmlElementAttribute("ZloCancelOrders", Namespace = "urn:sap-com:document:sap:soap:functions:mc-style")] ZloCancelOrders.ZloCancelOrders ZloCancelOrders1)
        {
            try
            {
                var settings = new SettingsService(SPContext.Current.Web);

                if (settings.SapCancelUrl == null)
                {
                    throw new NullReferenceException("Settings SAP-CANCEL-REQUEST is null");
                }
                if (settings.SapLogin == null)
                {
                    throw new NullReferenceException("Settings SAP-LOGIN is null");
                }
                if (settings.SapPass == null)
                {
                    throw new NullReferenceException("Settings SAP-PASS is null");
                }
                
                X5PackingLoggingService.LogInfo($"ZloCancelOrdersWs.ZloCancelOrders send to SAP with {settings.SapCancelUrl}");

                var netCredential = new NetworkCredential(settings.SapLogin, settings.SapPass);
                var uri = new Uri(settings.SapCancelUrl);
                var credentials = netCredential.GetCredential(uri, "Basic");

                var client = new ZloCancelOrdersClient
                {
                    Url = settings.SapCancelUrl,
                    Credentials = credentials,
                    PreAuthenticate = true
                    #if DEBUG
                    ,Proxy = new WebProxy() { Address = new Uri("http://localhost:8888") }
                    #endif
                };
                var xmlSerializerRequest = new XmlSerializer(ZloCancelOrders1.GetType());
                using (var textWriter = new StringWriter())
                {
                    xmlSerializerRequest.Serialize(textWriter, ZloCancelOrders1);
                    X5PackingLoggingService.LogInfo(textWriter.ToString());
                }
                var response = client.ZloCancelOrders(ZloCancelOrders1);
                var xmlSerializerResponse = new XmlSerializer(response.GetType());
                using (var textWriter = new StringWriter())
                {
                    xmlSerializerResponse.Serialize(textWriter, response);
                    X5PackingLoggingService.LogInfo(textWriter.ToString());
                }
                X5PackingLoggingService.LogInfo($"ZloCancelOrdersWs.ZloCancelOrders received from SAP with {settings.SapCancelUrl}");
                return response;
            }
            catch (Exception e)
            {
                X5PackingLoggingService.LogError(e);
                throw;
            }
        }
    }
}
