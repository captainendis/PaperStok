# Değişiklik Günlüğü

Bu dosyanın biçimi [Keep a Changelog](https://keepachangelog.com/tr/1.1.0/) düzenini izler.
Sürüm numaralandırma: **vX.Y.Z** — X büyük, Y orta ölçekli, Z küçük güncellemeler.
Başlıklarda sürüm öneksiz yazılır; `v` öneki künye ve git etiketlerinde kullanılır.

## [Yayınlanmamış]

### Eklendi
### Değiştirildi
- Varsayılan ambar stok sorgusu logoisortagim.com.tr'nin Logo veritabanı tabloları rehberine göre çapraz kontrol edildi: ambar tablosu `L_CAPIWHOUSE`'dan `L_CAPIDEF`'e düzeltildi, `ITEMS.ACTIVE = 0` (yalnızca aktif stoklar) filtresi eklendi.
- Sorgu, github.com/ugurozpinar/Logo deposundaki alan bazlı Logo şema dökümüne göre ikinci kez çapraz kontrol edildi ve düzeltildi: birim çözümlemesi `UNITSETF.UINFO` (var olmayan bir kolon) yerine `UNITSETL.MAINUNIT`'e taşındı; `STINVTOT.ORDERED` (var olmayan bir kolon) yerine `ACTPORDER` kullanıldı; `STINVTOT.INVENNO <> -1` filtresi eklendi ("tüm ambarlar" özet satırını dışlamak için).
### Düzeltildi
- "Yeni Bağlantı" penceresi açılır açılmaz çöken hata giderildi: XAML'de öndeğer seçili `SqlAuthRadio`, henüz oluşturulmamış `UsernameBox`/`PasswordBox` alanlarına erişen bir olay işleyicisini pencere daha kurulmadan (InitializeComponent sırasında) tetikliyordu. Ayrıca beklenmeyen bir hata uygulamayı sessizce çökertmek yerine artık bir hata penceresi gösteriyor.
- "Specified cast is not valid" hatası giderildi: sonuç satırları `SqlDataReader.GetDecimal()`/`GetInt32()` gibi katı tipli okuyucularla değil, esnek `Convert.ToXxx(...)` dönüşümüyle okunuyor artık. Logo'nun `ONHAND`/`RESERVED` gibi alanları belgelenmiş tipi "Double" (SQL `float`) — kesin `decimal` bekleyen eski kod bu yüzden gerçek bir Logo veritabanında da patlardı.
- "Invalid object name 'L_CAPIDEF'" hatası giderildi: gerçek bir müşteri veritabanına karşı çalıştırılan salt okunur bir keşif sorgusu, ambar tablosunun `L_CAPIDEF` değil `L_CAPIWHOUSE` olduğunu kesin olarak gösterdi (iki yazılı kaynağın iddiasının aksine). Sorgu buna göre düzeltildi.
- Varsayılan sorgunun tüm tablo/kolonları (`L_CAPIWHOUSE`, `STINVTOT`, `ITEMS`, `UNITSETL`) aynı gerçek veritabanına karşı `INFORMATION_SCHEMA.COLUMNS` ile tek tek doğrulandı; hiçbir düzeltme gerekmedi.

## [0.1.0] - 2026-08-17

### Eklendi
- İlk sürüm: Logo Tiger3 Enterprise'dan ambar bazlı stok toplamlarını çeken portable WPF uygulaması.
- Birden fazla bağlantı profili, DPAPI ile şifreli parola saklama.
- Ambar/stok filtreleme ve arama, Excel (.xlsx) ve CSV dışa aktarma.
- PaperAxis marka teması ve Hakkında ekranı.
- Salt okunur garantisi: her sorgu (varsayılan ve özel) çalıştırılmadan önce `SqlReadOnlyGuard` ile denetlenir, bağlantı `ApplicationIntent=ReadOnly` ile açılır; uygulama Logo Tiger3 veritabanında hiçbir yazma işlemi yapmaz.
