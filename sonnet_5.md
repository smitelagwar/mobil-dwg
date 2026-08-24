Bu MD dosyasını körü körüne onaylama. Aşağıdaki maddeleri tek tek doğrula ve MD'yi buna göre güncelle:

1. ProCad (wieslawsoltes/ProCad) reposunun gerçekten var olduğunu KANITLA: gerçek GitHub URL'si, gerçek commit geçmişi, gerçek LICENSE dosyası içeriği, yıldız/fork sayısı, son commit tarihi. Bulamıyorsan (ki büyük ihtimalle bulamayacaksın), bu kütüphaneye dayanan tüm bölümleri (özellikle "9. ProCad", "10. ProCad'e kör bağlanılmayacak", "85. En kritik yeni keşif", "86. Fallback kararı", "87-88. Nihai mimari") kaldır veya "doğrulanamadı" notuyla işaretle, ve planı ProCad OLMADAN, sadece ACadSharp + SkiaSharp + kendi RenderScene katmanımızla nasıl uygulanacağı üzerinden yeniden yaz. Renderer'ı sıfırdan yazmanın gerçekçi efor/süre tahminini plana ekle.

2. ACadSharp (DomCR/ACadSharp) için: reponun şu anki (bugünkü) README'sindeki "alpha" uyarısını ve implemente edilmemiş entity listesini (LEADER, WIPEOUT, MESH, ACAD_TABLE, ACAD_PROXY_OBJECT, POLYLINE_PFACE, DwgWriter durumu) plana ekle. Bunları mimari/statik mühendislik çizimlerinde (kolon aplikasyon planı, kalıp planı, donatı detayı, kesit) hangi entity'lerin kritik olduğuyla eşleştir ve her biri için somut bir fallback/geçici çözüm belirle (örn: LEADER'ı basit çizgi+ok olarak elle render etme).

3. SHX font meselesini plana bir bölüm olarak ekle: gerçek AutoCAD SHX font dosyalarının (txt.shx, romans.shx, isocp.shx vb.) Autodesk'e ait telifli kaynaklar olduğunu, bunların uygulamaya gömülemeyeceğini, bunun yerine permissive lisanslı SHX-uyumlu font üretimi/eşleştirmesi gerektiğini belirt.

4. IxMilia.Dxf / IxMilia.Dwg (MIT) kütüphanelerini ACadSharp'a alternatif/yedek olarak araştır, gerçek entity kapsamı ve güncel durumunu ACadSharp ile karşılaştıran bir tablo ekle.

5. Plana bir "bağımlılık risk kaydı" tablosu ekle: her bileşen için repo, lisans, pinlenen commit/sürüm, son commit tarihi, aktif geliştirici sayısı, olgunluk seviyesi (alpha/beta/stable), kütüphane terk edilirse fallback planı.

6. Performans hedeflerini somutlaştır: hangi entity sayısında kaç FPS hedefleniyor, hangi düşük/orta segment Android cihazlarda test edilecek, bellek tavanı ne.

7. Test corpus'unu somutlaştır: gerçek Türkiye mimari/statik projelerinden (kalıp planı, kolon aplikasyon planı, donatı detay paftası, mimari kat planı) LEADER, MLEADER, HATCH, dimension, block içeren örnek dosyalarla test edileceğini yaz.

Geri kalan ana kararları (ACadSharp+SkiaSharp+MAUI çekirdeği, GPL/AGPL/ticari SDK yasağı, doğrudan DWG okuma, preview-first, Android-first) DEĞİŞTİRME — bunlar doğru. Sadece yukarıdaki maddelerdeki varsayımları doğrula/düzelt ve MD'nin geri kalan yapısını (bölüm numaraları, Türkçe dil) koru.