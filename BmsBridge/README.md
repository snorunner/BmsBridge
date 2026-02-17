# BmsBridge  
Version **0.2.0**

BmsBridge is a service that polls Building Management System (BMS) controllers and sends their data to Azure IoT Hub. It is designed to run continuously with minimal configuration and a strong focus on controller safety.

---

## What This Program Does

- Connects to supported BMS controllers  
- Automatically discovers important data points  
- Polls the controllers on a schedule  
- Sends only **changed values** to Azure IoT Hub  
- Protects controllers from overload using a built‑in health monitor  
- Writes logs and error markers for troubleshooting  

---

## Supported Controllers

- Danfoss 800(A) series  
- Emerson E2  
- Emerson E3  
- More devices coming in future versions  

---

## Required Configuration

Before running BmsBridge, you **must** edit the `appsettings.json` file located next to the executable.

If the file is missing, the program will create one with default values.

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

## How to Run the Program

Double‑click the executable or run it from a command prompt. You may also register it as an nssm service.

The program will:

1. Load configuration  
2. Connect to Azure  
3. Begin polling devices  
4. Send telemetry to IoT Hub  

The program is designed to run indefinitely.

---

## Log Files

Logs are written to:

```
logs/app.log
```

If you need support, send this file to your administrator or support contact.

---

## Error Marker Files (`*.err`)

The program may create small `.err` files next to the executable.  
These indicate that a device is temporarily paused for safety.

- They are created and removed automatically  
- They contain no data  
- They are safe to delete when the program is stopped  

---

## Safety Features

BMS controllers can be sensitive to frequent polling.  
To protect them, BmsBridge includes:

- A **health monitor worker**  
- A **circuit breaker** that pauses polling when needed  
- Automatic recovery when the controller becomes healthy again  

These features run automatically and require no user action.

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

---

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

