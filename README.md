# IP Blocking API

A .NET 8 REST API for managing blocked countries and validating IP addresses.
Uses [ipapi.co](https://ipapi.co) for geolocation. No database — everything lives in memory.

---

## Tech Stack

| Concern | Choice |
|---|---|
| Framework | ASP.NET Core 8 Web API |
| In-memory storage | `ConcurrentDictionary`, `ConcurrentQueue` |
| Geolocation | ipapi.co |
| Background tasks | `BackgroundService` + `PeriodicTimer` |
| API docs | Swagger / Swashbuckle |
| JSON | Newtonsoft.Json |
| Tests | xUnit + Moq |

---

## Project Structure

```
IpBlockingApi/
├── BackgroundServices/   # Cleans up expired temporal blocks every 5 minutes
├── Common/               # ApiResponse<T>, ValidationHelper, CountryNameLookup
├── Controllers/          # CountriesController, IpController, LogsController
├── DTOs/                 # Request and response models
├── Extensions/           # HttpContext helpers (caller IP resolution)
├── Middleware/           # Global exception handler, security headers
├── Models/               # Domain models — BlockedCountry, TemporalBlock, AttemptLog
├── Repositories/         # Thread-safe in-memory storage
├── Services/             # Business logic, GeoLocation HTTP client
└── Settings/             # GeoLocationSettings (bound from appsettings.json)
```

---

## Getting Started

### 1. Geolocation API key

Open `appsettings.json`:

```json
"GeoLocation": {
  "Provider": "ipapi",
  "ApiKey": "",
  "BaseUrl": "https://ipapi.co"
}
```

**`ApiKey` is empty on purpose.** ipapi.co's free tier does not require registration or
an API key — just hit the endpoint directly. The limit is 1,000 lookups/day, which is
plenty for development and testing. If you have a paid key, paste it here and the
service picks it up automatically.

> See: [ipapi.co/api/#authentication](https://ipapi.co/api/#authentication)

### 2. Run

```bash
dotnet run --project IpBlockingApi.Api
```

### 3. Open Swagger

```
https://localhost:{port}/swagger
```

---

## Endpoints

### Countries

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/api/countries/block` | Add a country to the permanent block list |
| `DELETE` | `/api/countries/block/{countryCode}` | Remove a permanent block (404 if not found) |
| `GET` | `/api/countries/blocked` | Paginated + searchable list of blocked countries |
| `POST` | `/api/countries/temporal-block` | Block a country for 1–1440 minutes (409 on duplicate) |

### IP

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/ip/lookup` | Resolve geolocation for any IP (falls back to caller's IP if omitted) |
| `GET` | `/api/ip/check-block` | Check whether the caller's country is blocked — always returns 200, always logged |

### Logs

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/logs/blocked-attempts` | Paginated attempt log, ordered by timestamp descending |

Query params for paginated endpoints: `page`, `pageSize`, `search` (where applicable).

---

## Request Examples

**Permanently block a country**
```json
POST /api/countries/block
{ "countryCode": "US" }
```

**Block for 2 hours**
```json
POST /api/countries/temporal-block
{ "countryCode": "EG", "durationMinutes": 120 }
```

**Look up an IP**
```
GET /api/ip/lookup?ipAddress=8.8.8.8
```

---

## Response Shape

Every endpoint returns the same envelope:

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": {}
}
```

Errors follow the same shape with `"success": false` and a message describing what went wrong. Stack traces never appear outside the Development environment.

---

## Security

The middleware pipeline handles a few things worth calling out:

**Exception handling** — catches anything unhandled, logs it, returns a `ProblemDetails`-style response. The actual exception message only surfaces in Development.

**Security headers** — every response gets `X-Content-Type-Options`, `X-Frame-Options`, `X-XSS-Protection`, `Referrer-Policy`, and `Cache-Control: no-store`.

**Rate limiting** — the IP endpoints use .NET 8's built-in fixed-window limiter: 30 requests/minute per IP. Exceeding it returns 429 with a `Retry-After` header.

**Input validation** — country codes are checked against the full ISO 3166-1 alpha-2 list before any operation. IPs go through `IPAddress.TryParse` before hitting the geolocation service.

**Geo client guard** — a separate sliding-window limiter caps internal calls to ipapi.co at 45/minute to stay well inside the free tier.

**PII handling** — the last octet of every IP address is masked in logs (`192.168.1.xxx`). User-Agent strings are sanitized and capped at 256 characters before storage.

---

## Running Tests

```bash
dotnet test
```

Tests cover `CountryService` (block, unblock, duplicate detection, pagination, search, validation) and `GeoLocationService` (successful parse, network error, 429 handling).