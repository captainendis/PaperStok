# PaperStok

Logo Tiger3 Enterprise MSSQL veritabanından ambar bazlı stok toplamlarını çeken, kurulum gerektirmeyen (portable) Windows masaüstü uygulaması.

Bir **PaperAxis** ürünüdür. · [paperaxis.com](https://paperaxis.com)

## Özellikler

- Logo Tiger3 Enterprise veritabanına doğrudan (MSSQL) bağlanır; firma/dönem numarasına göre stok toplamlarını okur. Her profil için stok kaynağı `LV_<firma>_<dönem>_STINVTOT` görünümü (önerilen, varsayılan) veya `LG_<firma>_<dönem>_STINVTOT` tablosu olarak Bağlantı Ayarları'ndan seçilebilir.
- Birden fazla bağlantı profili kaydedebilir (SQL Server veya Windows kimlik doğrulaması); parolalar diskte DPAPI ile şifreli tutulur.
- Birden fazla profili aynı anda işaretleyip "Stokları Çek"e basarak farklı sunucudaki/firmadaki Logo kurulumlarının stoklarını tek tabloda birlikte görüntüleme (her satırda hangi profilden geldiğini gösteren "Kaynak" sütunuyla).
- Bir profil, bağlandığı sunucudaki Logo tablolarını doğrudan değil, o sunucuda tanımlı bir SQL Server linked server üzerinden de okuyabilir (Bağlantı Ayarları'ndaki "Linked Server" alanları) — gerçek Logo veritabanı başka bir sunucudaysa ve buraya yalnızca linked server ile erişiliyorsa kullanışlıdır.
- Ambar ve stok kodu/adına göre filtreleme, arama; aktif/pasif stok kartlarını "Aktif + Pasif", "Yalnızca Aktif" veya "Yalnızca Pasif" olarak seçme.
- **Ambar Raporu**: birden fazla ambarı aynı raporda görüntüleme, seçtiğiniz ambarları tek bir sütun altında birleştirme (ör. "Geçici Depo" stoğunu "Merkez"e ekleme), ürünleri arayıp seçerek filtreleme; rapor hem her ürün için satır toplamını hem her ambar sütunu için toplamı gösterir. Raporu adlandırıp kaydedip daha sonra tekrar çalıştırabilirsiniz.
- Tablolarda (ana ekran ve Ambar Raporu) sütunları sağ tıklayarak gösterme/gizleme; "Sütunları Sığdır" butonuyla sütun genişliklerini içeriğe göre bir kerelik ayarlama (sonrasında elle de yeniden boyutlandırılabilir).
- Excel (.xlsx) ve CSV (;) olarak dışa aktarma.
- Kurulum gerektirmez: tek `.exe` dosyası olarak dağıtılır, kayıt defterine yazmaz; ayarlar exe ile aynı klasördeki `profiles.json` ve `reports.json` dosyalarında tutulur.

## Salt okunur garantisi

PaperStok, Logo Tiger3 veritabanında **kesinlikle hiçbir değişiklik yapmaz** — yalnızca okur. Bu, tek bir katmana değil üç bağımsız katmana dayanır:

1. **Kod düzeyi:** Uygulama boyunca veritabanına yalnızca `SELECT` çalıştırılır; `INSERT`/`UPDATE`/`DELETE` gibi bir yazma çağrısı kod tabanında hiçbir yerde yoktur.
2. **Sorgu koruması (`SqlReadOnlyGuard`):** Çalıştırılacak her sorgu — ister varsayılan ambar sorgusu, ister Bağlantı Ayarları'ndan girilen özel SQL — çalıştırılmadan önce otomatik olarak denetlenir. Sorgu `SELECT`/`WITH` ile başlamıyorsa, birden fazla ifade içeriyorsa (`;` ile zincirleme) veya yazma/DDL anahtar kelimesi (`INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `EXEC`, `TRUNCATE`, `sp_`/`xp_` çağrıları vb.) barındırıyorsa, çalıştırılmadan reddedilir ve kullanıcıya Türkçe açık bir hata gösterilir.
3. **Bağlantı düzeyi:** Bağlantı dizesine `ApplicationIntent=ReadOnly` eklenir.

Bu üç katman uygulamanın kendisini korur; **asıl ve nihai güvence** ise Logo Tiger3 tarafında PaperStok'un kullandığı SQL Server girişine yalnızca okuma yetkisi (`db_datareader` rolü, `SELECT` dışında hiçbir yetki) verilmesidir. Metin tabanlı bir denetim teorik olarak bir string literal içindeki talihsiz bir kelimeyle (ör. `'... INTO ...'` geçen bir stok adı) yanlış pozitif üretebilir; asıl garantiyi veren veritabanı iznidir, uygulama katmanı ek bir emniyet kemeridir.

## Gereksinimler

- Çalıştırmak için: Windows 10/11 (x64). Ek bir .NET kurulumu gerekmez — dağıtım paketi self-contained'dır.
- Geliştirmek için: .NET 8 SDK (Windows Desktop iş yükü dahil — Windows'ta varsayılan .NET 8 SDK kurulumunda gelir).

Varsayılan ambar stok sorgusu (`PaperStok.Core/Logo/LogoQueryTemplates.cs`), dokunduğu her tablo ve kolonla (`L_CAPIWHOUSE.NR/NAME/FIRMNR`, `STINVTOT.STOCKREF/INVENNO/ONHAND/RESERVED/ACTPORDER`, `ITEMS.CODE/NAME/ACTIVE/UNITSETREF`, `UNITSETL.CODE/UNITSETREF/MAINUNIT`) birlikte **gerçek bir müşteri veritabanına karşı `INFORMATION_SCHEMA.COLUMNS` ile doğrulandı**. Bu süreçte iki yazılı kaynağın ([logoisortagim.com.tr](https://logoisortagim.com.tr/blog-logo-veritabani-tablolari.html) ve [ugurozpinar/Logo](https://github.com/ugurozpinar/Logo)) yanıldığı noktalar ortaya çıktı: ambar tablosu `L_CAPIDEF` değil `L_CAPIWHOUSE`'dur, birim adı `UNITSETF` değil `UNITSETL`'dendir. Ayrıca bu kurulumda `LG_<firma>_<dönem>_STINVTOT` tablosu hiç güncellenmiyordu (boştu); gerçek veri, aynı kolon yapısına sahip **`LV_<firma>_<dönem>_STINVTOT` görünümünden** geliyordu — sorgu varsayılan olarak bunu kullanır. Toplam güncelleme işi çalışan kurulumlarda tablo tercih edilebilir; bu yüzden LG_/LV_ seçimi sabit değil, her profil için Bağlantı Ayarları'ndaki "Stok Kaynağı" seçeneğinden ayarlanır. Müşteriye özel Logo kurulumlarında alan adları da farklılaşabilir; böyle bir durumda Bağlantı Ayarları ekranındaki "Gelişmiş: Özel Ambar Sorgusu" alanından ortamınıza göre uyarlayın ({STINVTOT_PREFIX} yer tutucusu Stok Kaynağı seçimine göre otomatik LG/LV olur).

## Kurulum

Son kullanıcı için: [Sürümler](../../releases) altındaki `PaperStok.exe` dosyasını indirip doğrudan çalıştırın — kurulum gerekmez.

Geliştirici için:

```bash
dotnet restore
dotnet build
```

## Portable derleme (tek exe)

```bash
./build/publish-win-x64.sh
# çıktı: dist/PaperStok.exe
```

Windows üzerinde PowerShell ile:

```powershell
dotnet publish src/PaperStok.App/PaperStok.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist
```

## Yapılandırma

| Dosya | Açıklama | Konum |
|---|---|---|
| `profiles.json` | Kayıtlı bağlantı profilleri (parolalar DPAPI ile şifreli) | exe ile aynı klasör |

## Geliştirme

```bash
dotnet test
```

Dal düzeni: `main` + `feature/<konu>`, birleştirme PR ile.
Kod ve commit mesajları İngilizce, dokümanlar Türkçe.

Proje yapısı:

```
src/PaperStok.Core/   iş mantığı: Logo sorguları, dışa aktarma, profil saklama (net8.0, platform bağımsız)
src/PaperStok.App/    WPF arayüzü (net8.0-windows, yalnızca Windows'ta derlenir)
tests/                PaperStok.Core için birim testleri
build/                portable derleme betiği
```

## Sürüm

Güncel sürüm: **v0.1.0** — değişiklikler için [CHANGELOG.md](CHANGELOG.md).

## İletişim

info@paperaxis.com

## Lisans

Kapalı kaynak. © 2026 PaperAxis. Tüm hakları saklıdır. Ayrıntılar için [LICENSE](LICENSE).
