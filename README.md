# heos-remote
A C# based systray utility for controlling HEOS devices, e.g. from Denon or Marantz.

## Use cases
* quick play/pause of a selected HEOS device (think: getting a phone call)
* switching between different input sources and favorites of the system
* managing multiple devices and building (pre-configured) groups
* searching for music and playing it (not successfull :disappointed: *1)
* playing of URLs
* browsing music sources and playing play-lists and songs
* managing song queue (at least in clearing it)

*1) It appears, that Denon/ HEOS cannot provide these services, as e.g. Amazon music is not opening for
    3rd parties.

## History of the tool
This utility started as a very small command line program. It quickly evoved and it became obvious
that putting it into the Systray (lower right hand section of the Windows desktop task-bar) is favourable.

## Notes to the current stage of implementation.

### Discovery
The utility uses a crude, limited implementation of UPnP / SSDP (simple device discovery protocol) to discover the IP addresses of the HEOS devices.
This was investiagted by trial and error.
It was only tested with IPv4 and is assumed to fail for IPv6.
Manual configuration is possible:
```
-d "Schlafzimmer|192.168.178.122" "Dining Room|192.168.178.136" "Office|192.168.178.111:1234"
```
