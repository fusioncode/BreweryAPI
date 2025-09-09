# BrewerySourceAPI By Sayan Paul 09/09/2025 for E.L.F Beauty

## Overview

`BrewerySourceAPI` is a service class responsible for retrieving brewery data from an external API (Open Brewery DB) with built-in caching and fallback to a local file. It implements the `IBrewerySourceAPI` interface.

---

## Structure

- **Namespace:** `BreweryAPI.Models.Service`
- **Implements:** `IBrewerySourceAPI`
- **Dependencies:**
  - `IConfiguration` for reading configuration (API base URL)
  - `IMemoryCache` for caching API responses
  - `ILogger<BrewerySourceAPI>` for logging

---

## Key Members

- **Constructor:**  
  Initializes configuration, memory cache, and logger. Loads the API base URL from configuration.

- **GetBrewery(string filter = "")**  
  - **Purpose:** Fetches brewery data from the external API, with caching and file fallback.
  - **Parameters:**  
    - `filter` (optional): Appended to the API URL for filtering or specific queries.
  - **Returns:**  
    - `Task<string>`: JSON string of brewery data.

---

## Design Decisions

- **Caching:**  
  API responses are cached in memory for 10 minutes to reduce external calls and improve performance.

- **Resilience:**  
  If the API call fails, the service attempts to read from a local `response.json` file as a fallback.

- **Configuration:**  
  The API base URL is read from `appsettings.json` under `OpenBreweryConfig:BaseAPIUrl`. Defaults to `https://api.openbrewerydb.org/v1/breweries` if not set.

---

## How to Run

1. **Configure `appsettings.json`:**
    ```json
    {
      "OpenBreweryConfig": {
        "BaseAPIUrl": "https://api.openbrewerydb.org/v1/breweries"
      }
    }
    ```

2. **Build and Run:**
    - Use Visual Studio 2022 or run:
      ```
      dotnet build
      dotnet run
      ```

---

## How to Test

1. **API Fetch:**
   - Call any controller endpoint that uses `BrewerySourceAPI` (e.g., `/brewery`).
   - Verify data is returned from the external API.

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

- The fallback file (`response.json`) should contain valid JSON data matching the expected API response.
- For automated testing, mock dependencies (`IMemoryCache`, `IConfiguration`, `ILogger<BrewerySourceAPI>`) to simulate different scenarios.

---