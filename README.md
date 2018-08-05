# MicroHttpd
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

Extend the code to do your own things that a normal web server wouldn't support, such as matching virtual host by cookies, HTTP over UDP, or optimizing performance or one specific use-case.
