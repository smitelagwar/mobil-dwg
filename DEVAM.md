# mobil-dwg — Yeni Sohbet İçin Tek Dosyalık Handoff

Bu dosya, yeni bir ChatGPT/AI oturumunda projeye kaldığı yerden devam etmek için tek giriş noktasıdır. Repo kayıtları sohbet/model belleğinden üstündür.

## Yeni AI için doğrudan talimat

1. `@GitHub` üzerinden `smitelagwar/mobil-dwg` reposunu ve gerçek `main` HEAD'i doğrula.
2. `Mobil_DWG_DXF_Royalty_Free_Android_iOS_Nihai_Plan.md`, `gecmis.md` ve `docs/USER_APPROVED_EXECUTION_OVERRIDE.md` dosyalarını oku.
3. **Çalışma bağlamını gerçek araç erişimine göre sınıflandır.** Kod/depo değişiklikleri ChatGPT sohbetinden GitHub üzerinden yapılıyor ve yerel repo/terminal/ADB'ye doğrudan erişim yoksa `CHATGPT_REMOTE_GITHUB` bağlamıdır; bu durumda `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md` dosyasını okumak zorunludur. Dosyanın okunması zorunlu olsa da içindeki batching/test sıklığı/25 dakikalık zaman yönetimi önerileri zorunlu değildir. AntiGravity, Visual Studio + Codex, Codex IDE veya başka bir yerel ajan gerçek yerel çalışma ağacı + terminal/ADB erişimiyle çalışıyorsa `LOCAL_IDE` bağlamıdır ve bu remote test modeli yürütme için geçersizdir.
4. AŞAMA 08 için `docs/evidence/STAGE_08.md` ve `docs/LOCAL_DEVICE_REVALIDATION.md`; AŞAMA 07 için `docs/evidence/STAGE_07.md` ve `docs/ADR/0002-procad-pinned-source-no-go.md` dosyalarını oku.
5. Kullanıcı yalnız `devam` diyorsa `NEXT_WORK_STAGE` üzerinden ilerle. Aktif aşama `IN_PROGRESS` ise aynı aşamadan devam et; bitmeden sonraki aşamaya geçme.
6. Bir kullanıcı turunda en fazla bir aşama tamamla; aynı turda sonraki aşamayı başlatma. Bir aşama tek turda bitmek zorunda değildir; kullanıcı `devam` dedikçe aynı aşama gerçek çıkış kriterleri sağlanana kadar sürdürülebilir.
7. Fiziksel cihaz/Mac/Apple hesabı gibi kullanıcının sağlayamadığı dış kapıları sahte PASS/DONE yapma; `DEFERRED_EXTERNAL_GATE` bırak.
8. Her aşama sonunda canonical checkpoint, `gecmis.md` ve `docs/evidence/STAGE_XX.md` kaydını gerçek CI/commit/artifact kanıtıyla güncelle.
9. Production dependency'yi evidence olmadan yükseltme veya ProCad'ı tekrar graph'a sokma.

## Repo / ürün

- Repo: `smitelagwar/mobil-dwg` (private), default `main`.
- Android-first, iOS zorunlu ikinci platform, local/offline 2D DWG/DXF viewer.
- v1 viewer-only; edit/save/export/cloud/account yok.

## Çalışma bağlamı notu

`CHATGPT_REMOTE_GITHUB` bağlamında mevcut Android test altyapısı her küçük GitHub değişikliğinde çalıştırılmak için tasarlanmamıştır. Ajan isterse aynı mantıksal işte birkaç dosyayı/değişikliği tamamlayıp sonra `android-test` hattını bir kez tetikleyebilir. Örneğin üç dosya değişikliğinden sonra tek emulator testi yapmak çoğu durumda daha verimli olabilir. Bunun tersi de mümkündür: riskli tek bir değişiklikten sonra hemen test edilebilir.

Bu batching davranışı **öneridir, zorunlu değildir**. Özellikle ChatGPT High çalışma penceresinde GitHub → self-hosted PC → emulator → artifact round-trip süresini gereksiz yere çoğaltmamak için ajan bunu teknik muhakemesinde hesaba katmalıdır.

Aynı şekilde bir aşamanın ilk turunda implementasyonun tamamı veya bir kısmı yapılabilir; sonraki `devam` turunda kalan değişiklikler, Android testleri veya evidence kapanışı yapılabilir. Ajan farklı bir sıra seçebilir. Değişmez olan kural: aktif aşama bitmeden sonraki aşamaya geçilmez ve gerçek kanıt olmadan `DONE` yazılmaz.

Ayrıntılı ve güncel remote test modeli: `docs/CHATGPT_REMOTE_ANDROID_TEST_WORKFLOW.md`.

## Güncel checkpoint

```text
LAST_COMPLETED_STAGE: AŞAMA 08 — CHARACTERIZATION / RISK_ACCEPTED_FOR_CONTINUATION; iOS PASS NOT CLAIMED
DEFERRED_STAGES: AŞAMA 01; AŞAMA 06; AŞAMA 08 local Mac/ios-arm64/physical iPhone gates
AŞAMA_01: BLOCKED / DEFERRED_EXTERNAL_GATE — gerçek Android install/launch + iOS erişim envanteri
AŞAMA_06: BLOCKED / DEFERRED_EXTERNAL_GATE — safe-open CI PASS; gerçek telefon FilePicker/SAF+lifecycle/cache gate açık
AŞAMA_07: DONE / NO-GO — exact unpatched ProCad candidate systematic precision blocker nedeniyle production reuse için reddedildi
AŞAMA_08: DONE / CHARACTERIZATION — evidence BLOCKED_PARTIAL_EVIDENCE; iOS runtime/device PASS yok
STAGE08_CI: run 32781026946 / #18 SUCCESS characterization; artifact 9540018558; sha256:1414e3bf5a9800e150019c48f620c64efcd3d5282ac7322ef9a5e5746ab746f7
STAGE08_HOST_BLOCKER: Xcode 26.6 hosted runner install_name_tool/clang lookup
STAGE08_TRIM_RISK: ACadSharp ILLink/reflection warnings
STAGE08_NATIVEAOT: iossimulator-arm64 NETSDK1203; ios-arm64 future real-device gate
STAGE08_PHYSICAL_IPHONE: NOT_RUN_DEFERRED_EXTERNAL_GATE
LOCAL_REVALIDATION: docs/LOCAL_DEVICE_REVALIDATION.md
NEXT_WORK_STAGE: AŞAMA 09
NEXT_WORK_STATUS: WAITING_EXPLICIT_USER_GO
STAGE09_GO_BARRIER: custom renderer effort/maintenance risk HIGH; AŞAMA 09 implementation öncesinde kullanıcı açık GO gerekir
NEXT_ACTION: generic `devam` ile AŞAMA 09 başlatma; `AŞAMA 09 GO` gibi explicit karar bekle.
```

## Tamamlanan / açık aşamalar

- AŞAMA 00 — DONE
- AŞAMA 01 — BLOCKED / DEFERRED_EXTERNAL_GATE
- AŞAMA 02 — DONE
- AŞAMA 03 — DONE
- AŞAMA 04 — DONE
- AŞAMA 05 — DONE; ACadSharp 3.7.1 read-only parser baseline GO
- AŞAMA 06 — BLOCKED / DEFERRED_EXTERNAL_GATE; cihazdan bağımsız safe-open/Android build CI PASS
- AŞAMA 07 — DONE / NO-GO; ProCad production reuse rejected
- AŞAMA 08 — DONE / CHARACTERIZATION; iOS PASS NOT CLAIMED

## AŞAMA 07 özeti

Exact candidate:

- ProCad `f8a862b3e7634e27664fee02ff5d68774b102985`
- ACadSharp submodule `0ed79df48de0806af3c3028d0e2826447cbc1d36`
- ProEdit `64759b79289a024d08463ed1a9094fdcd9a270df`

Lineage official upstream'de çözüldü ancak approved ACadSharp 3.7.1 source baseline 592 commit ileride. Pinned source Android build başarılı (`82 warning / 0 error`); clean MAUI Release smoke başarılı (`0 warning / 0 error`). Published ProCadSharp 0.1.1 restore graph ACadSharp 1.0.0 ve Skia 4.147.0-preview.2.1 çözüyor; source graph ile eşdeğer değil.

Hard blocker: ProCad scene boundary'sinde CAD world point doğrudan float Vector2'ye daralıyor. Origin 100 + 1 mm detay korunurken origin 5,000,000 + 1 mm detay float'ta aynı değere düşüyor; observed delta 0.0. Bu systematic P0 fidelity loss. Exact unpatched candidate `NO-GO`; production graph'a eklenmez.

Özel renderer garantili fallback değildir. ADR 0002, AŞAMA 09–16 renderer/fidelity kapsamını ve sonraki performance/full-corpus gate'lerini HIGH effort/maintenance risk olarak kaydeder. AŞAMA 09 implementation'dan önce kullanıcı GO gerekir.

## AŞAMA 08 özeti

Exact ACadSharp 3.7.1 + SkiaSharp 4.151.1 iOS hattı GitHub-hosted macOS üzerinde karakterize edildi. Run `32781026946`/#17 characterization SUCCESS; bu iOS PASS değildir. Hosted Xcode 26.6 `install_name_tool`/`clang` lookup final baseline/simulator runtime'ı engelledi. ACadSharp trimming/reflection ILLink riskleri görünür bırakıldı. `iossimulator-arm64` NativeAOT `NETSDK1203` ile desteklenmedi; gerçek AOT `ios-arm64`/physical iPhone'da tekrar gerekir. Fiziksel iPhone ve local Mac kapıları deferred. Ayrıntı `docs/evidence/STAGE_08.md`; gelecekteki ikinci-pass kontrol listesi `docs/LOCAL_DEVICE_REVALIDATION.md`. AŞAMA 09 custom renderer implementation ancak explicit kullanıcı GO ile başlayabilir.

## Değiştirilemez ilkeler

- Original CAD immutable; overwrite yok.
- Unsupported/proxy/font/XREF/raster sessiz kayıp olarak gizlenmez.
- UI parser entity'lerine doğrudan bağlanmaz.
- Runtime license allowlist varsayılanı MIT/Apache/BSD/ISC/0BSD; policy-RED/unknown release blocker.
- Gerçek cihaz kanıtı yoksa cihaz PASS yazılmaz.
- Bir turda en fazla bir aşama tamamlanır.
