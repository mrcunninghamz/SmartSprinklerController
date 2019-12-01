# Install to Rasberry PI

This will describe how to run the web application on the pi.

# Publish the application
Go to the application directory and publish the application with the following command

```
dotnet publish -r win-arm
```
When done it should put the files in this folder:

`{projet directory}\SmartSprinklerConfigurator\bin\Debug\netcoreapp3.0\win-arm`


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

go to the C directory and create the folder `configurator`

```
cd C:\
```
```
mkdir configurator
```

open explorer window and navigate to:
`\\{name | ipaddress}\c$\\configurator`

Copy the published files into this folder.

go back to powershell and go in the configurator folder
```
cd configurator
```

run the application
```
.\SmartSprinklerConfigurator.exe
```