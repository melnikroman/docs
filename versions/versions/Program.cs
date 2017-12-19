using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using File = Microsoft.SharePoint.Client.File;


namespace versions
{
    class Program
    {
        public const string SWeb = "http://srus270113/collaboration/sfa";
        public const string SWebUrl = "http://srus270113";
        public const string DWeb = "http://srus270111";
        public const string Temp = @"C:\Temp\docs\";

        public const string Login = "zzrus27sp_admin";
        public const string Password = "bS44rogZ";
        public const string Domain = "ferreronet";

        public const string SLibrary = "ACCENTURE";
        public const string SFolderPath = "/ACCENTURE/09_User acceptance test";
        public const string DLibrary = "COLLABORATION";
        public const string DFolderPath = "/COLLABORATION/SFA Project/ACCENTURE/09_User acceptance test 2";
        public const string SubsiteUrl = "/collaboration/sfa";

        static void Main(string[] args)
        {
            using (var site = new SPSite(SWeb))
            {
                using (var spWeb1 = site.OpenWeb())
                {
                    using (var ctx = new ClientContext(DWeb))
                    {
                        ctx.Credentials = new NetworkCredential(Login, Password, Domain);

                        var spWeb2 = ctx.Web;
                        ctx.Load(spWeb2,
                              w => w.Title);

                        ctx.ExecuteQuery(); 

                        ctx.Load(spWeb2.SiteGroups);
                        ctx.ExecuteQuery();
                        var usersString = String.Empty;

                        foreach (var spGroup in spWeb2.SiteGroups)
                        {

                            ctx.Load(spGroup);

                            ctx.ExecuteQuery();

                            var spSiteUsers = spGroup.Users;

                            ctx.Load(spSiteUsers);

                            ctx.ExecuteQuery();

                            foreach (var spUser in spSiteUsers  )
                            {
                                usersString += spUser.LoginName + ";";
                            }
                           // usersString += spSiteUsers.Select(u => u.LoginName);
                            // usersString += 
                            //  String.Join(";", spSiteUsers.Select(u => u.LoginName));
                        }
                       
                        usersString = usersString.Replace("ponomeuge1", String.Empty);
                        usersString = usersString.Replace("verkhroma1", String.Empty);
                        usersString = usersString.Replace("panfealex1", String.Empty);
                        usersString = usersString.Replace("shchemaxi1", String.Empty);
                        usersString = usersString.Replace("sivkoalek1", String.Empty);
                        usersString = usersString.Replace("berdnilya1", String.Empty);
                        usersString = usersString.Replace("reznimiro1", String.Empty);
                        usersString = usersString.Replace("kuznevadi1", String.Empty);
                        usersString = usersString.Replace("kozlodmit1", String.Empty);
                        usersString = usersString.Replace("bystrpave1", String.Empty);

                        var spList1 = spWeb1.Lists[SLibrary];
                        var spList2 = ctx.Web.Lists.GetByTitle(DLibrary);
                        ctx.Load(spList2);
                        ctx.ExecuteQuery();
                        spList2.EnableFolderCreation = true;
                        spList2.Update();
                        ctx.ExecuteQuery();

                        GetFiles(spWeb1.GetFolder(SubsiteUrl + SFolderPath), spList1, spList2, spWeb2, ctx, usersString);
                        spList2.Update();
                    }
                }
            }
        }

        public static void GetFiles(SPFolder folder, SPList spList1, List spList2, Web spWeb2, ClientContext ctx, string usersString)
        {
            try
            {
                var folderUrl = DFolderPath + folder.ServerRelativeUrl.Replace(SubsiteUrl + SFolderPath, String.Empty);
                var fol = spWeb2.GetFolderByServerRelativeUrl(folderUrl);

                ctx.Load(fol);
                ctx.ExecuteQuery();
                foreach (SPFile file in folder.Files)
                {
                    Console.WriteLine(file.Name);
                    foreach (SPFileVersion version in file.Versions)
                    {
                        var newPath = Temp + file.Name;
                        var webClient = new WebClient();
                        //using (var webClient = new WebClient())
                        //{
                           // try
                            //{
                                webClient.Credentials = new NetworkCredential(Login, Password, Domain);
                                webClient.DownloadFile(SWeb + '/' + version.Url, newPath);
                           // }
                           // catch
                           // {
                              //  webClient.Dispose();
                           // }
                        //}


                        //var fs = new FileStream(newPath, FileMode.OpenOrCreate);
                        using (var fs = new FileStream(newPath, FileMode.OpenOrCreate))
                        {
                            //try
                           // {
                                File.SaveBinaryDirect(ctx, fol.ServerRelativeUrl + "/" + file.Name, fs, true);
                          //  }
                          //  catch{
                               // fs.Dispose();
                            //}
                        }
                        
                        var fileUpload = spWeb2.GetFileByServerRelativeUrl(fol.ServerRelativeUrl + "/" + file.Name);
                        ctx.Load(fileUpload);
                        ctx.ExecuteQuery();

                        //set metadata
                        var item = fileUpload.ListItemAllFields;
                        ctx.Load(item);
                        ctx.ExecuteQuery();
                        item["Modified"] = version.Created;

                        var createdBy = version.CreatedBy;

                        if (usersString.ToLower().Contains(createdBy.LoginName.ToLower()))
                        {
                            item["Editor"] = spWeb2.EnsureUser(createdBy.LoginName);
                        }
                        else
                        {
                            item["Editor"] = spWeb2.EnsureUser(@"SHAREPOINT\system");
                        }

                        var num = item["_UIVersionString"].ToString();
                        double result;
                        double.TryParse(num.Replace('.', ','), out result);
                        result = result + 0.01;
                        item["_UIVersionString"] = result.ToString();
                        item.Update();

                        ctx.ExecuteQuery();
                    }
                    var newPath1 = Temp + file.Name;
                    var webClient1 = new WebClient();
                    //using (var webClient = new WebClient())
                    //{
                        //try
                        //{
                            webClient1.Credentials = new NetworkCredential(Login, Password, Domain);
                            webClient1.DownloadFile(SWebUrl + folder.ServerRelativeUrl + '/' + file.Name, newPath1);
                        //}
                        //catch
                       // {
                          //  webClient1.Dispose();
                       // }
                    //}
                    //var fs1 = new FileStream(newPath1, FileMode.OpenOrCreate);
                    using (var fs1 = new FileStream(newPath1, FileMode.OpenOrCreate))
                    {
                       // try
                       // {
                            File.SaveBinaryDirect(ctx, fol.ServerRelativeUrl + "/" + file.Name, fs1, true);
                        //}
                        //catch
                       // {
                            //fs.Dispose();
                       // }
                    }
                    var fileUpload1 = spWeb2.GetFileByServerRelativeUrl(fol.ServerRelativeUrl + "/" + file.Name);
                    ctx.Load(fileUpload1);
                    ctx.ExecuteQuery();

                    //Adding correct metadata to the file  
                    var item1 = fileUpload1.ListItemAllFields;
                    ctx.Load(item1);
                    ctx.ExecuteQuery();
                    var modifiedBy1 = file.ModifiedBy;

                    item1["Modified"] = file.TimeLastModified;

                    //check user
                    if (usersString.ToLower().Contains(modifiedBy1.LoginName.ToLower()))
                    {
                        item1["Editor"] = spWeb2.EnsureUser(modifiedBy1.LoginName);
                    }
                    else
                    {
                        item1["Editor"] = spWeb2.EnsureUser(@"SHAREPOINT\system");
                    }
                    var num1 = item1["_UIVersionString"].ToString();
                    double result1;
                    double.TryParse(num1.Replace('.', ','), out result1);
                    result1 = result1 + 0.01;
                    item1["_UIVersionString"] = result1.ToString();
                    item1.Update();

                    ctx.ExecuteQuery();
                }
                ctx.Load(fol.Files);
                ctx.ExecuteQuery();
                //remove extra versions
                foreach (var existFile in fol.Files)
                {
                    ctx.Load(existFile.Versions);
                    ctx.ExecuteQuery();

                    foreach (var existVersion in existFile.Versions)
                    {
                        if (existVersion.Created > DateTime.Now.AddDays(-1))
                        {
                            existFile.Versions.DeleteByID(existVersion.ID);
                        }
                    }
                }

                //Loop through all subfolders and call the function recursively
                foreach (SPFolder subFolder in folder.SubFolders)
                {
                    if (subFolder.Name != "Forms")
                    {
                        var itemCreation = new ListItemCreationInformation();
                        itemCreation.UnderlyingObjectType = FileSystemObjectType.Folder;
                        itemCreation.LeafName = subFolder.Name;
                        itemCreation.FolderUrl = fol.ServerRelativeUrl;
                        
                        var newFolder = spList2.AddItem(itemCreation);
                        newFolder["Title"] = subFolder.Name;

                        newFolder.Update();
                        ctx.ExecuteQuery();
                        GetFiles(subFolder, spList1, spList2, spWeb2, ctx, usersString);
                    }
                }

            } catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.ReadKey();
            }
        }
    
        public static void MoveFile(SPFile file, SPFolder folder, ClientContext ctx, Folder fol, Web spWeb2, string usersString)
        {
            var newPath1 = Temp + file.Name;
            using (var webClient1 = new WebClient())
            {
                webClient1.Credentials = new NetworkCredential(Login, Password, Domain);
                webClient1.DownloadFile(SWebUrl + folder.ServerRelativeUrl + '/' + file.Name, newPath1);
            }
            using (var fs = new FileStream(newPath1, FileMode.OpenOrCreate))
            {
                File.SaveBinaryDirect(ctx, fol.ServerRelativeUrl + "/" + file.Name, fs, true);
            }
            var fileUpload1 = spWeb2.GetFileByServerRelativeUrl(fol.ServerRelativeUrl + "/" + file.Name);
            ctx.Load(fileUpload1);
            ctx.ExecuteQuery();

            //Adding correct metadata to the file  
            var item1 = fileUpload1.ListItemAllFields;
            ctx.Load(item1);
            ctx.ExecuteQuery();
            var modifiedBy1 = file.ModifiedBy;

            item1["Modified"] = file.TimeLastModified;

            //check user
            if (usersString.ToLower().Contains(modifiedBy1.LoginName.ToLower()))
            {
                item1["Editor"] = spWeb2.EnsureUser(modifiedBy1.LoginName);
            }
            else
            {
                item1["Editor"] = spWeb2.EnsureUser(@"SHAREPOINT\system");
            }
            var num1 = item1["_UIVersionString"].ToString();
            double result1;
            double.TryParse(num1.Replace('.', ','), out result1);
            result1 = result1 + 0.01;
            item1["_UIVersionString"] = result1.ToString();
            item1.Update();

            ctx.ExecuteQuery();
        }
    }
}
