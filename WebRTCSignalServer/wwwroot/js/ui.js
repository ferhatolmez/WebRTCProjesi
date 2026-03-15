        // ─── Notification Sound ───────────────────────────────────
        function playNotificationSound() {
            try {
                const ctx = new (window.AudioContext || window.webkitAudioContext)();
                const oscillator = ctx.createOscillator();
                const gainNode = ctx.createGain();
                oscillator.connect(gainNode);
                gainNode.connect(ctx.destination);
                oscillator.frequency.value = 800;
                oscillator.type = 'sine';
                gainNode.gain.value = 0.1;
                oscillator.start();
                gainNode.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.3);
                oscillator.stop(ctx.currentTime + 0.3);
            } catch { }
        }

        // ─── Participant List Management ──────────────────────────
        function updateParticipantList() {
            el.participantCount.textContent = connectedUsers.size;
            if (connectedUsers.size === 0) {
                el.participantList.innerHTML = '';
                const noP = document.createElement('div');
                noP.className = 'no-participants';
                noP.innerHTML = '<i data-lucide="user-x" style="width:20px;height:20px;color:var(--text-muted)"></i>Henüz kimse yok';
                el.participantList.appendChild(noP);
                lucide.createIcons();
                return;
            }

            el.participantList.innerHTML = '';
            // Add self first
            if (currentUserName) {
                const selfDiv = document.createElement('div');
                selfDiv.className = 'participant-item';
                const initials = currentUserName.substring(0, 2).toUpperCase();
                selfDiv.innerHTML = `
                    <div class="participant-avatar">${initials}</div>
                    <div class="participant-info">
                        <div class="name">${escapeHtml(currentUserName)} (Sen)</div>
                        <div class="type">WebClient</div>
                    </div>
                    <div class="participant-status"></div>`;
                el.participantList.appendChild(selfDiv);
            }

            connectedUsers.forEach((info, connId) => {
                const div = document.createElement('div');
                div.className = 'participant-item';
                div.id = `participant-${connId}`;
                const initials = info.userName.substring(0, 2).toUpperCase();
                div.innerHTML = `
                    <div class="participant-avatar">${initials}</div>
                    <div class="participant-info">
                        <div class="name">${escapeHtml(info.userName)}</div>
                        <div class="type">${escapeHtml(info.userType)}</div>
                    </div>
                    <div class="participant-status"></div>`;
                el.participantList.appendChild(div);
            });
            lucide.createIcons();
        }

        // ─── XSS Protection ──────────────────────────────────────
        function escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }

        // ─── Chat ─────────────────────────────────────────────────
        async function sendMessage() {
            const message = el.messageInput.value.trim();
            if (!message) return;
            if (!connection || connection.state !== 'Connected') { alert('Bağlı değilsiniz!'); return; }
            const timestamp = new Date().toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
            try {
                await connection.invoke("SendMessage", currentRoomId, currentUserName, message, timestamp);
                el.messageInput.value = '';
            } catch (error) {
                logSystem(`Mesaj gönderilemedi: ${error.message}`, true);
            }
        }

        function addChatMessage(sender, message, isSystem = false, timestamp = null) {
            const container = el.chatMessages;
            const msgDiv = document.createElement('div');
            msgDiv.className = 'message';

            if (isSystem) {
                msgDiv.classList.add('system');
                const bubble = document.createElement('div');
                bubble.className = 'msg-bubble';
                bubble.textContent = message;  // XSS-safe
                msgDiv.appendChild(bubble);
            } else {
                const timeStr = timestamp || new Date().toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
                const isSelf = sender === currentUserName;
                msgDiv.classList.add(isSelf ? 'self' : 'other');

                const meta = document.createElement('div');
                meta.className = 'msg-meta';
                if (!isSelf) {
                    const nameSpan = document.createElement('span');
                    nameSpan.textContent = sender;
                    meta.appendChild(nameSpan);
                }
                const timeSpan = document.createElement('span');
                timeSpan.textContent = timeStr;
                meta.appendChild(timeSpan);

                const bubble = document.createElement('div');
                bubble.className = 'msg-bubble';
                bubble.textContent = message;  // XSS-safe

                msgDiv.appendChild(meta);
                msgDiv.appendChild(bubble);
            }

            container.appendChild(msgDiv);
            container.scrollTop = container.scrollHeight;
        }

        function logSystem(msg, isError = false) {
            const container = el.systemLogs;
            const time = new Date().toLocaleTimeString('tr-TR');
            const logItem = document.createElement('div');
            logItem.className = 'log-item' + (isError ? ' error' : '');
            logItem.textContent = `[${time}] ${msg}`;
            container.appendChild(logItem);
            container.scrollTop = container.scrollHeight;
        }

        // ─── UI State Management ──────────────────────────────────
        function updateUIForConnected() {
            el.connectBtn.style.display = 'none';
            el.disconnectBtn.style.display = 'flex';
            el.roomId.disabled = true;
            el.userName.disabled = true;
            el.status.textContent = 'Bağlı';
            el.status.className = 'status-badge connected';
            el.messageInput.disabled = false;
            el.sendMessageBtn.disabled = false;
            updateParticipantList();
        }

        function updateUIForDisconnected() {
            el.connectBtn.style.display = 'flex';
            el.disconnectBtn.style.display = 'none';
            el.roomId.disabled = false;
            el.userName.disabled = false;
            el.status.textContent = 'Bağlı Değil';
            el.status.className = 'status-badge disconnected';
            el.startCallBtn.disabled = true;
            el.startCallBtn.style.display = 'flex';
            el.endCallBtn.style.display = 'none';
            el.messageInput.disabled = true;
            el.sendMessageBtn.disabled = true;
            el.toggleAudioBtn.disabled = true;
            el.toggleVideoBtn.disabled = true;
            el.shareScreenBtn.disabled = true;
            el.emptyVideoState.style.display = 'flex';
            el.remoteVideo.style.display = 'none';

            isAudioMuted = false;
            isVideoMuted = false;
            el.toggleAudioBtn.innerHTML = '<i data-lucide="mic" style="width:20px;height:20px;"></i>';
            el.toggleVideoBtn.innerHTML = '<i data-lucide="camera" style="width:20px;height:20px;"></i>';
            el.toggleAudioBtn.classList.remove('active-danger');
            el.toggleVideoBtn.classList.remove('active-danger');
            el.shareScreenBtn.classList.remove('active-primary');
            lucide.createIcons();

            el.chatMessages.innerHTML = '<div class="message system"><div class="msg-bubble">Odaya katılarak sohbete başlayın.</div></div>';
            connectedUsers.clear();
            updateParticipantList();
        }

        function updateUIForCallActive(fromConnectionId) {
            el.startCallBtn.style.display = 'none';
            el.endCallBtn.style.display = 'flex';
            el.emptyVideoState.style.display = 'none';
            const user = connectedUsers.get(fromConnectionId);
            if (user) {
                el.remoteParticipantName.style.display = 'flex';
                el.remoteNameText.textContent = user.userName;
            }
        }