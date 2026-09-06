# Gemini uygulama başlangıç promptu

6 Eylül 2026 tarihinde denetlenmiş nihai plan içindir. Aşağıdaki mesajı Gemini'ye ver.

```text
Bu projede yeni plan hazırlamanı değil, nihai planı uygulamanı istiyorum.

Çalışma dizini:
C:\Users\hsyn\Desktop\MOBIL_UYGULAMA_DWG

Bağlayıcı plan:
C:\Users\hsyn\Desktop\MOBIL_UYGULAMA_DWG\docs\MOBIL_DWG_NIHAI_UYGULAMA_PLANI.md

Planın 6 Eylül 2026 bütünlük denetimi yapılmış sürümünü baştan sona oku. Aşama 01'den başlayıp Aşama 14'e kadar sırayla uygula. Bütün aşamaların uygulanmasını istiyorum; rutin aşama geçişlerinde yeniden onay isteme. Başlangıçta yalnız anlatım veya yeni bir plan verip durma: kodu, testleri ve kanıtları üret.

1. Güncel yerel dosyalar esas alınacak. GitHub main veya eski planlar yerel kodun önüne geçmez. Repo kurallarını oku; HEAD, mevcut diff ve gerekli kaynak hash'lerini kaydet. Kullanıcının yerel değişikliklerini koru. Reset/clean, otomatik stash/drop veya üzerine yazma ile başlangıç durumunu kaybettirme.

2. Nihai plandaki mimari, paket, kamera, native input, scheduler, BVH, cache, CAD aktarımı ve kabul kararlarını uygula. Eski dört planı yürütme talimatı olarak kullanma; alternatif motor, ikinci input yolu veya kapsam dışı yeniden yazım ekleme.

3. Her aşamayı derlenebilir, bağlı ve çalışır hâlde bitir. İlgili testleri ve geçiş koşullarını gerçekten çalıştır. Başarısız teknik önkoşulu atlama; implementasyonu düzelt. Test silme, eşik gevşetme, boş metot/TODO bırakma veya başarı uydurma ile ilerleme.

4. Her aşama sonunda planın rapor şablonuyla değişiklikleri, çalıştırılan komutları ve exit code'larını, ölçümleri, kanıt konumlarını ve eksik kontrolleri kaydet. Kısa ilerleme raporu verip uygun sonraki aşamaya devam et. Durumu artifacts/viewer-stability/PROGRESS.md içinde güncel tut; kanıt koşularının üzerine yazma.

5. Bağlam/oturum kesilirse ilerleme kaydını gerçek kod ve test kanıtlarıyla karşılaştırıp kaldığın yerden devam et. Önceki bir rapordaki TAMAMLANDI ifadesini doğrulamadan kabul etme.

6. Çalışan Android emülatörünü kullanabilir, gerekli APK'ları derleyip kurabilir ve testleri çalıştırabilirsin. Test edilen APK'nın kaynak ve lock graph'ıyla eşleşmesini kaydet. Native dokunma altyapısını Aşama 05'te kur; Aşama 13'e erteleme. Yeni alanların parmak ekrandayken çizildiğini ve gerçek çoklu dokunmayı doğrula. Controller simülasyonu gerçek Android pinch kanıtı değildir.

7. Fiziksel cihaz veya dış erişim eksikse ilgili kontrolü DIŞ DOĞRULAMA BEKLİYOR olarak kaydet. Bundan bağımsız yapılabilecek işleri sürdür; eksik doğrulamayı başarılı gösterme veya bütün ürüne tam kabul verme. Emülatör ölçümünü fiziksel telefon performansı diye sunma.

8. Gerçek API/kod ile plan arasında somut uyuşmazlık varsa önce kanıtla. Planın mimari ve davranış sözleşmesini koruyan en dar düzeltmeyi yapıp gerekçesini kaydet; sessiz paket yükseltmesi veya ürün kararı değişikliği yapma.

9. Bu işe ait değişiklikleri seçerek aşama bazında yerel commit oluştur. Gerekli olduğu doğrulanan başlangıçtaki yerel kaynak değişikliklerini ayrı ve açıkça başlangıç değişiklikleri olarak belirtilen commit'e alabilirsin; bunları kendi düzeltmen gibi sunma. İlgisiz değişiklikleri toplama ve git add . kullanma. Final kaynak temiz checkout'tan da yeniden üretilebilmeli. Özel CAD dosyalarını, özel görüntüleri ve gizli verileri commit'e koyma. Push/merge/yayın işlemlerini mevcut açık kullanıcı talimatı kapsamında yap; bu uygulama mesajı otomatik yayın talimatı değildir.

10. Son raporda tamamlanan aşamalar, kalan gerçek sorunlar, test edilen normal Release APK'sı ve kanıtları yer alsın. “Kusursuz”, “tam DWG uyumu” veya “bütün testler geçti” sözlerini kanıtsız kullanma.

Şimdi planı ve güncel kaynakları oku, başlangıç durumunu kaydet ve Aşama 01'in uygulamasına başla. Aşamaları sırayla, test kapılarını koruyarak ilerlet.
```
