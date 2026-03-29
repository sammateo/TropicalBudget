# Tropical Budget – README

Tropical Budget is a **.NET 10** web application that uses **Supabase** for database access, **Auth0** for authentication, and **Sentry** for logging. This guide explains how to configure the app, run it locally, and deploy it to a Raspberry Pi.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development](#local-development)
3. [Configuration](#configuration)
   - [Sentry](#sentry)
   - [Auth0](#auth0)
   - [Supabase](#supabase)

4. [Deployment to Raspberry Pi](#deployment-to-raspberry-pi)
   - [Manual Deployment Steps](#manual-deployment-steps)
   - [Service File Reference](#service-file-reference)

5. [Deployment Script](#deployment-script)
6. [Useful Commands](#useful-commands)

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- Supabase project with a connection string
- Auth0 application with Domain, Client ID, and Client Secret
- Sentry project for logging
- Raspberry Pi running Linux (for deployment)
- SSH access to the Raspberry Pi
- `scp` installed on your local machine

---

## Local Development

Run the application locally using .NET CLI:

```bash id="b2m3ad"
dotnet watch run --launch-profile https
```

> Hot reload is enabled, so code changes are reflected automatically.

---

## Configuration

The app requires some settings in `appsettings.json` (or `appsettings.Development.json`):

### Sentry

```json id="sentryconfig"
"Sentry": {
  "Dsn": "<your-sentry-dsn>"
}
```

### Auth0

```json id="auth0config"
"Auth0": {
  "Domain": "<your-auth0-domain>",
  "ClientId": "<your-auth0-client-id>",
  "ClientSecret": "<your-auth0-client-secret>"
}
```

### Supabase

```json id="supabaseconfig"
"ConnectionStrings": {
  "SupabaseConnection": "<your-supabase-connection-string>"
}
```

> **Tip:** Keep secrets private using `.NET Secret Manager` or environment variables. Do not commit them to version control.

---

## Deployment to Raspberry Pi

### Manual Deployment Steps

1. **Stop the running service:**

```bash id="stopservice"
ssh <pi-username>@<pi-ip-address>
sudo systemctl stop tropical_budget.service
```

2. **Publish the application:**

```bash id="publishapp"
dotnet publish -c Release -o <local-publish-folder> --runtime linux-arm64 --self-contained true
```

3. **Copy files to the Pi:**

```bash id="copypi"
scp -r <local-publish-folder>/* <pi-username>@<pi-ip-address>:<remote-publish-folder>
```

4. **Start the service:**

```bash id="startservice"
ssh <pi-username>@<pi-ip-address>
sudo systemctl start tropical_budget.service
```

5. **Check service status:**

```bash id="statusservice"
sudo systemctl status tropical_budget.service
```

---

### Service File Reference

```ini id="servicefile"
[Unit]
Description=Tropical Budget Web Application
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=always
RestartSec=1
User=<pi-username>
WorkingDirectory=<remote-publish-folder>
ExecStart=<remote-publish-folder>/TropicalBudget
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

**Apply changes after updating the file:**

```bash id="daemonreload"
sudo systemctl daemon-reload
sudo systemctl enable tropical_budget.service
sudo systemctl start tropical_budget.service
```

---

## Deployment Script

You can automate deployment using the following PowerShell script (`deploy.ps1`):

```powershell id="deployscript"
param(
    [Parameter(Mandatory=$true)]
    [string]$PiUser,

    [Parameter(Mandatory=$true)]
    [string]$PiHost,

    [Parameter(Mandatory=$true)]
    [string]$RemoteFolder,

    [Parameter(Mandatory=$false)]
    [string]$LocalPublishFolder = "C:\publishfolder",

    [Parameter(Mandatory=$false)]
    [string]$ServiceName = "tropical_budget.service"
)

Write-Host "Stopping service on Raspberry Pi..."
ssh "$PiUser@$PiHost" "sudo systemctl stop $ServiceName"

Write-Host "Publishing .NET app..."
dotnet publish -c Release -o $LocalPublishFolder --runtime linux-arm64 --self-contained true

Write-Host "Copying files to Raspberry Pi..."
scp -r "$LocalPublishFolder\*" "$PiUser@$PiHost:$RemoteFolder"

Write-Host "Starting service on Raspberry Pi..."
ssh "$PiUser@$PiHost" "sudo systemctl start $ServiceName"

Write-Host "Checking service status..."
ssh "$PiUser@$PiHost" "sudo systemctl status $ServiceName --no-pager"
```

**Usage:**

```powershell id="deployusage"
.\deploy.ps1 -PiUser <pi-username> -PiHost <pi-ip-address> -RemoteFolder <remote-publish-folder>
```

---

## Useful Commands

- **Restart service:**

  ```bash
  sudo systemctl restart tropical_budget.service
  ```

- **Stop service:**

  ```bash
  sudo systemctl stop tropical_budget.service
  ```

- **View logs:**

  ```bash
  journalctl -u tropical_budget.service -f
  ```

- **Make executable:**

  ```bash
  chmod +x <remote-publish-folder>/TropicalBudget
  ```

---

This setup allows you to **develop locally**, **test changes**, and **deploy updates to your Raspberry Pi** quickly and safely.
