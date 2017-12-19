[System.Reflection.Assembly]::LoadWithPartialName(“Microsoft.SharePoint.Client”)
[System.Reflection.Assembly]::LoadWithPartialName(“Microsoft.SharePoint.Client.Runtime”)
Add-PSSnapin Microsoft.SharePoint.PowerShell

$sWeb = "http://apprentice:801"
$dWeb = "http://apprentice:802"
$temp = "C:\Temp\"

$credentials = new-object System.Net.NetworkCredential("roman.melnik", "dowdyebry", "spellabs")

$sLibrary = "Документы"
$dLibrary = "Shared Documents"
$dFolderPath = "/Shared Documents/ACCENTURE"

$ctx1 = New-Object Microsoft.SharePoint.Client.ClientContext($sWeb)
$ctx1.Credentials = $credentials
$ctx1.ExecuteQuery()
	
$ctx2 = New-Object Microsoft.SharePoint.Client.ClientContext($dWeb)
$ctx2.Credentials = $credentials
$ctx2.ExecuteQuery()

$spWeb1 = $ctx1.Web
$ctx1.Load($spWeb1)
$ctx1.ExecuteQuery()

$spWeb2 = $ctx2.Web
$ctx2.Load($spWeb2)
$ctx2.ExecuteQuery()

$spList1 = $ctx1.Web.Lists.GetByTitle($sLibrary)
$ctx1.Load($spList1)
$ctx1.ExecuteQuery()

$spList2 = $ctx2.Web.Lists.GetByTitle($dLibrary)
$ctx2.Load($spList2)
$ctx2.ExecuteQuery()

$ctx1.Load($spList1.RootFolder)

$ctx1.ExecuteQuery()

	function GetFiles($Folder)
	{ 
	    Write-Host "+"$Folder.Name
		
        $ctx1.Load($Folder.Files)
        $ctx1.ExecuteQuery()

		foreach($file in $Folder.Files)
		{	
			Write-Host "`t" $file.Name				 
            
            if ($Folder -eq $spList1.RootFolder){
            $fol = $spWeb2.GetFolderByServerRelativeUrl($dFolderPath)  
            } else {
               
                $fol = $spWeb2.GetFolderByServerRelativeUrl($dFolderPath + $Folder.ServerRelativeUrl.Replace('/Shared Documents','')) 
            }
            
            $ctx2.Load($fol)
            $ctx2.ExecuteQuery()

            start-sleep -seconds 1

            $ctx1.Load($file.Versions)
            $ctx1.ExecuteQuery()

            foreach($version in $file.Versions)
            {
                $WebClient = New-Object System.Net.WebClient
                $WebClient.Credentials = $credentials
                #$sBytes = $WebClient.DownloadData($spWeb1.Url + '/' +$version.Url)

                $new = $temp + $file.Name
                $WebClient.DownloadFile($spWeb1.Url + '/' +$version.Url,$new)

                #$File1= Get-ChildItem -Force $new -Recurse
                $sBytes = [Microsoft.SharePoint.Client.File]::OpenBinaryDirect($ctx1, $file.ServerRelativeUrl)
                [System.IO.FileStream] $writeStream = [System.IO.File]::Open($new,[System.IO.FileMode]::Create);                $sBytes.Stream.CopyTo($writeStream)
                $writeStream.Close()                $fileStream = ([System.IO.FileInfo] (Get-Item $new)).OpenRead()

                $fileCreationInfo = New-Object Microsoft.SharePoint.Client.FileCreationInformation
                $fileCreationInfo.Overwrite = $true
                #$fileCreationInfo.Content = $sBytes
                $fileCreationInfo.ContentStream = $fileStream
                $fileCreationInfo.URL = $file.Name
                $fileUpload = $fol.Files.Add($fileCreationInfo) 

               
                $ctx2.Load($fileUpload)
                $ctx2.ExecuteQuery()

                #$ListItem = $fileUpload.ListItemAllFields
                #$ListItem["Modified"] = $version.Created
                
                #$ListItem.Update()

                #$ctx2.Load($fileUpload)
                #$ctx2.ExecuteQuery()
            }
            
            $WebClient = New-Object System.Net.WebClient
            $WebClient.Credentials = $credentials
            
            $new=$temp + $file.Name 
            $WebClient.DownloadFile($spWeb1.Url + $Folder.ServerRelativeUrl + '/' + $file.Name,$new)

            #$File1= Get-ChildItem -Force $new -Recurse
            $sBytes = [Microsoft.SharePoint.Client.File]::OpenBinaryDirect($ctx1,$file.ServerRelativeUrl)
            [System.IO.FileStream] $writeStream = [System.IO.File]::Open($new, [System.IO.FileMode]::Create);            $sBytes.Stream.CopyTo($writeStream)
            $writeStream.Close()            $fileStream = ([System.IO.FileInfo] (Get-Item $new)).OpenRead()            $fileCreationInfo = New-Object Microsoft.SharePoint.Client.FileCreationInformation
            $fileCreationInfo.Overwrite = $true
            $fileCreationInfo.ContentStream = $fileStream
            
            $fileCreationInfo.URL =$file.Name
            $fileUpload = $fol.Files.Add($fileCreationInfo)
               
            $ctx2.Load($fileUpload)
            $ctx2.ExecuteQuery()

            #$ListItem = $fileUpload.ListItemAllFields
            #$ListItem["Modified"] = $file.TimeLastModified
            #$ListItem["Author"] = $file.Author
            #$ListItem.Update()

            #$ctx2.Load($fileUpload)
            #$ctx2.ExecuteQuery()
  
		}
		$ctx1.Load($Folder.Folders)
        $ctx1.ExecuteQuery()

		#Loop through all subfolders and call the function recursively
		foreach ($SubFolder in $Folder.Folders)
		{
			if($SubFolder.Name -ne "Forms")
			{  
                $folder1 = $fol.Folders.Add($SubFolder.Name)
				$folder1.Update()
                $ctx2.Load($folder1)
                $ctx2.ExecuteQuery()
				Write-Host "`t" -NoNewline
				GetFiles($Subfolder)		 
			}
		}
	}    

			
	GetFiles ($spList1.RootFolder)
	$spList2.Update()
		
	   
$ctx1.Dispose() 
$ctx2.Dispose() 
