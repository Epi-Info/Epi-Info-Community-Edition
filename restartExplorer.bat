
:: ===================================================
:: Stopping all instances of Explorer and starting one 
:: to get the task bar back is good for the occasional
:: “Access Denied” error.
::
taskkill /f /im explorer.exe
start explorer.exe
:: ===================================================