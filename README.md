# heos-remote
A C# based systray utility for controlling HEOS devices, e.g. from Denon or Marantz.

![image](https://github.com/user-attachments/assets/e497fe27-c0f5-47e9-b6a8-54533bc122a5)

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

### Configuration
The utility does not use registry nor configuration files. The configuration is solely done by command-line parameters.
This limits the configrability of the tool.
It is currently not possible to start the utility without command-line parameters, therefore simply double-click to the .exe does not work.

The syntax is as follow (help screen):
```
heos-remote-systray
(C) 2025 by Michael Hoffmeister. MIT license.
  -d, --device          Required. Sequence of one to many friedly name of HEOS
                        device(s). Each may be of format name|host:port to
                        disable auto discovery.
  -g, --group           Sequence of zero to many group definitions. Each is of
                        format Name|<index>,..,<index>|..|<index>,..,<index>|,
                        with <index> being 1-based device indices.
  -m, --manufacturer    Manufacturer name of HEOS device; first device will be
                        taken.
  -i, --ifc-name        (Network) interface name to take for discovery.
  -v, --verbosity       Verbosity level >= 0.
  -t, --time-out        Time-out in [ms].
  -u, --username        Username of the HEOS account (cleartext).
  -p, --password        Password of the HEOS account (cleartext, sigh!).
  --cids                Sequence of tuples, which are starting points for
                        containers. Format name|sid|cid.
  --help                Display this help screen.
  --version             Display version information.
```

### Sample command-line

```
.\heos-remote-systray.exe -d 'Bedroom' 'Dining Room' 'Office|192.168.178.111' -g 'Single rooms|1|2' 'All rooms|1,2' -u 'user@example.com' -p 'MySecret' --cids 'Amazon playlists|13|library/playlists/#library_playlists-NAME-Playlisten'
```

These options:
* define 3 devices, one of which has a specific IP address and is therefore disabling discovery
* define 2 groups, the 1st with two individual players, the latter with two players combined
* define credentials for the HEOS sign in, on order to browse music
* define a favourite Amazon playlist (source id = 13) with a presumably standard name

### Music browser

![image](https://github.com/user-attachments/assets/d4a050fa-1174-4c87-99cf-1c50a8dc7a4e)

The music browser has roughly the same capabilities as the HEOS app. No suprise, as using the same API calls.
The user might go up (actually, this is a form of stack-based history).
The user might drag-down the combo box with configured favourite playlists and standard playlists.
The user might decide, if the music is directly played or added to the queue.
The queue might be cleared.

### API version
For the implementation, version 1.17 of the "HEOS CLI Protocol Specification" was used.
Google might help.

### Music services
The following music services were tested:

| Music service | SID | Status |
| ------------- | --- | ------ |
| TuneIn        | 3   | Working with free account |
| Amazon music  | 13  | Working with paid account |
