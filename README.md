# HomeBlaze

## Installation

**Docker**

To run HomeBlaze on port 9800 via Docker, just run:

```
docker run -d --restart unless-stopped --name homeblaze -p 9800:80 ghcr.io/ricosuter/homeblaze:main
```

To stop and remove the image, run: 

```
docker stop homeblaze
docker rm homeblaze
```
