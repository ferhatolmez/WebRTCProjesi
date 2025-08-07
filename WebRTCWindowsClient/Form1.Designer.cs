using System;
using System.Drawing;
using System.Windows.Forms;

namespace WebRTCWindowsClient
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.txtRoomId = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.txtMessages = new System.Windows.Forms.TextBox();
            this.txtMessageToSend = new System.Windows.Forms.TextBox();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblRoomId = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblMessages = new System.Windows.Forms.Label();
            this.btnStartVideo = new System.Windows.Forms.Button();
            this.btnStopVideo = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblLog = new System.Windows.Forms.Label();
            this.lblServerUrl = new System.Windows.Forms.Label();
            this.txtServerUrl = new System.Windows.Forms.TextBox();
            this.lblConnectionCount = new System.Windows.Forms.Label();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.btnClearMessages = new System.Windows.Forms.Button();
            this.groupBoxConnection = new System.Windows.Forms.GroupBox();
            this.groupBoxVideo = new System.Windows.Forms.GroupBox();
            this.groupBoxChat = new System.Windows.Forms.GroupBox();
            this.groupBoxLog = new System.Windows.Forms.GroupBox();

            // Kamera için yeni kontroller
            this.pictureBoxLocalVideo = new System.Windows.Forms.PictureBox();
            this.pictureBoxRemoteVideo = new System.Windows.Forms.PictureBox();
            this.cmbCameraDevices = new System.Windows.Forms.ComboBox();
            this.lblCameraDevice = new System.Windows.Forms.Label();
            this.btnRefreshCameras = new System.Windows.Forms.Button();
            this.lblLocalVideo = new System.Windows.Forms.Label();
            this.lblRemoteVideo = new System.Windows.Forms.Label();
            this.groupBoxLocalVideo = new System.Windows.Forms.GroupBox();
            this.groupBoxRemoteVideo = new System.Windows.Forms.GroupBox();

            this.groupBoxConnection.SuspendLayout();
            this.groupBoxVideo.SuspendLayout();
            this.groupBoxChat.SuspendLayout();
            this.groupBoxLog.SuspendLayout();
            this.groupBoxLocalVideo.SuspendLayout();
            this.groupBoxRemoteVideo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLocalVideo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRemoteVideo)).BeginInit();
            this.SuspendLayout();

            // groupBoxConnection
            this.groupBoxConnection.Controls.Add(this.lblServerUrl);
            this.groupBoxConnection.Controls.Add(this.txtServerUrl);
            this.groupBoxConnection.Controls.Add(this.lblRoomId);
            this.groupBoxConnection.Controls.Add(this.txtRoomId);
            this.groupBoxConnection.Controls.Add(this.lblUserName);
            this.groupBoxConnection.Controls.Add(this.txtUserName);
            this.groupBoxConnection.Controls.Add(this.btnConnect);
            this.groupBoxConnection.Controls.Add(this.btnDisconnect);
            this.groupBoxConnection.Controls.Add(this.lblStatus);
            this.groupBoxConnection.Controls.Add(this.lblConnectionCount);
            this.groupBoxConnection.Location = new System.Drawing.Point(12, 12);
            this.groupBoxConnection.Name = "groupBoxConnection";
            this.groupBoxConnection.Size = new System.Drawing.Size(480, 120);
            this.groupBoxConnection.TabIndex = 0;
            this.groupBoxConnection.TabStop = false;
            this.groupBoxConnection.Text = "Connection Settings";

            // lblServerUrl
            this.lblServerUrl.AutoSize = true;
            this.lblServerUrl.Location = new System.Drawing.Point(15, 25);
            this.lblServerUrl.Name = "lblServerUrl";
            this.lblServerUrl.Size = new System.Drawing.Size(68, 15);
            this.lblServerUrl.TabIndex = 0;
            this.lblServerUrl.Text = "Server URL:";

            // txtServerUrl
            this.txtServerUrl.Location = new System.Drawing.Point(100, 22);
            this.txtServerUrl.Name = "txtServerUrl";
            this.txtServerUrl.Size = new System.Drawing.Size(200, 23);
            this.txtServerUrl.TabIndex = 1;
            this.txtServerUrl.Text = "http://localhost:5000";

            // lblRoomId
            this.lblRoomId.AutoSize = true;
            this.lblRoomId.Location = new System.Drawing.Point(15, 55);
            this.lblRoomId.Name = "lblRoomId";
            this.lblRoomId.Size = new System.Drawing.Size(59, 15);
            this.lblRoomId.TabIndex = 2;
            this.lblRoomId.Text = "Room ID:";

            // txtRoomId
            this.txtRoomId.Location = new System.Drawing.Point(100, 52);
            this.txtRoomId.Name = "txtRoomId";
            this.txtRoomId.Size = new System.Drawing.Size(200, 23);
            this.txtRoomId.TabIndex = 3;
            this.txtRoomId.Text = "room1";

            // lblUserName
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(15, 85);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(68, 15);
            this.lblUserName.TabIndex = 4;
            this.lblUserName.Text = "User Name:";

            // txtUserName
            this.txtUserName.Location = new System.Drawing.Point(100, 82);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(200, 23);
            this.txtUserName.TabIndex = 5;
            this.txtUserName.Text = "WindowsUser";

            // btnConnect
            this.btnConnect.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Location = new System.Drawing.Point(320, 22);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 30);
            this.btnConnect.TabIndex = 6;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = false;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

            // btnDisconnect
            this.btnDisconnect.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDisconnect.ForeColor = System.Drawing.Color.White;
            this.btnDisconnect.Location = new System.Drawing.Point(320, 58);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(75, 30);
            this.btnDisconnect.TabIndex = 7;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = false;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.Red;
            this.lblStatus.Location = new System.Drawing.Point(410, 25);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(79, 15);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Disconnected";

            // lblConnectionCount
            this.lblConnectionCount.AutoSize = true;
            this.lblConnectionCount.Location = new System.Drawing.Point(410, 45);
            this.lblConnectionCount.Name = "lblConnectionCount";
            this.lblConnectionCount.Size = new System.Drawing.Size(56, 15);
            this.lblConnectionCount.TabIndex = 9;
            this.lblConnectionCount.Text = "Users: 0";

            // groupBoxVideo - Updated with camera controls
            this.groupBoxVideo.Controls.Add(this.lblCameraDevice);
            this.groupBoxVideo.Controls.Add(this.cmbCameraDevices);
            this.groupBoxVideo.Controls.Add(this.btnRefreshCameras);
            this.groupBoxVideo.Controls.Add(this.btnStartVideo);
            this.groupBoxVideo.Controls.Add(this.btnStopVideo);
            this.groupBoxVideo.Location = new System.Drawing.Point(500, 12);
            this.groupBoxVideo.Name = "groupBoxVideo";
            this.groupBoxVideo.Size = new System.Drawing.Size(320, 120);
            this.groupBoxVideo.TabIndex = 1;
            this.groupBoxVideo.TabStop = false;
            this.groupBoxVideo.Text = "Video Controls";

            // lblCameraDevice
            this.lblCameraDevice.AutoSize = true;
            this.lblCameraDevice.Location = new System.Drawing.Point(10, 25);
            this.lblCameraDevice.Name = "lblCameraDevice";
            this.lblCameraDevice.Size = new System.Drawing.Size(51, 15);
            this.lblCameraDevice.TabIndex = 20;
            this.lblCameraDevice.Text = "Camera:";

            // cmbCameraDevices
            this.cmbCameraDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCameraDevices.Location = new System.Drawing.Point(70, 22);
            this.cmbCameraDevices.Name = "cmbCameraDevices";
            this.cmbCameraDevices.Size = new System.Drawing.Size(180, 23);
            this.cmbCameraDevices.TabIndex = 21;

            // btnRefreshCameras
            this.btnRefreshCameras.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnRefreshCameras.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefreshCameras.ForeColor = System.Drawing.Color.White;
            this.btnRefreshCameras.Location = new System.Drawing.Point(260, 22);
            this.btnRefreshCameras.Name = "btnRefreshCameras";
            this.btnRefreshCameras.Size = new System.Drawing.Size(50, 23);
            this.btnRefreshCameras.TabIndex = 22;
            this.btnRefreshCameras.Text = "⟳";
            this.btnRefreshCameras.UseVisualStyleBackColor = false;
            this.btnRefreshCameras.Click += new System.EventHandler(this.btnRefreshCameras_Click);

            // btnStartVideo
            this.btnStartVideo.BackColor = System.Drawing.Color.FromArgb(0, 123, 255);
            this.btnStartVideo.Enabled = false;
            this.btnStartVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStartVideo.ForeColor = System.Drawing.Color.White;
            this.btnStartVideo.Location = new System.Drawing.Point(10, 55);
            this.btnStartVideo.Name = "btnStartVideo";
            this.btnStartVideo.Size = new System.Drawing.Size(145, 30);
            this.btnStartVideo.TabIndex = 10;
            this.btnStartVideo.Text = "🎥 Start Video Call";
            this.btnStartVideo.UseVisualStyleBackColor = false;
            this.btnStartVideo.Click += new System.EventHandler(this.btnStartVideo_Click);

            // btnStopVideo
            this.btnStopVideo.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            this.btnStopVideo.Enabled = false;
            this.btnStopVideo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopVideo.ForeColor = System.Drawing.Color.White;
            this.btnStopVideo.Location = new System.Drawing.Point(165, 55);
            this.btnStopVideo.Name = "btnStopVideo";
            this.btnStopVideo.Size = new System.Drawing.Size(145, 30);
            this.btnStopVideo.TabIndex = 11;
            this.btnStopVideo.Text = "🛑 Stop Video Call";
            this.btnStopVideo.UseVisualStyleBackColor = false;
            this.btnStopVideo.Click += new System.EventHandler(this.btnStopVideo_Click);

            // groupBoxLocalVideo
            this.groupBoxLocalVideo.Controls.Add(this.lblLocalVideo);
            this.groupBoxLocalVideo.Controls.Add(this.pictureBoxLocalVideo);
            this.groupBoxLocalVideo.Location = new System.Drawing.Point(12, 150);
            this.groupBoxLocalVideo.Name = "groupBoxLocalVideo";
            this.groupBoxLocalVideo.Size = new System.Drawing.Size(320, 280);
            this.groupBoxLocalVideo.TabIndex = 4;
            this.groupBoxLocalVideo.TabStop = false;
            this.groupBoxLocalVideo.Text = "Local Video";

            // lblLocalVideo
            this.lblLocalVideo.AutoSize = true;
            this.lblLocalVideo.Location = new System.Drawing.Point(15, 25);
            this.lblLocalVideo.Name = "lblLocalVideo";
            this.lblLocalVideo.Size = new System.Drawing.Size(67, 15);
            this.lblLocalVideo.TabIndex = 23;
            this.lblLocalVideo.Text = "Your Video:";

            // pictureBoxLocalVideo
            this.pictureBoxLocalVideo.BackColor = System.Drawing.Color.Black;
            this.pictureBoxLocalVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxLocalVideo.Location = new System.Drawing.Point(15, 45);
            this.pictureBoxLocalVideo.Name = "pictureBoxLocalVideo";
            this.pictureBoxLocalVideo.Size = new System.Drawing.Size(290, 220);
            this.pictureBoxLocalVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxLocalVideo.TabIndex = 24;
            this.pictureBoxLocalVideo.TabStop = false;

            // groupBoxRemoteVideo
            this.groupBoxRemoteVideo.Controls.Add(this.lblRemoteVideo);
            this.groupBoxRemoteVideo.Controls.Add(this.pictureBoxRemoteVideo);
            this.groupBoxRemoteVideo.Location = new System.Drawing.Point(350, 150);
            this.groupBoxRemoteVideo.Name = "groupBoxRemoteVideo";
            this.groupBoxRemoteVideo.Size = new System.Drawing.Size(320, 280);
            this.groupBoxRemoteVideo.TabIndex = 5;
            this.groupBoxRemoteVideo.TabStop = false;
            this.groupBoxRemoteVideo.Text = "Remote Video";

            // lblRemoteVideo
            this.lblRemoteVideo.AutoSize = true;
            this.lblRemoteVideo.Location = new System.Drawing.Point(15, 25);
            this.lblRemoteVideo.Name = "lblRemoteVideo";
            this.lblRemoteVideo.Size = new System.Drawing.Size(81, 15);
            this.lblRemoteVideo.TabIndex = 25;
            this.lblRemoteVideo.Text = "Remote Video:";

            // pictureBoxRemoteVideo
            this.pictureBoxRemoteVideo.BackColor = System.Drawing.Color.Black;
            this.pictureBoxRemoteVideo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxRemoteVideo.Location = new System.Drawing.Point(15, 45);
            this.pictureBoxRemoteVideo.Name = "pictureBoxRemoteVideo";
            this.pictureBoxRemoteVideo.Size = new System.Drawing.Size(290, 220);
            this.pictureBoxRemoteVideo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxRemoteVideo.TabIndex = 26;
            this.pictureBoxRemoteVideo.TabStop = false;

            // groupBoxChat - Repositioned
            this.groupBoxChat.Controls.Add(this.lblMessages);
            this.groupBoxChat.Controls.Add(this.txtMessages);
            this.groupBoxChat.Controls.Add(this.txtMessageToSend);
            this.groupBoxChat.Controls.Add(this.btnSendMessage);
            this.groupBoxChat.Controls.Add(this.btnClearMessages);
            this.groupBoxChat.Location = new System.Drawing.Point(690, 150);
            this.groupBoxChat.Name = "groupBoxChat";
            this.groupBoxChat.Size = new System.Drawing.Size(350, 280);
            this.groupBoxChat.TabIndex = 2;
            this.groupBoxChat.TabStop = false;
            this.groupBoxChat.Text = "Chat Messages";

            // lblMessages
            this.lblMessages.AutoSize = true;
            this.lblMessages.Location = new System.Drawing.Point(15, 25);
            this.lblMessages.Name = "lblMessages";
            this.lblMessages.Size = new System.Drawing.Size(64, 15);
            this.lblMessages.TabIndex = 12;
            this.lblMessages.Text = "Messages:";

            // txtMessages
            this.txtMessages.BackColor = System.Drawing.Color.White;
            this.txtMessages.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtMessages.Location = new System.Drawing.Point(15, 45);
            this.txtMessages.Multiline = true;
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ReadOnly = true;
            this.txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessages.Size = new System.Drawing.Size(320, 180);
            this.txtMessages.TabIndex = 13;

            // txtMessageToSend
            this.txtMessageToSend.Enabled = false;
            this.txtMessageToSend.Location = new System.Drawing.Point(15, 235);
            this.txtMessageToSend.Name = "txtMessageToSend";
            this.txtMessageToSend.PlaceholderText = "Type your message here...";
            this.txtMessageToSend.Size = new System.Drawing.Size(210, 23);
            this.txtMessageToSend.TabIndex = 14;

            // btnSendMessage
            this.btnSendMessage.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            this.btnSendMessage.Enabled = false;
            this.btnSendMessage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendMessage.ForeColor = System.Drawing.Color.White;
            this.btnSendMessage.Location = new System.Drawing.Point(235, 235);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(50, 23);
            this.btnSendMessage.TabIndex = 15;
            this.btnSendMessage.Text = "Send";
            this.btnSendMessage.UseVisualStyleBackColor = false;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);

            // btnClearMessages
            this.btnClearMessages.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnClearMessages.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearMessages.ForeColor = System.Drawing.Color.White;
            this.btnClearMessages.Location = new System.Drawing.Point(290, 235);
            this.btnClearMessages.Name = "btnClearMessages";
            this.btnClearMessages.Size = new System.Drawing.Size(45, 23);
            this.btnClearMessages.TabIndex = 16;
            this.btnClearMessages.Text = "Clear";
            this.btnClearMessages.UseVisualStyleBackColor = false;
            this.btnClearMessages.Click += new System.EventHandler(this.btnClearMessages_Click);

            // groupBoxLog - Repositioned
            this.groupBoxLog.Controls.Add(this.lblLog);
            this.groupBoxLog.Controls.Add(this.txtLog);
            this.groupBoxLog.Controls.Add(this.btnClearLog);
            this.groupBoxLog.Location = new System.Drawing.Point(12, 450);
            this.groupBoxLog.Name = "groupBoxLog";
            this.groupBoxLog.Size = new System.Drawing.Size(1028, 150);
            this.groupBoxLog.TabIndex = 3;
            this.groupBoxLog.TabStop = false;
            this.groupBoxLog.Text = "System Log";

            // lblLog
            this.lblLog.AutoSize = true;
            this.lblLog.Location = new System.Drawing.Point(15, 25);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(28, 15);
            this.lblLog.TabIndex = 17;
            this.lblLog.Text = "Log:";

            // txtLog
            this.txtLog.BackColor = System.Drawing.Color.Black;
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtLog.ForeColor = System.Drawing.Color.Lime;
            this.txtLog.Location = new System.Drawing.Point(15, 45);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(950, 80);
            this.txtLog.TabIndex = 18;

            // btnClearLog
            this.btnClearLog.BackColor = System.Drawing.Color.FromArgb(108, 117, 125);
            this.btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearLog.ForeColor = System.Drawing.Color.White;
            this.btnClearLog.Location = new System.Drawing.Point(970, 102);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(55, 23);
            this.btnClearLog.TabIndex = 19;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = false;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);

            // Form1 - Updated size for video support
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1060, 620);
            this.Controls.Add(this.groupBoxConnection);
            this.Controls.Add(this.groupBoxVideo);
            this.Controls.Add(this.groupBoxLocalVideo);
            this.Controls.Add(this.groupBoxRemoteVideo);
            this.Controls.Add(this.groupBoxChat);
            this.Controls.Add(this.groupBoxLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WebRTC Windows Client v2.0 - With Camera Support";
            this.groupBoxConnection.ResumeLayout(false);
            this.groupBoxConnection.PerformLayout();
            this.groupBoxVideo.ResumeLayout(false);
            this.groupBoxVideo.PerformLayout();
            this.groupBoxLocalVideo.ResumeLayout(false);
            this.groupBoxLocalVideo.PerformLayout();
            this.groupBoxRemoteVideo.ResumeLayout(false);
            this.groupBoxRemoteVideo.PerformLayout();
            this.groupBoxChat.ResumeLayout(false);
            this.groupBoxChat.PerformLayout();
            this.groupBoxLog.ResumeLayout(false);
            this.groupBoxLog.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLocalVideo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxRemoteVideo)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TextBox txtRoomId;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtServerUrl;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.TextBox txtMessages;
        private System.Windows.Forms.TextBox txtMessageToSend;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.Button btnClearMessages;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblRoomId;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblServerUrl;
        private System.Windows.Forms.Label lblMessages;
        private System.Windows.Forms.Label lblConnectionCount;
        private System.Windows.Forms.Button btnStartVideo;
        private System.Windows.Forms.Button btnStopVideo;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.GroupBox groupBoxConnection;
        private System.Windows.Forms.GroupBox groupBoxVideo;
        private System.Windows.Forms.GroupBox groupBoxChat;
        private System.Windows.Forms.GroupBox groupBoxLog;

        // Kamera kontrolleri
        private System.Windows.Forms.PictureBox pictureBoxLocalVideo;
        private System.Windows.Forms.PictureBox pictureBoxRemoteVideo;
        private System.Windows.Forms.ComboBox cmbCameraDevices;
        private System.Windows.Forms.Label lblCameraDevice;
        private System.Windows.Forms.Button btnRefreshCameras;
        private System.Windows.Forms.Label lblLocalVideo;
        private System.Windows.Forms.Label lblRemoteVideo;
        private System.Windows.Forms.GroupBox groupBoxLocalVideo;
        private System.Windows.Forms.GroupBox groupBoxRemoteVideo;
    }
}
