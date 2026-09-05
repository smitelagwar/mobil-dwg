# AŞAMA 22 Kanıtı — Android Release/AAB/compliance RC

## Durum

`DONE`

AŞAMA 22 çıkış kriterleri platform-neutral C# testleri (7/7 PASS), katman mimari testleri (`MobilDwg.Architecture.Tests`), yetkili uyumluluk denetçisi (`CadReleaseRcAuditor`), SPDX-2.3 uyumlu SBOM üretimi, 100% çevrimdışı Data Safety güvencesi, Autodesk ticari marka feragatnamesi, AndroidManifest IntentFilter dosya ilişkilendirmeleri, imzalı Release APK ve Android App Bundle (`.aab`) üretimleri ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama `schema=compliance-rc/v1` deterministik snapshot ile Android Release RC kapı onayını (`ANDROID_STAGE22_RELEASE_RC_PASS`) kapatır.

## Kapsam ve Kararlar

- Base `main` HEAD: `450f2b5` (A21 tamamlanması sonrası).
- Branch: `stage22-release-rc`.
- PR: `#35` (`feat(stage22): android release aab and compliance rc with api36 acceptance gate`).
- **Nihai Paket Metaverisi (Final Package Metadata)**:
  - Package ID: `com.smitelagwar.mobildwg`
  - Application Title: `Mobil DWG`
  - Application Display Version: `1.0.0`
  - Application Version Code: `1`
  - Min SDK Version: `24` (Android 7.0 Nougat)
  - Target SDK Version: `36` (Android 16 Vanilla Ice Cream)
  - Compile Framework: `net10.0-android36.0`
  - Single Project MAUI: `true`
  - Production Ready: `true`

- **Veri Güvenliği ve Gizlilik Denetimi (Data Safety & Privacy Audit)**:
  - 100% Çevrimdışı (Offline-only): `true`.
  - Ağ Erişimi: `false` — AndroidManifest dosyasında ve kod tabanında sıfır `android.permission.INTERNET`.
  - Kullanıcı Verisi Toplama: `false`.
  - Telemetri ve Analitik Takibi: `false` (sıfır 3. taraf takip kütüphanesi).
  - Reklam SDK Entegrasyonu: `false`.
  - Depolama Modeli: `AppPrivateScopedStorage` (Android Scoped Storage uyumlu, harici geniş depolama izni istemez).

- **Bağımlılık SBOM ve Royalty-Free Lisans Denetimi**:
  - SPDX-2.3 uyumlu SBOM üretildi (`artifacts/a22-android-release-rc/SBOM.json` ve SPDX Text).
  - 6 Yetkili Bağımlılık Doğrulandı (%100 Permissive / Royalty-Free):
    1. `ACadSharp` 3.7.1 [MIT] (Royalty Free, CAD parsing)
    2. `SkiaSharp` 4.151.1 [MIT] (Royalty Free, 2D vector rendering)
    3. `SkiaSharp.NativeAssets.Android` 4.151.1 [MIT] (Royalty Free, Android native rendering engine)
    4. `Microsoft.Maui.Controls` 10.0.100 [MIT] (Royalty Free, UI framework)
    5. `Microsoft.Maui.Core` 10.0.100 [MIT] (Royalty Free, Application core)
    6. `System.Text.Encoding.CodePages` 10.0.1 [MIT] (Royalty Free, Windows-1254 Türkçe kod sayfası)
  - Sıfır kapalı kaynaklı CAD SDK (AutoCAD ObjectARX, RealDWG, Teigha, ODA yoktur).
  - Sıfır GPL/AGPL copyleft bağımlılık.
  - Sıfır çalışma zamanı telif veya lisans ücreti yükümlülüğü.

- **Ticari Marka ve Yasal Bildirimler**:
  - Açık Autodesk Feragatnamesi: *"AutoCAD and DWG are trademarks or registered trademarks of Autodesk, Inc. in the United States and other countries. Mobil DWG is an independent project and is not affiliated with, endorsed by, sponsored by, or associated with Autodesk, Inc."*
  - Telif ve Yasal Bildirim Belgesi Üretildi: `artifacts/a22-android-release-rc/LEGAL_NOTICES.txt`.

- **Erişilebilirlik ve Tema Uyumluluğu**:
  - Ekran Okuyucu (TalkBack / Accessibility): Tüm etkileşimli butonlarda (`DWG/DXF seç`, `İptal iste`, `Çizimi kapat`) `AutomationProperties.SetName` ve `HelpText` tanımlandı.
  - Minimum Dokunma Alanı (Touch Target): Tüm etkileşimli kontroller için en az 48dp (`MinimumHeightRequest = 48`).
  - Koyu ve Açık Tema Desteği: `RenderColorContext.Dark` (`#101010` arkaplan / `#F2F2F2` önplan) ve `RenderColorContext.Light` (`#FAFAFA` arkaplan / `#101010` önplan) doğrulanmış renk çözücüsü.

- **DWG/DXF Dosya İlişkilendirmeleri (IntentFilters)**:
  - `MainActivity.cs` üzerinde `IntentFilter` yapılandırması:
    - `ACTION_VIEW` + `CATEGORY_DEFAULT` + `CATEGORY_BROWSABLE`.
    - MIME türleri: `application/acad`, `image/vnd.dwg`, `image/x-dwg`, `application/dxf`, `image/vnd.dxf`.
    - Şemalar: `content`, `file`.
    - Dosya kalıpları: `.*\.dwg`, `.*\.dxf`.

- **Paket ve Dağıtım Bütçeleri (Release Artifacts & Budgets)**:
  - İmzalı Release APK (`com.smitelagwar.mobildwg-Signed.apk`): **39,524,374 bayt** (~37.7 MB, < 45 MB tavan bütçesi altında).
  - İmzalı Release AAB (`com.smitelagwar.mobildwg-Signed.aab`): **38,978,709 bayt** (~37.1 MB, multi-ABI evrensel paket arm64-v8a + x86_64, < 45 MB tavan bütçesi altında; cihaz başına Play Store indirme boyutu ~19 MB).
  - İşletim Sistemi Toplam PSS (`dumpsys meminfo`): **133.0 MB** (< 250 MB tavan bütçesi altında).
  - Süreç ve Kararlılık: PID `3791`, 0 çökme, 0 ANR, süreç sürekliliği tam korundu.

- **Deterministik Semantik Snapshot (`schema=compliance-rc/v1`)**:
  - SHA256: `30bc1164cdb5406e0e79b0730b9fb2e77292cb89cebdce7359654c392b7eb439`.
  - Snapshot Doğrulama Dosyası: `artifacts/a22-android-release-rc/COMPLIANCE_RC_SNAPSHOT.json`.

- **Release RC Kararı (`CadReleaseRcVerdict`)**:
  - Karar: `ANDROID_STAGE22_RELEASE_RC_PASS` (`isPass=True`, `blockers=0`, `score=100/100`).

- **Android API 36 Emülatör Ölçümleri**:
  - Cihaz: `emulator-5554`, Android 16 (API 36, `sdk_gphone64_x86_64`).
  - Görsel Doğrulama Ekran Görüntüsü: `a22-real-app-release-rc.png` (214,908 bayt, PNG imzası geçerli, SHA256: `f62f5405c778d1e7e29ad78e6b5a4d6d185db7efcc29a16adde9699cb8dee4fa`).
  - UI Durumu: `uiautomator dump` hiyerarşisinde `ANDROID_STAGE22_RELEASE_RC_PASS` durumu doğrulandı.

- **Temiz Mimari ve Sınır Koruması**:
  - `MobilDwg.App` katmanında `SkiaSharp` veya `ACadSharp` ad alanlarına doğrudan bağımlılık olmaksızın, `MobilDwg.Architecture.Tests` (%100 PASS) ile sınır kuralları tam korundu.

- **Fiziksel Cihaz Sınır Beyanı (Claim Boundary)**:
  - `A22_RELEASE_RC_API36_ONLY_NOT_PHYSICAL_DEVICE_FIDELITY`
  - `PHYSICAL_ANDROID: DEFERRED_RELEASE_DEVICE_GATE` (Yayın öncesi fiziksel donanım kabul kapısı için ertelenmiştir).

## AŞAMA 22 Gereksinim Matrisi

| Gereksinim | Doğrulama Mekanizması | Durum |
|---|---|---|
| Paket Metaverisi (1.0.0, SDK 36, Min 24) | `TestPackageMetadataAndTargetSdk36` | PASS |
| 100% Çevrimdışı Data Safety (0 INTERNET) | `TestDataSafetyZeroNetworkPermissionsAudit` + Manifest Regex | PASS |
| Bağımlılık SBOM & Telifsiz Lisans Denetimi | `TestDependencySbomAndRoyaltyFreeLicenseAudit` + SBOM Export | PASS |
| Autodesk Ticari Marka Feragatnamesi | `TestTrademarkNoticeAndLegalDisclaimers` + Legal Notices Export | PASS |
| Erişilebilirlik (TalkBack, 48dp Dokunma) | `TestAccessibilityAndDarkLightThemeResolver` + UI Dump | PASS |
| Koyu ve Açık Tema Çözücüsü | `RenderColorContext` Dark & Light Doğrulaması | PASS |
| DWG/DXF IntentFilter İlişkilendirmesi | `MainActivity.cs` IntentFilter + Manifest Doğrulaması | PASS |
| Release APK Paket Boyutu (<45 MB) | 37.7 MB (39,524,374 bayt) | PASS |
| Release AAB Paket Boyutu (<45 MB) | 37.1 MB (38,978,709 bayt, multi-arch) | PASS |
| Dumpsys Meminfo PSS (<250 MB) | 133.0 MB | PASS |
| Deterministik Snapshot (`compliance-rc/v1`) | `ComplianceRcSemanticSnapshot.Create` | PASS |
| Release RC Kararı (100/100) | `CadReleaseRcAuditor.EvaluateReleaseRc` | PASS |
| Host C# Testleri | `Stage22ReleaseRcTests` (7/7) | PASS |
| Mimari Katman Testleri | `MobilDwg.Architecture.Tests` | PASS |
| Gerçek Android API 36 Emülatör Kabulü | `scripts/a22-android-release-rc-gate.ps1` | PASS |
| Gerçek App UI Doğrulaması | `uiautomator dump` -> `ANDROID_STAGE22_RELEASE_RC_PASS` | PASS |
| Byte-Safe PNG Ekran Görüntüsü | `a22-real-app-release-rc.png` (214,908 bayt) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 3791, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri
- `STAGE22_RELEASE_RC_TESTS_PASS`:
  - `TestPackageMetadataAndTargetSdk36`: PASS
  - `TestDependencySbomAndRoyaltyFreeLicenseAudit`: PASS
  - `TestDataSafetyZeroNetworkPermissionsAudit`: PASS
  - `TestTrademarkNoticeAndLegalDisclaimers`: PASS
  - `TestAccessibilityAndDarkLightThemeResolver`: PASS
  - `TestComplianceRcSemanticSnapshotDeterminism`: PASS
  - `TestReleaseRcVerdictGatingBudgets`: PASS
  - `ExportComplianceReports`: PASS (`A22_COMPLIANCE_REPORTS_EXPORTED_PASS`)

### 2. Mimari Katman Testleri (`MobilDwg.Architecture.Tests`)
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`

### 3. Android API 36 Emülatör Kabul Çıktısı
- Kabul Komutu: `powershell -ExecutionPolicy Bypass -File scripts/a22-android-release-rc-gate.ps1`
- APK Boyutu: `39,524,374` bayt (SHA256: `97d0e544d64cb20fdf9cc08948e8174e6d3e61a7621761ddf2fa7dca905b1cc9`)
- AAB Boyutu: `38,978,709` bayt (SHA256: `c8dcb33f925bbd3df43a96ae7b1dff90d1f7bebb6c8889c0953184cfb786fa0b`)
- Manifest Doğrulaması:
  - `A22_MANIFEST_DATA_SAFETY_PASS zeroInternetPermission=true`
  - `A22_MANIFEST_SDK_PASS minSdk=24 targetSdk=36`
  - `A22_MANIFEST_INTENT_FILTER_PASS dwgDxfAssociations=true`
- Emülatör Kurulumu ve Başlatma:
  - `A22_REAL_APP_INSTALL_PASS package=com.smitelagwar.mobildwg launcher=com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
  - `A22_REAL_APP_LAUNCH_PASS pid=3791`
- Logcat İşaretçileri:
  - `A22_PACKAGE_METADATA_PASS package=com.smitelagwar.mobildwg version=1.0.0 targetSdk=36 minSdk=24`
  - `A22_DATA_SAFETY_PASS internet=False tracking=False ads=False storage=AppPrivateScopedStorage`
  - `A22_DEPENDENCY_SBOM_PASS count=6 allRoyaltyFree=True allAudited=True`
  - `A22_TRADEMARK_NOTICES_PASS autodeskDisclaimed=True royaltyFree=True`
  - `A22_ACCESSIBILITY_THEME_PASS screenReader=True minTouch=48dp darkLight=True`
  - `A22_RC_GATE_VERDICT_PASS marker=ANDROID_STAGE22_RELEASE_RC_PASS isPass=True score=100`
  - `A22_SNAPSHOT_PASS sha256=30bc1164cdb5406e0e79b0730b9fb2e77292cb89cebdce7359654c392b7eb439`
  - `A22_ANDROID_RENDER_PASS bytes=76635 sha256=fe079fcd038a625862200a36c026a63efccf3e3cb65529ee9ca3611aaff10fab`
  - `A22_REAL_APP_STABILITY_PASS pid=3791`
  - `A22_REAL_APP_UI_IMAGE_READY sha256=fe079fcd038a625862200a36c026a63efccf3e3cb65529ee9ca3611aaff10fab`
  - `A22_REAL_APP_RC_MARKERS_PASS`
- UI ve Ekran Görüntüsü Kabulü:
  - `A22_REAL_APP_UI_STATUS_PASS` (`ANDROID_STAGE22_RELEASE_RC_PASS` UI dump içinde doğrulandı)
  - `A22_SCREENSHOT_PNG_PASS bytes=214908 sha256=f62f5405c778d1e7e29ad78e6b5a4d6d185db7efcc29a16adde9699cb8dee4fa`
- Dumpsys Meminfo PSS:
  - `A22_MEMINFO_PSS_PASS total_pss=133 MB`
- Kararlılık ve Sonuç:
  - `A22_REAL_APP_STABILITY_PASS pid=3791`
  - `A22_COMPLIANCE_REPORTS_PASS`
  - `ANDROID_STAGE22_RELEASE_RC_PASS`
