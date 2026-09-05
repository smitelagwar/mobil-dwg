# mobil-dwg

Android için tamamen local/offline çalışan, kullanıcıya ücretsiz sunulması hedeflenen 2D DWG/DXF görüntüleyici projesi. iOS aktif v1 kapsamından çıkarılmıştır; shared mimari ileride yeniden etkinleştirilebilecek şekilde korunur.

Implementation AŞAMA 17'ye kadar tamamlandı. Android geriye dönük doğrulama programı V01–V09 kapalıdır ve tüm aşamalar kendi claim sınırları içinde `VALIDATED` durumundadır. AŞAMA 17 XREF / Raster Image / Underlay / Dış Referans Uyumluluk motoru PR #30 ile `main`e merge edilmiş ve API 36 emülatör kabul testiyle doğrulanmıştır. Implementation cursor AŞAMA 18'dedir (Tam Android viewer UX / lifecycle).

V01 yalnız infrastructure smoke'u doğruladı. V04 gerçek `MobilDwg.App` APK build/install/cold-launch/UI/stability gate'ini geçti; V05 production ACadSharp parser'ı gerçek Android process içinde doğruladı. V06 gerçek FilePicker/DocumentsUI/SAF → stream → app-private safe-copy → production parser akışını API36 emulator üzerinde doğruladı. V07 exact unpatched ProCad candidate için `NO-GO` kararını ve production graph/precision izolasyonunu yeniden doğruladı. V08 tarihsel iOS kapsamını yeniden açmadan Android production/CI graph'ının iOS-specific TFM/RID/native/toolchain zorunluluğundan izole olduğunu kanıtladı. V09 ise RenderScene/camera/OCS/diagnostics temelini, deterministic `render-scene/v1` snapshot'ını, survey-origin `0.001` double precision'ı, Core/architecture sınırlarını ve gerçek Android app Release composition build'ini current exact revision üzerinde yeniden doğruladı. AŞAMA 10 platform-neutral P0 geometri primitiflerini, deterministik tessellation'ı, SkiaCadRenderer'ı ve API 36 Android emülatör üzerinde beklenen içerik piksel kabulünü (`56,163` piksel, byte-safe PNG) doğruladı. AŞAMA 11 mobil viewport yönetimini, odak noktası korumalı pinch zoom'u, pan jestini, double-tap ve fit-extents davranışını, ve CAD reparse olmaksızın oryantasyon boyut değişimini API 36 Android emülatör kabul testiyle doğruladı. AŞAMA 12 blok tanımlarını, INSERT referanslarını, 2D afin dönüşüm matrislerini (`Transform2D`), non-uniform scale/mirror ilkel dönüşümlerini, Layer 0/ByBlock mirasını, ATTRIB niteliklerini ve döngü/derinlik/bütçe koruma muhafızlarını API 36 Android emülatör kabul testiyle doğruladı. AŞAMA 13 katman durum yönetimini (`LayerTable`), ACI 1–255 ve TrueColor renk çözümlemesini (`CadColor`), standart kesikli çizgi desenlerini ve karmaşık çizgi tipi denetimli geri çekilmesini (`CadLinetype`), milimetrik çizgi kalınlığı dönüşümünü (`CadLineweight`), merkezi stil çözümleyiciyi (`CadStyleResolver`) ve SkiaSharp render entegrasyonunu API 36 Android emülatör kabul testiyle doğruladı. AŞAMA 14 Windows-1254 (CP1254) ve UTF-8 Türkçe karakter çözümlemesini (`CadTextEncoding`), AutoCAD Unicode (`\U+XXXX`) ve özel simge (`%%d`, `%%p`, `%%c`, `%%%`) kaçışlarını, ReDoS ve aşırı derinlik muhafızına sahip sınırlı MTEXT ayrıştırıcısını (`MTextParser`), telifli AutoCAD SHX dosyaları paketlenmeksizin açık kaynak sistem fontlarına denetimli eşleme tablosunu (`FontSubstitutionResolver`), metin hizalama/rotasyon/aynalama/sınır kutusu modelini (`TextPrimitive`, `CadTextAlignment`) ve SkiaSharp metin çizim entegrasyonunu API 36 Android emülatör kabul testiyle doğruladı. AŞAMA 15 AutoCAD anonim blok ilkliği kuralını (`*D...`), usulsel ölçülendirme geometrisini (Aligned, Rotated Linear, Radial, Diametric), dejenere ölçü korumalarını (`DEGENERATE_DIMENSION_POINTS`, `INVALID_DIMENSION_GEOMETRY`), lider/multileader geometrisini (`LeaderBuilder`), tarama sınır döngüsü otomatik kapanma toleransını (≤ 1 mm) ve kırık sınır teşhisini (`HATCH_BROKEN_BOUNDARY`), EvenOdd ada doldurma mantığını, ANSI31 kırpılmış desen çizgisi üretimini ve SkiaSharp render çıktısını API 36 Android emülatör kabul testiyle doğruladı. AŞAMA 16 Model ve Paper-Space ayrımını, pafta çerçevesi ve başlık bloğunu (`CadLayoutDefinition`), çoklu görünüm pencerelerini (`CadLayoutViewport`), Model -> Kağıt Alanı matris dönüşümünü, viewport bazlı katman dondurma geçersiz kılmalarını (`FrozenLayers`), Skia kırpma sınırlarını (`ClipRect` / `ClipPath`), dejenere viewport korumalarını (`INVALID_VIEWPORT_GEOMETRY`), sıfır-reparse bellek içi pafta geçişini (`CadLayoutManager`), ve deterministik sahne envanterini (`LayoutSceneSemanticSnapshot`) API 36 Android emülatör kabul testiyle doğruladı. AŞAMA 17 ise DWG XREF, Raster Görseller (PNG, JPG, BMP) ve PDF/DWF/DGN altlıklarını, uzak URL otomatik indirme engelini (`XREF_REMOTE_NOT_SUPPORTED`), dizin dışına çıkış güvenlik engelini (`PATH_TRAVERSAL_PREVENTED`), büyük/küçük harf duyarsız yerel dosya eşlemesini (`CadReferenceResolver`), eksik dış referanslar için görsel yer tutucuları (`MissingReferencePrimitive`), SkiaSharp raster görsel render motorunu (`RasterImagePrimitive`, `ClipBoundary`, `Fade`), ve deterministik referans envanterini (`ExternalReferenceSemanticSnapshot` schema `xref-compat/v1`) API 36 Android emülatör kabul testiyle doğruladı.

A17 claim'i `A17_XREF_COMPAT_API36_ONLY_NOT_CAD_PARSE_TO_SCENE_OR_PHYSICAL_DEVICE_FIDELITY` ile sınırlıdır. Tarihsel iOS AŞAMA 08 karakterizasyonu future option olarak arşivde kalır; iOS PASS değildir.

## Yeni sohbet / yeni AI başlangıcı

Normal proje devamı için [BASLA.md](BASLA.md) kullanılır. Bu dosya gerçek GitHub durumunu okuyup açık validation varsa onu, validation programı kapalıysa sıradaki implementation aşamasını yürütür. AŞAMA 17 kapandığı için sıradaki normal implementation cursor AŞAMA 18'dir.

`BASLA_A10.md` yalnız A10'un özel/izole workstream protokolüne ihtiyaç duyulan ayrı çalışma bağlamlarında kullanılabilir. A10 durumu [docs/A10_WORKSTREAM.md](docs/A10_WORKSTREAM.md) üzerinden doğrulanır; hiçbir durumda Android kanıtı olmadan `main` merge veya `DONE` yapılmaz.

Ana kaynaklar [ANDROID_DOGRULAMA_PLANI.md](ANDROID_DOGRULAMA_PLANI.md), [gecmis.md](gecmis.md) ve [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) dosyalarıdır. [DEVAM.md](DEVAM.md) anlık handoff snapshot'ıdır. Sohbet/model hafızası süreklilik kaynağı değildir; repo kayıtları esas alınır.

## Yetkili plan

Uygulama [Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md](Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md) planına göre geliştirilir. V01–V09 kapanış sonuçları [ANDROID_DOGRULAMA_PLANI.md](ANDROID_DOGRULAMA_PLANI.md) ve `docs/evidence/android-validation/` altında korunur.

## Temel ürün ilkeleri

- Viewer-first; edit ve save v1 kapsamı dışında
- Aktif v1 Android-only; iOS future option ve adapter sınırları korunmuş
- DWG/DXF doğrudan cihazda ve offline işlenir
- Zorunlu bulut, hesap veya dosya başına servis ücreti yok
- Ücretli CAD SDK/API ve runtime royalty yok
- Dependency, native binary, font ve test fixture lisansları release öncesi denetlenir
- Desteklenmeyen entity, eksik font ve dış referanslar sessizce gizlenmez
- Original CAD immutable kalır; FilePicker/SAF içeriği immediate app-private safe-copy üzerinden işlenir

## Yürütme

Her `BASLA.md dosyasını oku` veya normal `devam` komutunda gerçek `main`, açık PR/CI ve checkpoint doğrulanır. AŞAMA 17 tamamlandığından sonraki normal çalışma AŞAMA 18'dir (Tam Android viewer UX / lifecycle). Runner çevrim dışıysa kanıtsız PASS yazılmaz ve aynı test işi çoğaltılmaz.

## Güvenlik ve özel dosyalar

Gerçek müşteri/kullanıcı DWG-DXF dosyaları, fontlar, imzalama anahtarları ve özel test corpus'u repoya eklenmez. Yalnız redistribution/provenance durumu kaydedilmiş public/synthetic fixture ve asset'ler açıkça onaylanmış yollar altında tutulabilir.

## Lisans

Uygulama kaynak kodunun dağıtım lisansı henüz seçilmemiştir. Üçüncü taraf bileşenler ve test kaynakları kendi lisans/provenance kayıtlarına tabidir.
