# PaperStok

Logo Tiger3 Enterprise MSSQL veritabanından ambar bazlı stok toplamlarını çeken, kurulum gerektirmeyen (portable) Windows masaüstü uygulaması.

Bir **PaperAxis** ürünüdür. · [paperaxis.com](https://paperaxis.com)

## Özellikler

- Logo Tiger3 Enterprise veritabanına doğrudan (MSSQL) bağlanır; firma/dönem numarasına göre `LG_<firma>_<dönem>_STINVTOT` tablosundan ambar bazlı stok toplamlarını okur.
- Birden fazla bağlantı profili kaydedebilir (SQL Server veya Windows kimlik doğrulaması); parolalar diskte DPAPI ile şifreli tutulur.
- Ambar ve stok kodu/adına göre filtreleme, arama.
- Excel (.xlsx) ve CSV (;) olarak dışa aktarma.
- Kurulum gerektirmez: tek `.exe` dosyası olarak dağıtılır, kayıt defterine yazmaz; ayarlar exe ile aynı klasördeki `profiles.json` dosyasında tutulur.

## Salt okunur garantisi

PaperStok, Logo Tiger3 veritabanında **kesinlikle hiçbir değişiklik yapmaz** — yalnızca okur. Bu, tek bir katmana değil üç bağımsız katmana dayanır:

1. **Kod düzeyi:** Uygulama boyunca veritabanına yalnızca `SELECT` çalıştırılır; `INSERT`/`UPDATE`/`DELETE` gibi bir yazma çağrısı kod tabanında hiçbir yerde yoktur.
2. **Sorgu koruması (`SqlReadOnlyGuard`):** Çalıştırılacak her sorgu — ister varsayılan ambar sorgusu, ister Bağlantı Ayarları'ndan girilen özel SQL — çalıştırılmadan önce otomatik olarak denetlenir. Sorgu `SELECT`/`WITH` ile başlamıyorsa, birden fazla ifade içeriyorsa (`;` ile zincirleme) veya yazma/DDL anahtar kelimesi (`INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `EXEC`, `TRUNCATE`, `sp_`/`xp_` çağrıları vb.) barındırıyorsa, çalıştırılmadan reddedilir ve kullanıcıya Türkçe açık bir hata gösterilir.
3. **Bağlantı düzeyi:** Bağlantı dizesine `ApplicationIntent=ReadOnly` eklenir.

Bu üç katman uygulamanın kendisini korur; **asıl ve nihai güvence** ise Logo Tiger3 tarafında PaperStok'un kullandığı SQL Server girişine yalnızca okuma yetkisi (`db_datareader` rolü, `SELECT` dışında hiçbir yetki) verilmesidir. Metin tabanlı bir denetim teorik olarak bir string literal içindeki talihsiz bir kelimeyle (ör. `'... INTO ...'` geçen bir stok adı) yanlış pozitif üretebilir; asıl garantiyi veren veritabanı iznidir, uygulama katmanı ek bir emniyet kemeridir.

## Gereksinimler

- Çalıştırmak için: Windows 10/11 (x64). Ek bir .NET kurulumu gerekmez — dağıtım paketi self-contained'dır.
- Geliştirmek için: .NET 8 SDK (Windows Desktop iş yükü dahil — Windows'ta varsayılan .NET 8 SDK kurulumunda gelir).

> **TEYİT:** Varsayılan ambar stok sorgusu (`PaperStok.Core/Logo/LogoQueryTemplates.cs`) iki kaynağa göre çapraz kontrol edildi: [logoisortagim.com.tr'nin tablo rehberi](https://logoisortagim.com.tr/blog-logo-veritabani-tablolari.html) ve [ugurozpinar/Logo](https://github.com/ugurozpinar/Logo) deposundaki alan bazlı şema dökümü. Alan (kolon) düzeyinde doğrulanan kısımlar: `STINVTOT` (`STOCKREF`, `INVENNO` — `-1` "tüm ambarlar" anlamına gelir ve sorguda dışlanır —, `ONHAND`, `RESERVED`; `ORDERED` diye bir kolon yok, en yakın karşılık `ACTPORDER`/"Verilen Siparişler"dir), `ITEMS` (`CODE`, `NAME`, `CARDTYPE`, `ACTIVE`, `UNITSETREF`), birim çözümlemesi (`UNITSETF` birim setinin kendisidir, gerçek birim adı `UNITSETL.CODE`'dan `MAINUNIT` bayrağıyla gelir). Tablo adlandırma kalıbı (`LG_<firma>_<dönem>_TABLOADI`) ve `L_CAPIDEF`'in ambar tablosu olduğu (`L_CAPIWHOUSE` diye bir tablo hiçbir kaynakta geçmiyor) her iki kaynakta da doğrulandı; `L_CAPIDEF`'in kendi kolonları (`NR`/`NAME`/`FIRMNR`) hiçbir kaynakta doğrudan listelenmiyor ama aynı `L_CAPI*` ailesindeki kardeş tablolar (`L_CAPIFIRM`, `L_CAPIDEPT`, `L_CAPIUNIT`) gerçek, çalışan SQL örneklerinde bu üç kolonla kullanılmış durumda. Yine de müşteriye özel Logo kurulumlarında alan adları farklılaşabilir ve sorgu gerçek bir veritabanına karşı uçtan uca test edilmedi; Bağlantı Ayarları ekranındaki "Gelişmiş: Özel Ambar Sorgusu" alanından ortamınıza göre uyarlayın.

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
