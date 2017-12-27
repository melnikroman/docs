using System;
using System.Web;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.Office.Server.UserProfiles;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using System.Data.SqlClient;
using GJ.RECEIVER.Utils;
using System.Collections.Generic;

namespace GJ.RECEIVER.Receivers.UserParseEventReceiver
{
    /// <summary>
    /// List Item Events
    /// </summary>
    public class UserParseEventReceiver : SPItemEventReceiver
    {
        /// <summary>
        /// An item was added.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            SPWeb web = properties.Web;
            SPListItem item = properties.ListItem;
            item["textUsers"] = String.Empty;

            SPFieldUserValueCollection userValues = new SPFieldUserValueCollection(web, item["manyUsers"].ToString());

            string userValuesString = String.Empty;
            using (SPSite site = properties.Site)
            {
                SPServiceContext serviceContext = SPServiceContext.GetContext(site);
                try
                {
                    UserProfileManager userProfileManager = new UserProfileManager(serviceContext);

                    foreach (SPFieldUserValue userValue in userValues)
                    {
                        var userProfile = userProfileManager.GetUserProfile(userValue.User.LoginName);
                        userValuesString += "'" + userProfile["WorkEmail"] + "',";
                    }

               
                }
                catch
                {
                    //Empty
                }
            }

            string queryString = "SELECT "
                                + " d.Name as Dep, "
                                + " e.FName, "
                                + " p.Name as Pos "
                                + " FROM Departments d "
                                    + " LEFT JOIN KPlan kp ON kp.DepartmentId = d.Id "
                                    + " LEFT JOIN Emploees e ON kp.id = e.KP_Id "
                                    + " LEFT JOIN Positions p ON kp.PositionId = p.Id " 
                            + " WHERE "
                            + " e.EMail in (" + userValuesString.TrimEnd(',') + ")";


            using (var manager = new ManagerSQL(properties.Web))
            {
                List<Dictionary<string, string>> result = manager.SelectAll(queryString.Replace(@"\\", @"\"));
                for (int i = 0; i < result.Count; i++)
                 item["textUsers"] += String.Format("{0} / {1} / {2}; ", result[i]["FName"], result[i]["Dep"], result[i]["Pos"]);
            }

            item.Update();

        }

        /// <summary>
        /// An item was updated.
        /// </summary>
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            SPWeb web = properties.Web;
            SPListItem item = properties.ListItem;
            item["textUsers"] = String.Empty;

            SPFieldUserValueCollection userValues = new SPFieldUserValueCollection(web, item["manyUsers"].ToString());
            string userValuesString = String.Empty;

            using (SPSite site = properties.Site)
            {
                SPServiceContext serviceContext = SPServiceContext.GetContext(site);
                try
                {
                    UserProfileManager userProfileManager = new UserProfileManager(serviceContext);

                    foreach (SPFieldUserValue userValue in userValues)
                    {
                        var userProfile = userProfileManager.GetUserProfile(userValue.User.LoginName);
                        userValuesString += "'" + userProfile["WorkEmail"] + "',";
                    }
                }
                catch
                {
                    //Empty
                }
            }

            string queryString = "SELECT "
                                + " d.Name as Dep, "
                                + " e.FName, "
                                + " p.Name as Pos "
                                + " FROM Departments d "
                                    + " LEFT JOIN KPlan kp ON kp.DepartmentId = d.Id "
                                    + " LEFT JOIN Emploees e ON kp.id = e.KP_Id "
                                    + " LEFT JOIN Positions p ON kp.PositionId = p.Id "
                            + " WHERE "
                            + " e.EMail in (" + userValuesString.TrimEnd(',') + ")";


            using (var manager = new ManagerSQL(properties.Web))
            {
                List<Dictionary<string, string>> result = manager.SelectAll(queryString.Replace(@"\\", @"\"));
                for (int i = 0; i < result.Count; i++)
                 item["textUsers"] += String.Format("{0} / {1} / {2}; ", result[i]["FName"], result[i]["Dep"], result[i]["Pos"]);
            }

            item.Update();
        }


    }
}