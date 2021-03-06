#region Parameters

#Monitis API key
$apikey = "[your API key]"

#Monitis Secret key
$apisecretkey = "[your API secret key]"
# Address of Azure Sql Server
$server = "[your Azure SQL Server address]";

# Database name to monitoring
$database = "[your database name]";

# Need for a few queries
$masterDb = "master"

# Azure SQL credentials
$login = "[your Azure SQL Login]";
$pass = "[your Azure SQL pass]";
$cs1 = [string]::Format("Data Source={0}; Initial Catalog={1}; User={2}; Password={3};",
                        $server, $database, $login, $pass);
$cs2 = [string]::Format("Data Source={0}; Initial Catalog={1}; User={2}; Password={3};",
                        $server, $masterDb, $login, $pass);
#endregion

#region Functions

# Executes specified query on Azure SQL Server and returns resultin data table
function ExecuteQuery($connectionString, $query)
{
	$conn = New-Object System.Data.SqlClient.SqlConnection;
	$conn.ConnectionString = $connectionString;
	
	$command = New-Object System.Data.SqlClient.SqlCommand;
	$command.CommandText = $query;
	$command.Connection = $conn;
	
	$SqlAdapter = New-Object System.Data.SqlClient.SqlDataAdapter;
	$SqlAdapter.SelectCommand = $command;
	$DataSet = New-Object System.Data.DataSet;
	$nRec = $SqlAdapter.Fill($DataSet);
	$nRec | Out-Null
	$conn.Close();
	
	return ,$DataSet.Tables[0];
}

# Creates monitor with specified name and data table columns
function CreateMonitor($monitorName, $table)
{
	$monitor = Get-MonitisCustomMonitor -Name $monitorName
	
	# if monitor is not exists then we will create it
	if($monitor -eq $null)
	{
		$names = $table.Columns | Select-Object -Property Caption -expand Caption | ToArray
		$types = $table.Columns | Select-Object -Property DataType -expand DataType | ToArray
		
		Add-MonitisCustomMonitor -Name $monitorName -Parameter $names -Type $types
		
		$monitor = Get-MonitisCustomMonitor -Name $monitorName
		
		# error due to monitor creation
		if($monitor -eq $null)
		{
			Write-Error -Message "Monitor was not added"
			return
		}
	}
	
	return $monitor
}

# Uploads data from data table to specified monitor
function UploadDataToMonitor($monitor, $data)
{	
	for($i = 0; $i -lt $table.Rows.Count; $i++)
	{
		$items = @{}
		for($j = 0; $j -lt $table.Columns.Count; $j++)
		{
			$items.Add($table.Columns[$j].Caption, $($table.Rows[$i][$j]));
			#Write-Host $($table.Rows[$i][$j])
		}
		Update-MonitisCustomMonitor -TestId $monitor.MonitisTestId -value $items -CheckTime $([System.DateTime]::Now)
	}
}

# Converts collection to array
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

#endregion

#region AzureSQL Monitoring Queries
$AllTablesRowsCount = "SELECT OBJECT_NAME(object_id) AS [ObjectName], CONVERT(real, row_count)
						FROM sys.dm_db_partition_stats
						WHERE index_id < 2
						ORDER BY row_count DESC;";
						
$ConnectionsInCurrentDb = "SELECT CONVERT(int, s.session_id) AS session_id, s.login_name, CONVERT(nvarchar(300), e.connection_id) AS connection_id,
							      CONVERT(nvarchar(300), s.last_request_end_time) AS last_request_end_time, s.cpu_time,
							      CONVERT(nvarchar(300), e.connect_time) AS connect_time
							FROM sys.dm_exec_sessions AS s
							INNER JOIN sys.dm_exec_connections AS e
							ON s.session_id = e.session_id
							ORDER BY s.login_name;";

$HostSessionCount = "SELECT [host_name], COUNT(*) AS [SessionCount]
						FROM sys.dm_exec_sessions AS s
						GROUP BY [host_name]
						ORDER BY [host_name];";
						
$CachedPlansByExecCount = "SELECT q.[text], CONVERT(real, hcpu.total_worker_time) AS total_worker_time,
							       CONVERT(real, hcpu.execution_count) AS execution_count
							FROM
							    (SELECT TOP (50) qs.*
							     FROM sys.dm_exec_query_stats AS qs
							     ORDER BY qs.total_worker_time DESC) AS hcpu
							     CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS q
							ORDER BY hcpu.execution_count DESC;";
							
$CachedPlansByTotalWorkTime = "SELECT q.[text], CONVERT(real, hcpu.total_worker_time) AS total_worker_time,
								       CONVERT(real, hcpu.execution_count) AS execution_count
								FROM
								    (SELECT TOP (50) qs.*
								     FROM sys.dm_exec_query_stats AS qs
								     ORDER BY qs.total_worker_time DESC) AS hcpu
								     CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS q
								ORDER BY hcpu.total_worker_time DESC;";
								
$QueriesByAvgCpuTime = "SELECT TOP (25) MIN(query_stats.statement_text) AS [Statement Text],
						CONVERT(real, SUM(query_stats.total_worker_time) / SUM(query_stats.execution_count)) AS [Avg CPU Time]
						FROM (SELECT QS.*, SUBSTRING(ST.[text], (QS.statement_start_offset/2) + 1,
						    ((CASE statement_end_offset
						        WHEN -1 THEN DATALENGTH(st.[text])
						        ELSE QS.statement_end_offset END
						            - QS.statement_start_offset)/2) + 1) AS statement_text
						     FROM sys.dm_exec_query_stats AS QS
						     CROSS APPLY sys.dm_exec_sql_text(QS.sql_handle) AS ST) AS query_stats
						GROUP BY query_stats.query_hash
						ORDER BY [Avg CPU Time] DESC;";

$QueriesByTotalMemoryReading = "SELECT q.[text], CONVERT(real, hcpu.total_logical_reads) AS total_logical_reads,
								       CONVERT(real, hcpu.execution_count) AS execution_count
								FROM
								    (SELECT TOP (50) qs.*
								     FROM sys.dm_exec_query_stats AS qs
								     ORDER BY qs.total_worker_time DESC) AS hcpu
								     CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS q
								ORDER BY hcpu.total_logical_reads DESC;";

$PlansByTotalEstimatedTime = "SELECT q.[text], CONVERT(real, hcpu.total_elapsed_time) AS total_elapsed_time,
								       CONVERT(real, hcpu.execution_count) AS execution_count
								FROM
								    (SELECT TOP (50) qs.*
								     FROM sys.dm_exec_query_stats AS qs
								     ORDER BY qs.total_worker_time DESC) AS hcpu
								     CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS q
								ORDER BY total_elapsed_time DESC;";
#endregion
						
# Add Azure Snap-in
if ((Get-PSSnapin | ?{ $_.Name -eq "WAPPSCmdlets" }) -eq $null)
{
	Add-PSSnapin "WAPPSCmdlets"
}

#Import Monitis module
Import-Module [Add full path to monitis module]

#Connectiong to monitis
Connect-Monitis -ApiKey $apikey -SecretKey $apisecretkey

Write-Host "AllTablesRowsCount monitor updating..."
# You can specify your monitor name
$monitorName = "AllTablesRowsCount"
# Retreiving data table from Azure SQL database
$table = ExecuteQuery $cs1 $AllTablesRowsCount
# creating monitor with specified name and data table columns
$monitor = CreateMonitor $monitorName $table
# Uploading data to monitor
UploadDataToMonitor $monitor $table
Write-Host "AllTablesRowsCount monitor updated"

# Same for others
Write-Host "ConnectionsInCurrentDb monitor updating..."
$monitorName = "ConnectionsInCurrentDb"
$table = ExecuteQuery $cs1 $ConnectionsInCurrentDb
$monitor = CreateMonitor $monitorName $table
UploadDataToMonitor $monitor $table
Write-Host "ConnectionsInCurrentDb monitor updated"

Write-Host "HostSessionCount monitor updating..."
$monitorName = "HostSessionCount"
$table = ExecuteQuery $cs2 $HostSessionCount
$monitor = CreateMonitor $monitorName $table
UploadDataToMonitor $monitor $table
Write-Host "HostSessionCount monitor updated"

Write-Host "CachedPlansByExecCount monitor updating..."
$monitorName = "CachedPlansByExecCount"
$table = ExecuteQuery $cs1 $CachedPlansByExecCount
$monitor = CreateMonitor $monitorName $table
UploadDataToMonitor $monitor $table
Write-Host "CachedPlansByExecCount monitor updated"

Write-Host "CachedPlansByTotalWorkTime monitor updated..."
$monitorName = "CachedPlansByTotalWorkTime"
$table = ExecuteQuery $cs1 $CachedPlansByTotalWorkTime
$monitor = CreateMonitor $monitorName $table
UploadDataToMonitor $monitor $table
Write-Host "CachedPlansByTotalWorkTime monitor updated"

Write-Host "QueriesByAvgCpuTime monitor updating..."
$monitorName = "QueriesByAvgCpuTime"
$table = ExecuteQuery $cs1 $QueriesByAvgCpuTime
$monitor = CreateMonitor $monitorName $table
UploadDataToMonitor $monitor $table
Write-Host "QueriesByAvgCpuTime monitor updated"

Write-Host "QueriesByTotalMemoryReading monitor updating..."
$monitorName = "QueriesByTotalMemoryReading"
$table = ExecuteQuery $cs1 $QueriesByTotalMemoryReading
$monitor = CreateMonitor $monitorName $table
UploadDataToMonitor $monitor $table
Write-Host "QueriesByTotalMemoryReading monitor updated"

Write-Host "PlansByTotalEstimatedTime monitor updating..."
$monitorName = "PlansByTotalEstimatedTime"
$table = ExecuteQuery $cs1 $PlansByTotalEstimatedTime
$monitor = CreateMonitor $monitorName $table
UploadDataToMonitor $monitor $table
Write-Host "PlansByTotalEstimatedTime monitor updated"