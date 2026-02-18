# BmsBridge

BmsBridge is a cross‑platform (Windows today, Linux/macOS planned) telemetry bridge for commercial Building Management System (BMS) controllers. It polls supported controllers, discovers relevant points automatically, and publishes normalized change‑of‑value telemetry to Azure IoT Hub with minimal configuration.

## Features

- **Automatic point discovery** for supported BMS controllers  
- **Supported devices (today):**
  - Danfoss 800(A) series
  - Emerson E2
  - Emerson E3
- **Future support planned:** Veeder‑Root TLS series
- **Azure IoT Hub integration** using X.509 authentication  
- **Change‑of‑value JSON telemetry** for low‑cost, low‑bandwidth reporting  
- **Circuit‑breaker safety model** to prevent controller overload  
- **Replay mode** for offline development and testing  
- **Runs indefinitely** as a Windows service or console application  
- **Minimal configuration** — only IP + device type required per controller  

---

## Why BmsBridge Exists

Most BMS controllers were never designed for cloud telemetry. They are sensitive to polling frequency, have inconsistent data models, and often require vendor‑specific drivers.

BmsBridge solves this by:

- Providing a **unified polling engine**  
- Normalizing telemetry into a **simple 1‑D JSON format**  
- Automatically discovering points  
- Protecting controllers with a **dedicated health worker**  
- Handling **Azure IoT provisioning, authentication, and telemetry**  

This makes it possible to use Azure IoT Hub as a modern monitoring solution for legacy BMS hardware.

---

## Architecture Overview

BmsBridge is composed of several cooperating services:

- **Device Runner**  
  Polls a single BMS controller, discovers points, and emits telemetry.

- **Circuit Breaker Service**  
  Pauses/resumes polling when controllers show signs of overload.

- **Health Monitor Worker**  
  Continuously evaluates device health and manages circuit states.

- **Normalizer Service**  
  Converts raw controller data into 1‑D JSON change‑of‑value events.

- **IoT Device Layer**  
  Authenticates using X.509, provisions via DPS, and sends telemetry.

- **Replay Mode (dev only)**  
  Allows testing against previously captured device responses.

---

## Configuration

BmsBridge uses a single `appsettings.json` file.  
If the file is missing, the program will generate one with defaults.

### **Required Sections**

#### `AzureSettings`
All fields are required.

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

Used to authenticate with Azure Key Vault, retrieve the provisioning key, and register the IoT device.

#### `NetworkSettings`
List of BMS devices to poll.

```json
"NetworkSettings": {
  "bms_devices": [
    {
      "ip": "12.345.67.890",
      "device_type": "Danfoss"
    }
  ]
}
```

Supported device types: `Danfoss`, `EmersonE2`, `EmersonE3`.

#### `GeneralSettings`
Polling and runtime behavior.

```json
"GeneralSettings": {
  "http_request_delay_seconds": 5,
  "http_timeout_delay_seconds": 30,
  "http_retry_count": 1,
  "soft_reset_interval_hours": 12,
  "keep_alive": false
}
```

#### `LoggingSettings`
Serilog configuration.

```json
"LoggingSettings": {
  "MinimumLevel": "Information",
  "FileSizeLimitBytes": 1000000,
  "RetainedFileCountLimit": 7
}
```

---

## Running BmsBridge

### System Requirements

BmsBridge is built on .NET and requires the following:

#### **Runtime Requirements**
- **.NET 10.0 Runtime** (or newer) to run  
  Download from: https://dotnet.microsoft.com/download
- **.NET 10.0 SDK** (or newer) to develop  
  Download from: https://dotnet.microsoft.com/download

#### **Supported Operating Systems**
- **Windows 10 / Windows 11**  
- **Windows Server 2019+**

> Linux and macOS support are planned for future releases.

#### **Hardware Requirements**
- 1 GB RAM minimum  
- 50 MB free disk space  
- Stable network connection to BMS controllers  
- Outbound HTTPS access to Azure services

#### **Azure Requirements**
- Azure subscription  
- Azure IoT Hub  
- Azure Device Provisioning Service (DPS)  
- Azure Key Vault with:
  - X.509 certificate stored in the vault or local machine store  
  - Access policies allowing certificate retrieval  

#### **BMS Network Requirements**
- Direct IP reachability to each controller  
- Open ports required by the specific BMS device type  
- Consistent polling access (no aggressive firewall throttling)

### **Development**
```
dotnet run
```

### **Replay Mode (dev only)**
```
dotnet run -- --replay
```

### **Production (Windows)**
Publish as a single‑file executable:

```
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The output EXE can be run directly or installed as a Windows service.

---

## Telemetry Format

All telemetry is emitted as **flat 1‑D JSON** with only changed values, refreshed by soft interval window:

```json
{
  "ip": "12.345.67.890",
  "device_key": "E2-01",
  "data": {
    "Case1_Temp": 34.2,
    "Compressor1_State": "On"
  }
}
```

This keeps payloads small and easy to parse downstream.

---

## Replay Mode

Replay mode allows developers to test against previously captured device responses without needing access to real hardware.

- Only available in **Development** environment  
- Triggered via `--replay`  
- Useful for rapid iteration and debugging  

---

## Roadmap

- Support for **Veeder‑Root TLS** controllers  
- **Linux** and **macOS** runtime support  
- Additional cloud solution integration  
- Additional BMS controller drivers  
- Expanded telemetry normalization  

---

## Contributing

Pull requests are welcome.  
