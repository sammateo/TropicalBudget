# Deployment Guide: Tropical Budget on Raspberry Pi

This guide explains how to deploy the **Tropical Budget** .NET Core application to a Raspberry Pi and manage it using `systemd`.

---

## Prerequisites

- Raspberry Pi running Linux (Raspberry Pi OS recommended)
- SSH access to the Pi
- `scp` available on your local machine

---

## Deployment Steps

### 1. Stop the Running Service

SSH into your Raspberry Pi and stop the service:

```bash
ssh <pi-username>@<pi-ip-address>
sudo systemctl stop tropical_budget.service
```

---

### 2. Publish the Application

On your local machine, publish the app for Linux ARM64:

```bash
dotnet publish -c Release -o <local-publish-folder> --runtime linux-arm64 --self-contained true
```

> Notes:
>
> - `--self-contained true` bundles the .NET runtime.
> - Output goes to `<local-publish-folder>`

---

### 3. Copy Files to the Raspberry Pi

Use `scp` to transfer the files:

```bash
scp -r <local-publish-folder>/* <pi-username>@<pi-ip-address>:<remote-publish-folder>
```

---

### 4. Start the Service

On the Raspberry Pi, start the service:

```bash
sudo systemctl start tropical_budget.service
```

---

### 5. Check Service Status

Verify that the service is running correctly:

```bash
sudo systemctl status tropical_budget.service
```

---

## Service File Reference

If you need to create or modify the service file:

```ini
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

### Apply Changes

After updating the service file:

```bash
sudo systemctl daemon-reload
sudo systemctl enable tropical_budget.service
sudo systemctl start tropical_budget.service
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

---

## Notes

- Ensure your executable has proper permissions:

  ```bash
  chmod +x <remote-publish-folder>/TropicalBudget
  ```

- Running as a non-root user is safer.
