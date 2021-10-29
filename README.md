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
public interface ISimpleEventClass
{
    event EventHandler<int> Event1;
    string Method2(string input);
}

public class SimpleEventClass : ISimpleEventClass
{
    public event EventHandler<int>? Event1;

    public void RaiseEvent1()
    {
        Event1?.Invoke(this, 777);
    }

    public string Method2(string input)
    {
        return $"Hello, input = {input}";
    }
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

// If the server does not use the library where the shared code is located, it must be loaded.
await factory.LoadAssemblyAsync(typeof(SimpleEventClass).Assembly.Location);

// You will have access to an interface through which you will interact with the object created on the server.
var instance = await factory.CreateInstanceAsync<ISimpleEventClass>(
    typeof(SimpleEventClass).FullName ?? string.Empty);
instance.Event1 += (_, args) =>
{
    WriteLine($"{nameof(instance.Event1)}: {args}");
};
instance.RaiseEvent1();
var result = Instance.Method2("argument");
```

### Contacts
* [mail](mailto:havendv@gmail.com)
