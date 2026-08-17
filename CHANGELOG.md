# Değişiklik Günlüğü

Bu dosyanın biçimi [Keep a Changelog](https://keepachangelog.com/tr/1.1.0/) düzenini izler.
Sürüm numaralandırma: **vX.Y.Z** — X büyük, Y orta ölçekli, Z küçük güncellemeler.
Başlıklarda sürüm öneksiz yazılır; `v` öneki künye ve git etiketlerinde kullanılır.

## [Yayınlanmamış]

### Eklendi
- Ambar Raporu ekranı: birden fazla ambarı tek seferde görüntüleme, seçtiğiniz ambarları tek bir sütun altında birleştirme (ör. Geçici Depo'yu Merkez'e ekleme), ürünleri arayıp seçerek filtreleme, satır bazında (ürün) ve sütun bazında (ambar) toplamlar, Excel/CSV'ye aktarma.
- Raporları adlandırıp kaydetme ve daha sonra tekrar çalıştırma (`reports.json`, `profiles.json` ile aynı portable saklama düzeni).
- "Durum" filtresi: hem ana ekranda hem Ambar Raporu'nda "Aktif + Pasif", "Yalnızca Aktif" veya "Yalnızca Pasif" seçilebiliyor — hangi stok kartlarının listeleneceğine artık kullanıcı karar veriyor. Ürün listesine ve Excel/CSV çıktısına "Durum" (Aktif/Pasif) sütunu eklendi.
### Değiştirildi
- "Ambar No" sütunu ürün listesinden ve Excel/CSV çıktısından kaldırıldı; artık sadece "Ambar Adı" gösteriliyor (ambar numarası birleştirme/filtreleme mantığında dahili olarak kullanılmaya devam ediyor).
- Varsayılan ambar stok sorgusu logoisortagim.com.tr'nin Logo veritabanı tabloları rehberine göre çapraz kontrol edildi: ambar tablosu `L_CAPIWHOUSE`'dan `L_CAPIDEF`'e düzeltildi, `ITEMS.ACTIVE = 0` (yalnızca aktif stoklar) filtresi eklendi.
- Sorgu, github.com/ugurozpinar/Logo deposundaki alan bazlı Logo şema dökümüne göre ikinci kez çapraz kontrol edildi ve düzeltildi: birim çözümlemesi `UNITSETF.UINFO` (var olmayan bir kolon) yerine `UNITSETL.MAINUNIT`'e taşındı; `STINVTOT.ORDERED` (var olmayan bir kolon) yerine `ACTPORDER` kullanıldı; `STINVTOT.INVENNO <> -1` filtresi eklendi ("tüm ambarlar" özet satırını dışlamak için).
### Düzeltildi
- Bir profilin Bağlantı Ayarları'ndaki "Gelişmiş: Özel Ambar Sorgusu" kutusu doluyken bunun fark edilmemesi (kaydedilmiş bir tanı/deneme sorgusu sessizce varsayılan sorgunun yerine geçmeye devam ediyordu) giderildi: kutu doluyken hem Bağlantı Ayarları'nda hem ana ekranda hem Ambar Raporu'nda görünür bir uyarı gösteriliyor, "Sıfırla (varsayılana dön)" butonu eklendi.
- "Yeni Bağlantı" penceresi açılır açılmaz çöken hata giderildi: XAML'de öndeğer seçili `SqlAuthRadio`, henüz oluşturulmamış `UsernameBox`/`PasswordBox` alanlarına erişen bir olay işleyicisini pencere daha kurulmadan (InitializeComponent sırasında) tetikliyordu. Ayrıca beklenmeyen bir hata uygulamayı sessizce çökertmek yerine artık bir hata penceresi gösteriyor.
- "Specified cast is not valid" hatası giderildi: sonuç satırları `SqlDataReader.GetDecimal()`/`GetInt32()` gibi katı tipli okuyucularla değil, esnek `Convert.ToXxx(...)` dönüşümüyle okunuyor artık. Logo'nun `ONHAND`/`RESERVED` gibi alanları belgelenmiş tipi "Double" (SQL `float`) — kesin `decimal` bekleyen eski kod bu yüzden gerçek bir Logo veritabanında da patlardı.
- "Invalid object name 'L_CAPIDEF'" hatası giderildi: gerçek bir müşteri veritabanına karşı çalıştırılan salt okunur bir keşif sorgusu, ambar tablosunun `L_CAPIDEF` değil `L_CAPIWHOUSE` olduğunu kesin olarak gösterdi (iki yazılı kaynağın iddiasının aksine). Sorgu buna göre düzeltildi.
- Varsayılan sorgunun tüm tablo/kolonları (`L_CAPIWHOUSE`, `STINVTOT`, `ITEMS`, `UNITSETL`) aynı gerçek veritabanına karşı `INFORMATION_SCHEMA.COLUMNS` ile tek tek doğrulandı; hiçbir düzeltme gerekmedi.
- Ambarlar boş/sıfır geliyordu: bu kurulumda `LG_<firma>_<dönem>_STINVTOT` tablosu hiç güncellenmiyormuş (toplam güncelleme işi çalışmıyor). Aynı kolon yapısına sahip `LV_<firma>_<dönem>_STINVTOT` görünümü gerçek zamanlı veriyi tutuyor — varsayılan sorgu `LG_` yerine `LV_` kullanacak şekilde düzeltildi.
- Varsayılan sorgudaki `ITEMS.CARDTYPE = 1` (yalnızca "Ticari Mal") filtresi kaldırıldı; artık tüm stok kartı tipleri listeleniyor.
- Varsayılan sorgudaki sabit `ITEMS.ACTIVE = 0` (yalnızca aktif) filtresi kaldırıldı; sorgu artık her satır için `IsActive` bilgisini döndürüyor, aktif/pasif seçimi ekrandaki "Durum" filtresine taşındı.

## [0.1.0] - 2026-08-17

### Eklendi
- İlk sürüm: Logo Tiger3 Enterprise'dan ambar bazlı stok toplamlarını çeken portable WPF uygulaması.
- Birden fazla bağlantı profili, DPAPI ile şifreli parola saklama.
- Ambar/stok filtreleme ve arama, Excel (.xlsx) ve CSV dışa aktarma.
- PaperAxis marka teması ve Hakkında ekranı.
- Salt okunur garantisi: her sorgu (varsayılan ve özel) çalıştırılmadan önce `SqlReadOnlyGuard` ile denetlenir, bağlantı `ApplicationIntent=ReadOnly` ile açılır; uygulama Logo Tiger3 veritabanında hiçbir yazma işlemi yapmaz.
