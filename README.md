# PaperStok

Logo Tiger3 Enterprise MSSQL veritabanından ambar bazlı stok toplamlarını çeken, kurulum gerektirmeyen (portable) Windows masaüstü uygulaması.

Bir **PaperAxis** ürünüdür. · [paperaxis.com](https://paperaxis.com)

## Özellikler

- Logo Tiger3 Enterprise veritabanına doğrudan (MSSQL) bağlanır; firma/dönem numarasına göre `LG_<firma>_<dönem>_STINVTOT` tablosundan ambar bazlı stok toplamlarını okur.
- Birden fazla bağlantı profili kaydedebilir (SQL Server veya Windows kimlik doğrulaması); parolalar diskte DPAPI ile şifreli tutulur.
- Ambar ve stok kodu/adına göre filtreleme, arama.
- Excel (.xlsx) ve CSV (;) olarak dışa aktarma.
- Kurulum gerektirmez: tek `.exe` dosyası olarak dağıtılır, kayıt defterine yazmaz; ayarlar exe ile aynı klasördeki `profiles.json` dosyasında tutulur.

## Gereksinimler

- Çalıştırmak için: Windows 10/11 (x64). Ek bir .NET kurulumu gerekmez — dağıtım paketi self-contained'dır.
- Geliştirmek için: .NET 8 SDK (Windows Desktop iş yükü dahil — Windows'ta varsayılan .NET 8 SDK kurulumunda gelir).

> **TEYİT:** Varsayılan ambar stok sorgusu (`PaperStok.Core/Logo/LogoQueryTemplates.cs`) Logo Tiger3'ün standart şema adlarını (`STINVTOT`, `ITEMS`, `L_CAPIWHOUSE`) esas alır. Gerçek bir Logo Tiger3 veritabanına karşı doğrulanmamıştır ve müşteriye özel Logo kurulumlarında alan adları farklılaşabilir. Bağlantı Ayarları ekranındaki "Gelişmiş: Özel Ambar Sorgusu" alanından ortamınıza göre uyarlayın.

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
