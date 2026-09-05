# Historical Evidence Archive

Bu klasör tamamlanmış v1 geliştirme ve Android doğrulama çalışmalarının **tarihsel kanıt arşividir**. Aktif iş listesi veya yürütme planı değildir.

Kurallar:

- `STAGE_XX.md` ve `android-validation/VXX.md` dosyaları geçmiş exact commit/run/artifact sonuçlarını korur.
- İçlerinde artık çalışma ağacında bulunmayan eski `BASLA`, `DEVAM`, validation planı veya stage planı referansları olabilir. Bunlar tarihsel bağlamdır; ilgili dosyalar gerektiğinde Git geçmişinden okunur.
- Yeni geliştirme için eski stage/cursor devam ettirilmez.
- Yeni bir regression geçmiş kanıtı geriye dönük değiştirmez; yeni commit + yeni evidence üretilir.
- Güncel başlangıç noktaları `README.md`, `docs/ARCHITECTURE.md`, `docs/ANDROID_TESTING.md`, `docs/GOLDEN_CONTRACT.md` ve `compliance/` belgeleridir.

Bu klasörün tutulma amacı geçmiş claim'lerin neye dayandığını kaybetmemektir; normal yeni işte tüm dosyaların topluca okunması gerekmez.
