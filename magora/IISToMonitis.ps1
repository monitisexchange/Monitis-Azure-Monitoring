Import-Module WebAdministration
Import-Module [Add full path to monitis module]

# converts collection to array
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
# Monitis Secret key
$apikey = "[your API key]"
#Monitis Secret API key
$apisecretkey = "[your API secret key]"

# Types supported by monitis
$intType = [System.Type]::GetType("System.Int32")
$floatType = [System.Type]::GetType("System.Single")
$stringType = [System.Type]::GetType("System.String")

# IIS properties to monitoring
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
	}
)

# Getting properties names
$propNames = @()
foreach($prop in $properties)
{
	$propNames += $prop["Property"]
}

# Getting values of specified properties for each site on IIS including _Total
$wsParams = Get-WmiObject -Namespace root\CIMV2 -Class Win32_PerfFormattedData_W3SVC_WebService -ComputerName . | Select -Property $propNames

Connect-Monitis -ApiKey $apikey -SecretKey $apisecretkey

# Creating monitor for each site and uploading data
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