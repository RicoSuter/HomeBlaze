# HomeBlaze - Home Automation with .NET/Blazor

HomeBlaze is a home automation software built using .NET/C# and ASP.NET Core Blazor, designed to help you automate various tasks and control devices in your smart home. It supports integration with popular smart home devices and protocols like Philips Hue, Z-Wave or MQTT. With HomeBlaze, you can create custom automations and rules based on various triggers such as time, weather, device states, or user interactions. HomeBlaze provides a web-based user interface that allows you to monitor and control your smart home devices. 

HomeBlaze is built as a Blazor Server-Side application using the [MudBlazor](https://www.mudblazor.com/) UI library. This choice is done because a home automation software usually runs in the local network (low latency) with a low number of users (no scaling issues) and it gives the possibility to build a framework in which is very simple to build new device integrations and dashboard widgets (no Web API required between FE/BE, projects with device implementation and UIs, etc.). 

**The software is still in preview and APIs might change.**

Currently the software supports the following devices:

- **Philips Hue**
- **MQTT**
- **Z-Wave Controllers** (door sensors, temperature sensors, smoke detectors, etc.)
- Tesla Vehicles
- Tesla Wall Connector (Gen 3)
- Logitech Harmony Hub
- Microsoft Xbox
- ASUS Routers
- OpenWeatherMap Temperatures
- PushOver (Push Notifications)
- Gardena Irrigation Control (control garden watering)
- Luxtronik Heaters
- myStrom Switches
- Nuki Bridges (door lock)
- Sonos
- ThanksMister WallPanel (Android app for wall panels)

We hope to find more people from the .NET community to provide support for more devices, new widgets, bug fixes and new features. 

## Screenshots

Custom built dashboard (in German): 

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226210834-52e3f90b-a764-4266-9dfa-9d4244342877.png)

Dashboard editor: 

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226210866-17165a2d-5775-4671-8ae8-38fdf6e4ea8a.png)

Things with properties in Thing manager (Philips Hue Lamp): 

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226211192-46b002b6-59bd-4a8e-963a-87343394dd90.png)

Thing property details: 

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226211207-01eca45d-7153-45da-b1c1-632e3d6d74b4.png)

Thing property history graph: 

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226211217-b6d2148b-042b-431b-b7cc-b20287693545.png)

Trigger automation to send push notification to close a window after 15 minutes: 

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226211110-28677381-7e40-481a-af1e-c1ee36d84cf7.png)

State machine automation to change light brightness at night: 

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226211035-f337aa9a-9b2b-4dcf-b3df-d4aaf41e013d.png)

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226211066-abd3bfd4-a412-41b4-9fa7-ba8a5cb366de.png)

Custom script to be used in an automation (blinking lights can be used as alarm):

![192 168 1 200_9800_](https://user-images.githubusercontent.com/2603405/226211732-40d14b67-f1f0-40f5-9496-87e5864c8ac7.png)

## Installation

### Docker

To run HomeBlaze on port 9800 via Docker, create a `HomeBlaze` directory in your `C:` drive and run:

```
docker run -d --restart unless-stopped -v C:/HomeBlaze:/app/Config --name homeblaze -p 9800:80 ghcr.io/ricosuter/homeblaze:main
```

All configuration and state history files are now written to your `C:/HomeBlaze` directory.

To get the latest version, delete the docker instance and pull the latest version with:

```
docker stop homeblaze
docker rm homeblaze
docker pull ghcr.io/ricosuter/homeblaze:main
```

After this, run the `docker run` command above again.

### Docker-Compose

Example `docker-compose.yml` using Azure Blobs:

```
version: "3"

services:
  homeblaze:
    image: ghcr.io/ricosuter/homeblaze:main
    restart: always
    environment:
      - Storage__Type=AzureBlobs
      - Storage__Container=...
      - Storage__ConnectionString=...
    ports:
      - 9800:80
```

Initial run: 

```
docker-compose up -d
```

Update: 

```
docker-compose pull
docker-compose up -d
```

**Support Z-Wave**

For Z-Wave you need to run the Docker image in priviledge mode: 

```
version: "3"

services:
  homeblaze:
    image: ghcr.io/ricosuter/homeblaze:main
    restart: always
    privileged: true
    ports:
      - 9800:80
```

**Support UDP**

Some things require "special" network capabilities (e.g. Philips Hue Bridge requires UDP): 

```
version: "3"

services:
  homeblaze:
    image: ghcr.io/ricosuter/homeblaze:main
    restart: always
    privileged: true
    network_mode: host
    environment:
      - ASPNETCORE_URLS=http://0.0.0.0:9800
```

**Change timezone**

It seems that the docker container is run in UTC timezone and not in the host's timezone, to change that set the `TZ` environment variable: 

```
version: "3"

services:
  homeblaze:
    image: ghcr.io/ricosuter/homeblaze:main
    environment:
      - TZ=Europe/Zurich
```
