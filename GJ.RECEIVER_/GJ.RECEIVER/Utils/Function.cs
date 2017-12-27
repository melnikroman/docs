using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GJ.RECEIVER.Utils
{
    public class Function
    {
        internal static SPListItemCollection FilterList(SPList spList, string Query)
        {
            if (String.IsNullOrEmpty(Query))
            {
                return spList.Items;
            }
            SPQuery spQuery = new SPQuery();
            spQuery.Query = Query;
            return spList.GetItems(spQuery);
        }

        public static T GetSPFieldValue<T>(object FieldValue, object ValueIsNull = null, SPWeb web = null)
        {
            Type type = typeof(T);
            Type UnderlyingType = Nullable.GetUnderlyingType(type) ?? type;
            if (FieldValue != null && !String.IsNullOrEmpty(FieldValue.ToString()))
            {
                switch (UnderlyingType.Name)
                {
                    case "SPFieldLookupValue": return (T)Convert.ChangeType(new SPFieldLookupValue(FieldValue.ToString()), UnderlyingType);
                    case "SPFieldLookupValueCollection": return (T)Convert.ChangeType(new SPFieldLookupValueCollection(FieldValue.ToString()), UnderlyingType);
                    case "SPFieldUserValue": return (T)Convert.ChangeType(new SPFieldUserValue(web, FieldValue.ToString()), UnderlyingType);
                    case "SPFieldUserValueCollection": return (T)Convert.ChangeType(new SPFieldUserValueCollection(web, FieldValue.ToString()), UnderlyingType);
                    case "DateTime":
                        {
                            Regex pattern = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z");
                            if (pattern.IsMatch(FieldValue.ToString()))
                            {
                                return (T)Convert.ChangeType(DateTime.ParseExact(FieldValue.ToString(), "yyyy-MM-ddThh:mm:ssZ", CultureInfo.InvariantCulture), UnderlyingType);
                            }
                            pattern = new Regex(@"\d{4}-\d{2}-\d{2}");
                            if (pattern.IsMatch(FieldValue.ToString()))
                            {
                                return (T)Convert.ChangeType(DateTime.ParseExact(FieldValue.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture), UnderlyingType);
                            }
                            return (T)Convert.ChangeType(DateTime.ParseExact(FieldValue.ToString(), "dd.MM.yyyy h:mm:ss", CultureInfo.InvariantCulture), UnderlyingType);
                        } 
                    default:
                        {
                            try
                            {
                                return (T)Convert.ChangeType(FieldValue, UnderlyingType);
                            }
                            catch
                            {
                                return (T)Convert.ChangeType(FieldValue.ToString(), UnderlyingType);
                            }
                        }
                }
            }
            else
            {
                if (ValueIsNull == null)
                {
                    return (T)ValueIsNull;
                }
                else
                {
                    return (T)Convert.ChangeType(ValueIsNull, UnderlyingType);
                }
            }
        }

        /// <summary>
        /// Отправляет уведомление пользователю (возможны вложения)
        /// </summary>
        /// <param name="message"></param>
        internal static void SendMailMessage(MailMessage message)
        {
            message.From = new MailAddress(SPAdministrationWebApplication.Local.OutboundMailSenderAddress);
            if (message.From.User == null || message.To.Count == 0)
            {
                return;
            }
            SmtpClient client = new SmtpClient(SPAdministrationWebApplication.Local.OutboundMailServiceInstance.Server.Address)
            {
                EnableSsl = false,
                //Credentials = new NetworkCredential(WFMailSetting.USER_LOGIN, WFMailSetting.USER_PASSWORD),
                UseDefaultCredentials = true,
            };
            client.Send(message);
        }

        internal static SPList GetList(SPWeb web, string CustomUrl)
        {
            string FullUrl = SPUtility.ConcatUrls(web.ServerRelativeUrl, CustomUrl);

            return web.GetList(FullUrl);
        }

        public static SPFile GetFile(SPWeb web, string CustomUrl)
        {
            string FullUrl = SPUtility.ConcatUrls(web.ServerRelativeUrl, CustomUrl);

            return web.GetFile(FullUrl);
        }
    }
}
