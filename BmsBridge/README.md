
# BmsBridge  
Version **0.5.0**


**BmsBridge** 
BmsBridge is a service that polls Building Management System (BMS) controllers and sends their data to Azure IoT Hub. It is designed to run continuously with minimal configuration and a strong focus on controller safety.
- Connects to supported BMS controllers  
- Automatically discovers important data points  
- Polls the controllers on a schedule  
- Sends only **changed values** to Azure IoT Hub  
- Protects controllers from overload using a built‑in health monitor  
- Writes logs and error markers for troubleshooting  


## Supported Controllers

- Danfoss 800(A) series  
- Emerson E2  
- Emerson E3  
- More devices coming in future versions  


---

## Table of Contents

- [High-level overview](#high-level-overview)
- [Core concepts](#core-concepts)
- [Configuration model](#configuration-model)
- [Running the service](#running-the-service)
- [Log Files](#log-files)
- [Error Marker files](#error-markerfiles)
- [Runtime architecture](#runtime-architecture)
- [Polling lifecycle](#polling-lifecycle)
- [Health monitoring & circuit breaking](#health-monitoring--circuit-breaking)
- [Repository structure](#repository-structure)
- [Startup behavior](#startup-behavior)
- [Azure integration](#azure-integration)
- [Replay / development mode](#replay--development-mode)
- [Troubleshooting](#troubleshooting)
- [Building a Single File Executable](#building-a-single-file-executable)
- [Support](#support)

---

## High-level overview

At a high level, BmsBridge does the following:

1. Reads a list of BMS devices (IP + device type) from configuration
2. Starts one independent **polling loop per device**
3. Periodically queries each device via HTTP
4. Normalizes the raw responses into a consistent JSON format
5. Diffs each poll against the previous poll
6. Publishes **only changes** to Azure IoT Hub
7. Continuously monitors device health and dynamically pauses polling when devices are unhealthy

The system is intentionally **pull-based**, **fault-tolerant**, and **conservative** in how it interacts with field devices.

---

## Core concepts

### Polling
Polling is the act of repeatedly querying a device for its current state.  
In this system, polling is:
- continuous
- stateful (each poll is compared to the previous poll)
- isolated per device (one failing device does not affect others)

### Device runner
A **device runner** is a long-lived background task responsible for exactly one device IP.  
Each runner:
- owns its polling loop
- can be paused, resumed, or put into probe mode
- tracks its own health via shared infrastructure

### Device client
A **device client** contains the vendor-specific logic required to talk to a controller (E2, E3, Danfoss).  
It knows:
- which endpoints to call
- how to parse responses
- how to assemble a logical snapshot of the device

### Diff-based telemetry
Instead of publishing full snapshots every time, BmsBridge:
- stores the previous normalized payload in memory
- computes a JSON diff against the new payload
- publishes only changed fields

This dramatically reduces telemetry volume and downstream noise.

---

## Configuration model
Before running BmsBridge, you **must** edit the `appsettings.json` file located next to the executable.

If the file is missing, the program will create one with default values.

### Configuration sections

| Section | Purpose |
|------|------|
| `AzureSettings` | Azure IoT / DPS / Key Vault wiring |
| `NetworkSettings` | Defines which devices exist |
| `GeneralSettings` | Controls polling & HTTP behavior |
| `LoggingSettings` | Logging verbosity & file paths |

---

### 1. Azure Settings (Required)

These values allow the program to authenticate with Azure Key Vault and provision an IoT device.

```json
"AzureSettings": {
  "tenant_id": "",
  "client_id": "",
  "device_id": "",
  "scope_id": "",
  "secret_name": "",
  "vault_name": "",
  "certificate_subject": "",
  "sas_ttl_days": "90"
}
```

All fields must be filled in.

### 2. BMS Devices (Required)

List each controller you want the program to poll.
device_type must be one of ("Danfoss", "EmersonE2", or "EmersonE3").

```json
"NetworkSettings": {
  "bms_devices": [
    {
      "ip": "11.170.71.182",
      "device_type": "Danfoss"
    }
  ]
}
```

### 3. General Settings

Controls polling behavior and timeouts.

```json
"GeneralSettings": {
  "http_request_delay_seconds": 5,
  "http_timeout_delay_seconds": 30,
  "http_retry_count": 1,
  "soft_reset_interval_hours": 12,
  "keep_alive": false,
  "use_cloud": true
}
```
When use_cloud is false, telemetry will be dumped **without truncation** to a jsonl file. This is useful for testing and seeing local payloads.

### 4. Logging Settings

Controls log file size and retention.

```json
"LoggingSettings": {
  "MinimumLevel": "Information",
  "FileSizeLimitBytes": 1000000,
  "RetainedFileCountLimit": 7
}
```

Recommended options for "MinimumLevel" are "Information" and "Debug"

---

### Key principle
**Configuration is a deployment artifact, not a source artifact.**

The repository does not ship with a real `appsettings.json`.

---

### First-run behavior

On startup:
- If `appsettings.json` does not exist:
  - a default file is generated
  - the program exits
- If it exists:
  - missing keys are merged in automatically
  - existing values are preserved

This allows safe upgrades without manual config migration.

---




## Running the service

### First run
Double‑click the executable or run it from a command prompt. You may also register it as an nssm service.

The program will:

1. Load configuration  
2. Connect to Azure  
3. Begin polling devices  
4. Send telemetry to IoT Hub  

The program is designed to run indefinitely.

Running from the command line:
```bash
dotnet run
```

### Configure

Edit appsettings.json:
add device IPs
add Azure configuration
adjust polling parameters if needed

## Log Files

Logs are written to:

```
logs/app.log


## Error Marker Files (`*.err`)

The program may create small `.err` files next to the executable.  
These indicate that a device is temporarily paused for safety.



## Runtime architecture
┌────────────┐
│ Program.cs │
└─────┬──────┘
      │
      ▼
┌──────────────────┐
│ DeviceWorker     │   ← creates & supervises runners
└─────┬────────────┘
      │
      ▼
┌──────────────────┐
│ DeviceRunner     │   ← one per device IP
│ (BaseDeviceRunner)│
└─────┬────────────┘
      │
      ▼
┌──────────────────┐
│ DeviceClient     │   ← vendor-specific logic
└─────┬────────────┘
      │
      ▼
┌──────────────────┐
│ HTTP Pipeline    │   ← throttle / retry / timeout
└─────┬────────────┘
      │
      ▼
┌──────────────────┐
│ Normalization    │
└─────┬────────────┘
      │
      ▼
┌──────────────────┐
│ Diff + IoT Send  │
└──────────────────┘

### In Parallel
┌────────────────────┐
│ HealthMonitorWorker│
└─────┬──────────────┘
      │
      ▼
┌────────────────────┐
│ Circuit Breaker    │
└─────┬──────────────┘
      │
      ▼
┌────────────────────┐
│ Runner Control     │  ← pause / resume / probe
└────────────────────┘



---

## Polling lifecycle

Each device follows this lifecycle:

### 1. Initialization (once per runner)
Before polling begins, the device client performs any required discovery:
- E2: controller list, cell list, index mappings
- E3 / Danfoss: static metadata and supported endpoints

Initialization failures are treated as health failures.

---

### 2. Poll loop (continuous)

Each poll iteration:
1. Executes a sequence of HTTP operations against the device
2. Records latency and success/failure for each request
3. Aggregates raw responses into a logical snapshot
4. Normalizes the snapshot into flattened JSON
5. Diffs against the previous snapshot
6. Sends changes to IoT Hub (if any)
7. Waits before starting the next poll

Polling frequency is governed by configuration and health state.

---

### 3. Soft reset

At a configurable interval (`soft_reset_interval_hours`), all runners are:
- cancelled
- reconstructed
- restarted cleanly

This mitigates long-running edge cases such as:
- leaked resources
- stuck HTTP connections
- partial device state corruption

---

## Health monitoring & circuit breaking

Health is a **first-class concern** in this system. BMS controllers can be sensitive to frequent polling.  
To protect them, BmsBridge includes:

### Health tracking
Every HTTP request:
- is timed
- is classified into a success or error type
- updates a per-device health snapshot

Health snapshots track:
- consecutive failures
- last success timestamp
- last failure timestamp
- last observed latency
- current circuit state

---

### Circuit breaker behavior

Each device operates in one of three states:

- **Closed** – normal polling
- **Open** – polling paused after repeated failures
- **Half-open** – limited probing to test recovery

The health monitor:
- evaluates circuit state periodically
- pauses or resumes runners accordingly
- prevents unhealthy devices from being hammered continuously

This logic is centralized and independent from polling code. These features run automatically and require no user action.

---


## Repository structure

BmsBridge/
├── Configuration/        # Typed config models + config bootstrap
├── Workers/              # Hosted background services
├── Devices/              # Polling runners, clients, operations
│   ├── E2/
│   ├── E3/
│   └── Danfoss/
├── Infrastructure/
│   ├── Http/             # HTTP pipeline (retry/throttle/timeout)
│   ├── HealthMonitor/    # Health, circuit breaker, runner control
│   ├── IotHub/           # IoT Hub + diff logic
│   └── Normalization/    # JSON flattening & envelopes
├── Resources/            # Embedded static data
├── Program.cs            # Application entry point
└── BmsBridge.Tests/      # Unit tests


---

## Startup behavior

At startup, the application:

1. Ensures configuration exists and is valid
2. Configures logging
3. Registers all services via dependency injection
4. Starts hosted workers:
   - `DeviceWorker`
   - `HealthMonitorWorker`

No polling begins until startup completes successfully.

---

## Azure integration

Telemetry is sent to **Azure IoT Hub** using **DPS (Device Provisioning Service)**.

Key points:
- No secrets are stored in configuration
- DPS enrollment group key is retrieved from Azure Key Vault
- Authentication to Key Vault uses a client certificate
- Device-specific symmetric keys are derived via HMAC
- Messages are chunked to remain under IoT Hub size limits

Azure integration is isolated behind `IIotDevice`.

---




## Replay Mode (Optional)

Replay mode is for development only and currently works only on Linux machines.  
It allows testing without real controllers.

To use it:

```
BmsBridge.exe --replay
```

Most users will never need this.
---

## Troubleshooting

**The program won’t start**  
- Ensure `appsettings.json` exists and is valid JSON  
- Ensure all Azure settings are filled in  

**No data is appearing in IoT Hub**  
- Check the log file  
- Verify network connectivity to the controllers  
- Ensure the device IDs and IPs are correct  

**A controller keeps pausing**  
- This is usually due to overload  
- Check the `.err` file and logs for details  


## Building a Single‑File Executable (Advanced)

If you need to rebuild the program:

```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The output EXE will appear in:

```
bin/Release/net10.0/win-x64/publish/
```

---

## Support

For assistance, contact your system administrator.
