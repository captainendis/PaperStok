# Değişiklik Günlüğü

Bu dosyanın biçimi [Keep a Changelog](https://keepachangelog.com/tr/1.1.0/) düzenini izler.
Sürüm numaralandırma: **vX.Y.Z** — X büyük, Y orta ölçekli, Z küçük güncellemeler.
Başlıklarda sürüm öneksiz yazılır; `v` öneki künye ve git etiketlerinde kullanılır.

## [Yayınlanmamış]

### Eklendi
### Değiştirildi
### Düzeltildi

## [0.1.0] - 2026-08-17

### Eklendi
- İlk sürüm: Logo Tiger3 Enterprise'dan ambar bazlı stok toplamlarını çeken portable WPF uygulaması.
- Birden fazla bağlantı profili, DPAPI ile şifreli parola saklama.
- Ambar/stok filtreleme ve arama, Excel (.xlsx) ve CSV dışa aktarma.
- PaperAxis marka teması ve Hakkında ekranı.
- Salt okunur garantisi: her sorgu (varsayılan ve özel) çalıştırılmadan önce `SqlReadOnlyGuard` ile denetlenir, bağlantı `ApplicationIntent=ReadOnly` ile açılır; uygulama Logo Tiger3 veritabanında hiçbir yazma işlemi yapmaz.
