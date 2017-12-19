[System.Reflection.Assembly]::LoadWithPartialName(“Microsoft.SharePoint.Client”)
[System.Reflection.Assembly]::LoadWithPartialName(“Microsoft.SharePoint.Client.Runtime”)
Add-PSSnapin Microsoft.SharePoint.PowerShell

$sWeb = "http://mysite2010/subsite/subsite"
$sWebUrl = "http://mysite2010/subsite/subsite"
$dWeb = "http://mysite2013"
$temp = "C:\Temp\"

Function Invoke-LoadMethod() {
param(
   $ctx,
   $clientObject
)
   $load = [Microsoft.SharePoint.Client.ClientContext].GetMethod("Load")
   $type = $clientObject.GetType()
   $genericMethodInvoker = $load.MakeGenericMethod($type)
   $genericMethodInvoker.Invoke($ctx,@($clientObject,$null))
}

$credentials = new-object System.Net.NetworkCredential("user", "pass", "domain")

$sLibrary = "SourceLib"
$sFolderPath = "/SourceLib/Subfolder"
$dLibrary = "DestLib"
$dFolderPath = "/DestLib/Subfolder/Subfolder/Subfolder"


#$ctx1 = New-Object Microsoft.SharePoint.Client.ClientContext($sWeb)
#$ctx1.Credentials = $credentials
#$ctx1.ExecuteQuery()
	
$ctx2 = New-Object Microsoft.SharePoint.Client.ClientContext($dWeb)
$ctx2.Credentials = $credentials
$ctx2.ExecuteQuery()

$spWeb1 = Get-SPWeb $sWeb

#$spWeb1 = $ctx1.Web
#$ctx1.Load($spWeb1)
#$ctx1.ExecuteQuery()

$spWeb2 = $ctx2.Web
Invoke-LoadMethod -ctx $ctx2 -clientObject $spWeb2
#$ctx2.Load($spWeb2)
$ctx2.ExecuteQuery()

 #users
                        
#$ctx2.Load($spWeb2.sitegroups) 
Invoke-LoadMethod -ctx $ctx2 -clientObject $spWeb2.sitegroups
$ctx2.ExecuteQuery()  

$usersString = ""

foreach($spGroup in $spWeb2.sitegroups)

{ 

    #$ctx2.Load($spGroup) 
    Invoke-LoadMethod -ctx $ctx2 -clientObject $spGroup
    $ctx2.ExecuteQuery() 

    $spSiteUsers = $spGroup.Users 

    #$ctx2.Load($spSiteUsers) 
	Invoke-LoadMethod -ctx $ctx2 -clientObject $spSiteUsers
    $ctx2.ExecuteQuery() 

    foreach($spUser in $spSiteUsers)

    { 
        $usersString += $spUser.LoginName + ";" 

    } 

}

$usersString = $usersString.Replace('user','')


$spList1 = $spWeb1.Lists[$sLibrary]
#$spList1 = $ctx1.Web.Lists.GetByTitle($sLibrary)
#$ctx1.Load($spList1)
#$ctx1.ExecuteQuery()

$spList2 = $ctx2.Web.Lists.GetByTitle($dLibrary)
#$ctx2.Load($spList2)
Invoke-LoadMethod -ctx $ctx2 -clientObject $spList2
$ctx2.ExecuteQuery()

#$ctx1.Load($spList1.RootFolder)

#$ctx1.ExecuteQuery()

	function GetFiles($Folder)
	{ 
	    #$ctx1.Load($Folder)
        #$ctx1.ExecuteQuery()
		
        #$ctx1.Load($Folder.Files)
        #$ctx1.ExecuteQuery()

        Write-Host "+"$Folder.Name

		    foreach($file in $Folder.Files)
		    {	
			    Write-Host "`t" $file.Name				 
            
                if ($Folder -eq $spList1.RootFolder){
                    $fol = $spWeb2.GetFolderByServerRelativeUrl($dFolderPath)  
                } else {
               
                    $fol = $spWeb2.GetFolderByServerRelativeUrl($dFolderPath + $Folder.ServerRelativeUrl.Replace($sFolderPath,'')) 
                }
            
                #$ctx2.Load($fol)
				Invoke-LoadMethod -ctx $ctx2 -clientObject $fol
                $ctx2.ExecuteQuery()
                
                #$ctx1.Load($file.Versions)
				
                #$ctx1.ExecuteQuery()

                 foreach($version in $file.Versions)
                {
                    $WebClient = New-Object System.Net.WebClient
                    $WebClient.Credentials = $credentials
                    
                    $new = $temp + $file.Name
                    $WebClient.DownloadFile($sWeb + '/' +$version.Url,$new)

                                       $fileStream = ([System.IO.FileInfo] (Get-Item $new)).OpenRead()

                    $fileCreationInfo = New-Object Microsoft.SharePoint.Client.FileCreationInformation
                    $fileCreationInfo.Overwrite = $true
                    $fileCreationInfo.ContentStream = $fileStream
                    $fileCreationInfo.URL = $file.Name
                    $fileUpload = $fol.Files.Add($fileCreationInfo) 

               
                    #$ctx2.Load($fileUpload)
					Invoke-LoadMethod -ctx $ctx2 -clientObject $fileUpload
                    $ctx2.ExecuteQuery()
                    
                       #set metadata
                        $ListItem = $fileUpload.ListItemAllFields
                        $ListItem["Modified"] = $version.Created

                        $CreatedBy = $version.CreatedBy

                         #$ctx1.Load($CreatedBy)
                         #$ctx1.ExecuteQuery()
                         
                         
                        
                        
                        if ($usersString -match $CreatedBy.LoginName.Replace("\","\\"))   {
                                
                             $ListItem["Editor"] = $spWeb2.EnsureUser($CreatedBy.LoginName)
                        }   
                        else 
                        {
                            $ListItem["Editor"] = $spWeb2.EnsureUser("SHAREPOINT\system")
                        }
                       
                        $ListItem.Update()

               
                        $ctx2.ExecuteQuery()
                    
                }
            
                $WebClient = New-Object System.Net.WebClient
                $WebClient.Credentials = $credentials
            
                $new=$temp + $file.Name 
                $WebClient.DownloadFile($sWebUrl + $Folder.ServerRelativeUrl + '/' + $file.Name,$new)

                $fileStream = ([System.IO.FileInfo] (Get-Item $new)).OpenRead()                $fileCreationInfo = New-Object Microsoft.SharePoint.Client.FileCreationInformation
                $fileCreationInfo.Overwrite = $true
                $fileCreationInfo.ContentStream = $fileStream
            
                $fileCreationInfo.URL =$file.Name
                # Upload document

            $fileUpload = $fol.Files.Add($fileCreationInfo)
     
            # Adding correct metadata to the file  
            $ListItem = $fileUpload.ListItemAllFields
            $ModifiedBy = $file.ModifiedBy

             #$ctx1.Load($ModifiedBy)
             #$ctx1.ExecuteQuery()
        
       
            $ListItem["Modified"] = $file.TimeLastModified
            #check user
                        
            if ($usersString -match $CreatedBy.LoginName.Replace("\","\\"))   {

                    $ListItem["Editor"] = $spWeb2.EnsureUser($CreatedBy.LoginName)
            }  
             
            else 

            {
                $ListItem["Editor"] = $spWeb2.EnsureUser("SHAREPOINT\system")
            }
             
            $ListItem.Update()

           
            $ctx2.ExecuteQuery()
		       
            }
		#$ctx1.Load($Folder.Folders)
        #$ctx1.ExecuteQuery()

         # remove extra versions
            foreach ($existFile in $fol.Files){            
                
                #$ctx2.Load($existFile.Versions)
				Invoke-LoadMethod -ctx $ctx2 -clientObject $existFile.Versions
                $ctx2.ExecuteQuery()
                $versionsToDelete = @()
                    
                foreach ($existVersion in $existFile.Versions)
                        {
                            if ($existVersion.Created -gt ($(Get-Date).AddDays(-1)))
                            {
                                $versionsToDelete += $existVersion
                            }               
                        }
                        	
                foreach($versionToDelete in $versionsToDelete) 
                    {
                        $versionToDelete.DeleteObject()
                    }
	        }


		#Loop through all subfolders and call the function recursively
		#foreach ($SubFolder in $Folder.Folders)
		foreach ($SubFolder in $Folder.SubFolders)
		{
            
            if ($Folder -eq $spList1.RootFolder){
            $fol = $spWeb2.GetFolderByServerRelativeUrl($dFolderPath)  
            } else {
               
                $fol = $spWeb2.GetFolderByServerRelativeUrl($dFolderPath + $Folder.ServerRelativeUrl.Replace($sFolderPath,'')) 
            }
            
           

			if($SubFolder.Name -ne "Forms")
			{  
                $folder1 = $fol.Folders.Add($SubFolder.Name)
				$folder1.Update()
                #$ctx2.Load($folder1)
				Invoke-LoadMethod -ctx $ctx2 -clientObject $folder1
                $ctx2.ExecuteQuery()
				Write-Host "`t" -NoNewline
				GetFiles($Subfolder)		 
			}
		}
	}    

			
	#GetFiles ($spList1.RootFolder)

	#GetFiles ($spWeb1.GetFolderByServerRelativeUrl($sFolderPath))
	GetFiles ($spWeb1.GetFolder($sFolderPath))
	$spList2.Update()
		
	   
#$ctx1.Dispose() 
$ctx2.Dispose() 
