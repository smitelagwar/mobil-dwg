# Android Emulator Automated Build/Test Machine Guide

## 1. Overview & Architecture

This Windows machine is configured as a dedicated **Self-Hosted Android Emulator Build & Test Node** for the `mobil-dwg` project.

```text
GitHub Actions / Dispatch Request
  │
  ▼
Self-Hosted Runner (Windows Node: C:\actions-runner)
  │
  ├─► .\scripts\doctor-local-environment.ps1 (Validates environment)
  ├─► solution build + executable harness marker runs (V01 validated)
  ├─► .\scripts\start-emulator.ps1 (Launches AVD 'mobil-dwg-api36')
  ├─► dotnet build -f net10.0-android (Currently temporary Stage01Smoke APK)
  ├─► adb install & launch (Infrastructure smoke on API 36)
  ├─► byte-safe PNG + numeric PID/crash/post-launch ANR evidence
  │
  ▼
GitHub Actions Artifacts (Screenshots & Logs)
```

---

## 2. Toolchain & Environment Specifications

| Component | Verified Version | Installation Path / Identifier |
|---|---|---|
| **Operating System** | Windows 11 Home 64-bit | Build 26100 (AMD Ryzen 5 7640HS, NVIDIA RTX 4060) |
| **Hypervisor / Accel** | Windows Hypervisor Platform (WHPX) | Firmware Virtualization Enabled (`SVM`); current AVD has `hw.gpu.enabled=no`, GPU Host Direct is not claimed |
| **.NET SDK** | `10.0.400` (Pinned) | `C:\Program Files\dotnet\sdk\10.0.400` |
| **.NET MAUI Workload** | `maui-android` (`10.0.20/10.0.100`) | Workload set `10.0.400` |
| **Java JDK** | `Microsoft OpenJDK 21.0.12.1` | `C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot` (`JAVA_HOME`) |
| **Android SDK Root** | API Level `36` (Android 16) | `C:\Users\hsyn\AppData\Local\Android\Sdk` (`ANDROID_HOME`, `ANDROID_SDK_ROOT`) |
| **Android Build-Tools** | `36.0.0` | `$ANDROID_SDK_ROOT\build-tools\36.0.0` |
| **Android Platform-Tools** | `37.0.1-15733141` (ADB) | `$ANDROID_SDK_ROOT\platform-tools` |
| **Android Emulator** | `37.1.11.0` | `$ANDROID_SDK_ROOT\emulator` |
| **Android Virtual Device** | `mobil-dwg-api36` | Google APIs x86_64, Pixel 7 Profile, Target API 36 |
| **Android Studio** | `2026.1.3.7` | `C:\Program Files\Android\Android Studio` |
| **GitHub Actions Runner** | `v2.336.0` | `C:\actions-runner` |

---

## 3. Local Maintenance & Automation Scripts

The repository includes ready-to-run automation scripts in `scripts/`:

### Environment Health Check
```powershell
.\scripts\doctor-local-environment.ps1
```
Audits all 11 system dependencies (.NET, JDK, SDK, Build-Tools, ADB, Emulator, AVD, Runner) and reports health status.

### Start Emulator
```powershell
.\scripts\start-emulator.ps1
```
Launches `mobil-dwg-api36` in background and blocks until `sys.boot_completed=1`.

### Stop Emulator
```powershell
.\scripts\stop-emulator.ps1
```
Gracefully shuts down running emulator instances.

### Android Emulator Automated Gate
```powershell
.\scripts\android-emulator-gate.ps1 -Configuration Release
```
Current V01-hardened gate executes prerequisite checks, builds the solution, explicitly runs the custom executable harnesses with required markers, boots/reuses the exact AVD, and builds/installs a temporary `Stage01Smoke` MAUI APK. It captures a signature-validated byte-safe PNG and requires numeric PID plus package/PID crash and post-launch ANR checks. It still does **not** install the real `MobilDwg.App`; therefore `ANDROID_EMULATOR_GATE_PASS` means `INFRASTRUCTURE_SMOKE_ONLY`. The real app artifact becomes the gate target in V04.

---

## 4. GitHub Actions Runner Configuration

The runner binaries are installed in `C:\actions-runner`.

### Required Runner Labels
- `self-hosted`
- `windows`
- `android-test`
- `mobil-dwg`

### Registering the Runner (One-Time User Action)
1. Go to repository settings: `https://github.com/smitelagwar/mobil-dwg/settings/actions/runners/new`
2. Copy the registration token shown under **Configure**.
3. Open PowerShell as Administrator and run:
```powershell
& C:\actions-runner\register-and-run.ps1
```
4. Enter the token when prompted.

### Runner Execution Mode (Interactive vs Service)
> [!IMPORTANT]
> **Recommended Execution Model: Interactive User Session (`.\run.cmd`)**
> Android Emulator and the runner are operated in the interactive desktop session. Keep `C:\actions-runner\run.cmd` active or configure it via user startup. A powered-on PC without a connected listener is not test-ready.

---

## 5. Automation & Trigger Policy

1. **Emulator Trigger Isolation**: The legacy `android-emulator-test.yml` workflow is not triggered by normal commits or pushes to `main` or feature branches. Separate V02/V03 self-hosted audit workflows may run on `main` or pull requests when their dependency/fixture path filters match. The V04 workflow introduced by PR `#17` may also use the self-hosted emulator on pull-request `opened/synchronize/reopened` events when app/Core/Cad/Rendering/architecture paths match; a push to an already-open PR is therefore not offline-safe by default.
2. **Dedicated Emulator Test Branch**: The Android emulator workflow triggers when:
   - Commits are pushed to the dedicated `android-test` branch, OR
   - Manually dispatched via GitHub Actions (`workflow_dispatch`).
3. **Physical Device Integrity**: Emulator automation is an additional automated gate in `scripts/android-emulator-gate.ps1`. It does not replace or modify the mandatory physical device gate defined in `scripts/stage01-device-gate.ps1`.
4. **Zero Secrets in Code**: No authentication tokens, PATs, or signing keys are stored in files or commit history.
5. **Offline Queue**: When the listener is offline, do not accumulate repeated workflow runs. Record the exact SHA in `PENDING_EMULATOR_QUEUE`, continue safe host-side work, and trigger only the latest still-required checkpoint after the runner returns.
6. **Trusted Refs Only**: Never execute an untrusted third-party PR/ref on this self-hosted Windows runner.

