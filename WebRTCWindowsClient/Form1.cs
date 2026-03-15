using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using SIPSorcery.Net;
using SIPSorcery.Media;
using SIPSorceryMedia.Windows;
using SIPSorceryMedia.Abstractions;
using SIPSorceryMedia.Encoders;
using System.Net;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace WebRTCWindowsClient
{
    public partial class Form1 : Form
    {
        private HubConnection? hubConnection;
        private string currentRoomId = "";
        private string currentUserName = "";
        private Dictionary<string, string> connectedUsers = new Dictionary<string, string>();

        private RTCPeerConnection? peerConnection;
        private WindowsVideoEndPoint? videoEndPoint;
        private WindowsAudioEndPoint? audioEndPoint;
        private bool isVideoCallActive = false;
        private System.Windows.Forms.Timer connectionTimer;
        private DateTime lastFrameTime = DateTime.MinValue;
        private bool firstFrameLogged = false;
        private bool firstRemoteDataLogged = false;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            connectionTimer = new System.Windows.Forms.Timer();
            connectionTimer.Interval = 30000;
            connectionTimer.Tick += ConnectionTimer_Tick;
            InitializeCameraDevices();
        }

        private void InitializeCameraDevices()
        {
            try
            {
                cmbCameraDevices.Items.Clear();
                cmbCameraDevices.Items.Add("Default Video Device");
                cmbCameraDevices.SelectedIndex = 0;
            }
            catch (Exception ex) { LogMessage($"Camera init error: {ex.Message}"); }
        }

        private async void ConnectionTimer_Tick(object? sender, EventArgs e)
        {
            if (hubConnection?.State == HubConnectionState.Disconnected) { await ReconnectAsync(); }
        }

        private async Task ReconnectAsync()
        {
            try
            {
                if (hubConnection != null)
                {
                    await hubConnection.StartAsync();
                    await hubConnection.InvokeAsync("JoinRoom", currentRoomId, currentUserName, "WindowsClient");
                }
            }
            catch { }
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            txtMessageToSend.KeyPress += (s, ev) =>
            {
                if (ev.KeyChar == (char)Keys.Enter) { btnSendMessage_Click(s, ev); ev.Handled = true; }
            };
            if (string.IsNullOrEmpty(txtServerUrl.Text)) { txtServerUrl.Text = "https://webrtcprojesi.onrender.com"; }
            if (string.IsNullOrEmpty(txtRoomId.Text)) { txtRoomId.Text = "room1"; }
            LogMessage("Application started (Professional WebRTC)");
        }

        // ─── Camera Management ───────────────────────────────────────
        private async Task StartCamera()
        {
            try
            {
                if (videoEndPoint != null) { await videoEndPoint.CloseVideo(); videoEndPoint = null; }

                videoEndPoint = new WindowsVideoEndPoint(new VpxVideoEncoder());
                firstFrameLogged = false;

                // Subscribe to raw frames for local preview
                videoEndPoint.OnVideoSourceRawSample += OnLocalVideoFrame;

                await videoEndPoint.StartVideo();
                this.Invoke(() => { lblLocalVideo.Text = "Your Video: ON"; lblLocalVideo.ForeColor = Color.Green; });
                LogMessage("Camera started.");
            }
            catch (Exception ex)
            {
                LogMessage($"Camera error: {ex.Message}");
            }
        }

        private void OnLocalVideoFrame(uint durationMs, int width, int height, byte[] sample, VideoPixelFormatsEnum pixelFormat)
        {
            try
            {
                // Throttle to ~15 fps for UI performance
                var now = DateTime.UtcNow;
                if ((now - lastFrameTime).TotalMilliseconds < 50) return; // ~20 fps
                lastFrameTime = now;

                if (!firstFrameLogged)
                {
                    firstFrameLogged = true;
                    this.BeginInvoke(() => LogMessage($"First frame: {width}x{height}, format={pixelFormat}, bytes={sample.Length}"));
                }

                if (width <= 0 || height <= 0 || sample == null || sample.Length == 0) return;

                Bitmap? bmp = null;

                if (pixelFormat == VideoPixelFormatsEnum.Bgr || pixelFormat == VideoPixelFormatsEnum.Rgb)
                {
                    // 24-bit per pixel
                    bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    int copyLen = Math.Min(sample.Length, Math.Abs(bmpData.Stride) * height);
                    Marshal.Copy(sample, 0, bmpData.Scan0, copyLen);
                    bmp.UnlockBits(bmpData);
                }
                else if (pixelFormat == VideoPixelFormatsEnum.Bgra)
                {
                    // 32-bit per pixel
                    bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                    var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    int copyLen = Math.Min(sample.Length, Math.Abs(bmpData.Stride) * height);
                    Marshal.Copy(sample, 0, bmpData.Scan0, copyLen);
                    bmp.UnlockBits(bmpData);
                }
                else if (pixelFormat == VideoPixelFormatsEnum.NV12)
                {
                    // NV12: Y plane (width*height) + interleaved UV (width*height/2)
                    bmp = ConvertNV12ToBitmap(sample, width, height);
                }
                else if (pixelFormat == VideoPixelFormatsEnum.I420)
                {
                    // I420: Y (w*h) + U (w*h/4) + V (w*h/4)
                    bmp = ConvertI420ToBitmap(sample, width, height);
                }
                else
                {
                    // Unknown format - try to create from raw bytes as 24bpp
                    int expectedLen = width * height * 3;
                    if (sample.Length >= expectedLen)
                    {
                        bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                        var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        Marshal.Copy(sample, 0, bmpData.Scan0, expectedLen);
                        bmp.UnlockBits(bmpData);
                    }
                }

                if (bmp != null)
                {
                    this.BeginInvoke(() =>
                    {
                        var old = pictureBoxLocalVideo.Image;
                        pictureBoxLocalVideo.Image = bmp;
                        old?.Dispose();

                        // Send optimized Base64 frame to web clients via SignalR
                        // (SIPSorcery VP8 encoder output is not compatible with Chrome's decoder,
                        //  so we use this as the reliable Windows→Browser video transport)
                        if (isVideoCallActive && hubConnection?.State == HubConnectionState.Connected)
                        {
                            try
                            {
                                using var ms = new MemoryStream();
                                // Use 60% JPEG quality for good balance of quality vs bandwidth
                                var jpegEncoder = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                                    .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                                var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                                encoderParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(
                                    System.Drawing.Imaging.Encoder.Quality, 60L);
                                bmp.Save(ms, jpegEncoder, encoderParams);
                                string base64 = Convert.ToBase64String(ms.ToArray());
                                _ = hubConnection.InvokeAsync("SendVideoFrame", currentRoomId, hubConnection.ConnectionId, base64);
                            }
                            catch { /* Ignore send errors */ }
                        }
                    });
                }
            }
            catch { /* Silently ignore frame errors */ }
        }

        private static Bitmap ConvertNV12ToBitmap(byte[] nv12, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int ySize = width * height;
            int stride = bmpData.Stride;

            unsafe
            {
                byte* dst = (byte*)bmpData.Scan0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        int yIndex = j * width + i;
                        int uvIndex = ySize + (j / 2) * width + (i / 2) * 2;

                        if (yIndex >= nv12.Length || uvIndex + 1 >= nv12.Length) continue;

                        int y = nv12[yIndex];
                        int u = nv12[uvIndex] - 128;
                        int v = nv12[uvIndex + 1] - 128;

                        int r = (int)(y + 1.402 * v);
                        int g = (int)(y - 0.344 * u - 0.714 * v);
                        int b = (int)(y + 1.772 * u);

                        int offset = j * stride + i * 3;
                        dst[offset] = (byte)Math.Clamp(b, 0, 255);       // B
                        dst[offset + 1] = (byte)Math.Clamp(g, 0, 255);   // G
                        dst[offset + 2] = (byte)Math.Clamp(r, 0, 255);   // R
                    }
                }
            }

            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private static Bitmap ConvertI420ToBitmap(byte[] i420, int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            int ySize = width * height;
            int uvSize = ySize / 4;
            int stride = bmpData.Stride;

            unsafe
            {
                byte* dst = (byte*)bmpData.Scan0;
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        int yIndex = j * width + i;
                        int uIndex = ySize + (j / 2) * (width / 2) + (i / 2);
                        int vIndex = ySize + uvSize + (j / 2) * (width / 2) + (i / 2);

                        if (yIndex >= i420.Length || uIndex >= i420.Length || vIndex >= i420.Length) continue;

                        int y = i420[yIndex];
                        int u = i420[uIndex] - 128;
                        int v = i420[vIndex] - 128;

                        int r = (int)(y + 1.402 * v);
                        int g = (int)(y - 0.344 * u - 0.714 * v);
                        int b = (int)(y + 1.772 * u);

                        int offset = j * stride + i * 3;
                        dst[offset] = (byte)Math.Clamp(b, 0, 255);
                        dst[offset + 1] = (byte)Math.Clamp(g, 0, 255);
                        dst[offset + 2] = (byte)Math.Clamp(r, 0, 255);
                    }
                }
            }

            bmp.UnlockBits(bmpData);
            return bmp;
        }

        private void StopCamera()
        {
            if (videoEndPoint != null)
            {
                videoEndPoint.OnVideoSourceRawSample -= OnLocalVideoFrame;
                videoEndPoint.CloseVideo();
                videoEndPoint = null;
            }
            if (audioEndPoint != null) { audioEndPoint.CloseAudio(); audioEndPoint = null; }
            this.Invoke(() => { pictureBoxLocalVideo.Image = null; lblLocalVideo.Text = "Your Video: OFF"; lblLocalVideo.ForeColor = Color.Red; });
        }


        // ─── SignalR Connection ──────────────────────────────────────
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            currentRoomId = txtRoomId.Text.Trim();
            currentUserName = txtUserName.Text.Trim();
            if (string.IsNullOrEmpty(currentRoomId) || string.IsNullOrEmpty(currentUserName)) return;
            try
            {
                hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{txtServerUrl.Text.Trim()}/webrtchub")
                    .WithAutomaticReconnect()
                    .Build();
                SetupSignalRHandlers();
                await hubConnection.StartAsync();
                await hubConnection.InvokeAsync("JoinRoom", currentRoomId, currentUserName, "WindowsClient");
                UpdateUIForConnected();
                connectionTimer.Start();
                LogMessage($"Joined room: {currentRoomId}");
            }
            catch (Exception ex)
            {
                LogMessage($"Connection failed: {ex.Message}");
                if (ex.InnerException != null) LogMessage($"  Inner: {ex.InnerException.Message}");
            }
        }

        private void SetupSignalRHandlers()
        {
            hubConnection?.On<string, string, string>("UserJoined", (userName, type, id) =>
            {
                connectedUsers[id] = userName;
                LogMessage($"User joined: {userName} ({type})");
                AddMessage($"[System] {userName} joined the room.");
                UpdateVideoButtons();
            });
            hubConnection?.On<string>("UserDisconnected", (id) =>
            {
                if (connectedUsers.TryGetValue(id, out var name)) LogMessage($"User disconnected: {name}");
                connectedUsers.Remove(id);
                CleanupPeerConnection();
                UpdateVideoButtons();
            });
            hubConnection?.On<string, string>("UserLeft", (userName, id) =>
            {
                connectedUsers.Remove(id);
                LogMessage($"User left: {userName}");
                CleanupPeerConnection();
                UpdateVideoButtons();
            });
            hubConnection?.On<string, string, string>("ReceiveMessage", (user, msg, time) =>
            {
                AddMessage($"[{time}] {user}: {msg}");
            });
            hubConnection?.On<string, string>("ReceiveOffer", async (sdp, id) => await HandleOffer(sdp, id));
            hubConnection?.On<string, string>("ReceiveAnswer", async (sdp, id) => await HandleAnswer(sdp, id));
            hubConnection?.On<string, string>("ReceiveIceCandidate", async (c, id) => await HandleIceCandidate(c, id));
        }

        private void UpdateVideoButtons()
        {
            this.Invoke(() =>
            {
                btnStartVideo.Enabled = hubConnection?.State == HubConnectionState.Connected
                    && connectedUsers.Count > 0 && !isVideoCallActive;
            });
        }

        // ─── WebRTC Signaling ────────────────────────────────────────
        private async void btnStartVideo_Click(object sender, EventArgs e)
        {
            try
            {
                await StartCamera();
                CreatePeerConnection();
                var offerInit = peerConnection!.createOffer(null);
                await peerConnection.setLocalDescription(offerInit);

                // CRITICAL: use offerInit.sdp (actual SDP text), NOT offerInit.ToString() (C# class name)
                var offerJson = JsonSerializer.Serialize(new { type = "offer", sdp = offerInit.sdp });
                LogMessage($"Sending offer (SDP length: {offerInit.sdp?.Length ?? 0})");

                foreach (var user in connectedUsers)
                {
                    await hubConnection!.InvokeAsync("SendOffer", currentRoomId, user.Key, offerJson);
                }
                isVideoCallActive = true;
                this.Invoke(() => { btnStartVideo.Enabled = false; btnStopVideo.Enabled = true; });
            }
            catch (Exception ex) { LogMessage($"Offer error: {ex.Message}"); CleanupPeerConnection(); }
        }

        private void CreatePeerConnection()
        {
            if (peerConnection != null) return;
            peerConnection = new RTCPeerConnection(new RTCConfiguration
            {
                iceServers = new List<RTCIceServer> { new RTCIceServer { urls = "stun:stun.l.google.com:19302" } }
            });

            if (videoEndPoint == null)
            {
                videoEndPoint = new WindowsVideoEndPoint(new VpxVideoEncoder());
                videoEndPoint.OnVideoSourceRawSample += OnLocalVideoFrame;
                _ = videoEndPoint.StartVideo();
            }
            if (audioEndPoint == null)
            {
                audioEndPoint = new WindowsAudioEndPoint(new AudioEncoder());
            }

            peerConnection.addTrack(new MediaStreamTrack(videoEndPoint.GetVideoSourceFormats(), MediaStreamStatusEnum.SendRecv));
            peerConnection.addTrack(new MediaStreamTrack(audioEndPoint.GetAudioSourceFormats(), MediaStreamStatusEnum.SendRecv));

            videoEndPoint.OnVideoSourceEncodedSample -= peerConnection.SendVideo;
            videoEndPoint.OnVideoSourceEncodedSample += peerConnection.SendVideo;
            audioEndPoint.OnAudioSourceEncodedSample -= peerConnection.SendAudio;
            audioEndPoint.OnAudioSourceEncodedSample += peerConnection.SendAudio;

            // Hook up remote video decoding
            videoEndPoint.OnVideoSinkDecodedSample -= OnRemoteVideoSample;
            videoEndPoint.OnVideoSinkDecodedSample += OnRemoteVideoSample;

            bool firstRemoteEncodedLogged = false;
            peerConnection.OnVideoFrameReceived += (remoteEP, timestamp, payload, format) =>
            {
                if (!firstRemoteEncodedLogged)
                {
                    firstRemoteEncodedLogged = true;
                    LogMessage($"First remote encoded frame rx: {payload.Length} bytes, format={format.Codec}");
                }
                videoEndPoint.GotVideoFrame(remoteEP, timestamp, payload, format);
            };
            // peerConnection.OnAudioFrameReceived += audioEndPoint.GotAudioFrame;

            peerConnection.onicecandidate += async (candidate) =>
            {
                var json = JsonSerializer.Serialize(new RTCIceCandidateInit
                {
                    candidate = candidate.candidate,
                    sdpMid = candidate.sdpMid,
                    sdpMLineIndex = (ushort)candidate.sdpMLineIndex
                });
                foreach (var user in connectedUsers)
                {
                    await hubConnection!.InvokeAsync("SendIceCandidate", currentRoomId, user.Key, json);
                }
            };

            peerConnection.OnVideoFormatsNegotiated += (f) => videoEndPoint.SetVideoSourceFormat(f.First());
            peerConnection.OnAudioFormatsNegotiated += (f) => audioEndPoint.SetAudioSourceFormat(f.First());
            peerConnection.onconnectionstatechange += (s) => LogMessage($"WebRTC State: {s}");
        }

        private void OnRemoteVideoSample(byte[] sample, uint width, uint height, int stride, VideoPixelFormatsEnum pixelFormat)
        {
            try
            {
                if (width <= 0 || height <= 0 || sample == null || sample.Length == 0) return;

                if (!firstRemoteDataLogged)
                {
                    firstRemoteDataLogged = true;
                    this.BeginInvoke(() => LogMessage($"First decoded remote frame: {width}x{height}, Format={pixelFormat}"));
                }

                Bitmap? bmp = null;
                int w = (int)width;
                int h = (int)height;

                if (pixelFormat == VideoPixelFormatsEnum.Bgr || pixelFormat == VideoPixelFormatsEnum.Rgb)
                {
                    bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    var bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    int copyLen = Math.Min(sample.Length, Math.Abs(bmpData.Stride) * h);
                    Marshal.Copy(sample, 0, bmpData.Scan0, copyLen);
                    bmp.UnlockBits(bmpData);
                }
                else if (pixelFormat == VideoPixelFormatsEnum.Bgra)
                {
                    bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    var bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    int copyLen = Math.Min(sample.Length, Math.Abs(bmpData.Stride) * h);
                    Marshal.Copy(sample, 0, bmpData.Scan0, copyLen);
                    bmp.UnlockBits(bmpData);
                }
                else if (pixelFormat == VideoPixelFormatsEnum.NV12)
                {
                    bmp = ConvertNV12ToBitmap(sample, w, h);
                }
                else if (pixelFormat == VideoPixelFormatsEnum.I420)
                {
                    bmp = ConvertI420ToBitmap(sample, w, h);
                }
                else
                {
                    int expectedLen = w * h * 3;
                    if (sample.Length >= expectedLen)
                    {
                        bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                        var bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                        Marshal.Copy(sample, 0, bmpData.Scan0, expectedLen);
                        bmp.UnlockBits(bmpData);
                    }
                }

                if (bmp != null)
                {
                    this.BeginInvoke(() =>
                    {
                        if (peerConnection == null) { bmp.Dispose(); return; }
                        var old = pictureBoxRemoteVideo.Image;
                        pictureBoxRemoteVideo.Image = bmp;
                        old?.Dispose();
                    });
                }
            }
            catch { }
        }

        private async Task HandleOffer(string offerJsonStr, string fromId)
        {
            try
            {
                // Parse JSON: {"type":"offer","sdp":"v=0\r\n..."}
                string sdpText = offerJsonStr;
                try
                {
                    using var doc = JsonDocument.Parse(offerJsonStr);
                    if (doc.RootElement.TryGetProperty("sdp", out var sdpProp))
                        sdpText = sdpProp.GetString() ?? offerJsonStr;
                }
                catch { /* Not JSON, use as raw SDP */ }

                LogMessage($"Received offer (SDP length: {sdpText.Length})");
                await StartCamera();
                CreatePeerConnection();
                var result = peerConnection!.setRemoteDescription(new RTCSessionDescriptionInit
                {
                    type = RTCSdpType.offer,
                    sdp = sdpText
                });

                if (result == SetDescriptionResultEnum.OK)
                {
                    var answerInit = peerConnection.createAnswer(null);
                    await peerConnection.setLocalDescription(answerInit);
                    // CRITICAL: use answerInit.sdp, NOT answerInit.ToString()
                    var answerJson = JsonSerializer.Serialize(new { type = "answer", sdp = answerInit.sdp });
                    await hubConnection!.InvokeAsync("SendAnswer", currentRoomId, fromId, answerJson);
                    LogMessage($"Sent answer (SDP length: {answerInit.sdp?.Length ?? 0})");
                    this.Invoke(() => { isVideoCallActive = true; btnStartVideo.Enabled = false; btnStopVideo.Enabled = true; });
                }
                else
                {
                    LogMessage($"SetRemoteDescription failed: {result}");
                }
            }
            catch (Exception ex) { LogMessage($"HandleOffer error: {ex.Message}"); }
        }

        private async Task HandleAnswer(string answerJsonStr, string fromId)
        {
            try
            {
                string sdpText = answerJsonStr;
                try
                {
                    using var doc = JsonDocument.Parse(answerJsonStr);
                    if (doc.RootElement.TryGetProperty("sdp", out var sdpProp))
                        sdpText = sdpProp.GetString() ?? answerJsonStr;
                }
                catch { }

                LogMessage($"Received answer (SDP length: {sdpText.Length})");
                peerConnection?.setRemoteDescription(new RTCSessionDescriptionInit
                {
                    type = RTCSdpType.answer,
                    sdp = sdpText
                });
            }
            catch (Exception ex) { LogMessage($"HandleAnswer error: {ex.Message}"); }
        }

        private async Task HandleIceCandidate(string json, string fromId)
        {
            try
            {
                var candidateInit = JsonSerializer.Deserialize<RTCIceCandidateInit>(json);
                if (peerConnection != null && candidateInit != null)
                    peerConnection.addIceCandidate(candidateInit);
            }
            catch { }
        }

        // ─── Call Management ─────────────────────────────────────────
        private void CleanupPeerConnection()
        {
            peerConnection?.Close("Normal closure");
            peerConnection = null;
            isVideoCallActive = false;
            this.Invoke(() =>
            {
                UpdateVideoButtons();
                btnStopVideo.Enabled = false;
                pictureBoxRemoteVideo.Image = null;
            });
        }

        private async void btnStopVideo_Click(object sender, EventArgs e)
        {
            CleanupPeerConnection();
            StopCamera();
            if (hubConnection?.State == HubConnectionState.Connected)
                await hubConnection.InvokeAsync("StopVideoCall", currentRoomId);
        }

        // ─── Messaging ──────────────────────────────────────────────
        private async void btnSendMessage_Click(object? sender, EventArgs e)
        {
            if (hubConnection == null || hubConnection.State != HubConnectionState.Connected
                || string.IsNullOrEmpty(txtMessageToSend.Text)) return;
            try
            {
                var msg = txtMessageToSend.Text;
                var time = DateTime.Now.ToShortTimeString();
                await hubConnection.InvokeAsync("SendMessage", currentRoomId, currentUserName, msg, time);
                txtMessageToSend.Clear();
            }
            catch (Exception ex) { LogMessage($"Send error: {ex.Message}"); }
        }

        // ─── UI Helpers ──────────────────────────────────────────────
        private void btnDisconnect_Click(object sender, EventArgs e) { CleanupPeerConnection(); StopCamera(); hubConnection?.StopAsync(); UpdateUIForDisconnected(); }
        private void btnClearMessages_Click(object sender, EventArgs e) { txtMessages.Clear(); }
        private void btnClearLog_Click(object sender, EventArgs e) { txtLog.Clear(); }
        private void btnRefreshCameras_Click(object sender, EventArgs e) { InitializeCameraDevices(); }

        private void UpdateUIForConnected()
        {
            this.Invoke(() =>
            {
                lblStatus.Text = "Connected"; lblStatus.ForeColor = Color.Green;
                btnConnect.Enabled = false; btnDisconnect.Enabled = true;
                txtMessageToSend.Enabled = true; btnSendMessage.Enabled = true;
                txtRoomId.Enabled = false; txtUserName.Enabled = false; txtServerUrl.Enabled = false;
                cmbCameraDevices.Enabled = true; btnRefreshCameras.Enabled = true;
                UpdateVideoButtons();
            });
        }

        private void UpdateUIForDisconnected()
        {
            this.Invoke(() =>
            {
                lblStatus.Text = "Disconnected"; lblStatus.ForeColor = Color.Red;
                btnConnect.Enabled = true; btnDisconnect.Enabled = false;
                txtMessageToSend.Enabled = false; btnSendMessage.Enabled = false;
                btnStartVideo.Enabled = false; btnStopVideo.Enabled = false;
                txtRoomId.Enabled = true; txtUserName.Enabled = true; txtServerUrl.Enabled = true;
            });
        }

        private void AddMessage(string m) { this.Invoke(() => txtMessages.AppendText(m + Environment.NewLine)); }
        private void LogMessage(string m) { this.Invoke(() => txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {m}{Environment.NewLine}")); }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CleanupPeerConnection();
            StopCamera();
            base.OnFormClosing(e);
        }
    }
}
