# HomeBlaze

## Installation

**Docker**

To run HomeBlaze on port 9800 via Docker, create a HomeBlaze directory in your C: drive and run:

```
docker run -d --restart unless-stopped -v C:/HomeBlaze:/app/Config --name homeblaze -p 9800:80 ghcr.io/ricosuter/homeblaze:main
```

All configuration and state history files are now written to your `C:\HomeBlaze` directory.

To stop and remove the image, run: 

```
docker stop homeblaze
docker rm homeblaze
```
