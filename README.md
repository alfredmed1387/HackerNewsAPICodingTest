# 🚀 HackerNewsAPICodingTest

A coding challenge project built in **C#**, designed to interact with the Hacker News API and demonstrate clean coding, effective API usage, and modern .NET practices.


---

## 📝 Table of Contents

- [About](#about)
- [Features](#features)
- [Getting Started](#getting-started)
- [Usage](#usage)
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

- [.NET SDK](https://dotnet.microsoft.com/download) (v8.0 or newer)

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

### API Usage

- **GET** `/api/hackernews?n=20`  
  Returns up to 20 best Hacker News stories, sorted by score.

Swagger UI is available for interactive API exploration in development mode.

---

## 💡 Usage

1. Run the API project.
2. Use a browser or a tool like curl/Postman to fetch top stories from the endpoint.
3. Adjust the `n` query parameter as needed (default: 10, max: 100).

---

## 🛠️ Technologies Used

- **C# / .NET 8+**
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
- **Endpoint Parameter Assumptions:**
  - The query parameter `n` in `/api/hackernews?n=VALUE` determines the number of top stories to return.
  - The default value for `n` is 10 if not provided.
  - The maximum allowed value for `n` is 100; any value above this is capped to 100.
  - If `n` is less than 1, it is set to the default (10).
  - Requests with invalid `n` values (negative, zero, or excessively high) are automatically adjusted and not rejected unless egregiously out of bounds.
  - If no stories are found, a 404 response is returned.
  - API is stateless and does not retain request context between calls.

---

## 🌱 Future Enhancements

- **Advanced Pagination and Filtering:** Support paging, searching, and filtering stories by keyword, author, or time range.
- **Distributed Caching:** Use Redis or another distributed cache to enable scaling across multiple server instances.
- **Resilient HTTP Strategies:** Add retry, backoff, and circuit breaker policies with Polly.
- **Batch API Requests:** Optimize outbound network calls by batching requests (if supported by the Hacker News API).
- **Rate Limiting:** Enforce rate limits to protect the API and external resources.
- **Authentication and Authorization:** Secure endpoints for restricted environments.

