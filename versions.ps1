Add-PSSnapin Microsoft.SharePoint.PowerShell
$spWeb1 = Get-SPWeb http://apprentice:801
$spWeb2 = Get-SPWeb http://apprentice:802

$spList1 = $spWeb1.Lists["Документы"];
$spList2 = $spWeb2.Lists["Shared Documents"];



	function GetFiles($Folder)
	{ 
	    Write-Host "+"$Folder.Name
		
		foreach($file in $Folder.Files)
		{	
			Write-Host "`t" $file.Name				
			$fol = $spWeb2.getfolder($Folder.Name)       
            foreach($version in $file.Versions)
            {
                $WebClient = New-Object System.Net.WebClient
				$WebClient.Credentials = new-object System.Net.NetworkCredential("roman.melnik", "dowdyebr", "spellabs")
                $sBytes = $WebClient.DownloadData($spWeb1.Url + '/' +$version.Url)
			    $newVersion = $fol.Files.Add($Folder.Url + "/" + $version.File.Name, $sBytes, $true)
            }

            $sBytes = $file.OpenBinary() 
			$dFile  = $fol.Files.Add($Folder.Url + "/" + $file.Name, $sBytes, $true) 
  
		}
		
		#Loop through all subfolders and call the function recursively
		foreach ($SubFolder in $Folder.SubFolders)
		{
			if($SubFolder.Name -ne "Forms")
			{  
				$folder1 = $spList2.AddItem($Folder, [Microsoft.SharePoint.SPFileSystemObjectType]::Folder, $SubFolder.Name)
				$folder1.Update()
				Write-Host "`t" -NoNewline
				GetFiles($Subfolder)		 
			}
		}
	}    

			
	GetFiles ($spList1.RootFolder)
	$spList2.Update()
		
	   

