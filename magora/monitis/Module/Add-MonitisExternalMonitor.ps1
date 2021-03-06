function Add-MonitisExternalMonitor 
{
    <#
    .Synopsis
        Adds a monitor for an external website to Monitis
    .Description
        Adds a monitor for an external website to Monitis, the platform that lets you monitor anything. 
        
        External monitors allow you to regularly monitor a web resource to ensure it's up and running.                
    .Example
        Add-MonitisExternalMonitor MonitisWebSite -Tag MonitisWebSites -MonitorType http -Url www.monitis.com
    .Link
        Get-MonitisExternalMonitor
    .Link
        Remove-MonitisExternalMonitor
    .Link
        Suspend-MonitisExternalMonitor
    #>
    param(
    # The name of the monitor
    [Parameter(Mandatory=$true,ValueFromPipelineByPropertyName=$true)]
    [string]
    $Name,
    
    # The internet protocol the monitor will use
    [Parameter(Mandatory=$true,ValueFromPipelineByPropertyName=$true )]
    [ValidateSet("http", "https", "ftp", "ping", "ssh", "dns", "mysql", "udp", "tcp", "sip", "smtp", "imap", "pop")]
    [string]
    $MonitorType,
    
    # The type of operation the monitor should perform
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [ValidateSet('Get','Post','Put', 'Delete')]
    [string]
    $OperationType = 'Get',
    
    # The url Monitis should monitor
    [Parameter(Mandatory=$true,ValueFromPipelineByPropertyName=$true)]
    [uri]
    $Url,
    
    # The interval, in minutes, between each check in monitis
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [ValidateSet(1, 3, 5, 10, 15, 20, 30, 40, 60)]
    [int]
    $Interval = 5,
    
    # The timeout until the operation fails
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [Timespan]
    $Timeout = "0:0:10",
        
    # The Monitis Server locations that should monitor the URL
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [int[]]
    $locationIds = @(1,4),
    
    # A tag describing the site
    [Parameter(Mandatory=$true, ValueFromPipelineByPropertyName=$true)]
    [string]
    $Tag = "All",
        
    # Additional post data sent with the request.  This can be used to test web applications.
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [string]
    $PostData,
    
    # Information to be sent to mySQL
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [Hashtable]
    $MySqlParameter,
    
    # Information to be sent to DNS
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [Hashtable]
    $DnsParameter,
    
    # The required % uptime
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [ValidateRange(0,100)]
    [int]
    $RequiredPercentUptime,
    
    # The required response time 
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [Timespan]
    $RequiredResponseTime,
    
    # A regular expression that should be found in the returned content
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [string]
    $ExpectedContentPattern,
    
    # The credentail used to make the request
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [Management.Automation.PSCredential]
    $Credential,
    
    # If set, the request will be made overSSL
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [switch]
    $OverSsl,
    
    # The Monitis API Key   
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [string]$ApiKey,
    
    # The Monitis Secret Key
    [Parameter(ValueFromPipelineByPropertyName=$true)]
    [string]$SecretKey
    )
    
    begin {
        $xmlHttp = New-Object -ComObject Microsoft.XMLHTTP
        Set-StrictMode -Off
    }
    process {
        #region Reconnect To Monitis
        if ($psBoundParameters.ApiKey -and $psBoundParameters.SecretKey) {
            Connect-Monitis -ApiKey $ApiKey -SecretKey $SecretKey
        } elseif ($script:ApiKey -and $script:SecretKey) {
            Connect-Monitis -ApiKey $script:ApiKey -SecretKey $script:SecretKey
        }
        
        if (-not $apiKey) { $apiKey = $script:ApiKey } 
        
        if (-not $script:AuthToken) 
        {
            Write-Error "Must connect to Monitis first.  Use Connect-Monitis to connect"
            return
        } 
        #endregion      
        $xmlHttp.Open("POST", "http://www.monitis.com/api", $false)
        $xmlHttp.SetRequestHeader("Content-Type","application/x-www-form-urlencoded")
        $timeStamp = (Get-Date).ToUniversalTime().ToString("s").Replace("T", " ")
        $detailedTestType =if ($operationType -eq 'Get') { 
            1
        } elseif ($operationType -eq 'Post') {
            2
        } elseif ($operationType -eq 'Put') {
            3
        } elseif ($operationType -eq 'Delete') {
            4
        }
        $overSslNum = if ($overSsl) { 1}  else { 0 }
        $lids = $locationIds -join ','        
        $postData = "apiKey=$script:ApiKey&authToken=$script:AuthToken&validation=token&locationIds=$lids&timestamp=$timestamp&interval=$interval&url=$url&action=addExternalMonitor&name=$Name&type=$MonitorType&overSsl=$overSslNum&detailedTestType=$detailedTestType&timeout=$($timeout.TotalMilliseconds)&$(if ($postData) {'postData=$postData'})"
        $postdata = $postData.TrimEnd("&")
        if ($tag) {
            $postData += "&tag=$tag"
        }
        if ($credential) {
            $postdata+="&basicAuthUser=$($Credential.username)&basicAuthPass=$($credential.GetNetworkCredential().password)"
        }
        if ($requiredResponseTime) {
            $postdata+="&uptimeSLA=$($requiredResponseTime.TotalMilliseconds)"
        }
        if ($RequiredPercentUptime) {
            $postdata+="&responseSLA=$($RequiredPercentUptime.TotalMilliseconds)"
        }
        $xmlHttp.Send($postData)
        $response = $xmlHttp.ResponseText
        if ($response -like '*"status":"ok"*') {
        
        } else {
            $result = $response -replace '[\"\{\}\"]', ""
            $errorMessage = $result.Split(':')[1].Trim()
            Write-Error $errorMessage
        }
        
    }
} 
