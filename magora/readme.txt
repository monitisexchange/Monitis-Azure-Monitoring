1. Run startHere.cmd inside WAPPSCmdlets.Bin folder.
When program asks how to install cmdlets: as snap-in or as module, choose snap-in (i.e. enter 1 in console and press Enter).
2. Inside AzureToMonitis.ps1 you need to specify full path to the monitis powershell module
3. You should modify settings to access to your azure service and monitis account
3.1. Rows 19 and 22 - it is monitis API key and API secret key of your monitis account. They can be found at: Tools->API->API key
3.2. Row 25 - name of custom monitor.
3.3. Row 28 - name of azure table storage that is used to store performance counters data. This value can be found from configuration of azure service.
3.4. Row 31 - access key to azure table storage. This value can be found when you select the table storage and you click on "View" button under "primary access key" title in the right menu.
3.5. Row 34 - name of deployment slot of azure service. This field is equal to value in Environment column of azure management portal.
3.6. Row 37 - name of service. This field is equal to value from "DNS prefix" item in the right menu when you selected your azure service.
3.7. Row 40 and 43 - start and end time (in hours) for which the performance counters data will be downloaded. These values are relative to the current time.
3.8. You need to login to your azure account and follow the link: https://windows.azure.com/download/publishprofile.aspx?wa=wsignin1.0
You should download a file with .publishsettings extension. Then you need to specify full path to this file in row 55.