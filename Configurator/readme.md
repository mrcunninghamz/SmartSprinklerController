# Install to Rasberry PI

This will describe how to run the web application on the pi.

# Publish the application
Go to the application directory and publish the application with the following command

```
dotnet publish -r win-arm
```
When done it should put the files in this folder:

`{projet directory}\Configurator\Server\bin\Debug\netcoreapp3.1\win-arm\publish\`


# Setup the Pi

You need to be able to SSH through powershell. 

Set a trust between your machine and your pi
```
net start WinRM
```
```
Set-Item WSMan:\localhost\Client\TrustedHosts -Value {name | ipaddress}
```

Open a powershell session
```
Enter-PSSession -ComputerName {name | ipaddress} -Credential {name | ipaddress}\Administrator
```

go to the C directory and create the folder `Configurator`

```
cd C:\
```
```
mkdir Configurator
```

open explorer window and navigate to:
`\\{name | ipaddress}\c$\\Configurator`

Copy the published files into this folder.

go back to powershell 

lets make sure we open a port up in the firewall
```
netsh advfirewall firewall add rule name="ASP.NET Core" dir=in action=allow protocol=TCP localport=5000
```

go in the Configurator folder
```
cd Configurator
```

run the application
```
.\Configurator.Server.exe
```

## Run on startup
Copy files from `PiStartup` directory to your pi C:\

connect to pi and run
```
schtasks /create /tn "Startup Configurator" /tr c:\Startup.bat /sc onstart /ru SYSTEM
```