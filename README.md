# 🎥 WebRTC Pro — Gerçek Zamanlı İletişim Platformu

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![SignalR](https://img.shields.io/badge/SignalR-Core-blue?style=flat-square)](https://learn.microsoft.com/aspnet/core/signalr/)
[![WebRTC](https://img.shields.io/badge/WebRTC-Standard-green?style=flat-square)](https://webrtc.org/)
[![CI](https://img.shields.io/badge/CI-GitHub_Actions-orange?style=flat-square&logo=github)](https://github.com/ferhatolmez/WebRTCProjesi/actions)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

Profesyonel, yüksek performanslı **gerçek zamanlı video görüşme, ses ve mesajlaşma** platformu.  
Modern glassmorphic Web Client ile standartlara uyumlu Windows C# Desktop Client birlikte, aynı odada çalışır.

---

## ✨ Özellikler

### 🌐 Web Client (Tarayıcı)
| Özellik | Açıklama |
|---------|----------|
| **Glassmorphic UI** | Dark/Light tema, cam efekti, mikro-animasyonlar |
| **Gerçek Zamanlı Video** | Tarayıcı WebRTC API ile düşük gecikme |
| **Ekran Paylaşımı** | Doğrudan tarayıcıdan masaüstü paylaşımı |
| **Katılımcı Listesi** | Odadaki tüm kullanıcıları canlı takip |
| **Sohbet Sistemi** | Anlık mesajlaşma, XSS korumalı |
| **Bildirim Sesleri** | Yeni katılımcı ve mesaj bildirimleri |
| **Otomatik Yeniden Bağlanma** | Bağlantı koptuğunda otomatik recovery |
| **Tam Responsive** | Mobil, tablet ve masaüstü uyumlu |

### 🖥️ Windows Client (SIPSorcery)
| Özellik | Açıklama |
|---------|----------|
| **WebRTC P2P** | SDP Offer/Answer ve ICE candidate standartları |
| **VP8 Video Codec** | SIPSorcery VpxVideoEncoder ile video kodlama |
| **Opus Ses Codec** | Düşük gecikmeli ses kodlama |
| **Lokal Video Önizleme** | Kamera görüntüsünü anında önizleme (PictureBox) |
| **Uzak Video Görüntüleme** | Karşı tarafın görüntüsünü decode edip gösterme |
| **Optimize Base64 Fallback** | Chrome uyumsuzluk durumunda SignalR üzerinden ~20 FPS, %60 JPEG kalitesinde akış |
| **Otomatik Yeniden Bağlanma** | Bağlantı kaybında otomatik recovery |

### ⚡ Sinyal Sunucusu (ASP.NET Core)
| Özellik | Açıklama |
|---------|----------|
| **Thread-Safe Hub** | `ConcurrentDictionary` ile eşzamanlılık güvenliği |
| **SignalR Core** | Gerçek zamanlı çift yönlü WebSocket iletişim |
| **Health Check** | `/health` ve `/api/stats` endpoint'leri |
| **Oda Yönetimi** | Çoklu oda desteği, katılma/ayrılma yönetimi |
| **Video Frame Relay** | Windows → Tarayıcı Base64 frame aktarımı |

---

## 🏗️ Mimari

```
           Sinyalleşme (SignalR)                   Sinyalleşme (SignalR)
┌──────────────┐  ◄───────────────►  ┌───────────────────────┐  ◄───────────────►  ┌──────────────────┐
│              │                     │                       │                     │                  │
│  Web Client  │   WebRTC P2P ────►  │    ASP.NET Core       │                     │  Windows Client  │
│  (Browser)   │   (Tarayıcı→Win)   │    SignalR Server      │                     │  (SIPSorcery)    │
│              │                     │  http://localhost:5050 │                     │                  │
│              │  ◄──── Base64 ───── │    Video Frame Relay   │  ◄── Base64 ─────── │                  │
│              │  (Windows→Tarayıcı) │                       │  (Optimize JPEG)    │                  │
└──────────────┘                     └───────────────────────┘                     └──────────────────┘
```

### Video Akış Yönleri

| Yön | Teknoloji | Performans |
|-----|-----------|------------|
| **Tarayıcı → Windows** | Saf WebRTC P2P (VP8) | ⚡ Düşük gecikme, yüksek kalite |
| **Windows → Tarayıcı** | SignalR Base64 (JPEG) | 🔄 ~20 FPS, %60 JPEG kalite |
| **Tarayıcı → Tarayıcı** | Saf WebRTC P2P (VP8) | ⚡ Düşük gecikme, yüksek kalite |

> **Not:** Windows → Tarayıcı yönünde SIPSorcery kütüphanesinin VP8 encoder çıkışı Chrome'un decoder'ı ile tam uyumlu olmadığından, optimize edilmiş SignalR Base64 fallback kullanılmaktadır.

---

## 📂 Proje Yapısı

```
WebRTCProjesi/
├── .github/
│   └── workflows/
│       └── dotnet-desktop.yml    # GitHub Actions CI Pipeline
├── WebRTCSignalServer/           # ASP.NET Core SignalR Sunucusu
│   ├── Program.cs                # Hub + API tanımları + Video Frame Relay
│   ├── Properties/
│   │   └── launchSettings.json   # Sunucu başlatma ayarları (port 5050)
│   ├── wwwroot/
│   │   ├── index.html            # Web Client SPA (glassmorphic UI)
│   │   ├── css/                  # Ayrıştırılmış stil dosyaları
│   │   └── js/                   # Modüler JavaScript kodları
│   └── appsettings.json
├── WebRTCWindowsClient/          # WinForms + SIPSorcery Desktop Client
│   ├── Form1.cs                  # Ana form: kamera, WebRTC, Base64 fallback
│   ├── Form1.Designer.cs         # UI bileşenleri (video panelleri, chat)
│   ├── Program.cs                # Giriş noktası
│   └── WebRTCWindowsClient.csproj
└── README.md
```

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Modern tarayıcı (Chrome, Edge, Firefox)
- Visual Studio 2022+ (Windows Client için)
- Webcam ve mikrofon (video görüşme için)

### 1. Web Client'a Bağlan (Canlı Sürüm)
Projenin tarayıcı sürümü şu adreste canlı yayındadır:  
👉 **[https://webrtcprojesi.onrender.com/index.html](https://webrtcprojesi.onrender.com/index.html)**  
Oda ID ve kullanıcı adı girerek hemen test etmeye başlayabilirsiniz.

### 2. Web Client'a Bağlan
### 2. Windows Client'ı İndir ve Çalıştır
Proje dosyaları arasındaki `WebRTCWindowsClient_Release.zip` dosyasını bilgisayarınıza indirin ve klasöre çıkarın.
İçindeki `WebRTCWindowsClient.exe` dosyasına çift tıklayarak uygulamayı başlatın.
Varsayılan olarak canlı sunucuya ayarlanmıştır. Odanızı ve isminizi seçip hemen bağlanabilirsiniz.

### 3. Geliştirici Olarak Kendi Bilgisayarınızda (Lokal) Çalıştırma
```bash
cd WebRTCSignalServer
dotnet run
```
Ardından Windows Client için:
```bash
cd WebRTCWindowsClient
dotnet run
```
Veya Visual Studio'da `WebRTCWindowsClient` projesini açıp **Start** butonuna basın.

### 4. Görüşme Başlatma
1. Her iki client'tan da aynı **Oda ID** ile bağlanın
2. **Görüşme Başlat** butonuna tıklayın
3. Video ve ses otomatik olarak karşı tarafa iletilir
4. Sohbet panelinden mesaj gönderin

---

## 📦 Bağımlılıklar

| Bileşen | Teknoloji | Versiyon |
|---------|-----------|----------|
| **Sunucu** | ASP.NET Core, SignalR Core | .NET 8.0 |
| **Web Client** | SignalR JS Client, Browser WebRTC API, Lucide Icons | — |
| **Windows Client** | SIPSorcery, SIPSorceryMedia.Windows, SIPSorceryMedia.Encoders | 10.x |
| **İletişim** | Microsoft.AspNetCore.SignalR.Client | 9.0.6 |
| **Serileştirme** | System.Text.Json | 9.0.6 |

---

## 🔗 API Endpoint'leri

| Endpoint | Metot | Açıklama |
|----------|-------|----------|
| `/` | `GET` | Web Client (index.html) |
| `/health` | `GET` | Sunucu sağlık kontrolü |
| `/api/stats` | `GET` | Bağlı kullanıcı ve aktif oda sayısı |
| `/webrtchub` | `WS` | SignalR WebSocket Hub |

### SignalR Hub Metotları

| Metot | Yön | Açıklama |
|-------|-----|----------|
| `JoinRoom` | Client → Server | Odaya katılma |
| `LeaveRoom` | Client → Server | Odadan ayrılma |
| `SendOffer` | Client → Client | WebRTC SDP Offer gönderme |
| `SendAnswer` | Client → Client | WebRTC SDP Answer gönderme |
| `SendIceCandidate` | Client → Client | ICE Candidate gönderme |
| `SendMessage` | Client → Client | Chat mesajı gönderme |
| `SendVideoFrame` | Client → Client | Base64 video frame aktarımı |

---

## 🔧 Teknik Detaylar

### Port Yapılandırması
Sunucu varsayılan olarak **port 5050** üzerinde çalışır. Bu, port 5000'in Windows/macOS sistemlerinde sıkça kullanılması nedeniyle tercih edilmiştir. Port değiştirmek için:
- `WebRTCSignalServer/Program.cs` → `app.Run("http://localhost:PORT")`
- `WebRTCSignalServer/wwwroot/index.html` → SignalR bağlantı URL'si
- `WebRTCWindowsClient/Form1.cs` → Varsayılan sunucu URL'si

### Hibrit Video Mimarisi
Proje, **akıllı hibrit mimari** kullanır:
- **Saf WebRTC (P2P):** Tarayıcılar arası ve tarayıcıdan Windows'a video akışı
- **Base64 SignalR Fallback:** Windows'tan tarayıcıya video akışı (~20 FPS, %60 JPEG)

Bu yaklaşım, SIPSorcery VP8 encoder'ın Chrome ile yaşadığı codec uyumsuzluğunu transparan şekilde çözer.

### GitHub Actions CI
Her `push` ve `pull_request` işleminde otomatik olarak:
1. `.NET 8 SDK` kurulumu
2. `WebRTCSignalServer` derlemesi
3. `WebRTCWindowsClient` derlemesi

---

## 📄 Lisans

MIT License — Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

*Geliştirici: **Ferhat Ölmez***
