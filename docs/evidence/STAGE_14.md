# AŞAMA 14 Kanıtı — TEXT / MTEXT / Türkçe / Font / SHX (Metin Ayrıştırma, Türkçe Karakter/CP1254, AutoCAD Kaçış Kodları, Sınırlı MTEXT Ayrıştırıcısı, Denetimli Font Eşleme ve Skia Render)

## Durum

`DONE`

AŞAMA 14 çıkış kriterleri platform-neutral C# unit testleri ve gerçek `MobilDwg.App` API 36 Android Emulator kabul testi üzerinde eksiksiz olarak sağlandı. Bu aşama Windows-1254 (CP1254) ve UTF-8 Türkçe karakter çözümlemesini (`CadTextEncoding`), AutoCAD Unicode (`\U+XXXX`) ve özel simge (`%%d`, `%%p`, `%%c`, `%%%`) kaçışlarını, ReDoS ve aşırı bellek tüketimini önleyen sınırlı MTEXT ayrıştırıcısını (`MTextParser`), telifli AutoCAD SHX dosyaları paketlenmeksizin standart sistem fontlarına denetimli eşleme tablosunu (`FontSubstitutionResolver`), metin hizalama/rotasyon/aynalama/sınır kutusu modelini (`TextPrimitive`, `CadTextAlignment`) ve SkiaSharp render entegrasyonunu (`SkiaCadRenderer`) kapatır; tam DWG/DXF dosya parse-to-scene entegrasyonu (AŞAMA 15–16) ve fiziksel cihaz tipografi testleri sonraki aşamalardadır.

## Kapsam ve Kararlar

- Base `main` HEAD: `91815ed` (PR #26 sonrası).
- Branch: `stage14-text-font-shx`.
- `CadTextEncoding` Türkçe karakter ve kodlama motoru:
  - `CodePagesEncodingProvider.Instance` kaydı ve yerel CP1254 arama tablosu (`Cp1254Lookup`).
  - Windows-1254 bayt dizilerini Türkçe karakter kaybı olmadan eksiksiz çözme: `Ç, ç, Ğ, ğ, İ, ı, Ö, ö, Ş, ş, Ü, ü`.
  - Otomatik UTF-8 tespiti ve geçersiz UTF-8 durumunda kontrollü CP1254 geri çekilme.
  - AutoCAD Unicode kaçış dizileri (`\U+XXXX`) ve küçük harfli (`\u+xxxx`) varyantların çözümlenmesi (`\U+00E7` -> ç, `\U+011F` -> ğ, `\U+0130` -> İ, `\U+015F` -> ş, vb.).
  - AutoCAD özel simge kodları: `%%d`/`%%D` -> `°` (derece), `%%p`/`%%P` -> `±` (artı-eksi), `%%c`/`%%C` -> `Ø` (çap), `%%%` -> `%` (yüzde); `%%o` ve `%%u` formatlama bayraklarının metinden temizlenmesi.
- `MTextParser` güvenli sınırlı MTEXT ayrıştırıcısı:
  - Güvenlik bütçeleri: En fazla 65,536 karakter giriş uzunluğu, en fazla 32 seviye parantez (`{...}`) derinliği, en fazla 4,096 satır/belirteç.
  - Biçimlendirme etiketlerinin (`\A...;`, `\C...;`, `\H...;`, `\W...;`, `\Q...;`, `\F...;`, `\S...^...;`) ve stil bayraklarının (`\L`, `\O`, `\K`) ayıklanması.
  - Satır sonu (`\P`, `\X`), bölünemez boşluk (`\~`), kaçışlı ters eğik çizgi (`\\`) ve parantezlerin (`\{`, `\}`) doğru ayrıştırılması.
  - Kesirli ifadelerin (`\S1^2;` -> `1/2`) temiz metin formatına çevrilmesi.
  - Aşırı derinlik durumunda `MTEXT_NESTING_EXCEEDED` ve aşırı uzunlukta `MTEXT_LENGTH_EXCEEDED` teşhisi üretimi.
- `FontSubstitutionResolver` denetimli font eşleme tablosu (Zero-Proprietary Font Policy):
  - **Telifli Font Paketlenmeme İlkesi:** Projenin lisans politikası (`compliance/LICENSE_POLICY.md`) gereğince hiçbir telifli AutoCAD `.shx` veya tescilli font dosyası repoya veya APK içerisine gömülmemiştir.
  - AutoCAD SHX fontları (`txt.shx`, `romans.shx`, `simplex.shx`, `isocp.shx`, `monotxt.shx`, `complex.shx` vb.) ve CAD TTF fontları (`arial`, `times`, `courier`) izin verilen standart sistem font ailelerine (`sans-serif`, `monospace`, `serif`) eşlenir.
  - Yapılan her eşleme için `SceneDiagnosticKind.Substituted` kategorisinde denetimli `FONT_SUBSTITUTION` teşhisi üretilir.
  - Bilinmeyen fontlarda sistem çökmesi önlenerek güvenli varsayılan fonta (`sans-serif`) geri çekilme sağlanır.
- `TextPrimitive` & `CadTextAlignment`:
  - `TextPrimitive : RenderGeometryPrimitive`: Metin, ekleme noktası, metin yüksekliği, radyan cinsinden rotasyon açısı, genişlik faktörü (`WidthFactor`), eğiklik açısı (`ObliqueAngleRadians`), yatay hizalama (`Left`, `Center`, `Right`, `Aligned`, `Middle`, `Fit`), düşey hizalama (`Baseline`, `Bottom`, `Middle`, `Top`), aynalama bayrakları (`Backward` [X aynalama], `UpsideDown` [Y aynalama]), istenen font ve çözümlenen font.
  - MTEXT 9 bağlantı noktasını (`TopLeft`, `TopCenter`, `TopRight`, `MiddleLeft`, `MiddleCenter`, `MiddleRight`, `BottomLeft`, `BottomCenter`, `BottomRight`) otomatik yatay/düşey hizalamaya çeviren yardımcı yöntem.
  - Metin sınır kutusu (`WorldBounds2`): Hizalama ötelemeleri, karakter sayısı, genişlik faktörü, aynalama ve rotasyon açısını dikkate alarak sahne çerçeveleme (`Camera2D.Fit`) için çift duyarlıklı kesin sınır kutusu üretimi.
- `SkiaCadRenderer` entegrasyonu:
  - `DrawPrimitive` içinde `TextPrimitive` kontrolü.
  - `Camera2D` ölçeğine göre ekran piksel metin boyutu hesaplama (`Height / WorldUnitsPerPixel`), alt-piksel metinleri eleme.
  - SkiaSharp `SKFont` ve `SKTypeface` üzerinden genişlik ölçekleme (`ScaleX`), eğiklik (`SkewX`), rotasyon (`RotateDegrees`) ve aynalama (`Scale(1f, -1f)`).
  - Skia native `SKTextAlign` (Sol, Orta, Sağ) ve düşey ofsetleme ile hassas metin çizimi.
  - Katman ve varlık renk bağlamına uygun `SKPaint` kullanımı.
- Deterministik format `text-scene/v1` (`TextSceneSemanticSnapshot`).

## AŞAMA 14 Gereksinim Matrisi

| Gereksinim | Uygulama / Test | Durum |
|---|---|---|
| CP1254 Türkçe Karakter Çözümleme | CadTextEncoding.DecodeCp1254, DecodeBytes | PASS |
| UTF-8 Otomatik Tespit & Fallback | CadTextEncoding.DecodeBytes | PASS |
| AutoCAD Unicode Kaçışları (\U+XXXX) | CadTextEncoding.DecodeAutoCadEscapes (\U+00E7, \U+011F, vb.) | PASS |
| AutoCAD Özel Simge Kodları (%%d, %%p, %%c, %%%) | CadTextEncoding.DecodeAutoCadEscapes (°, ±, Ø, %) | PASS |
| Sınırlı MTEXT Ayrıştırıcısı (\P, format etiketleri) | MTextParser.Parse | PASS |
| MTEXT ReDoS & Derinlik Muhafızı | MTextParser MaxNestingDepth=32, MaxInputLength=65536 | PASS |
| Zero-Proprietary SHX Font Eşleme Tablosu | FontSubstitutionResolver statik eşleme tablosu | PASS |
| Denetimli Font Eşleme Teşhisi | SceneDiagnosticKind.Substituted, FONT_SUBSTITUTION | PASS |
| Bilinmeyen Font Güvenli Geri Çekilme | FontSubstitutionResolver default sans-serif | PASS |
| Metin Hizalama Modeli | CadTextHorizontalAlignment, CadTextVerticalAlignment, AttachmentPoint | PASS |
| Metin Aynalama ve Rotasyon | CadTextMirrorFlags (Backward, UpsideDown), rotasyon açısı | PASS |
| Metin Dünya Sınır Kutusu Hesabı | TextPrimitive.CalculateBounds (rotasyon, ayna, genişlik) | PASS |
| SkiaSharp Metin Render Entegrasyonu | SkiaCadRenderer.DrawTextPrimitive | PASS |
| Deterministik Metin Sahne Envanteri | TextSceneSemanticSnapshot formatı text-scene/v1 | PASS |
| Host Testleri (Release) | Stage14TextTests (12/12 test) | PASS |
| Gerçek Android App Derleme & Paketleme | MobilDwg.App net10.0-android36.0 Release APK (A14Validation=true) | PASS |
| Gerçek Android API 36 Emülatör Kabulü | scripts/a14-android-text-gate.ps1 | PASS |
| Byte-Safe PNG Ekran Görüntüsü | a14-real-app-text.png (113,001 byte) | PASS |
| Bellek, Liveness, ANR/Crash Denetimi | PID 8961, no crash, no ANR | PASS |

## Yetkili Test ve Çalıştırma Kanıtları

### 1. Host Testleri (Release)
- `STAGE14_TEXT_FONT_TESTS_PASS`:
  - Turkish Character Encoding & CP1254 Test: PASS
  - AutoCAD Unicode Escape Sequences Test: PASS
  - AutoCAD Special Symbol Codes Test: PASS
  - Bounded MText Parser Basic Test: PASS
  - Bounded MText Parser Nesting & Depth Guard Test: PASS
  - Font Substitution Table Known SHX Test: PASS
  - Font Substitution Unknown Fallback Test: PASS
  - Text Alignment Calculations Test: PASS
  - Text Mirror Flags and Rotation Test: PASS
  - Text World Bounds Calculation Test: PASS
  - Skia Text Render Dark & Light Themes Test: PASS
  - Text Scene Semantic Snapshot Determinism Test: PASS
- `STAGE13_LAYER_STYLE_TESTS_PASS`
- `STAGE12_BLOCK_INSERT_TESTS_PASS`
- `STAGE11_VIEWPORT_GESTURE_TESTS_PASS`
- `STAGE10_GEOMETRY_PRIMITIVES_TESTS_PASS`
- `STAGE10_TESSELLATION_PRECISION_TESTS_PASS`
- `STAGE10_P0_SEMANTIC_GOLDEN_PASS`
- `STAGE04_RENDER_CONTRACT_TESTS_PASS`
- `STAGE09_RENDER_SCENE_TESTS_PASS`
- `STAGE04_ARCHITECTURE_TESTS_PASS`
- `STAGE05_DEPENDENCY_BOUNDARY_PASS`
- `V04_REAL_ANDROID_APP_PROJECT_PASS`

### 2. Android API 36 Emülatör Metin & Font Kabulü
- Cihaz: `sdk_gphone64_x86_64` / Android 16 (API 36) / x86_64 (Seri: `emulator-5554`)
- Paket: `com.smitelagwar.mobildwg`
- Başlatıcı Aktivite: `com.smitelagwar.mobildwg/crc64d52a5cdc4f267319.MainActivity`
- Canlı PID: `8961`
- Release APK Boyutu: `39,621,552` byte
- Release APK SHA-256: `2f853f638daba940b45f1685b92cb92750a52f09e16d0017e312c0430fe33b90`
- Ekran Görüntüsü Boyutu: `113,001` byte
- Ekran Görüntüsü SHA-256: `ad06c5c032d89f0f6000188aa1f3f895da3af930208cc76623ac778d9b05c3f7`
- Logcat Belirteçleri:
  - `A14_ANDROID_TURKISH_UNICODE_PASS`
  - `A14_ANDROID_AUTOCAD_ESCAPES_PASS`
  - `A14_ANDROID_BOUNDED_MTEXT_PASS`
  - `A14_ANDROID_FONT_SUBSTITUTION_PASS`
  - `A14_ANDROID_ALIGNMENT_MIRROR_PASS`
  - `A14_ANDROID_SKIA_TEXT_PNG_PASS bytes=79522 nonBgPixels=14605`
  - `ANDROID_STAGE14_TEXT_FONT_PASS`
  - `A14_REAL_APP_UI_IMAGE_READY sha256=...`
  - `A14_REAL_APP_UI_STATUS_PASS`
  - `A14_REAL_APP_STABILITY_PASS pid=8961`
  - `CLAIM_LIMIT=A14_TEXT_FONT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY`

## Sınır ve İddia Kısıtı (Claim Limit)

```text
CLAIM_LIMIT=A14_TEXT_FONT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY
```

AŞAMA 14 metin kodlama çözümleme (CP1254/UTF-8), AutoCAD kaçış kodları, sınırlı bütçeli MTEXT ayrıştırma, telifli SHX dosyaları barındırmaksızın açık kaynaklı sistem fontlarına denetimli eşleme ve Skia metin çiziminin sentetik RenderScene üzerinde çalıştığını ve Android API 36 emülatör üzerinde MAUI arayüzünde doğru gösterimini kanıtlar. Bu aşama DWG/DXF parser nesnelerinin doğrudan RenderScene'e haritalanmasını (AŞAMA 15–16) veya fiziksel cihaz dokunmatik gecikme testlerini kapsamaz.

AŞAMA 15 (Dimension / Leader / Hatch) A14'ün main'e merge edilmesiyle açılmaya hazırdır.
