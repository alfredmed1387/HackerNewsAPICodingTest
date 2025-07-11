# 🚀 HackerNewsAPICodingTest

A coding challenge project built in **C#**, designed to interact with the Hacker News API and demonstrate clean coding, effective API usage, and modern .NET practices.

This project exposes a RESTful API endpoint designed to provide clients with a customizable list of the top stories from Hacker News. The endpoint fetches, filters, and returns high-quality stories sorted by score, making it easy for users or client applications to access trending news content programmatically. The API returns key story details such as title, author, score, time, and number of comments, supporting efficient integration and display of Hacker News content in other applications or dashboards.

---

## 📝 Table of Contents

- [About](#about)
- [Features](#features)
- [Getting Started](#getting-started)
- [Endpoints](#endpoints)
- [Technologies Used](#technologies-used)
- [Architecture](#architecture)
- [Assumptions](#assumptions)
- [Future Enhancements](#future-enhancements)

---

## 📖 About

**HackerNewsAPICodingTest** is a C#/.NET project crafted as a coding test. It showcases integration with the Hacker News API, focusing on maintainable architecture and best software engineering practices.

---

## ✨ Features

- Fetches and displays top stories from Hacker News via a RESTful API
- Exposes an HTTP API endpoint to retrieve best stories, sorted by score
- Displays story details: title, author, score, time, and comments
- Implements asynchronous, parallel API calls for story details
- In-memory caching for best stories and individual story responses
- Robust error handling and logging for API/network issues
- Configurable parameters (e.g., number of stories, cache durations)
- Unit tests for controllers and services
- Swagger/OpenAPI documentation for HTTP endpoints

---

## 🚀 Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (v8.0)

### Installation

```bash
git clone https://github.com/alfredmed1387/HackerNewsAPICodingTest.git
cd HackerNewsAPICodingTest
dotnet restore
```

### Running the Project

```bash
dotnet run --project HackerNews.Api
```

The API will be available at `https://localhost:7222/api/hackernews` (adjust for your environment).

---

## 🛣️ Endpoints

### GET `/api/hackernews?n=VALUE`
 
Returns a list of top Hacker News stories sorted by score.
 
#### Parameter Validation
 
 - **n** (optional, integer): Number of top stories to return.
   - **Default:** 10 (if not provided or invalid)
   - **Minimum:** 1 (values less than 1 are set to 10)
   - **Maximum:** 100 (values above 100 are capped to 100)
   - Non-integer or negative values are adjusted to the default (10).
   - If no stories are found, a `404 Not Found` response is returned.
 
#### Example Request
 
```http
GET /api/hackernews?n=5
```
 
#### Example Response
 
```json
[
  {
    "id": 123456,
    "title": "Example Hacker News Story",
    "author": "username",
    "score": 150,
    "time": "2024-06-30T13:45:22Z",
    "url": "https://news.ycombinator.com/item?id=123456",
    "comments": 42
  }
]
```

- Each object includes: `id`, `title`, `author`, `score`, `time` (UTC), `url`, and `comments`.
- Swagger UI is available for interactive API exploration in development mode.
 
---

## 💡 Usage

1. Run the API project.
2. Use a browser or a tool like curl/Postman to fetch top stories from the endpoint.
3. Adjust the `n` query parameter as needed (default: 10, max: 100).

---

## 🛠️ Technologies Used

- **C# / .NET 8**
- ASP.NET Core Web API
- MemoryCache for in-memory caching
- xUnit, Moq for unit testing
- Swagger/OpenAPI for documentation

---

## 🏗️ Architecture

The solution follows a layered architecture:
- **Api Layer:** ASP.NET Core controllers expose HTTP endpoints.
- **Service Layer:** Handles all business logic, external API calls, caching, and error handling.
- **Models:** DTOs and domain models for data transfer and structure.
- **Unit Tests:** Isolate and validate controller/service logic.

**Key design points:**
- All external calls to Hacker News use `HttpClientFactory` and async/await for efficiency.
- SemaphoreSlim limits concurrent outbound requests for stability.
- In-memory caching avoids redundant external API calls.
- Logging is implemented at controller and service layers for observability.
- Tests validate correctness and error handling paths.

---

## 📚 Assumptions

- The Hacker News API is publicly available and does not require authentication.
- Only the "best stories" endpoint is required for the core functionality; other endpoints (e.g., comments, user details) are not implemented.
- The API is primarily read-only; no write or mutation operations are supported.
- Returned stories may be fewer than requested if the upstream API omits or removes stories.
- All time values are returned as UTC.
- No persistent storage is used; all cache is in-memory and resets on application restart.

---

## 🌱 Future Enhancements

- **Advanced Pagination and Filtering:** Support paging, searching, and filtering stories by keyword, author, or time range.
- **Distributed Caching:** Use Redis or another distributed cache to enable scaling across multiple server instances.
- **Resilient HTTP Strategies:** Add retry, backoff, and circuit breaker policies with Polly.
- **Batch API Requests:** Optimize outbound network calls by batching requests (if supported by the Hacker News API).
- **Rate Limiting:** Enforce rate limits to protect the API and external resources.
- **Authentication and Authorization:** Secure endpoints for restricted environments.

