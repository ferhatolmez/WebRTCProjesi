using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// SignalR servislerini ekle
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
});

// CORS politikasını ekle
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5000", "https://localhost:5001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
    Service = "WebRTC SignalR Server"
}));

Console.WriteLine("🚀 WebRTC SignalR Server başlatılıyor...");
Console.WriteLine("📡 Server URL: http://localhost:5000");
Console.WriteLine("🌐 Web Client: http://localhost:5000");
Console.WriteLine("🔗 SignalR Hub: http://localhost:5000/webrtchub");
Console.WriteLine("❤️  Health Check: http://localhost:5000/health");

app.Run("http://localhost:5000");

public class WebRTCHub : Hub
{
    private static readonly Dictionary<string, UserInfo> ConnectedUsers = new();
    private static readonly Dictionary<string, HashSet<string>> RoomUsers = new();

    public class UserInfo
    {
        public string ConnectionId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string UserType { get; set; } = "";
        public string RoomId { get; set; } = "";
        public DateTime ConnectedAt { get; set; }
    }

    public async Task JoinRoom(string roomId, string userName, string userType)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            ConnectedUsers[Context.ConnectionId] = new UserInfo
            {
                ConnectionId = Context.ConnectionId,
                UserName = userName,
                UserType = userType,
                RoomId = roomId,
                ConnectedAt = DateTime.UtcNow
            };

            if (!RoomUsers.ContainsKey(roomId))
                RoomUsers[roomId] = new HashSet<string>();

            RoomUsers[roomId].Add(Context.ConnectionId);

            await Clients.OthersInGroup(roomId).SendAsync("UserJoined", userName, userType, Context.ConnectionId);

            foreach (var existingConnectionId in RoomUsers[roomId])
            {
                if (existingConnectionId != Context.ConnectionId && ConnectedUsers.ContainsKey(existingConnectionId))
                {
                    var existingUser = ConnectedUsers[existingConnectionId];
                    await Clients.Caller.SendAsync("UserJoined", existingUser.UserName, existingUser.UserType, existingConnectionId);
                }
            }

            Console.WriteLine($"✅ {userName} ({userType}) joined room {roomId} - Total users in room: {RoomUsers[roomId].Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in JoinRoom: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to join room: {ex.Message}");
        }
    }

    public async Task SendOffer(string roomId, string targetConnectionId, string offer)
    {
        try
        {
            if (ConnectedUsers.ContainsKey(Context.ConnectionId))
            {
                var sender = ConnectedUsers[Context.ConnectionId];
                await Clients.Client(targetConnectionId).SendAsync("ReceiveOffer", offer, Context.ConnectionId);
                Console.WriteLine($"📤 Offer sent from {sender.UserName} to {targetConnectionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending offer: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send offer: {ex.Message}");
        }
    }

    public async Task SendAnswer(string roomId, string targetConnectionId, string answer)
    {
        try
        {
            if (ConnectedUsers.ContainsKey(Context.ConnectionId))
            {
                var sender = ConnectedUsers[Context.ConnectionId];
                await Clients.Client(targetConnectionId).SendAsync("ReceiveAnswer", answer, Context.ConnectionId);
                Console.WriteLine($"📤 Answer sent from {sender.UserName} to {targetConnectionId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending answer: {ex.Message}");
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
            Console.WriteLine($"❌ Error sending ICE candidate: {ex.Message}");
        }
    }

    public async Task SendMessage(string roomId, string userName, string message, string timestamp)
    {
        try
        {
            await Clients.Group(roomId).SendAsync("ReceiveMessage", userName, message, timestamp);
            Console.WriteLine($"💬 Message from {userName} in {roomId}: {message} @ {timestamp}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending message: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
        }
    }


    public async Task SendVideoFrame(string roomId, string senderConnectionId, string base64Frame)
    {
        try
        {
            await Clients.GroupExcept(roomId, senderConnectionId)
                         .SendAsync("ReceiveVideoFrame", base64Frame, senderConnectionId);

            Console.WriteLine($"📹 Video frame relayed from {senderConnectionId} to room {roomId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending video frame: {ex.Message}");
            await Clients.Caller.SendAsync("Error", $"Failed to send video frame: {ex.Message}");
        }
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        try
        {
            if (ConnectedUsers.ContainsKey(Context.ConnectionId))
            {
                var user = ConnectedUsers[Context.ConnectionId];

                if (RoomUsers.ContainsKey(user.RoomId))
                {
                    RoomUsers[user.RoomId].Remove(Context.ConnectionId);
                    if (RoomUsers[user.RoomId].Count == 0)
                        RoomUsers.Remove(user.RoomId);
                }

                await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);

                ConnectedUsers.Remove(Context.ConnectionId);
                Console.WriteLine($"❌ {user.UserName} ({user.UserType}) disconnected from room {user.RoomId}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in OnDisconnectedAsync: {ex.Message}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetRoomInfo(string roomId)
    {
        try
        {
            if (RoomUsers.ContainsKey(roomId))
            {
                var roomUsers = RoomUsers[roomId].Where(id => ConnectedUsers.ContainsKey(id))
                                                 .Select(id => ConnectedUsers[id])
                                                 .ToList();

                await Clients.Caller.SendAsync("RoomInfo", roomUsers);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting room info: {ex.Message}");
        }
    }
}
