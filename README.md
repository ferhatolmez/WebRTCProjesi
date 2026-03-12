# 🎥 WebRTC Pro — Gerçek Zamanlı İletişim Platformu

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![SignalR](https://img.shields.io/badge/SignalR-Core-blue?style=flat-square)](https://learn.microsoft.com/aspnet/core/signalr/)
[![WebRTC](https://img.shields.io/badge/WebRTC-Standard-green?style=flat-square)](https://webrtc.org/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

Profesyonel, yüksek performanslı gerçek zamanlı video, ses ve mesajlaşma platformu. Modern Web Client ve standartlara uyumlu Windows C# Client birlikte çalışır.

---

## ✨ Özellikler

### 🌐 Web Client
- **Glassmorphic Arayüz** — Modern dark/light tema, cam efekti, mikro-animasyonlar
- **Gerçek Zamanlı Video** — Tarayıcı WebRTC API ile düşük gecikme
- **Ekran Paylaşımı** — Doğrudan tarayıcıdan masaüstü paylaşımı
- **Katılımcı Listesi** — Odadaki tüm kullanıcıları canlı takip
- **Sohbet Sistemi** — Anlık mesajlaşma, XSS korumalı
- **Bildirim Sesleri** — Yeni katılımcı ve mesaj bildirimleri
- **Otomatik Yeniden Bağlanma** — Bağlantı koptuğunda otomatik recovery
- **Tam Responsive** — Mobil, tablet ve masaüstü uyumlu

### 🖥️ Windows Client (SIPSorcery)
- **P2P Bağlantı** — Düşük gecikmeli peer-to-peer medya akışı
- **SDP/ICE Standartları** — Gerçek WebRTC SDP Offer/Answer ve ICE candidate
- **Video/Ses Kodlama** — VP8/VP9 video ve Opus ses codec desteği
- **Otomatik Yeniden Bağlanma** — Bağlantı kaybında otomatik recovery

### ⚡ Sinyal Sunucusu
- **Thread-Safe Hub** — ConcurrentDictionary ile eşzamanlılık güvenliği
- **SignalR Core** — Gerçek zamanlı çift yönlü iletişim
- **Health Check** — `/health` ve `/api/stats` endpoint'leri
- **Oda Yönetimi** — Çoklu oda desteği, katılma/ayrılma yönetimi

---

## 🏗️ Mimari

```
┌──────────────┐     SignalR      ┌─────────────────────┐     SignalR      ┌──────────────────┐
│  Web Client  │ ◄──────────────► │  ASP.NET Core       │ ◄──────────────► │  Windows Client  │
│  (Browser)   │     WebRTC       │  SignalR Server      │                  │  (SIPSorcery)    │
│              │ ◄──────────────► │  http://localhost:5050│                  │                  │
└──────────────┘ P2P Media / B64  └─────────────────────┘                  └──────────────────┘
```

---

## 📂 Proje Yapısı

```
WebRTCProjesi/
├── WebRTCSignalServer/       # ASP.NET Core SignalR Sunucusu
│   ├── Program.cs            # Hub + API tanımları
│   ├── wwwroot/
│   │   └── index.html        # Web Client (tek dosya SPA)
│   └── appsettings.json
├── WebRTCWindowsClient/      # WinForms + SIPSorcery Client
│   ├── Form1.cs              # Ana form mantığı
│   ├── Form1.Designer.cs     # UI bileşenleri
│   └── Program.cs            # Giriş noktası
└── README.md
```

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Modern tarayıcı (Chrome, Edge, Firefox)
- Visual Studio 2022+ (Windows Client için)

### 1. Sinyal Sunucusunu Başlat
```bash
cd WebRTCSignalServer
dotnet run
```
Sunucu `http://localhost:5050` adresinde çalışmaya başlar.

### 2. Web Client
Tarayıcınızda `http://localhost:5050` adresini açın.

### 3. Windows Client (İsteğe Bağlı)
- `WebRTCWindowsClient.sln` dosyasını Visual Studio ile açın
- Build & Run

---

## 📦 Bağımlılıklar

| Bileşen | Teknoloji |
|---------|-----------|
| **Sunucu** | ASP.NET Core 8, SignalR Core |
| **Web Client** | SignalR JS Client, Browser WebRTC API, Lucide Icons |
| **Windows Client** | SIPSorcery 10.x, SIPSorceryMedia.Windows, SIPSorceryMedia.Encoders |

---

## 🔗 API Endpoint'leri

| Endpoint | Açıklama |
|----------|----------|
| `GET /` | Web Client (index.html) |
| `GET /health` | Sunucu sağlık kontrolü |
| `GET /api/stats` | Bağlı kullanıcı ve aktif oda sayısı |
| `WS /webrtchub` | SignalR WebSocket Hub & Base64 Video Fallback |

---

*Profesyonel standartlarla görsel mükemmellik bir arada.*
