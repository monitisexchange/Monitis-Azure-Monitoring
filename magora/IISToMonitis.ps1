Import-Module WebAdministration
Import-Module "[Add path to monitis module]"

function ToArray
{
  begin
  {
    $output = @(); 
  }
  process
  {
    $output += $_; 
  }
  end
  {
    return ,$output; 
  }
}

$apikey = "[your API key]"
$apisecretkey = "[your secret API key]"

$intType = [System.Type]::GetType("System.Int32")
$floatType = [System.Type]::GetType("System.Single")
$stringType = [System.Type]::GetType("System.String")

$properties = @(
	@{
		Property = "Name"
		Type = $stringType
		Name = "Site Name"
	},
	@{
		Property = "CurrentConnections"
		Type = $intType
		Name = "Current connections count"
	},
	@{
		Property = "CurrentNonAnonymousUsers"
		Type = $intType
		Name = "Current non anonymous users on site"
	},
	@{
		Property = "CurrentAnonymousUsers"
		Type = $intType
		Name = "Current anonymous users on site"
	},
	@{
		Property = "TotalBytesSent"
		Type = $floatType
		Name = "Total bytes sent by site"
	},
	@{
		Property = "TotalBytesReceived"
		Type = $floatType
		Name = "Total bytes received by site"
	},
	@{
		Property = "TotalBytesTransferred"
		Type = $floatType
		Name = "Total bytes transferred by site"
	},
	@{
		Property = "TotalGetRequests"
		Type = $floatType
		Name = "Total GET requests"
	},
	@{
		Property = "TotalPostRequests"
		Type = $floatType
		Name = "Total POST Requests"
	},
	@{
		Property = "TotalNotFoundErrors"
		Type = $intType
		Name = "Total 'Not Found' Errors on site"
	},
	@{
		Property = "AnonymousUsersPersec"
		Type = $intType
		Name = "Anonymous Users Per second"
	},
	@{
		Property = "BytesReceivedPersec"
		Type = $intType
		Name = "Bytes Received Per second"
	},
	@{
		Property = "BytesSentPersec"
		Type = $intType
		Name = "Bytes Sent Per second"
	},
	@{
		Property = "BytesTotalPersec"
		Type = $intType
		Name = "Total Bytes Per second"
	},
	@{
		Property = "FilesReceivedPersec"
		Type = $intType
		Name = "Bytes Received Per second"
	},
	@{
		Property = "FilesSentPersec"
		Type = $intType
		Name = "Bytes Sent Per second"
	},
	@{
		Property = "FilesPersec"
		Type = $intType
		Name = "Total Bytes Per second"
	},
	@{
		Property = "TotalFilesReceived"
		Type = $intType
		Name = "Bytes Received Per second"
	},
	@{
		Property = "TotalFilesSent"
		Type = $intType
		Name = "Bytes Sent Per second"
	},
	@{
		Property = "TotalFilesTransferred"
		Type = $intType
		Name = "Total Bytes Per second"
	},
	@{
		Property = "CGIRequestsPersec"
		Type = $intType
		Name = "Bytes Received Per second"
	},
	@{
		Property = "CurrentCGIRequests"
		Type = $intType
		Name = "Current CGI Requests"
	},
	@{
		Property = "ConnectionAttemtpsPersec"
		Type = $intType
		Name = "Connection Attemtps Per second"
	},
	@{
		Property = "HeadRequestsPersec"
		Type = $intType
		Name = "Head Requests Persec"
	},
	@{
		Property = "LockRequestsPersec"
		Type = $intType
		Name = "Lock Requests Persec"
	},
	@{
		Property = "LogonRequestsPersec"
		Type = $intType
		Name = "Logon Requests Persec"
	},
	@{
		Property = "DeleteRequestsPersec"
		Type = $intType
		Name = "Delete Requests Persec"
	},
	@{
		Property = "MkcolRequestsPersec"
		Type = $intType
		Name = "Mkcol Requests Persec"
	},
	@{
		Property = "MoveRequestsPersec"
		Type = $intType
		Name = "Move Requests Persec"
	},
	@{
		Property = "OptionsRequestsPersec"
		Type = $intType
		Name = "Options Requests Persec"
	},
	@{
		Property = "PostRequestsPersec"
		Type = $intType
		Name = "Post Requests Persec"
	},
	@{
		Property = "GetRequestsPersec"
		Type = $intType
		Name = "Get Requests Persec"
	},
	@{
		Property = "PutRequestsPersec"
		Type = $intType
		Name = "Put Requests Persec"
	},
	@{
		Property = "SearchRequestsPersec"
		Type = $intType
		Name = "Search Requests Persec"
	},
	@{
		Property = "ServiceUptime"
		Type = $intType
		Name = "Service Uptime"
	},
	@{
		Property = "CopyRequestsPersec"
		Type = $intType
		Name = "Copy Requests Persec"
	},
	@{
		Property = "TotalCGIRequests"
		Type = $intType
		Name = "Total CGI Requests"
	},
	@{
		Property = "TotalCopyRequests"
		Type = $intType
		Name = "Total Copy Requests"
	},
	@{
		Property = "TotalDeleteRequests"
		Type = $intType
		Name = "Total Delete Requests"
	},
	@{
		Property = "TotalHeadRequests"
		Type = $intType
		Name = "Total Head Requests"
	},
	@{
		Property = "TotalLockRequests"
		Type = $intType
		Name = "Total Lock Requests"
	},
	@{
		Property = "TotalLogonAttempts"
		Type = $intType
		Name = "Total Logon Attempts"
	},
	@{
		Property = "TotalMkcolRequests"
		Type = $intType
		Name = "Total Mkcol Requests"
	},
	@{
		Property = "TotalMoveRequests"
		Type = $intType
		Name = "Total Move Requests"
	},
	@{
		Property = "TotalOptionsRequests"
		Type = $intType
		Name = "Total Options Requests"
	},
	@{
		Property = "TotalPutRequests"
		Type = $intType
		Name = "Total Put Requests"
	}
)

$propNames = @()
foreach($prop in $properties)
{
	$propNames += $prop["Property"]
}

$wsParams = Get-WmiObject -Namespace root\CIMV2 -Class Win32_PerfFormattedData_W3SVC_WebService -ComputerName . | Select -Property $propNames

Connect-Monitis -ApiKey $apikey -SecretKey $apisecretkey

foreach($entry in $wsParams)
{
	$monitorName = $entry.Name
	$monitor = Get-MonitisCustomMonitor -Name $monitorName

	if($monitor -eq $null)
	{
		# names of columns for custom monitor table
		$names = [String[]]($properties | %{[string]$_.Name} | ToArray)
		
		# types of columns for custom monitor table
		$types = [Type[]]($properties | %{[Type]$_.Type} | ToArray)
		
		# create custom monitor
		Add-MonitisCustomMonitor -Name $monitorName -Parameter $names -Type $types
		
		# again try to get custom monitor
		$monitor = Get-MonitisCustomMonitor -Name $monitorName
		
		# if monitor is not found then it is an error, the script cannot be performed
		if ($monitor -eq $null)
		{
			Write-Error -Message "Monitor was not added"
			return;
		}
	}
	
	$item = @{}
	$updateTime = $([System.DateTime]::Now)
	foreach($prop in $properties)
	{
		$name = $prop.Property
		$value = $entry.$name
		if ([System.String]::IsNullOrEmpty($value))
		{
			$value = 0;
		}
		$item.Add($prop.Name, $value)
	}
	Update-MonitisCustomMonitor -TestId $monitor.MonitisTestId -value $item -CheckTime $updateTime
	Write-Host $entry.Name " uploaded"
}

Write-Host "All data uploaded"