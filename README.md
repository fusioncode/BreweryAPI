
# BreweryAPI By Sayan Paul 

BreweryAPI is an **ASP.NET Core Web API** that aggregates brewery data, applies advanced search and filtering, and exposes **RESTful endpoints secured via JWT authentication**.  
The service follows **SOLID principles**, supports **API versioning (v1 & v2)**, and integrates caching, fallback storage, and analytics-friendly logging.

---

## 📖 Table of Contents
1. [Overview](#-overview)  
2. [Goals & Non-Goals](#-goals--non-goals)  
3. [Architecture & Components](#-architecture--components)  
   - [Web Layer](#web-layer)  
   - [Service Layer](#service-layer)  
   - [Infrastructure Services](#infrastructure-services)  
   - [Authentication & Authorization](#authentication--authorization)  
4. [Data Design](#-data-design)  
5. [Request Flows](#-request-flows)  
6. [Error Handling & Resilience](#-error-handling--resilience)  
7. [Security Considerations](#-security-considerations)  
8. [Performance & Scalability](#-performance--scalability)  
9. [Observability](#-observability)  
10. [Key Interfaces & Classes](#-key-interfaces--classes)  
11. [Configuration Example](#-configuration-example)  
12. [Future Enhancements](#-future-enhancements)

---

## 🔎 Overview
- **Framework:** ASP.NET Core Web API  
- **Authentication:** JWT-based (Bearer tokens)  
- **Data Source:** [Open Brewery DB](https://www.openbrewerydb.org/documentation)  
- **Cache:** In-memory (extensible to Redis)  
- **Fallback:** Local JSON file for resilience  
- **Versioning:** `/api/v{version}/...` with v1 (basic) & v2 (advanced)  

---

## 🎯 Goals & Non-Goals

### ✅ Goals
- Deliver a **resilient, secure, and extensible backend** for brewery discovery.  
- Enable **advanced search** (filtering, pagination, geospatial distance).  
- Maintain clear **separation of concerns** via interfaces (`IBreweryFetcher`, `ICacheProvider`, etc.).  
- Support **JWT authentication** & role-driven authorization.  
- Provide **high observability, testing coverage, and deployment readiness**.  

### ❌ Non-Goals
- No public frontend UI (handled by consumers).  
- No long-term brewery data persistence (only cache + fallback).  
- No internal analytics dashboards (data exposed for external BI).  

---

## 🏗️ Architecture & Components

### Web Layer
- **Controllers**
  - `BreweryController` (v1): Basic search endpoints.  
  - `BreweryV2Controller` (v2): Advanced search (filters, pagination, autocomplete, metadata).  
- **API Versioning**
  - Path-based (`/api/v{version}/...`) with Swagger integration.  
  - Supports query/header/media-type fallback.  

### Service Layer
- **BreweryService**
  - Orchestrates **fetch → cache → fallback**.  
  - Handles logging, filtering, DTO conversion.  
- **IBrewerySearch**
  - Encapsulates search & sorting (v2 extends filters + metadata).  
- **IDistanceCalculationService**
  - Provides Haversine-based distance calculations.  

### Infrastructure Services
- **IBreweryFetcher → ApiBreweryFetcher**  
  - Uses `HttpClient` to call Open Brewery DB.  
- **ICacheProvider → MemoryCacheProvider**  
  - In-memory cache (TTL = 10 minutes).  
- **IFallbackProvider → FileFallbackProvider**  
  - Local JSON fallback file.  
- **IJwtTokenService**  
  - Issues & validates JWT tokens (symmetric key).  

### Authentication & Authorization
- Endpoints: `/api/auth/register`, `/api/auth/login`, `/api/auth/me`, `/api/auth/test`.  
- **JWT Workflow:**  
  1. User registers → password hashed (SHA256; upgrade recommended).  
  2. Login → JWT issued (60 min expiration).  
  3. Protected endpoints → `[Authorize]` attributes + role-based checks.  

---

## 🗂️ Data Design

### Entities
- **User** → `Id, Username, Email, PasswordHash, Role, CreatedAt, LastLoginAt, IsActive`  
- **BrewerySource** → Mirrors Open Brewery DB fields  

### DTOs
- **V1:** Basic fields (login, register, search results).  
- **V2:** `BreweryV2Dto`, `BrewerySearchV2RequestDto`, `BrewerySearchV2ResponseDto`  
  - Includes ratings, review counts, tags, metadata, pagination, and distance info.  

---

## 🔄 Request Flows

### Brewery Search (V2)
1. `POST /api/v2.0/Brewery/search` → `BrewerySearchV2RequestDto`.  
2. Controller validates request + logs.  
3. `BreweryService.GetBreweries()` → (cache → API → fallback).  
4. Apply filters + enrich data (distance, tags).  
5. Return response with breweries, pagination, metadata.  

### Authentication
1. `/api/auth/register` → create user with hashed password.  
2. `/api/auth/login` → validate & issue JWT.  
3. Authenticated requests → `Authorization: Bearer <token>`.  

---

## ⚠️ Error Handling & Resilience
- **Logging:** Structured via `ILogger`.  
- **Fallback:** Local file used when API unavailable.  
- **Validation:** `[ApiController]` model validation.  
- **Exceptions:** Catch → log → return generic 500 to clients.  

---

## 🔐 Security Considerations
- **JWT Secret Management:** Use env vars or Key Vault (not plaintext).  
- **Password Hashing:** Upgrade SHA256 → `PBKDF2`, `bcrypt`, or `argon2`.  
- **Rate Limiting:** Consider ASP.NET middleware.  
- **Role-Based Access:** Extend `[Authorize(Roles="Admin")]`.  

---

## 🚀 Performance & Scalability
- **Caching:** Reduce API calls (configurable TTL).  
- **Pagination:** Controls payload size.  
- **Async/await:** Non-blocking I/O.  
- **Stateless API:** Horizontal scaling-ready.  
- **Fallback at Scale:** Replace file with blob storage.  

---

## 📊 Observability
- **Logging:** Structured JSON logs.  
- **Metrics:** Cache hits/misses, API latency, token usage.  
- **Tracing:** OpenTelemetry for distributed tracing.  

---

## 📘 Key Interfaces & Classes

| Class / Interface | Responsibility |
|-------------------|----------------|
| `BreweryService` | Orchestrates fetch → cache → fallback |
| `BreweryV2Controller` | Exposes v2 search endpoints |
| `IBrewerySearch` | Encapsulates search & filtering |
| `IDistanceCalculationService` | Haversine distance calculation |
| `JwtTokenService` | Issues & validates JWT tokens |
| `AuthService` | Handles register/login workflow |

---

## ⚙️ Configuration Example
```json
{
  "JwtSettings": {
    "SecretKey": "<replace-in-prod>",
    "Issuer": "BreweryAPI",
    "Audience": "BreweryAPIClients",
    "TokenExpirationMinutes": 60
  },
  "BreweryApi": {
    "BaseUrl": "https://api.openbrewerydb.org/"
  },
  "CacheSettings": {
    "ExpirationMinutes": 10
  },
  "Fallback": {
    "FilePath": "Data/fallback_breweries.json"
  }
}


## How to Test

1. **API Fetch:**
 ### 1. Get Breweries
**`GET /Brewery`**

Retrieve a list of breweries with optional sorting and search filters.

#### Query Parameters
| Name         | Type    | Default | Description |
|--------------|---------|---------|-------------|
| `sortBy`     | string  | city    | Field to sort results by (e.g., `city`, `name`). |
| `descending` | boolean | false   | Whether to sort in descending order. |
| `search`     | string  | -       | Search term to filter breweries. |


### 2. Brewery Autocomplete
**`GET /Brewery/autocomplete`**

Fetch autocomplete suggestions for brewery names.

#### Query Parameters
| Name    | Type    | Default | Description |
|---------|---------|---------|-------------|
| `query` | string  | -       | Search term for autocomplete. |
| `limit` | integer | 10      | Maximum number of suggestions to return. |

#### Responses
- **200 OK** – Returns a list of brewery suggestions. 

#### Responses
- **200 OK** – Returns a list of breweries.  

2. **Caching:**
   - Repeat the same request within 10 minutes.
   - Confirm the response is served from cache (faster, no new API call).

3. **API Failure Fallback:**
   - Disconnect from the internet or set an invalid API URL in `appsettings.json`.
   - Place a valid `response.json` file in the project root.
   - Make a request; data should be served from the file.

4. **File Fallback Failure:**
   - Remove or rename `response.json`.
   - With the API still unreachable, make a request.
   - The response should be empty, and errors should be logged.

5. **Logging:**
   - Check application logs for messages about API errors, file fallback, and cache usage.

---

## Notes

For Auth Login User ID : admin@brewery.com
                   pwd : admin123

This project now supports API versioning. All endpoints are versioned using the URL segment method.

Available Versions

v1 – initial release (in-memory cache, search, sort, autocomplete).

v2 – same functionality as v1 but extended to support future changes without breaking existing clients.

- The fallback file (`response.json`) should contain valid JSON data matching the expected API response.
- For automated testing, mock dependencies (`IMemoryCache`, `IConfiguration`, `ILogger<BrewerySourceAPI>`) to simulate different scenarios.

---
