        // ─── Connect / Disconnect ─────────────────────────────────
        async function connectToServer() {
            if (!el.roomId.value.trim() || !el.userName.value.trim()) {
                alert('Lütfen Oda ID ve Kullanıcı Adı alanlarını doldurun!');
                return;
            }
            try {
                connection = new signalR.HubConnectionBuilder()
                    .withUrl("http://localhost:5050/webrtchub")
                    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
                    .configureLogging(signalR.LogLevel.Warning)
                    .build();

                setupSignalRHandlers();
                setupReconnectionHandlers();
                await connection.start();

                currentRoomId = el.roomId.value.trim();
                currentUserName = el.userName.value.trim();
                await connection.invoke("JoinRoom", currentRoomId, currentUserName, "WebClient");

                updateUIForConnected();
                logSystem(`Bağlantı başarılı. Oda: ${currentRoomId}`);
                await startLocalMedia();
            } catch (error) {
                logSystem(`Bağlantı hatası: ${error.message}`, true);
                alert(`Bağlantı başarısız: ${error.message}`);
            }
        }

        function setupReconnectionHandlers() {
            connection.onreconnecting(() => {
                el.reconnectBanner.classList.add('visible');
                el.status.textContent = 'Yeniden Bağlanıyor';
                el.status.className = 'status-badge disconnected';
                logSystem('Bağlantı kesildi, yeniden bağlanılıyor...', true);
            });
            connection.onreconnected(() => {
                el.reconnectBanner.classList.remove('visible');
                el.status.textContent = 'Bağlı';
                el.status.className = 'status-badge connected';
                logSystem('Yeniden bağlantı başarılı!');
                // Re-join room
                connection.invoke("JoinRoom", currentRoomId, currentUserName, "WebClient");
            });
            connection.onclose(() => {
                el.reconnectBanner.classList.remove('visible');
                logSystem('Bağlantı tamamen kesildi.', true);
                updateUIForDisconnected();
            });
        }

        async function startLocalMedia() {
            try {
                localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
                el.localVideo.srcObject = localStream;
                el.localVideoContainer.style.display = 'block';
                el.toggleAudioBtn.disabled = false;
                el.toggleVideoBtn.disabled = false;
                el.shareScreenBtn.disabled = false;
                if (connectedUsers.size > 0) el.startCallBtn.disabled = false;
                logSystem("Kamera ve mikrofon erişimi sağlandı.");
            } catch (error) {
                logSystem(`Medya cihazlarına erişilemedi: ${error.message}`, true);
            }
        }

        function setupSignalRHandlers() {
            if (!connection) return;

            connection.on("UserJoined", (userName, userType, connectionId) => {
                connectedUsers.set(connectionId, { userName, userType });
                addChatMessage("SİSTEM", `${userName} (${userType}) odaya katıldı.`, true);
                logSystem(`Kullanıcı katıldı: ${userName}`);
                playNotificationSound();
                updateParticipantList();
                if (localStream) el.startCallBtn.disabled = false;
            });

            connection.on("UserLeft", (userName, connectionId) => {
                if (connectedUsers.has(connectionId)) {
                    connectedUsers.delete(connectionId);
                    addChatMessage("SİSTEM", `${userName} odadan ayrıldı.`, true);
                    logSystem(`Kullanıcı ayrıldı: ${userName}`);
                    updateParticipantList();
                    if (connectedUsers.size === 0) {
                        el.startCallBtn.disabled = true;
                        if (peerConnection) endCall();
                    }
                }
            });

            connection.on("UserDisconnected", connectionId => {
                if (connectedUsers.has(connectionId)) {
                    const user = connectedUsers.get(connectionId);
                    connectedUsers.delete(connectionId);
                    addChatMessage("SİSTEM", `${user.userName} bağlantısı koptu.`, true);
                    logSystem(`Bağlantı koptu: ${user.userName}`);
                    updateParticipantList();
                    if (connectedUsers.size === 0) {
                        el.startCallBtn.disabled = true;
                        if (peerConnection) endCall();
                    }
                }
            });

            connection.on("ReceiveMessage", (userName, message, timestamp) => {
                const isSystem = userName === "SİSTEM";
                addChatMessage(userName, message, isSystem, timestamp);
                if (userName !== currentUserName && !isSystem) playNotificationSound();
            });

            connection.on("ReceiveOffer", async (offer, fromConnectionId) => {
                logSystem(`Arama teklifi alındı.`);
                await handleWebRTCOffer(offer, fromConnectionId);
            });

            connection.on("ReceiveAnswer", async (answer, fromConnectionId) => {
                logSystem(`Arama yanıtı alındı.`);
                await handleWebRTCAnswer(answer, fromConnectionId);
            });

            connection.on("ReceiveIceCandidate", async (candidate, fromConnectionId) => {
                await handleIceCandidate(candidate, fromConnectionId);
            });

            connection.on("ReceiveVideoFrame", (frameData, fromConnectionId) => {
                el.remoteImage.src = "data:image/jpeg;base64," + frameData;
                el.remoteImage.style.display = "block";
                el.remoteVideo.style.display = "none";
                el.emptyVideoState.style.display = "none";
                const user = connectedUsers.get(fromConnectionId);
                if (user) {
                    el.remoteParticipantName.style.display = 'flex';
                    el.remoteNameText.textContent = user.userName + " (Windows)";
                }
            });

            connection.on("VideoCallStopped", (connectionId) => {
                logSystem("Karşı taraf video aramasını durdurdu.");
                el.remoteImage.style.display = "none";
                el.remoteParticipantName.style.display = 'none';
                el.emptyVideoState.style.display = "flex";
            });
        }

        // ─── Connection Cleanup ───────────────────────────────────
        async function disconnectFromServer() {
            try {
                if (connection && connection.state === "Connected") {
                    await connection.invoke("LeaveRoom", currentRoomId, currentUserName);
                    await connection.stop();
                }
                connection = null;
                connectedUsers.clear();
                endCall();
                if (localStream) { localStream.getTracks().forEach(t => t.stop()); localStream = null; }
                el.localVideo.srcObject = null;
                el.localVideoContainer.style.display = 'none';
                updateUIForDisconnected();
                logSystem("Sunucu bağlantısı kesildi.");
                addChatMessage("SİSTEM", "Odadan ayrıldınız.", true);
            } catch (error) {
                logSystem(`Bağlantı kesme hatası: ${error.message}`, true);
            }
        }
