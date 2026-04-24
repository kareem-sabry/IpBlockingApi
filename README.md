# IP Blocking API

A **.NET 9** REST API for managing blocked countries and validating IP addresses
using the [ipapi.co](https://ipapi.co) geolocation service.  
All state is stored **in-memory** — no database is required.

---

## Tech Stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 9 Web API |
| In-memory storage | `ConcurrentDictionary`, `ConcurrentBag` |
| Geolocation | ipapi.co (free tier, no key required for basic use) |
| Background tasks | `BackgroundService` (hosted service) |
| API docs | Swagger / Swashbuckle |
| JSON | Newtonsoft.Json |

---

## Project Structure

```
IpBlockingApi/
├── BackgroundServices/   # Temporal block cleanup (runs every 5 min)
├── Common/               # ApiResponse wrapper, ValidationHelper, CountryNameLookup
├── Controllers/          # CountriesController, IpController, LogsController
├── DTOs/                 # Requests + Responses
├── Extensions/           # HttpContext helpers (caller IP extraction)
├── Middleware/           # Exception handling, security headers
├── Models/               # Domain models (BlockedCountry, TemporalBlock, Log)
├── Repositories/         # In-memory thread-safe storage layer
├── Services/             # Business logic + GeoLocation HTTP client
└── Settings/             # GeoLocationSettings (bound from appsettings.json)
```

---

## Getting Started

### 1. Configure the geolocation API key

Open `appsettings.Development.json` and set your key:

```json
{
  "GeoLocation": {
    "Provider": "ipapi",
    "ApiKey": "YOUR_KEY_HERE",
    "BaseUrl": "https://ipapi.co"
  }
}
```

> **No key needed for basic testing.** The free tier allows 1 000 requests/day
> without authentication. Leave `ApiKey` empty to use it unauthenticated.

### 2. Run the project

```bash
dotnet run
```

### 3. Open Swagger UI

Navigate to `https://localhost:{port}/swagger` in your browser.

---

## Endpoints

### Countries

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/countries/block` | Permanently block a country |
| `DELETE` | `/api/countries/block/{countryCode}` | Remove a permanent block |
| `GET` | `/api/countries/blocked?page=1&pageSize=10&search=` | List blocked countries (paginated + filtered) |
| `POST` | `/api/countries/temporal-block` | Block a country for 1–1440 minutes |

### IP

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/ip/lookup?ipAddress={ip}` | Resolve geolocation for an IP (uses caller IP if omitted) |
| `GET` | `/api/ip/check-block` | Check if the caller's country is blocked (always logged) |

### Logs

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/logs/blocked-attempts?page=1&pageSize=10` | Paginated blocked-attempt log |

---

## Sample Requests

**Block a country**
```json
POST /api/countries/block
{ "countryCode": "US" }
```

**Temporary block for 2 hours**
```json
POST /api/countries/temporal-block
{ "countryCode": "EG", "durationMinutes": 120 }
```

**IP lookup**
```
GET /api/ip/lookup?ipAddress=8.8.8.8
```

---

## All Responses Follow This Shape

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": { }
}
```

## Security

- Global exception middleware — stack traces never leak in production
- Security headers on every response (`X-Content-Type-Options`, `X-Frame-Options`, etc.)
- Internal rate-limit guard (45 geo calls / minute) to protect the free API tier
- IP address sanitized and validated before any external call
- `X-Forwarded-For` parsing handles comma-separated lists and proxy chains