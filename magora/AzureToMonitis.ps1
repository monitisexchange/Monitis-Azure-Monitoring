# function converts a collection to array
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

#Monitis API key
$apikey = "1J2GOD370620P229PP346FM6U1"

#Monitis Secret key
$apisecretkey = "38H92D44TDFGFDREU9ENQMPJJL"

#Name of custom monitor
$monitorName = "newazure2"

#storage for performance counters data
$storageAccountName = "perfdata"

#key to storage
$storageAccountKey = "zbcl9Na/BLk9TeIwlH8Ygdkc2AzlkSXcCTBy/gLmQ+pbh8hCoSzHXhmP3MTVWDR/77CBt+5cA4GHB5I0zdTI6Q=="

# deployment slot for the azure role
$deploymentSlot = "Staging"

# name of the role to load performance counters data
$serviceName = "testservice1326"

# start time (in hours) from current time
$startHours = -72

# end time (in hours) from current time
$endHours = 0;

# Add Azure Snap-in
if ((Get-PSSnapin | ?{ $_.Name -eq "WAPPSCmdlets" }) -eq $null)
{
	Add-PSSnapin "WAPPSCmdlets"
}

#Import Monitis module
Import-Module c:\Users\kokorin.MAGORA\Downloads\monitis\Module\Monitis.psm1

#publish settings for azure account
$subscript = Import-Subscription -PublishSettingsFile "c:\Users\kokorin.magora\downloads\MyTestCredentials.publishsettings"

# available monitis types
$intType = [System.Type]::GetType("System.Int32")
$floatType = [System.Type]::GetType("System.Single")
$stringType = [System.Type]::GetType("System.String")

# time performance data column name
$time = "(PDH-CSV 4.0) (GMT Standard Time)(0)"

# performance counters data column names and types
# Property is name in performance data document
# Type is type that is used in custom monitor table
# Name is name of columns in custom monitor table
$properties = @(
# ASP.NET 4.0
@{
Property = "\ASP.NET Apps v4.0.30319`(__Total__`)\Requests/Sec"
Type = $floatType
Name = "Total Requests/Sec"
},
@{
Property = "\ASP.NET v4.0.30319\Request Execution Time"
Type = $intType
Name = "Request Execution Time, ms"
},
@{
Property = "\ASP.NET v4.0.30319\Request Wait Time"
Type = $intType
Name = "Request Wait Time, ms"
},
@{
Property = "\ASP.NET v4.0.30319\Requests Current"
Type = $intType
Name = "Requests Current"
},
@{
Property = "\ASP.NET v4.0.30319\Requests Queued"
Type = $intType
Name = "Requests Queued"
},
@{
Property = "\ASP.NET v4.0.30319\Requests Rejected"
Type = $intType
Name = "Requests Rejected"
},
# ASP.NET Process
@{
Property = "\Process(w3wp)\% Processor Time"
Type = $floatType
Name = "Process Processor Time, percent"
},
@{
Property = "\Process(w3wp)\Handle Count"
Type = $intType
Name = "Process Handle Count"
},
@{
Property = "\Process(w3wp)\Private Bytes" 
Type = $intType
Name = "Process Private Bytes"
},
@{
Property = "\Process(w3wp)\Virtual Bytes"
Type = $intType
Name = "Process Virtual Bytes"
},
# Memory
@{
Property = "\Memory\Available MBytes"
Type = $intType
Name = "Available Memory, MB"
},
@{
Property = "\Memory\Committed Bytes"
Type = $intType
Name = "Memory Committed Bytes"
},
# TCP
@{
Property = "\TCPv4\Segments Sent/sec"
Type = $floatType
Name = "TCP Segments Sent/sec"
},
@{
Property= "\TCPv4\Connections Established"
Type = $intType
Name = "TCP Connections Established"
},
# Disk
@{
Property = "\LogicalDisk(_Total)\% Free Space"
Type = $floatType
Name = "Total Free Space, percent"
},
@{
Property = "\LogicalDisk(_Total)\Free Megabytes"
Type = $intType
Name = "Total Free Space, MB"
},
# Processor
@{
Property = "\Processor(_Total)\% Processor Time"
Type = $floatType;
Name = "Total Processor Time, percent"
},
# Network Interface
@{
Property = "\Network Interface(Microsoft Virtual Machine Bus Network Adapter _3)\Bytes Received/sec"
Type = $floatType
Name = "Bytes Received/sec"
},
@{
Property = "\Network Interface(Microsoft Virtual Machine Bus Network Adapter _3)\Bytes Sent/sec"
Type = $floatType
Name = "Bytes Sent/sec"
},
@{
Property = "\TCPv4\Connection Failures"
Type = $intType
Name = "TCP Connection Failures"
},
@{
Property = "\TCPv4\Connections Reset"
Type = $intType
Name = "TCP Connections Reset"
},
# .NET CLR
@{
Property  = "\.NET CLR Memory(_Global_)\% Time in GC"
Type = $floatType
Name = "Time in GC, percent"
},
@{
Property = "\.NET CLR Memory(_Global_)\# Bytes in all Heaps"
Type = $intType
Name = "Bytes in all Heaps"
}
)

# create subscription parameters
Set-Subscription -SubscriptionName "mysub" -SubscriptionId $subscript.SubscriptionId -Certificate $subscript.Certificate

# set subscription parameters as current
Select-Subscription -SubscriptionName "mysub"

# connect to monitis
Connect-Monitis -ApiKey $apikey -SecretKey $apisecretkey

# try to get custom monitor by name
$monitor = Get-MonitisCustomMonitor -Name $monitorName

# if monitor with the specified name is not found then create it
if ($monitor -eq $null)
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

# get service deployment
$deployment = Get-Deployment -Slot $deploymentSlot -ServiceName $serviceName

# get roles from deployment that have enabled performance counters
$roles = Get-DiagnosticAwareRoles -DeploymentId $deployment.DeploymentId -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey

# step hour to extract data
$stepHours = 1;

# end time to extract
$lastDate = [DateTime]::Now.AddHours($endHours)

foreach ($role in $roles)
{
	# start date and time to extract
	$startDate = [DateTime]::Now.AddHours($startHours)
	
	# end date and time to extract
	$endDate = $startDate.AddHours($stepHours)
	while ($startDate -lt $lastDate)
	{
		$tempFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.IO.Path]::GetRandomFileName())
		#$tempFolder = [System.IO.Path]::Combine("D:\info", [System.IO.Path]::GetRandomFileName())
		
		# create temp folder to store performance counters data
		[System.IO.Directory]::CreateDirectory($tempFolder)
		
		# try to download performance counters data several times, there is a problem with loading data because of fixed timeout for azure table service
		$iteration = 0;
		while ($iteration -lt 5)
		{
			try
			{
				# load performance counters data
				Get-PerfmonLog -Format CSV -LocalPath $tempFolder -FromUtc $startDate -ToUtc $endDate -DeploymentId $deployment.DeploymentId -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey -RoleName $role.RoleName
				if ($?)
				{
					break;
				}
			}
			catch
			{
			}
			
			Write-Host "Time: "([DateTime]::Now)" Loading info failed. Try "$iteration
			$iteration++;
		}
		
		# if data is not loaded then nothing to process move to next time range
		if ($iteration -eq 5)
		{
			Write-Host "Time: "([DateTime]::Now)" Range from "$startDate" to "$endDate" is skipped"
			$startDate = $startDate.AddHours($stepHours)
			$endDate = $endDate.AddHours($stepHours)
			if ($lastDate -lt $endDate)
			{
				$endDate = $lastDate
			}
			
			continue;
		}
		
		Write-Host "Time: "([DateTime]::Now)" Info loaded for range from "$startDate" to "$endDate
		
		# parse loaded data
		$updates = dir $tempFolder | Import-Csv
		
		# get role instance name
		$instance = Get-DiagnosticAwareRoleInstances -DeploymentId $deployment.DeploymentId -StorageAccountName $storageAccountName -StorageAccountKey $storageAccountKey -RoleName $role.RoleName
		
		# process loaded data
		foreach ($update in $updates)
		{
			# extract each property and save it to hash table
			$item = @{}
			foreach ($property in $properties)
			{
				$value = $update.("\\" + $instance + $property.Property)
				if ([System.String]::IsNullOrEmpty($value))
				{
					$value = 0;
				}
				
				$item.Add($property.Name, $value)
			}

			$timevalue = $update.$time
			
			# load new item to the custom monitor table
			Update-MonitisCustomMonitor -TestId $monitor.MonitisTestId -value $item -CheckTime $timevalue
		}
		
		Write-Host "Time: "([DateTime]::Now)" Info processed"
	
		[System.IO.Directory]::Delete($tempFolder,$true)
		
		# move to next time
		$startDate = $startDate.AddHours($stepHours)
		$endDate = $endDate.AddHours($stepHours)
		if ($lastDate -lt $endDate)
		{
			$endDate = $lastDate
		}
	}
}