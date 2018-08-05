# MicroHttpd [![CircleCI](https://circleci.com/gh/nam178/MicroHttpd.svg?style=shield)](https://circleci.com/gh/nam178/MicroHttpd)
MicroHttpd is a HTTP (Web) Server written in .NET Core.
Currently, it supports:
* SSL, using a PFX file.
* Async IO.
* Dynamic and static content. 
* Fixed length as well as variable length request/respond.
* Byte-range request for video streaming.
* Virtual hosts.
* Completely extensible.

# What Can You Do With It?

Extend the code to do your own things that a normal web server wouldn't support, for example:

* Matching virtual host by cookies.
* HTTP over UDP.
* Optimizing performance or one specific use-case.
* Building your own MVC framework.

# Using The Code

A Demo project is included, those are the steps to have a website up and running, with SSL:

1. Start a new console application (.NET or .NET Core), then add a project reference to MicroHttp.Core.
2. In your application's entry point Main(string[] args) method, create an instance of IHttpService: 

```csharp
using MicroHttpd.Core;

// Create the HTTP server, 
// it doesn't do anything until you configure and start it.
var httpService = HttpServiceFacade.Create();
```

3. Configuring virtual host, a.k.a document root, where the HTML/CSS/JS files are, as well as the ports and domain name.

```csharp
// Configure virtual host - the domain name,
// the web root and ports.
httpService.AddVirtualHost(new VirtualHostConfig
{
  // We'll set document root to the www directory of this project.
  DocumentRoot = "/var/www/my-personal-website.com",

  // Accept all host names,
  // Other than MatchAll, you can also use
  // MicroHttpd.Core.StringMatch.Regex, or,
  // MicroHttpd.Core.StringMatch.ExactCaseSensitive
  HostName = new MatchAll(),

  // Accept incoming connections on port 8443,
  // If you want to use port 443 and/or 80, add them here.
  // (You'll need to start the application as root)
  ListenOnPorts = new int[] { 8443 }
});
```

4. Optionally, if you have a SSL certificate, you can configure the server to use it.

```csharp
// Configure the server to use SSL,
// specifying path to the PFX file and its password.
httpService.AddSSL(
  "/etc/my-personal-website.com.pfx"), 
  ",g9e/~ArH=aH.k8C"
  );
```

5. That's it. Now you can start the server.
```csharp
// Start the server
httpService.Start();

// Wait, this prevents the console program from exiting.
httpService.Wait();
```

# Extending The Code

More details on extending the code coming soon as a CodeProject tutorial, please watch this space.
