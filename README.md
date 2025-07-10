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
- [Contributing](#contributing)
- [License](#license)

---

## 📖 About

**HackerNewsAPICodingTest** is a C#/.NET project crafted as a coding test. It showcases integration with the Hacker News API, focusing on maintainable architecture and best software engineering practices.

---

## ✨ Features

- Fetches and displays top stories from Hacker News
- Shows story details: title, author, score, and publication time
- Simple command-line interface (CLI)
- Error handling for API/network issues

---

## 🚀 Getting Started

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (v6.0 or newer)

### Installation

```bash
git clone https://github.com/alfredmed1387/HackerNewsAPICodingTest.git
cd HackerNewsAPICodingTest
dotnet restore
```

### Running the Project

```bash
dotnet run
```

---

## 💡 Usage

1. Run the application from the command line.
2. The program will fetch and display the top Hacker News stories.
3. Follow prompts (if any) to view more details about a story.

---

## 🛠️ Technologies Used

- **C# / .NET 6+**
- Hacker News REST API

---

## 🏗️ Architecture

The project follows a simple, modular architecture leveraging .NET's best practices for maintainability and clarity. The application entry point is a CLI (Command Line Interface) that orchestrates API calls to the Hacker News REST API, processes results, and handles user interaction and error states. The codebase is organized so that core logic (API interaction, data parsing, and display logic) is separated for future extensibility and easy testing.

---

## 📚 Assumptions

- The Hacker News API is publicly available and does not require authentication.
- Only the "top stories" endpoint is required for the core functionality.
- The application runs in a console environment.
- User input is minimal and does not require advanced validation.

---

## 🌱 Future Enhancements (Performance-Focused)

- **Implement asynchronous and parallel API calls:** Fetch story details in parallel to minimize total loading time.
- **Introduce local caching:** Cache previously fetched stories to reduce unnecessary API requests and improve responsiveness.
- **Pagination and lazy loading:** Load and display stories incrementally (e.g., 20 at a time) to minimize memory and bandwidth usage.
- **Optimize data processing:** Profile and refactor code to minimize CPU and memory consumption, especially when handling large datasets.
- **Implement retry and backoff strategies:** Handle intermittent network issues gracefully without blocking UI or causing unnecessary load on the API.
- **Configurable cache expiration:** Allow customization of cache duration for fresh vs. stale data trade-offs.
- **Batch API requests (if supported):** Combine multiple story or comment requests in single network calls to reduce latency.
- **Minimize allocations:** Use memory-efficient collections and avoid unnecessary object instantiation.
- **Add diagnostics and logging:** Provide performance metrics and logging to help identify bottlenecks.
- **Evaluate and use HTTP connection pooling:** Ensure HTTP connections are reused efficiently to reduce overhead.

---

## 🤝 Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

---

## 📄 License

This project is currently unlicensed. Please request license information if you wish to use or contribute.

---

## 🙏 Acknowledgments

- [Hacker News](https://news.ycombinator.com/)
- [Hacker News API](https://github.com/HackerNews/API)
