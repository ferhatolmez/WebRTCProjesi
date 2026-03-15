using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

// SignalR servislerini ekle
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

// CORS politikasını ekle (SignalR uyumlu)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// CORS'u etkinleştir
app.UseCors("AllowAll");

// Statik dosyalar için wwwroot klasörünü etkinleştir
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// SignalR Hub'ı yapılandır
app.MapHub<WebRTCHub>("/webrtchub");

// Ana sayfa
app.MapGet("/", () => Results.Redirect("/index.html"));

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "OK",
    Timestamp = DateTime.UtcNow,
    Service = "WebRTC SignalR Server",
    ConnectedUsers = WebRTCHub.GetConnectedUserCount(),
    ActiveRooms = WebRTCHub.GetActiveRoomCount()
}));

// API: Aktif oda bilgileri
app.MapGet("/api/stats", () => Results.Ok(new
{
    ConnectedUsers = WebRTCHub.GetConnectedUserCount(),
    ActiveRooms = WebRTCHub.GetActiveRoomCount(),
    Uptime = DateTime.UtcNow
}));

Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║   🚀 WebRTC SignalR Server v2.0             ║");
Console.WriteLine("╠══════════════════════════════════════════════╣");
Console.WriteLine("║   📡 Server:      http://localhost:5050      ║");
Console.WriteLine("║   🌐 Web Client:  http://localhost:5050      ║");
Console.WriteLine("║   🔗 SignalR Hub: /webrtchub                 ║");
Console.WriteLine("║   ❤️  Health:      /health                    ║");
Console.WriteLine("║   📊 Stats:       /api/stats                 ║");
Console.WriteLine("╚══════════════════════════════════════════════╝");

var port = Environment.GetEnvironmentVariable("PORT") ?? "5050";
app.Run($"http://0.0.0.0:{port}");

/// <summary>
/// WebRTC Sinyal Sunucusu Hub'ı - Thread-safe implementasyon
/// </summary>
public class WebRTCHub : Hub
{
    private static readonly ConcurrentDictionary<string, UserInfo> ConnectedUsers = new();
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> RoomUsers = new();

    public class UserInfo
    {
        public string ConnectionId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserType { get; set; } = "";
        public string RoomId { get; set; } = "";
        public DateTime ConnectedAt { get; set; }
    }

    // Public stats methods
    public static int GetConnectedUserCount() => ConnectedUsers.Count;
    public static int GetActiveRoomCount() => RoomUsers.Count;

    public async Task JoinRoom(string roomId, string userName, string userType)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var userInfo = new UserInfo
            {
                ConnectionId = Context.ConnectionId,
                UserName = userName,
                UserType = userType,
                RoomId = roomId,
                ConnectedAt = DateTime.UtcNow
            };

            ConnectedUsers.AddOrUpdate(Context.ConnectionId, userInfo, (_, _) => userInfo);

            var roomDict = RoomUsers.GetOrAdd(roomId, _ => new ConcurrentDictionary<string, byte>());
            roomDict.TryAdd(Context.ConnectionId, 0);

            // Yeni kullanıcıyı odadaki diğer kullanıcılara bildir
            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", userName, userType, Context.ConnectionId);

            // Mevcut oda kullanıcılarını yeni katılana bildir
            foreach (var connId in roomDict.Keys)
            {
                if (connId != Context.ConnectionId && ConnectedUsers.TryGetValue(connId, out var existingUser))
                {
                    await Clients.Caller.SendAsync("UserJoined", existingUser.UserName, existingUser.UserType, connId);
                }
            }

            var roomCount = roomDict.Count;
            Console.WriteLine($"  ✅ {userName} ({userType}) joined room '{roomId}' — {roomCount} user(s) in room");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error in JoinRoom: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to join room: {ex.Message}");
        }
    }

    public async Task SendOffer(string roomId, string targetConnectionId, string offer)
    {
        try
        {
            if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var sender))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", offer, Context.ConnectionId);
                Console.WriteLine($"  📤 Offer: {sender.UserName} → {targetConnectionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error sending offer: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send offer: {ex.Message}");
        }
    }

    public async Task SendAnswer(string roomId, string targetConnectionId, string answer)
    {
        try
        {
            if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var sender))
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", answer, Context.ConnectionId);
                Console.WriteLine($"  📤 Answer: {sender.UserName} → {targetConnectionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error sending answer: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send answer: {ex.Message}");
        }
    }

    public async Task SendIceCandidate(string roomId, string targetConnectionId, string candidate)
    {
        try
        {
            await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", candidate, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error sending ICE candidate: {ex.Message}");
        }
    }

    public async Task SendMessage(string roomId, string userName, string message, string timestamp)
    {
        try
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", userName, message, timestamp);
            Console.WriteLine($"  💬 [{roomId}] {userName}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error sending message: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
        }
    }

    public async Task SendVideoFrame(string roomId, string senderConnectionId, string base64Frame)
    {
        try
        {
            await Clients.GroupExcept(roomId, senderConnectionId)
                         .SendAsync("ReceiveVideoFrame", base64Frame, senderConnectionId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error sending video frame: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send video frame: {ex.Message}");
        }
    }

    public async Task StopVideoCall(string roomId)
    {
        try
        {
            await Clients.GroupExcept(roomId, Context.ConnectionId).SendAsync("VideoCallStopped", Context.ConnectionId);
            Console.WriteLine($"  ⏹️ Video call stopped in room '{roomId}' by {Context.ConnectionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error stopping video call: {ex.Message}");
        }
    }

    public async Task LeaveRoom(string roomId, string userName = "")
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);

            if (RoomUsers.TryGetValue(roomId, out var roomDict))
            {
                roomDict.TryRemove(Context.ConnectionId, out _);
                if (roomDict.IsEmpty)
                    RoomUsers.TryRemove(roomId, out _);
            }

            if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var user))
            {
                if (string.IsNullOrEmpty(userName)) userName = user.UserName;
                user.RoomId = "";
            }

            await Clients.Group(roomId).SendAsync("UserLeft", userName, Context.ConnectionId);
            Console.WriteLine($"  🚪 {userName} left room '{roomId}'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error leaving room: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out var user))
            {
                if (!string.IsNullOrEmpty(user.RoomId) && RoomUsers.TryGetValue(user.RoomId, out var roomDict))
                {
                    roomDict.TryRemove(Context.ConnectionId, out _);
                    if (roomDict.IsEmpty)
                        RoomUsers.TryRemove(user.RoomId, out _);
                }

                await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
                Console.WriteLine($"  ❌ {user.UserName} ({user.UserType}) disconnected from room '{user.RoomId}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error in OnDisconnectedAsync: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetRoomInfo(string roomId)
    {
        try
        {
            if (RoomUsers.TryGetValue(roomId, out var roomDict))
            {
                var roomUserList = roomDict.Keys
                    .Where(id => ConnectedUsers.ContainsKey(id))
                    .Select(id => ConnectedUsers[id])
                    .ToList();

                await Clients.Caller.SendAsync("RoomInfo", roomUserList);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ Error getting room info: {ex.Message}");
        }
    }
}
