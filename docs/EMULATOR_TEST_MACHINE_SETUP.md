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
  ├─► dotnet test MobilDwg.sln (Unit, Contract & Architecture Gates)
  ├─► .\scripts\start-emulator.ps1 (Launches AVD 'mobil-dwg-api36')
  ├─► dotnet build -f net10.0-android (Generates Debug/Release APK)
  ├─► adb install & launch (Verifies crash-free activity on API 36)
  ├─► adb screencap & logcat (Saves test evidence artifacts)
  │
  ▼
GitHub Actions Artifacts (Screenshots & Logs)
```

---

## 2. Toolchain & Environment Specifications

| Component | Verified Version | Installation Path / Identifier |
|---|---|---|
| **Operating System** | Windows 11 Home 64-bit | Build 26100 (AMD Ryzen 5 7640HS, NVIDIA RTX 4060) |
| **Hypervisor / Accel** | Windows Hypervisor Platform (WHPX) | Firmware Virtualization Enabled (`SVM`), GPU Host Direct |
| **.NET SDK** | `10.0.400` (Pinned) | `C:\Program Files\dotnet\sdk\10.0.400` |
| **.NET MAUI Workload** | `maui-android` (`10.0.20/10.0.100`) | Workload set `10.0.400` |
| **Java JDK** | `Microsoft OpenJDK 21.0.12.1` | `C:\Program Files\Microsoft\jdk-21.0.12.101-hotspot` (`JAVA_HOME`) |
| **Android SDK Root** | API Level `36` (Android 16) | `C:\Users\hsyn\AppData\Local\Android\Sdk` (`ANDROID_HOME`, `ANDROID_SDK_ROOT`) |
| **Android Build-Tools** | `36.0.0` | `$ANDROID_SDK_ROOT\build-tools\36.0.0` |
| **Android Platform-Tools** | `37.0.1-15733141` (ADB) | `$ANDROID_SDK_ROOT\platform-tools` |
| **Android Emulator** | `37.1.11.0` | `$ANDROID_SDK_ROOT\emulator` |
| **Android Virtual Device** | `mobil-dwg-api36` | Google APIs x86_64, Pixel 7 Profile, Target API 36 |
| **Android Studio** | `2026.1.3.7` | `C:\Program Files\Android\Android Studio` |
| **GitHub Actions Runner** | `v2.322.0` | `C:\actions-runner` |

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

### Run Full Test Suite on Emulator
```powershell
.\scripts\run-emulator-tests.ps1 -Configuration Debug
```
Executes solution tests, boots emulator, builds APK, installs on emulator, verifies activity launch, and outputs `artifacts/emulator_test_result.png` and `artifacts/emulator_logcat.txt`.

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
2. Copy the registration token under **Configure**.
3. Open PowerShell as Administrator and run:
```powershell
& C:\actions-runner\register-and-run.ps1
```
4. Enter the token when prompted.
5. To run the runner in interactive mode:
```powershell
cd C:\actions-runner
.\run.cmd
```
6. (Optional) To run permanently as a Windows background service:
```powershell
cd C:\actions-runner
.\actions.runner.service.exe install
.\actions.runner.service.exe start
```

---

## 5. Automation & Safety Policy

1. **No Automatic Push Triggers**: The runner is configured exclusively for `workflow_dispatch` via `.github/workflows/android-emulator-test.yml`. It will **never** trigger unprompted on routine git commits or pushes.
2. **Physical Device Integrity**: Emulator automation is an additional automated gate. It does not replace or modify the mandatory physical device gate defined in `scripts/stage01-device-gate.ps1`.
3. **Zero Secrets in Code**: No authentication tokens, PATs, or signing keys are stored in files or commit history.
