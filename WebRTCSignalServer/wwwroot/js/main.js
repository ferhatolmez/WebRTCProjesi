        // ─── Event Listeners ──────────────────────────────────────
        el.connectBtn.addEventListener('click', connectToServer);
        el.disconnectBtn.addEventListener('click', disconnectFromServer);
        el.startCallBtn.addEventListener('click', startCall);
        el.endCallBtn.addEventListener('click', endCall);
        el.sendMessageBtn.addEventListener('click', sendMessage);
        el.messageInput.addEventListener('keypress', e => { if (e.key === 'Enter') { e.preventDefault(); sendMessage(); } });
        el.toggleAudioBtn.addEventListener('click', toggleAudio);
        el.toggleVideoBtn.addEventListener('click', toggleVideo);
        el.shareScreenBtn.addEventListener('click', toggleScreenShare);
