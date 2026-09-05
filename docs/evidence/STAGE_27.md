# AŞAMA 27 Kanıtı — Android v1 artifact / yayın / handoff

## Durum

`DONE` — **PLAN COMPLETED (100%)**

AŞAMA 27 çıkış kriterleri; test/doğrulama kancalarından arındırılmış temiz üretim Release APK ve AAB paketlerinin derlenmesi, SHA-256 sağlama toplamlarının üretilmesi (`release/SHA256SUMS.txt`), 5 ana başlık altında eksiksiz yayın ve teslim dokümantasyonunun (`docs/release/`) hazırlanması, saf üretim paketinin API 36 Android Emulator üzerinde başarıyla kurularak başlatılması (PID: 5339, 88.9 MB PSS) ve UI hiyerarşisinin doğrulanması ile eksiksiz olarak sağlandı (`ANDROID_STAGE27_RELEASE_HANDOFF_PASS`). Bu aşama ile **Mobil DWG Android v1.0.0 Nihai Planı** %100 tamamlanmıştır.

## Kapsam ve Kararlar

- Base `main` HEAD: `2b51bcd`
- Branch: `stage27-release-handoff`
- **Nihai Üretim Paketleri (Release Artifacts)**:
  - **APK**: `release/MobilDwg-v1.0.0.apk` — Boyut: `37.96 MB` (< 45 MB bütçesi altında).
    - SHA-256: `f3fde60a2d6983c3f6d48887453fa06d58f0386131bd18e2645a1e2472b3b8aa`
  - **AAB**: `release/MobilDwg-v1.0.0.aab` — Boyut: `37.54 MB` (< 45 MB bütçesi altında).
    - SHA-256: `748bddf4e85c5162301a73cbcf09b488b6c30d631f1301ea353224c5a3b1a7ed`
  - **Sağlama Toplamları**: `release/SHA256SUMS.txt`
- **Yayın ve Teslimat Dokümantasyonu (`docs/release/`)**:
  - `BUILD_INSTRUCTIONS.md`: .NET SDK 10.0.400, maui-android, locked-mode restore ve temiz derleme yönergeleri (`A27_DOCUMENTATION_PASS`).
  - `PLAY_STORE_SUBMISSION_GUIDE.md`: Google Play Console formları, Target SDK 36, Scoped Storage, Data Safety kılavuzu.
  - `PRIVACY_POLICY.md`: %100 çevrimdışı, sıfır ağ izni, sıfır veri toplama/telemetri gizlilik politikası.
  - `COMPATIBILITY_AND_LIMITATIONS.md`: R12–2018+ DWG/DXF desteği, C4/C3 CAD varlık sadakat matrisi ve bilinen kısıtlar.
  - `THIRD_PARTY_NOTICES.md`: ACadSharp, SkiaSharp, Microsoft.Maui ve System.Text.Encoding lisansları ve Autodesk marka feragatnamesi.
- **Gerçek Android MAUI Saf Üretim Kabul Testi (API 36, emulator-5554)**:
  - Kurulum: `com.smitelagwar.mobildwg-Signed.apk` başarıyla kuruldu.
  - Canlı PID: `5339` (`A27_REAL_APP_PRODUCTION_LAUNCH_PASS`).
  - UI Hiyerarşi Onayı: `artifacts/a27-android-release-handoff/a27_window.xml` içinde standart görüntüleyici kontrolleri (`DWG/DXF seç`, `Mobil DWG`) doğrulandı.
  - Bellek: Dumpsys meminfo Total PSS `88.9 MB` (< 250 MB bütçesi altında, `A27_MEMINFO_PSS_PASS`).
  - Ekran Görüntüsü: `artifacts/a27-android-release-handoff/a27-real-app-release-production.png` (64.7 KB, byte-safe PNG).

## Kanıt Özeti

- `A27_DOCUMENTATION_PASS`
- `A27_HOST_TESTS_PASS`
- `A27_CHECKSUMS_PASS`
- `A27_REAL_APP_PRODUCTION_LAUNCH_PASS pid=5339`
- `A27_MEMINFO_PSS_PASS pss=88.9 MB`
- `ANDROID_STAGE27_RELEASE_HANDOFF_PASS`

## Claim Sınırı
`A27_CLAIM: A27_RELEASE_HANDOFF_API36_ONLY_NOT_PHYSICAL_DEVICE_FIDELITY`

## Nihai Plan Durumu
`PLAN_STATUS: ANDROID_V1_PLAN_COMPLETED_100%`
