using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using AForge.Video;
using AForge.Video.DirectShow;
using System.IO;

namespace WebRTCWindowsClient
{
    public partial class Form1 : Form
    {
        private HubConnection? hubConnection;
        private string currentRoomId = "";
        private string currentUserName = "";
        private Dictionary<string, string> connectedUsers = new Dictionary<string, string>();
        private bool isVideoCallActive = false;
        private System.Windows.Forms.Timer connectionTimer;

        // Kamera deðiþkenleri
        private FilterInfoCollection? videoDevices;
        private VideoCaptureDevice? videoSource;
        private bool isCameraRunning = false;

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            InitializeTimer();
            InitializeCameraDevices();
        }

        private void InitializeTimer()
        {
            connectionTimer = new System.Windows.Forms.Timer();
            connectionTimer.Interval = 30000; // 30 saniye
            connectionTimer.Tick += ConnectionTimer_Tick;
        }

        private void InitializeCameraDevices()
        {
            try
            {
                // Kamera cihazlarýný listele
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                cmbCameraDevices.Items.Clear();

                if (videoDevices.Count == 0)
                {
                    cmbCameraDevices.Items.Add("No camera devices found");
                    cmbCameraDevices.SelectedIndex = 0;
                    cmbCameraDevices.Enabled = false;
                    LogMessage("No camera devices detected");
                }
                else
                {
                    foreach (FilterInfo device in videoDevices)
                    {
                        cmbCameraDevices.Items.Add(device.Name);
                    }
                    cmbCameraDevices.SelectedIndex = 0;
                    LogMessage($"Found {videoDevices.Count} camera device(s)");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error initializing camera devices: {ex.Message}");
                cmbCameraDevices.Items.Add("Camera initialization failed");
                cmbCameraDevices.SelectedIndex = 0;
                cmbCameraDevices.Enabled = false;
            }
        }

        private async void ConnectionTimer_Tick(object sender, EventArgs e)
        {
            // Baðlantý durumunu kontrol et
            if (hubConnection?.State == HubConnectionState.Disconnected)
            {
                LogMessage("Connection lost, attempting to reconnect...");
                await ReconnectAsync();
            }
        }

        private async Task ReconnectAsync()
        {
            try
            {
                if (hubConnection != null)
                {
                    await hubConnection.StartAsync();
                    await hubConnection.InvokeAsync("JoinRoom", currentRoomId, currentUserName, "WindowsClient");
                    LogMessage("Reconnected successfully");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Reconnection failed: {ex.Message}");
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Form yüklendiðinde gerekli ayarlarý yap
            txtMessageToSend.KeyPress += TxtMessageToSend_KeyPress;

            // Varsayýlan server URL'ini kontrol et
            if (string.IsNullOrEmpty(txtServerUrl.Text))
            {
                txtServerUrl.Text = "http://localhost:5000";
            }

            LogMessage("Application started");
        }

        private void TxtMessageToSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnSendMessage_Click(sender, e);
                e.Handled = true;
            }
        }

        private void btnRefreshCameras_Click(object sender, EventArgs e)
        {
            LogMessage("Refreshing camera devices...");
            InitializeCameraDevices();
        }

        private void StartCamera()
        {
            try
            {
                if (videoDevices == null || videoDevices.Count == 0)
                {
                    LogMessage("No camera devices available");
                    return;
                }

                if (cmbCameraDevices.SelectedIndex < 0 || cmbCameraDevices.SelectedIndex >= videoDevices.Count)
                {
                    LogMessage("Invalid camera device selected");
                    return;
                }

                // Mevcut kamerayý durdur
                StopCamera();

                // Seçilen kamera cihazýný baþlat
                videoSource = new VideoCaptureDevice(videoDevices[cmbCameraDevices.SelectedIndex].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame;
                videoSource.Start();

                isCameraRunning = true;
                LogMessage($"Camera started: {videoDevices[cmbCameraDevices.SelectedIndex].Name}");

                // UI güncellemesi
                lblLocalVideo.Text = "Your Video: ON";
                lblLocalVideo.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                LogMessage($"Error starting camera: {ex.Message}");
                MessageBox.Show($"Failed to start camera: {ex.Message}", "Camera Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void StopCamera()
        {
            try
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    videoSource.SignalToStop();
                    videoSource.WaitForStop();
                    videoSource.NewFrame -= VideoSource_NewFrame;
                    videoSource = null;
                }

                isCameraRunning = false;

                // PictureBox'ý temizle
                pictureBoxLocalVideo.Image = null;
                pictureBoxLocalVideo.BackColor = Color.Black;

                // UI güncellemesi
                lblLocalVideo.Text = "Your Video: OFF";
                lblLocalVideo.ForeColor = Color.Red;

                LogMessage("Camera stopped");
            }
            catch (Exception ex)
            {
                LogMessage($"Error stopping camera: {ex.Message}");
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Frame'i PictureBox'a aktar
                if (pictureBoxLocalVideo.InvokeRequired)
                {
                    pictureBoxLocalVideo.Invoke(new Action(() =>
                    {
                        var bitmap = (Bitmap)eventArgs.Frame.Clone();
                        var oldImage = pictureBoxLocalVideo.Image;
                        pictureBoxLocalVideo.Image = bitmap;
                        oldImage?.Dispose();
                    }));
                }
                else
                {
                    var bitmap = (Bitmap)eventArgs.Frame.Clone();
                    var oldImage = pictureBoxLocalVideo.Image;
                    pictureBoxLocalVideo.Image = bitmap;
                    oldImage?.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing video frame: {ex.Message}");
            }
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoomId.Text) ||
                string.IsNullOrWhiteSpace(txtUserName.Text) ||
                string.IsNullOrWhiteSpace(txtServerUrl.Text))
            {
                MessageBox.Show("Please enter Server URL, Room ID and User Name!", "Missing Information",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnConnect.Enabled = false;
                lblStatus.Text = "Connecting...";
                lblStatus.ForeColor = Color.Orange;

                LogMessage("Attempting to connect...");

                // SignalR baðlantýsýný oluþtur
                string serverUrl = txtServerUrl.Text.TrimEnd('/');
                hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{serverUrl}/webrtchub")
                    .WithAutomaticReconnect()
                    .Build();

                // Event handler'larý ekle
                SetupSignalRHandlers();

                // Baðlantýyý baþlat
                await hubConnection.StartAsync();

                currentRoomId = txtRoomId.Text.Trim();
                currentUserName = txtUserName.Text.Trim();

                // Odaya katýl
                await hubConnection.InvokeAsync("JoinRoom", currentRoomId, currentUserName, "WindowsClient");

                // UI'ý güncelle
                UpdateUIForConnected();
                LogMessage($"Connected to room: {currentRoomId} as {currentUserName}");

                // Connection timer'ý baþlat
                connectionTimer.Start();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Connection Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage($"Connection error: {ex.Message}");
                UpdateUIForDisconnected();
            }
        }

        private void SetupSignalRHandlers()
        {
            if (hubConnection == null) return;

            // Baðlantý durumu deðiþiklikleri
            hubConnection.Closed += async (error) =>
            {
                this.Invoke(() =>
                {
                    LogMessage("Connection closed");
                    if (error != null)
                    {
                        LogMessage($"Connection error: {error.Message}");
                    }
                    UpdateUIForDisconnected();
                });
            };

            hubConnection.Reconnecting += (error) =>
            {
                this.Invoke(() =>
                {
                    LogMessage("Attempting to reconnect...");
                    lblStatus.Text = "Reconnecting...";
                    lblStatus.ForeColor = Color.Orange;
                });
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += (connectionId) =>
            {
                this.Invoke(() =>
                {
                    LogMessage($"Reconnected with ID: {connectionId}");
                    UpdateUIForConnected();
                });
                return Task.CompletedTask;
            };

            // Yeni kullanýcý katýldýðýnda
            hubConnection.On<string, string, string>("UserJoined", (userName, userType, connectionId) =>
            {
                this.Invoke(() =>
                {
                    connectedUsers[connectionId] = userName;
                    AddMessage($"[SYSTEM] {userName} ({userType}) joined the room");
                    LogMessage($"User joined: {userName} ({userType}) - {connectionId}");
                    UpdateConnectionCount();

                    // Video butonlarýný etkinleþtir (eðer baðlantý varsa)
                    if (connectedUsers.Count > 0)
                    {
                        btnStartVideo.Enabled = true;
                    }
                });
            });

            // Kullanýcý ayrýldýðýnda
            hubConnection.On<string>("UserDisconnected", (connectionId) =>
            {
                this.Invoke(() =>
                {
                    if (connectedUsers.ContainsKey(connectionId))
                    {
                        string userName = connectedUsers[connectionId];
                        connectedUsers.Remove(connectionId);
                        AddMessage($"[SYSTEM] {userName} left the room");
                        LogMessage($"User disconnected: {userName}");
                        UpdateConnectionCount();

                        // Eðer kimse kalmadýysa video butonunu devre dýþý býrak
                        if (connectedUsers.Count == 0)
                        {
                            btnStartVideo.Enabled = false;
                            if (isVideoCallActive)
                            {
                                btnStopVideo_Click(null, null);
                            }
                        }
                    }
                });
            });

            // Oda kullanýcýlarý listesi alýndýðýnda
            hubConnection.On<Dictionary<string, string>>("RoomUsers", (users) =>
            {
                this.Invoke(() =>
                {
                    connectedUsers = users;
                    LogMessage($"Room users updated: {users.Count} users");
                    UpdateConnectionCount();

                    foreach (var user in users)
                    {
                        AddMessage($"[SYSTEM] {user.Value} is in the room");
                    }
                });
            });

            // Mesaj alýndýðýnda
            hubConnection.On<string, string, string>("ReceiveMessage", (userName, message, timestamp) =>
            {
                this.Invoke(() =>
                {
                    AddMessage($"[{timestamp}] {userName}: {message}");
                });
            });

            // WebRTC Offer alýndýðýnda
            hubConnection.On<string, string>("ReceiveOffer", (offer, fromConnectionId) =>
            {
                this.Invoke(() =>
                {
                    string fromUser = connectedUsers.ContainsKey(fromConnectionId) ?
                                     connectedUsers[fromConnectionId] : "Unknown";
                    LogMessage($"Received WebRTC Offer from: {fromUser}");
                    HandleWebRTCOffer(offer, fromConnectionId);
                });
            });

            // WebRTC Answer alýndýðýnda
            hubConnection.On<string, string>("ReceiveAnswer", (answer, fromConnectionId) =>
            {
                this.Invoke(() =>
                {
                    string fromUser = connectedUsers.ContainsKey(fromConnectionId) ?
                                     connectedUsers[fromConnectionId] : "Unknown";
                    LogMessage($"Received WebRTC Answer from: {fromUser}");
                    HandleWebRTCAnswer(answer, fromConnectionId);
                });
            });

            // ICE Candidate alýndýðýnda
            hubConnection.On<string, string>("ReceiveIceCandidate", (candidate, fromConnectionId) =>
            {
                this.Invoke(() =>
                {
                    string fromUser = connectedUsers.ContainsKey(fromConnectionId) ?
                                     connectedUsers[fromConnectionId] : "Unknown";
                    LogMessage($"Received ICE Candidate from: {fromUser}");
                    HandleIceCandidate(candidate, fromConnectionId);
                });
            });

            // Video frame alýndýðýnda
            hubConnection.On<string, string>("ReceiveVideoFrame", (frameData, fromConnectionId) =>
            {
                this.Invoke(() =>
                {
                    HandleVideoFrame(frameData, fromConnectionId);
                });
            });

            // Video call durdurulduðunda
            hubConnection.On("VideoCallStopped", () =>
            {
                this.Invoke(() =>
                {
                    AddMessage("[SYSTEM] Video call has been stopped");
                    lblRemoteVideo.Text = "Remote Video: OFF";
                    lblRemoteVideo.ForeColor = Color.Red;
                    pictureBoxRemoteVideo.Image = null;
                    pictureBoxRemoteVideo.BackColor = Color.Black;
                });
            });

            // Hata mesajlarý
            hubConnection.On<string>("Error", (errorMessage) =>
            {
                this.Invoke(() =>
                {
                    LogMessage($"Server error: {errorMessage}");
                    MessageBox.Show($"Server error: {errorMessage}", "Server Error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            });
        }

        private void HandleVideoFrame(string frameData, string fromConnectionId)
        {
            try
            {
                // Base64 encoded frame data'yý decode et ve göster
                byte[] imageBytes = Convert.FromBase64String(frameData);
                using (var ms = new MemoryStream(imageBytes))
                {
                    var image = Image.FromStream(ms);
                    var oldImage = pictureBoxRemoteVideo.Image;
                    pictureBoxRemoteVideo.Image = image;
                    oldImage?.Dispose();

                    lblRemoteVideo.Text = "Remote Video: ON";
                    lblRemoteVideo.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing video frame: {ex.Message}");
            }
        }

        private void UpdateConnectionCount()
        {
            lblConnectionCount.Text = $"Users: {connectedUsers.Count}";
        }

        private async void HandleWebRTCOffer(string offer, string fromConnectionId)
        {
            try
            {
                LogMessage("Processing WebRTC Offer...");

                // Gerçek WebRTC kütüphanesi kullanýlacaksa buraya implementasyon gelecek
                // Þimdilik demo answer oluþtur
                var answerData = new
                {
                    type = "answer",
                    sdp = $"demo-answer-sdp-{DateTime.Now.Ticks}",
                    timestamp = DateTime.UtcNow.ToString("O")
                };

                var demoAnswer = JsonSerializer.Serialize(answerData);

                await hubConnection.InvokeAsync("SendAnswer", currentRoomId, fromConnectionId, demoAnswer);
                LogMessage("Answer sent successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing WebRTC Offer: {ex.Message}");
            }
        }

        private void HandleWebRTCAnswer(string answer, string fromConnectionId)
        {
            try
            {
                LogMessage("Processing WebRTC Answer...");

                // JSON'ý parse et
                var answerObj = JsonSerializer.Deserialize<JsonElement>(answer);

                if (answerObj.TryGetProperty("type", out var typeElement) &&
                    typeElement.GetString() == "answer")
                {
                    LogMessage("Valid WebRTC Answer received");
                    // Gerçek WebRTC answer processing burada yapýlacak
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing WebRTC Answer: {ex.Message}");
            }
        }

        private void HandleIceCandidate(string candidate, string fromConnectionId)
        {
            try
            {
                LogMessage("Processing ICE Candidate...");

                // JSON'ý parse et
                var candidateObj = JsonSerializer.Deserialize<JsonElement>(candidate);

                if (candidateObj.TryGetProperty("candidate", out var candidateElement))
                {
                    LogMessage("Valid ICE Candidate received");
                    // Gerçek ICE candidate processing burada yapýlacak
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error processing ICE Candidate: {ex.Message}");
            }
        }

        private async void btnDisconnect_Click(object sender, EventArgs e)
        {
            try
            {
                connectionTimer.Stop();

                // Kamerayý durdur
                StopCamera();

                if (hubConnection != null)
                {
                    // Odadan ayrýl
                    if (hubConnection.State == HubConnectionState.Connected)
                    {
                        await hubConnection.InvokeAsync("LeaveRoom", currentRoomId);
                    }

                    await hubConnection.StopAsync();
                    await hubConnection.DisposeAsync();
                    hubConnection = null;
                }

                UpdateUIForDisconnected();
                LogMessage("Disconnected from server");
                connectedUsers.Clear();
                isVideoCallActive = false;
            }
            catch (Exception ex)
            {
                LogMessage($"Disconnect error: {ex.Message}");
                UpdateUIForDisconnected();
            }
        }

        private async void btnSendMessage_Click(object sender, EventArgs e)
        {
            if (hubConnection?.State != HubConnectionState.Connected ||
                string.IsNullOrWhiteSpace(txtMessageToSend.Text))
                return;

            try
            {
                string message = txtMessageToSend.Text.Trim();
                string timestamp = DateTime.Now.ToString("HH:mm:ss");

                await hubConnection.InvokeAsync("SendMessage", currentRoomId, currentUserName, message, timestamp);

                txtMessageToSend.Clear();
                txtMessageToSend.Focus();
            }
            catch (Exception ex)
            {
                LogMessage($"Send message error: {ex.Message}");
            }
        }


        private async void btnStartVideo_Click(object sender, EventArgs e)
        {
            if (hubConnection?.State != HubConnectionState.Connected) return;

            try
            {
                LogMessage("Starting video call...");

                // Kamerayý baþlat
                StartCamera();

                isVideoCallActive = true;

                // Gerçek WebRTC offer oluþtur (demo amaçlý)
                var offerData = new
                {
                    type = "offer",
                    sdp = $"demo-offer-sdp-{DateTime.Now.Ticks}",
                    timestamp = DateTime.UtcNow.ToString("O"),
                    initiator = currentUserName
                };

                var demoOffer = JsonSerializer.Serialize(offerData);

                // Tüm baðlý kullanýcýlara offer gönder
                foreach (var user in connectedUsers)
                {
                    await hubConnection.InvokeAsync("SendOffer", currentRoomId, user.Key, demoOffer);
                    LogMessage($"Offer sent to: {user.Value}");
                }

                btnStartVideo.Enabled = false;
                btnStopVideo.Enabled = true;

                AddMessage($"[SYSTEM] {currentUserName} started a video call");

                // Video frame gönderme timer'ý baþlat (demo amaçlý)
                StartVideoFrameTimer();
            }
            catch (Exception ex)
            {
                LogMessage($"Start video error: {ex.Message}");
                isVideoCallActive = false;
                StopCamera();
            }
        }

        private System.Windows.Forms.Timer? videoFrameTimer;

        private void StartVideoFrameTimer()
        {
            // Demo amaçlý video frame gönderme
            videoFrameTimer = new System.Windows.Forms.Timer();
            videoFrameTimer.Interval = 100; // 10 FPS
            videoFrameTimer.Tick += async (s, e) =>
            {
                try
                {
                    if (isCameraRunning && pictureBoxLocalVideo.Image != null)
                    {
                        // Görüntüyü Base64'e çevir ve gönder
                        using (var ms = new MemoryStream())
                        {
                            pictureBoxLocalVideo.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            string frameData = Convert.ToBase64String(ms.ToArray());

                            // Tüm kullanýcýlara video frame gönder
                            foreach (var user in connectedUsers)
                            {
                                await hubConnection.InvokeAsync("SendVideoFrame", currentRoomId, user.Key, frameData);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"Error sending video frame: {ex.Message}");
                }
            };
            videoFrameTimer.Start();
        }

        private async void btnStopVideo_Click(object sender, EventArgs e)
        {
            try
            {
                LogMessage("Stopping video call...");
                isVideoCallActive = false;

                // Video frame timer'ý durdur
                videoFrameTimer?.Stop();
                videoFrameTimer?.Dispose();
                videoFrameTimer = null;

                // Kamerayý durdur
                StopCamera();

                // Video call stop mesajý gönder
                if (hubConnection?.State == HubConnectionState.Connected)
                {
                    await hubConnection.InvokeAsync("StopVideoCall", currentRoomId);
                }

                btnStartVideo.Enabled = connectedUsers.Count > 0;
                btnStopVideo.Enabled = false;

                // Remote video'yu temizle
                pictureBoxRemoteVideo.Image = null;
                pictureBoxRemoteVideo.BackColor = Color.Black;
                lblRemoteVideo.Text = "Remote Video: OFF";
                lblRemoteVideo.ForeColor = Color.Red;

                AddMessage($"[SYSTEM] {currentUserName} stopped the video call");
            }
            catch (Exception ex)
            {
                LogMessage($"Stop video error: {ex.Message}");
            }
        }

        private void btnClearMessages_Click(object sender, EventArgs e)
        {
            txtMessages.Clear();
            LogMessage("Chat messages cleared");
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            LogMessage("System log cleared");
        }

        private void UpdateUIForConnected()
        {
            lblStatus.Text = "Connected";
            lblStatus.ForeColor = Color.Green;
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;
            txtMessageToSend.Enabled = true;
            btnSendMessage.Enabled = true;
            txtRoomId.Enabled = false;
            txtUserName.Enabled = false;
            txtServerUrl.Enabled = false;

            // Video butonlarý sadece baþka kullanýcýlar varsa aktif
            btnStartVideo.Enabled = connectedUsers.Count > 0;
            btnStopVideo.Enabled = false;

            // Kamera kontrollerini etkinleþtir
            cmbCameraDevices.Enabled = videoDevices != null && videoDevices.Count > 0;
            btnRefreshCameras.Enabled = true;
        }

        private void UpdateUIForDisconnected()
        {
            lblStatus.Text = "Disconnected";
            lblStatus.ForeColor = Color.Red;
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
            txtMessageToSend.Enabled = false;
            btnSendMessage.Enabled = false;
            btnStartVideo.Enabled = false;
            btnStopVideo.Enabled = false;
            txtRoomId.Enabled = true;
            txtUserName.Enabled = true;
            txtServerUrl.Enabled = true;
            lblConnectionCount.Text = "Users: 0";

            // Video label'larýný sýfýrla
            lblLocalVideo.Text = "Your Video: OFF";
            lblLocalVideo.ForeColor = Color.Red;
            lblRemoteVideo.Text = "Remote Video: OFF";
            lblRemoteVideo.ForeColor = Color.Red;
        }

        private void AddMessage(string message)
        {
            if (txtMessages.InvokeRequired)
            {
                txtMessages.Invoke(new Action(() => AddMessage(message)));
                return;
            }

            txtMessages.AppendText(message + Environment.NewLine);
            txtMessages.SelectionStart = txtMessages.Text.Length;
            txtMessages.ScrollToCaret();
        }

        private void LogMessage(string message)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => LogMessage(message)));
                return;
            }

            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";
            txtLog.AppendText(logEntry + Environment.NewLine);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                connectionTimer?.Stop();
                connectionTimer?.Dispose();

                // Video frame timer'ý durdur
                videoFrameTimer?.Stop();
                videoFrameTimer?.Dispose();

                // Kamerayý durdur
                StopCamera();

                if (hubConnection != null)
                {
                    if (hubConnection.State == HubConnectionState.Connected)
                    {
                        await hubConnection.InvokeAsync("LeaveRoom", currentRoomId);
                    }
                    await hubConnection.StopAsync();
                    await hubConnection.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                // Form kapanýrken hata olsa bile devam et
                System.Diagnostics.Debug.WriteLine($"Error during form closing: {ex.Message}");
            }

            base.OnFormClosing(e);
        }
    }
}
