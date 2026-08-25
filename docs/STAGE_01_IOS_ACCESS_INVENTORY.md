# AŞAMA 01 — iOS Erişim Envanteri

> **Future option / aktif değil:** Bu envanter Android-only v1'in AŞAMA 01, V01, beta veya release kapısı değildir. Kullanıcı iOS'u açıkça yeniden etkinleştirirse gerçek değerlerle güncellenir; o zamana kadar komut çalıştırılmaz.

Bu kayıt AŞAMA 01 için yalnız erişim durumunu belgeler. iOS toolchain kurulumu, build, signing veya gerçek cihaz smoke bu aşamada yapılmaz; bunlar AŞAMA 08 ve AŞAMA 23 kapsamındadır.

## Gizlilik kuralı

Bu dosyaya veya sohbet/loglara Apple ID/e-posta, parola, 2FA kodu, Team ID, cihaz UDID/seri numarası, provisioning profile, sertifika private key'i, API key/token veya signing secret yazılmaz.

## Envanter

Durum: `PENDING_USER_EVIDENCE`

- Kayıt tarihi: `UNKNOWN`
- Kanıt kaynağı: `UNKNOWN` (`scripts/stage01-ios-inventory.sh` veya manuel teyit)
- Mac erişimi: `UNKNOWN` (`YES` / `NO`)
- Xcode erişimi: `UNKNOWN` (`YES` / `NO` / `N/A`)
- Xcode sürümü: `UNKNOWN` (Xcode erişimi `YES` ise)
- Fiziksel iPhone erişimi: `UNKNOWN` (`YES` / `NO`)
- Apple Developer hesap/portal erişimi: `UNKNOWN` (`YES` / `NO`)
- Yerel code-signing identity sayısı: `UNKNOWN` (yalnız sayı; kimlik adı yazılmaz)
- Not: `UNKNOWN`

## Mac üzerinde yardımcı script

Erişilebilir bir Mac varsa repo kökünde çalıştır:

```bash
APPLE_DEVELOPER_ACCESS=yes bash scripts/stage01-ios-inventory.sh
```

Apple Developer erişimi yoksa:

```bash
APPLE_DEVELOPER_ACCESS=no bash scripts/stage01-ios-inventory.sh
```

Erişim henüz kontrol edilmediyse `APPLE_DEVELOPER_ACCESS` verilmeden de çalıştırılabilir; script `inventory_complete=NO` döndürür.

Script:

- macOS host bilgisini ve mimariyi kaydeder,
- Xcode erişimini ve sürümünü ölçer,
- Xcode cihaz listesindeki fiziksel iPhone sayısını yalnız sayı olarak kaydeder,
- yerel code-signing identity sayısını yalnız sayı olarak kaydeder,
- Apple Developer erişimini yalnız kullanıcının `yes/no` manuel teyidiyle kaydeder,
- cihaz UDID'si, Apple hesabı veya signing secret yazmaz.

## Mac yoksa

Erişilebilir Mac yoksa bu envanter yine tamamlanabilir. Yukarıdaki alanlar gerçek duruma göre örneğin `Mac erişimi: NO`, `Xcode erişimi: N/A`, `Fiziksel iPhone erişimi: NO`, `Apple Developer erişimi: YES/NO` şeklinde kaydedilir. Bu durum AŞAMA 08/23 için dış blocker/risk oluşturabilir fakat burada varmış gibi gösterilmez.

## Tamamlama kuralı

Bu envanter ancak şu dört erişim alanının tamamı gerçek bilgiyle `YES/NO` (uygunsa `N/A`) olduğunda `DONE` sayılır:

1. Mac erişimi.
2. Xcode erişimi.
3. Fiziksel iPhone erişimi.
4. Apple Developer hesap/portal erişimi.

`UNKNOWN` kalan alan varsa AŞAMA 01'in bu alt maddesi tamamlanmış sayılmaz.
