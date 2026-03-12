# WebRTC Pro - Gerçek Zamanlı İletişim Platformu

WebRTC Pro, kullanıcıların odalar (rooms) kurarak birbirleriyle sesli, görüntülü ve yazılı olarak haberleşmesini sağlayan modern bir gerçek zamanlı iletişim platformudur. Proje, farklı platformlardaki istemcilerin (Web ve Windows Masaüstü) aynı oda içerisinde buluşmasına olanak tanır.

## 🚀 Özellikler

- **Çoklu Platform Desteği:**
  - **Web İstemcisi:** Modern, responsive ve koyu/açık tema destekli Web arayüzü. (Gerçek WebRTC kullanır).
  - **Windows Masaüstü İstemcisi:** C# Windows Forms tabanlı masaüstü uygulaması. (Kameradan Base64 kareler yakalayıp sunucu üzerinden yayınlar).
- **Gerçek Zamanlı İletişim:** ASP.NET Core SignalR üzerinden anlık mesajlaşma ve sinyalleşme (Signaling).
- **Gelişmiş Medya Kontrolleri:** Web istemcisi üzerinde Kamera (Aç/Kapat), Mikrofon (Sustur/Sesi Aç) kontrolleri.
- **Ekran Paylaşımı (Geliştirme):** Web istemcisinde tarayıcının yerleşik ekran yakalama (`getDisplayMedia`) API'si ile kesintisiz ekran paylaşımı.
- **Modern Arayüz (Geliştirme):** Glassmorphism (cam efekti) detaylarına sahip modern UI/UX mimarisi.

## 🏗️ Proje Mimarisi

Proje iki temel bileşenden oluşmaktadır:

### 1. WebRTCSignalServer (Backend & Web İstemcisi)
ASP.NET Core (net8.0) tabanlı bir SignalR sunucusudur. 
- İstemciler (Web veya Windows) sunucuya SignalR üzerinden (`/webrtchub`) bağlanır.
- WebRTC sinyalleşmesi (Offer, Answer, ICE Candidates) bu sunucu üzerinden diğer istemcilere yönlendirilir.
- Odaya katılma/ayrılma, metin tabanlı mesajlaşma gibi durumları yönetir.
- **Web İstemcisi:** Sunucunun `wwwroot/index.html` konumu altında yer alır ve sunucu başlatıldığında doğrudan `http://localhost:5000` adresinden erişilebilir.

### 2. WebRTCWindowsClient (Windows İstemcisi)
C# Windows Forms (net8.0-windows) kullanılarak geliştirilmiş masaüstü arayüzüdür.
- `AForge.Video` kütüphanesi kullanarak kullanıcının varsayılan web kamerasını yakalar.
- Görüntü karelerini Base64 string'ine kodlayarak SignalR üzerinden (`SendVideoFrame` metoduyla) sunucuya iletir. Sunucu da bu kareleri Web istemcilerinde `<img src="...">` olarak render edilmek üzere dağıtır.

---

## 🛠️ Kurulum ve Çalıştırma

### Ön Koşullar
- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) veya daha yeni bir sürüm.
- Kamera (Tercihen Mikrofon) donanımı olan bir bilgisayar.

### Adım 1: Sinyal Sunucusunu (WebRTCSignalServer) Başlatma
1. Terminal veya Komut İstemi'ni açıp projenin ana dizinine gidin.
2. Signal Server dizinine girin ve çalıştırın:
```bash
cd WebRTCSignalServer
dotnet run
```
3. Sunucu başlatıldığında **http://localhost:5000** adresi üzerinden hizmet vermeye başlayacaktır.
4. Herhangi bir tarayıcıdan bu adrese giderek **Web İstemcisine** ulaşabilirsiniz.

### Adım 2: Windows İstemcisini (WebRTCWindowsClient) Başlatma
1. Yeni bir Terminal açın.
2. Windows Client dizinine gidin ve çalıştırın:
```bash
cd WebRTCWindowsClient
dotnet run
```
3. Masaüstü uygulaması açıldıktan sonra *Kamerayı Başlat* (varsa) ve ardından *Bağlan* butonlarına basarak sunucuya bağlanın.

---

## 💡 Son Yapılan İyileştirmeler (Güncelleme Notları)
- **SignalR Backend Hata Çözümleri:** İstemcilerin sunucuda bulamadığı ancak çağırmaya çalıştığı (`LeaveRoom`, `StopVideoCall`) metotlar eklendi. Bağlantı kesilme anlarında oluşan sunucu hataları giderildi.
- **Web İstemcisi Modernizasyonu:** Eski web arayüzü; *Inter* fontu, *Lucide* ikonları ve modern renk paletleriyle (Açık ve Koyu tema anahtarı ile) baştan yazıldı.
- **Ekran Paylaşma Yeteneği:** Web arayüzüne sadece bir tuşla ekran/uygulama penceresi paylaşma yeteneği kazandırıldı.
- **Medya Toggle Butonları:** Konuşma sırasında Mikrofon susturma ve Kamerayı kapatma butonları eklendi.

## 📜 Katkı ve Lisans
Açık kaynak olarak geliştirilmiştir. Pull Request ve benzeri katkılarınıza açıktır.
