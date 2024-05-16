# Hacker News API

This API provides endpoints to retrieve top stories from Hacker News.
https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty

## Startup
Clone the project from the url-: https://github.com/ABHINAV32/hackernewsapi
If HackerNewsAPI is not main project, close the solution.
Delete the .vs folder from the folder where code is downloaded
Reopen the solution

## Endpoints

### Get Top Stories From Server. It always fetches from server

Endpoint: `GET /api/Stories/GetStoriesFromServer`

Description: Fetches the top stories from Hacker News server.

### Get Top Stories. It fetches from server and then cache it for 60 mins( which can be configurable)

Endpoint: `GET /api/Stories/GetStories`

Description: Fetches the top stories from Hacker News.

### Get Top Stories MultiThreaded. It uses Multithreading to fetch the data

Endpoint: `GET /api/Stories/GetStoriesMultiThreaded`

Description: Fetches the top stories from Hacker News using multi-threading.

## Response

All endpoints return JSON objects with the following structure:

```json
[
  {
    "title": "GPT-4o",
    "url": "https://openai.com/index/hello-gpt-4o/"
  },
  {
    "title": "Disney's Robots Use Rockets to Stick the Landing",
    "url": "https://spectrum.ieee.org/disney-robot-2668135204"
  }
]
