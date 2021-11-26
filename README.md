# [H.ProxyFactory](https://github.com/HavenDV/H.ProxyFactory/) 

[![Language](https://img.shields.io/badge/language-C%23-blue.svg?style=flat-square)](https://github.com/HavenDV/H.ProxyFactory/search?l=C%23&o=desc&s=&type=Code) 
[![License](https://img.shields.io/github/license/HavenDV/H.ProxyFactory.svg?label=License&maxAge=86400)](LICENSE.md) 
[![Requirements](https://img.shields.io/badge/Requirements-.NET%20Standard%202.0-blue.svg)](https://github.com/dotnet/standard/blob/master/docs/versions/netstandard2.0.md)
[![Build Status](https://github.com/HavenDV/H.ProxyFactory/workflows/.NET/badge.svg?branch=master)](https://github.com/HavenDV/H.ProxyFactory/actions?query=workflow%3A%22.NET%22)

Allows you to interact with remote objects. 
You will have access to an interface through which you will interact with the object created on the server.

Features:
- Create proxy objects that look exactly like the original objects
- Proxy target can be located anywhere where there is access to pipes

### Nuget

[![NuGet](https://img.shields.io/nuget/dt/H.ProxyFactory.Pipes.svg?style=flat-square&label=H.ProxyFactory.Pipes)](https://www.nuget.org/packages/H.ProxyFactory.Pipes/)

```
Install-Package H.ProxyFactory.Pipes
```

### Usage
Shared code:
```cs
public interface IActionService
{
    void SendText(string text);
    void ShowTrayIcon();
    void HideTrayIcon();

    event EventHandler<string> TextReceived;
}

public class ActionService { }
```

Implementation on target platform:
```cs
public class ActionService : IActionService
{
    private TrayIconService trayIconService = new();

    public void SendText(string text)
    {
        Console.WriteLine($"Text from client: {text}");

        TextReceived?.Invoke(this, "Hi from server");
    }

    public void ShowTrayIcon()
    {
        trayIconService.ShowTrayIcon();
    }

    public void HideTrayIcon()
    {
        trayIconService.HideTrayIcon();
    }

    public event EventHandler<string>? TextReceived;
}
```

Server:
```cs
await using var server = new PipeProxyServer();

await server.InitializeAsync("UniquePipeServerName");
```

Client:
```cs
await using var factory = new PipeProxyFactory();

await factory.InitializeAsync("UniquePipeServerName");

// You will have access to an interface through which you will interact with the object created on the server.
var service = await factory.CreateInstanceAsync<ActionService, IActionService>();
instance.TextReceived += (_, text) =>
{
    WriteLine($"{nameof(instance.TextReceived)}: {text}");
};
instance.ShowTrayIcon();
Instance.SendText("hello!");
```

![1](/assets/1.png)

### Contacts
* [mail](mailto:havendv@gmail.com)
