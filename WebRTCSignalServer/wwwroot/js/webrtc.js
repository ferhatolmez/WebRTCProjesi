        // ─── WebRTC Signaling ─────────────────────────────────────
        async function handleWebRTCOffer(offerJson, fromConnectionId) {
            try {
                const offer = JSON.parse(offerJson);
                createPeerConnection(fromConnectionId);
                await peerConnection.setRemoteDescription(new RTCSessionDescription(offer));
                const answer = await peerConnection.createAnswer();
                await peerConnection.setLocalDescription(answer);
                await connection.invoke("SendAnswer", currentRoomId, fromConnectionId, JSON.stringify(answer));
                updateUIForCallActive(fromConnectionId);
            } catch (error) {
                logSystem(`Teklif işleme hatası: ${error.message}`, true);
            }
        }

        async function handleWebRTCAnswer(answerJson, fromConnectionId) {
            try {
                const answer = JSON.parse(answerJson);
                await peerConnection.setRemoteDescription(new RTCSessionDescription(answer));
                updateUIForCallActive(fromConnectionId);
            } catch (error) {
                logSystem(`Yanıt işleme hatası: ${error.message}`, true);
            }
        }

        async function handleIceCandidate(candidateJson, fromConnectionId) {
            try {
                const candidate = JSON.parse(candidateJson);
                if (peerConnection) await peerConnection.addIceCandidate(new RTCIceCandidate(candidate));
            } catch { }
        }

        function createPeerConnection(targetConnectionId) {
            if (peerConnection) return;
            peerConnection = new RTCPeerConnection(rtcConfiguration);

            peerConnection.onicecandidate = async event => {
                if (event.candidate) {
                    const candidateJson = JSON.stringify(event.candidate);
                    for (let [connId] of connectedUsers) {
                        if (connId !== connection.connectionId) {
                            await connection.invoke("SendIceCandidate", currentRoomId, connId, candidateJson);
                        }
                    }
                }
            };

            peerConnection.ontrack = event => {
                if (!remoteStream) {
                    remoteStream = new MediaStream();
                    el.remoteVideo.srcObject = remoteStream;
                }
                
                // Add tracks regardless of whether they arrived inside a MediaStream or as discrete tracks 
                if (event.streams && event.streams.length > 0) {
                    event.streams[0].getTracks().forEach(track => remoteStream.addTrack(track));
                } else {
                    remoteStream.addTrack(event.track);
                }

                el.remoteVideo.style.display = "block";
                el.remoteImage.style.display = "none";
                el.emptyVideoState.style.display = "none";
            };

            peerConnection.onconnectionstatechange = () => {
                logSystem(`WebRTC durumu: ${peerConnection.connectionState}`);
                if (peerConnection.connectionState === 'failed' || peerConnection.connectionState === 'disconnected') {
                    logSystem('WebRTC bağlantısı koptu.', true);
                }
            };

            if (localStream) {
                localStream.getTracks().forEach(track => peerConnection.addTrack(track, localStream));
            }
        }

        async function startCall() {
            if (!localStream) { alert("Kamera/Mikrofon erişimi yok!"); return; }
            if (connectedUsers.size === 0) { alert("Odada başka kullanıcı yok."); return; }
            try {
                createPeerConnection();
                const offer = await peerConnection.createOffer();
                await peerConnection.setLocalDescription(offer);
                for (let [connId] of connectedUsers) {
                    if (connId !== connection.connectionId) {
                        await connection.invoke("SendOffer", currentRoomId, connId, JSON.stringify(offer));
                    }
                }
                logSystem("Arama başlatılıyor...");
            } catch (error) {
                logSystem(`Arama hatası: ${error.message}`, true);
            }
        }

        function endCall() {
            if (peerConnection) { peerConnection.close(); peerConnection = null; }
            if (remoteStream) { remoteStream.getTracks().forEach(t => t.stop()); remoteStream = null; }
            el.remoteVideo.srcObject = null;
            el.remoteVideo.style.display = "none";
            el.remoteImage.style.display = "none";
            el.remoteParticipantName.style.display = "none";
            el.emptyVideoState.style.display = "flex";
            el.startCallBtn.style.display = 'flex';
            el.endCallBtn.style.display = 'none';
            if (connectedUsers.size > 0 && localStream) el.startCallBtn.disabled = false;
            logSystem("Görüşme sonlandırıldı.");
        }

        // ─── Media Controls ──────────────────────────────────────
        function toggleAudio() {
            if (!localStream) return;
            isAudioMuted = !isAudioMuted;
            localStream.getAudioTracks().forEach(t => t.enabled = !isAudioMuted);
            el.toggleAudioBtn.innerHTML = `<i data-lucide="${isAudioMuted ? 'mic-off' : 'mic'}" style="width:20px;height:20px;"></i>`;
            el.toggleAudioBtn.classList.toggle('active-danger', isAudioMuted);
            lucide.createIcons();
            logSystem(`Mikrofon ${isAudioMuted ? 'kapatıldı' : 'açıldı'}.`);
        }

        function toggleVideo() {
            if (!localStream) return;
            isVideoMuted = !isVideoMuted;
            localStream.getVideoTracks().forEach(t => t.enabled = !isVideoMuted);
            el.toggleVideoBtn.innerHTML = `<i data-lucide="${isVideoMuted ? 'camera-off' : 'camera'}" style="width:20px;height:20px;"></i>`;
            el.toggleVideoBtn.classList.toggle('active-danger', isVideoMuted);
            lucide.createIcons();
            logSystem(`Kamera ${isVideoMuted ? 'kapatıldı' : 'açıldı'}.`);
        }

        async function toggleScreenShare() {
            if (isScreenSharing) {
                try {
                    const videoStream = await navigator.mediaDevices.getUserMedia({ video: true });
                    const videoTrack = videoStream.getVideoTracks()[0];
                    if (peerConnection) {
                        const sender = peerConnection.getSenders().find(s => s.track && s.track.kind === 'video');
                        if (sender) sender.replaceTrack(videoTrack);
                    }
                    const oldTrack = localStream.getVideoTracks()[0];
                    localStream.removeTrack(oldTrack);
                    oldTrack.stop();
                    localStream.addTrack(videoTrack);
                    el.localVideo.srcObject = localStream;
                    isScreenSharing = false;
                    el.shareScreenBtn.classList.remove('active-primary');
                    logSystem("Ekran paylaşımı durduruldu.");
                } catch (err) {
                    logSystem("Kameraya geri dönülemedi: " + err.message, true);
                }
            } else {
                try {
                    const screenStream = await navigator.mediaDevices.getDisplayMedia({ video: true });
                    const screenTrack = screenStream.getVideoTracks()[0];
                    screenTrack.onended = () => toggleScreenShare();
                    if (peerConnection) {
                        const sender = peerConnection.getSenders().find(s => s.track && s.track.kind === 'video');
                        if (sender) sender.replaceTrack(screenTrack);
                    }
                    const oldTrack = localStream.getVideoTracks()[0];
                    localStream.removeTrack(oldTrack);
                    oldTrack.stop();
                    localStream.addTrack(screenTrack);
                    el.localVideo.srcObject = localStream;
                    isScreenSharing = true;
                    el.shareScreenBtn.classList.add('active-primary');
                    logSystem("Ekran paylaşımı başlatıldı.");
                } catch (err) {
                    logSystem("Ekran paylaşımı iptal edildi.", true);
                }
            }
        }
