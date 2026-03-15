// Initialize Lucide icons
        lucide.createIcons();

        // ─── Theme Toggle ─────────────────────────────────────────
        const themeToggle = document.getElementById('themeToggle');
        const themeIcon = document.getElementById('themeIcon');
        const html = document.documentElement;
        const savedTheme = localStorage.getItem('webrtc-theme');
        if (savedTheme) {
            html.setAttribute('data-theme', savedTheme);
            themeIcon.setAttribute('data-lucide', savedTheme === 'dark' ? 'moon' : 'sun');
            lucide.createIcons();
        }
        themeToggle.addEventListener('click', () => {
            const current = html.getAttribute('data-theme');
            const next = current === 'dark' ? 'light' : 'dark';
            html.setAttribute('data-theme', next);
            themeIcon.setAttribute('data-lucide', next === 'dark' ? 'moon' : 'sun');
            localStorage.setItem('webrtc-theme', next);
            lucide.createIcons();
        });

        // ─── State Variables ──────────────────────────────────────
        let connection = null;
        let localStream = null;
        let remoteStream = null;
        let peerConnection = null;
        let currentRoomId = '';
        let currentUserName = '';
        let connectedUsers = new Map();
        let isAudioMuted = false;
        let isVideoMuted = false;
        let isScreenSharing = false;

        const rtcConfiguration = {
            iceServers: [
                { urls: 'stun:stun.l.google.com:19302' },
                { urls: 'stun:stun1.l.google.com:19302' }
            ]
        };

        // ─── DOM Elements ─────────────────────────────────────────
        const el = {
            roomId: document.getElementById('roomId'),
            userName: document.getElementById('userName'),
            connectBtn: document.getElementById('connectBtn'),
            disconnectBtn: document.getElementById('disconnectBtn'),
            status: document.getElementById('status'),
            reconnectBanner: document.getElementById('reconnectBanner'),

            localVideo: document.getElementById('localVideo'),
            localVideoContainer: document.getElementById('localVideoContainer'),
            remoteVideo: document.getElementById('remoteVideo'),
            remoteImage: document.getElementById('remoteImage'),
            remoteParticipantName: document.getElementById('remoteParticipantName'),
            remoteNameText: document.getElementById('remoteNameText'),
            emptyVideoState: document.getElementById('emptyVideoState'),

            toggleAudioBtn: document.getElementById('toggleAudioBtn'),
            toggleVideoBtn: document.getElementById('toggleVideoBtn'),
            shareScreenBtn: document.getElementById('shareScreenBtn'),
            startCallBtn: document.getElementById('startCallBtn'),
            endCallBtn: document.getElementById('endCallBtn'),

            chatMessages: document.getElementById('chatMessages'),
            messageInput: document.getElementById('messageInput'),
            sendMessageBtn: document.getElementById('sendMessageBtn'),
            systemLogs: document.getElementById('systemLogs'),

            participantList: document.getElementById('participantList'),
            participantCount: document.getElementById('participantCount'),
            noParticipants: document.getElementById('noParticipants'),
        };
