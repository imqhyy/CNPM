/**
 * AutoVoiceOver - User Web App Engine
 * Mô phỏng di chuyển du khách, Nhận diện khoảng cách Euclid & Thuyết minh tự động đa ngôn ngữ
 */

// ==========================================
// 1. Khởi tạo State & Biến toàn cục
// ==========================================
const state = {
    nickname: 'Du khách',
    currentLang: 'vi',
    currentMap: null,
    spots: [],
    
    // Tọa độ hiện tại của du khách
    userPos: { x: 250, y: 350 },
    targetPos: { x: 250, y: 350 },
    isMoving: false,

    // Trạng thái thuyết minh
    activeSpotId: null,
    activeNarration: null,
    isPlaying: false,

    // Bản đồ hình ảnh
    mapImage: null
};

// Canvas & Audio elements
const canvas = document.getElementById('userMapCanvas');
const ctx = canvas ? canvas.getContext('2d') : null;
const audioEl = document.getElementById('nativeAudioElement');
let signalrConnection = null;
let animationFrameId = null;

// ==========================================
// SoundEngine: Động cơ Web Audio API chuyên dụng cho iOS Safari & Android
// Giải quyết triệt để lỗi Range 206, blob: URL của WebKit và chính sách Autoplay
// ==========================================
const SoundEngine = {
    ctx: null,
    currentBuffer: null,
    currentSource: null,
    startedAt: 0,
    pausedAt: 0,
    isPlaying: false,
    timerId: null,

    init() {
        if (!this.ctx) {
            const AudioCtxClass = window.AudioContext || window.webkitAudioContext;
            if (AudioCtxClass) {
                this.ctx = new AudioCtxClass();
            }
        }
        if (this.ctx && this.ctx.state === 'suspended') {
            this.ctx.resume().catch(() => {});
        }
    },

    async load(url) {
        this.init();
        if (!url) return null;
        try {
            const res = await fetch(url, {
                headers: { 'ngrok-skip-browser-warning': 'true' }
            });
            if (!res.ok) {
                console.warn(`HTTP ${res.status} when fetching audio: ${url}`);
                return null;
            }
            const arrayBuffer = await res.arrayBuffer();
            const decodedBuffer = await new Promise((resolve, reject) => {
                this.ctx.decodeAudioData(arrayBuffer, resolve, reject);
            });
            this.currentBuffer = decodedBuffer;
            this.pausedAt = 0;
            return decodedBuffer;
        } catch (err) {
            console.error('Lỗi nạp & giải mã Web Audio:', err);
            return null;
        }
    },

    play(offset = 0) {
        this.init();
        if (!this.currentBuffer) return;

        if (this.currentSource) {
            try { this.currentSource.stop(); } catch(e) {}
            this.currentSource.disconnect();
            this.currentSource = null;
        }

        const source = this.ctx.createBufferSource();
        source.buffer = this.currentBuffer;
        source.connect(this.ctx.destination);

        const duration = this.currentBuffer.duration;
        const startOffset = Math.max(0, Math.min(offset, duration));

        source.onended = () => {
            if (this.isPlaying && this.getCurrentTime() >= duration - 0.2) {
                this.isPlaying = false;
                this.pausedAt = 0;
                this.stopProgressTicker();
                state.isPlaying = false;
                updatePlayPauseIcon();
                const desc = document.getElementById('proximityDesc');
                if (desc) desc.innerText = 'Đã phát xong thuyết minh.';
                const btnPlay = document.getElementById('btnProximityPlay');
                if (btnPlay) {
                    btnPlay.style.display = 'flex';
                    btnPlay.innerHTML = '<i class="fa-solid fa-rotate-right"></i> Nghe lại';
                }
            }
        };

        source.start(0, startOffset);
        this.currentSource = source;
        this.startedAt = this.ctx.currentTime - startOffset;
        this.isPlaying = true;
        this.pausedAt = 0;
        state.isPlaying = true;
        updatePlayPauseIcon();
        this.startProgressTicker();
    },

    pause() {
        if (!this.isPlaying) return;
        this.pausedAt = this.getCurrentTime();
        if (this.currentSource) {
            try { this.currentSource.stop(); } catch(e) {}
            this.currentSource.disconnect();
            this.currentSource = null;
        }
        this.isPlaying = false;
        state.isPlaying = false;
        updatePlayPauseIcon();
        this.stopProgressTicker();
    },

    resume() {
        if (this.isPlaying) return;
        this.play(this.pausedAt);
    },

    seek(time) {
        if (!this.currentBuffer) return;
        const duration = this.currentBuffer.duration;
        const target = Math.max(0, Math.min(time, duration));
        if (this.isPlaying) {
            this.play(target);
        } else {
            this.pausedAt = target;
            updateDockProgress(target, duration);
        }
    },

    seekRelative(deltaSeconds) {
        const cur = this.getCurrentTime();
        this.seek(cur + deltaSeconds);
    },

    getCurrentTime() {
        if (!this.currentBuffer) return 0;
        if (this.isPlaying && this.ctx) {
            return Math.min(this.ctx.currentTime - this.startedAt, this.currentBuffer.duration);
        }
        return this.pausedAt;
    },

    getDuration() {
        return this.currentBuffer ? this.currentBuffer.duration : 0;
    },

    startProgressTicker() {
        this.stopProgressTicker();
        this.timerId = setInterval(() => {
            if (this.isPlaying) {
                updateDockProgress(this.getCurrentTime(), this.getDuration());
            }
        }, 100);
    },

    stopProgressTicker() {
        if (this.timerId) {
            clearInterval(this.timerId);
            this.timerId = null;
        }
    }
};

function updateDockProgress(cur, dur) {
    const curTimeLabel = document.getElementById('currentTimeLabel');
    const totalTimeLabel = document.getElementById('totalDurationLabel');
    const scrubberFill = document.getElementById('scrubberFill');
    const scrubberThumb = document.getElementById('scrubberThumb');

    if (curTimeLabel) curTimeLabel.innerText = formatTime(cur);
    if (totalTimeLabel && dur > 0) totalTimeLabel.innerText = formatTime(dur);

    if (dur > 0 && scrubberFill && scrubberThumb) {
        const percent = Math.min(100, Math.max(0, (cur / dur) * 100));
        scrubberFill.style.width = `${percent}%`;
        scrubberThumb.style.left = `${percent}%`;
    }
}

// Lắng nghe cử chỉ chạm đầu tiên trên màn hình để giữ ấm và mở khóa AudioContext của iOS Safari
['touchstart', 'touchend', 'click'].forEach(evtType => {
    document.addEventListener(evtType, () => {
        if (SoundEngine.ctx && SoundEngine.ctx.state === 'suspended') {
            SoundEngine.ctx.resume().catch(() => {});
        }
    }, { passive: true });
});

// ==========================================
// 2. DOM Ready
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
    initWelcomeFlow();
    initLanguageSelector();
    initCanvasTouchEvents();
    initAudioPlayerControls();
    initNarrationSheet();
    initHelpModal();

    // Tự động kiểm tra nếu du khách đã từng nhập tên trong phiên
    const savedName = sessionStorage.getItem('visitor_name');
    const savedLang = sessionStorage.getItem('visitor_lang');
    if (savedName) {
        state.nickname = savedName;
        state.currentLang = savedLang || 'vi';
        document.getElementById('inputNickname').value = savedName;
        document.getElementById('welcomeLangSelect').value = state.currentLang;
    }

    // Tải trước bản đồ & điểm tham quan để sẵn sàng ngay lập tức (đặc biệt cho Safari iOS)
    loadMapAndSpots().then(() => {
        resizeCanvas();
        startCanvasLoop();
    });

    window.addEventListener('resize', resizeCanvas);
    window.addEventListener('orientationchange', () => setTimeout(resizeCanvas, 200));
});

function resizeCanvas() {
    const viewport = document.getElementById('mapViewport');
    const stage = document.getElementById('mapStage');
    if (!viewport || !stage || !canvas) return;
    const vWidth = viewport.clientWidth;
    const vHeight = viewport.clientHeight;
    if (vWidth === 0 || vHeight === 0) return;

    const scale = Math.min(vWidth / 1200, vHeight / 800);
    const w = Math.floor(1200 * scale);
    const h = Math.floor(800 * scale);
    stage.style.width = `${w}px`;
    stage.style.height = `${h}px`;
}

// ==========================================
// 3. Welcome Flow & Khởi tạo phiên
// ==========================================
function initWelcomeFlow() {
    const welcomeForm = document.getElementById('formWelcome');
    welcomeForm?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const inputName = document.getElementById('inputNickname').value.trim();
        const inputLang = document.getElementById('welcomeLangSelect').value;

        state.nickname = inputName || 'Du khách';
        state.currentLang = inputLang || 'vi';

        sessionStorage.setItem('visitor_name', state.nickname);
        sessionStorage.setItem('visitor_lang', state.currentLang);

        updateHeaderUI();
        closeModal('welcomeModal');

        // Căn lại kích thước canvas
        resizeCanvas();

        // Mở khóa âm thanh Web Audio & Native Audio trong cử chỉ submit
        SoundEngine.init();
        unlockMobileAudio();

        // Kết nối SignalR với vai trò Du Khách
        initSignalR();
    });
}

function unlockMobileAudio() {
    if (audioEl) {
        audioEl.src = 'data:audio/wav;base64,UklGRiQAAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YQAAAAA=';
        audioEl.play().catch(() => {});
    }
}

function updateHeaderUI() {
    const nameEl = document.getElementById('displayNickname');
    const langLabel = document.getElementById('currentLangLabel');
    if (nameEl) nameEl.innerText = state.nickname;
    if (langLabel) langLabel.innerText = state.currentLang.toUpperCase();
}

// ==========================================
// 4. Kết nối SignalR Hub (Du khách tham gia)
// ==========================================
function initSignalR() {
    if (signalrConnection) {
        if (signalrConnection.state === signalR.HubConnectionState.Connected) {
            signalrConnection.invoke('ThamGiaPhien', state.nickname)
                .then(() => signalrConnection.invoke('CapNhatToaDo', state.userPos.x, state.userPos.y))
                .catch(console.error);
        }
        return;
    }

    try {
        signalrConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hub/theodoi')
            .withAutomaticReconnect()
            .build();

        signalrConnection.start()
            .then(async () => {
                console.log('SignalR connected as Visitor');
                // 1. Gửi nickname lên máy chủ trước
                await signalrConnection.invoke('ThamGiaPhien', state.nickname);
                // 2. Gửi tọa độ ban đầu
                await signalrConnection.invoke('CapNhatToaDo', state.userPos.x, state.userPos.y);
            })
            .catch(err => console.warn('SignalR connection failed:', err));

        signalrConnection.onreconnected(async () => {
            await signalrConnection.invoke('ThamGiaPhien', state.nickname).catch(console.error);
            await signalrConnection.invoke('CapNhatToaDo', state.userPos.x, state.userPos.y).catch(console.error);
        });

        // Nhịp tim định kỳ duy trì tọa độ trên Radar Admin khi du khách đứng yên (không gửi khi đang di chuyển)
        if (!window.__signalrHeartbeatInterval) {
            window.__signalrHeartbeatInterval = setInterval(() => {
                if (!state.isMoving && signalrConnection && signalrConnection.state === signalR.HubConnectionState.Connected) {
                    signalrConnection.invoke('CapNhatToaDo', state.userPos.x, state.userPos.y).catch(() => {});
                }
            }, 5000);
        }

    } catch (e) {
        console.error('SignalR init error:', e);
    }
}

// ==========================================
// Helper: Tải ảnh qua Blob -> Base64 Data URL
// Giúp iOS Safari & Chrome không bao giờ bị ngrok trả về trang cảnh báo HTML
// và hoàn toàn miễn nhiễm với lỗi giải mã trên WebKit ITP
// ==========================================
async function loadMapImage(url) {
    if (!url) return null;
    try {
        const res = await fetch(url, {
            headers: { 'ngrok-skip-browser-warning': 'true' }
        });
        if (res.ok) {
            const blob = await res.blob();
            const dataUrl = await new Promise((resolve, reject) => {
                const reader = new FileReader();
                reader.onloadend = () => resolve(reader.result);
                reader.onerror = reject;
                reader.readAsDataURL(blob);
            });

            const img = new Image();
            await new Promise((resolve, reject) => {
                img.onload = () => resolve(img);
                img.onerror = reject;
                img.src = dataUrl;
            });
            return img;
        }
    } catch (err) {
        console.warn('Không tải được map qua Data URL, thử nạp trực tiếp:', err);
    }

    // Fallback: nạp trực tiếp
    return new Promise((resolve) => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = () => resolve(null);
        img.src = url;
    });
}

async function fetchAssetBlobUrl(url) {
    if (!url) return '';
    if (url.startsWith('data:') || url.startsWith('blob:')) return url;

    try {
        const res = await fetch(url, {
            headers: { 'ngrok-skip-browser-warning': 'true' }
        });
        if (!res.ok) {
            console.warn(`HTTP ${res.status} when fetching asset: ${url}`);
            return url;
        }
        const blob = await res.blob();
        return new Promise((resolve) => {
            const reader = new FileReader();
            reader.onloadend = () => resolve(reader.result);
            reader.onerror = () => resolve(url);
            reader.readAsDataURL(blob);
        });
    } catch (err) {
        console.warn('Lỗi khi fetch asset blob:', url, err);
        return url;
    }
}

// ==========================================
// 5. Tải dữ liệu Bản đồ & Điểm tham quan từ API
// ==========================================
async function loadMapAndSpots() {
    try {
        const fetchOpts = {
            headers: { 'ngrok-skip-browser-warning': 'true' }
        };

        // 1. Tải bản đồ: GET /api/DuKhach/bando
        const resMap = await fetch('/api/DuKhach/bando', fetchOpts);
        if (resMap.ok) {
            state.currentMap = await resMap.json();
            const mapTitleHeader = document.getElementById('mapTitleHeader');
            if (mapTitleHeader && state.currentMap.tenBanDo) {
                mapTitleHeader.innerText = state.currentMap.tenBanDo;
            }

            const mapUrl = state.currentMap.hinhAnhBanDo || '/uploads/maps/default_map.jpg';
            const loadedImg = await loadMapImage(mapUrl);
            if (loadedImg) {
                state.mapImage = loadedImg;
                const bgImg = document.getElementById('mapBackgroundImage');
                if (bgImg) bgImg.src = loadedImg.src;
                renderMap();
            }
        }

        // 2. Tải danh sách điểm: GET /api/DuKhach/diadiem
        const resSpots = await fetch('/api/DuKhach/diadiem', fetchOpts);
        if (resSpots.ok) {
            state.spots = await resSpots.json();
            console.log('Loaded spots:', state.spots.length);
            renderMap();
        }
    } catch (err) {
        console.error('Failed to load map/spots:', err);
    }
}

// ==========================================
// 6. Canvas Rendering & Mô phỏng Di chuyển
// ==========================================
function startCanvasLoop() {
    function animate() {
        updateMovement();
        renderMap();
        animationFrameId = requestAnimationFrame(animate);
    }
    if (animationFrameId) cancelAnimationFrame(animationFrameId);
    animate();
}

let lastServerCoordUpdateTime = 0;

function updateMovement() {
    const dx = state.targetPos.x - state.userPos.x;
    const dy = state.targetPos.y - state.userPos.y;
    const distance = Math.hypot(dx, dy);

    if (distance > 1.5) {
        state.isMoving = true;
        const speed = Math.min(distance * 0.1, 7.0);
        state.userPos.x += (dx / distance) * speed;
        state.userPos.y += (dy / distance) * speed;

        // Cập nhật tọa độ hiển thị trên giao diện người dùng
        const coordsEl = document.getElementById('currentCoords');
        if (coordsEl) {
            coordsEl.innerText = `(${state.userPos.x.toFixed(0)}, ${state.userPos.y.toFixed(0)})`;
        }

        // Tiết lưu (Throttle) gửi tọa độ lên máy chủ: tối đa 1 lần mỗi 150ms để tối ưu băng thông và chống nghẽn SignalR
        const now = Date.now();
        if (now - lastServerCoordUpdateTime > 150) {
            lastServerCoordUpdateTime = now;
            if (signalrConnection && signalrConnection.state === signalR.HubConnectionState.Connected) {
                signalrConnection.invoke('CapNhatToaDo', state.userPos.x, state.userPos.y).catch(() => {});
            }
        }

        // Kiểm tra xem vị trí mới có nằm trong bán kính điểm tham quan nào không
        checkProximity(state.userPos.x, state.userPos.y);

    } else if (state.isMoving) {
        state.isMoving = false;
        state.userPos.x = state.targetPos.x;
        state.userPos.y = state.targetPos.y;
        lastServerCoordUpdateTime = Date.now();

        // Chốt tọa độ điểm dừng chân cuối cùng lên Server
        if (signalrConnection && signalrConnection.state === signalR.HubConnectionState.Connected) {
            signalrConnection.invoke('CapNhatToaDo', state.userPos.x, state.userPos.y).catch(() => {});
        }

        checkProximity(state.userPos.x, state.userPos.y);
    }
}

function drawHeritageVectorMap(ctx, width, height) {
    // 1. Nền tổng thể khuôn viên Cấm Thành (Vibrant dark slate)
    ctx.fillStyle = '#0F172A';
    ctx.fillRect(0, 0, width, height);

    // Hào nước bao quanh thành (Moat / Water Canal)
    ctx.fillStyle = '#0369A1';
    ctx.fillRect(30, 30, width - 60, height - 60);

    // Bờ trong hào nước
    ctx.fillStyle = '#0F172A';
    ctx.fillRect(54, 54, width - 108, height - 108);

    // 2. Tường thành cổ bao quanh (Ancient Citadel Wall)
    ctx.strokeStyle = '#D97706';
    ctx.lineWidth = 6;
    ctx.strokeRect(60, 60, width - 120, height - 120);

    // 4 Tháp canh góc thành
    const towers = [
        [60, 60], [width - 60, 60], [60, height - 60], [width - 60, height - 60]
    ];
    towers.forEach(([tx, ty]) => {
        ctx.fillStyle = '#B45309';
        ctx.fillRect(tx - 18, ty - 18, 36, 36);
        ctx.strokeStyle = '#FCD34D';
        ctx.lineWidth = 2.5;
        ctx.strokeRect(tx - 18, ty - 18, 36, 36);
    });

    // 3. Vườn cỏ di tích xanh mướt (Lush Heritage Lawns)
    ctx.fillStyle = '#14532D';
    ctx.fillRect(80, 80, width - 160, height - 160);

    // Họa tiết đốm cỏ sáng màu
    ctx.fillStyle = '#166534';
    for (let lx = 100; lx < width - 100; lx += 80) {
        for (let ly = 100; ly < height - 100; ly += 70) {
            ctx.beginPath();
            ctx.arc(lx, ly, 18, 0, Math.PI * 2);
            ctx.fill();
        }
    }

    // 4. Trục Thần Đạo chính Bắc - Nam (Paved Sacred Way)
    ctx.fillStyle = '#334155';
    ctx.fillRect(530, 60, 140, height - 120);
    ctx.strokeStyle = '#64748B';
    ctx.lineWidth = 2;
    ctx.strokeRect(530, 60, 140, height - 120);

    // Đường ngang Đông - Tây
    ctx.fillRect(80, 350, width - 160, 90);
    ctx.strokeRect(80, 350, width - 160, 90);

    // Họa tiết gạch lát đá trục chính
    ctx.strokeStyle = 'rgba(255, 255, 255, 0.15)';
    ctx.lineWidth = 1.5;
    for (let py = 80; py < height - 80; py += 35) {
        ctx.beginPath(); ctx.moveTo(530, py); ctx.lineTo(670, py); ctx.stroke();
    }

    // 5. Khuôn viên Cổng Đoan Môn
    ctx.fillStyle = '#1E293B';
    ctx.fillRect(360, 230, 180, 180);
    ctx.strokeStyle = '#F59E0B';
    ctx.lineWidth = 2.5;
    ctx.strokeRect(360, 230, 180, 180);
    ctx.fillStyle = 'rgba(245, 158, 11, 0.18)';
    ctx.fillRect(375, 245, 150, 150);

    ctx.font = 'bold 12px "Plus Jakarta Sans", sans-serif';
    ctx.fillStyle = '#FCD34D';
    ctx.textAlign = 'center';
    ctx.fillText('KHU VỰC CỔNG ĐOAN MÔN', 450, 400);

    // 6. Khuôn viên Thềm Rồng Điện Kính Thiên
    ctx.fillStyle = '#1E293B';
    ctx.fillRect(750, 440, 200, 200);
    ctx.strokeStyle = '#A855F7';
    ctx.lineWidth = 2.5;
    ctx.strokeRect(750, 440, 200, 200);
    ctx.fillStyle = 'rgba(168, 85, 247, 0.18)';
    ctx.fillRect(765, 455, 170, 170);

    ctx.fillStyle = '#D8B4FE';
    ctx.fillText('KHU VỰC ĐIỆN KÍNH THIÊN', 850, 630);

    // 7. Hồ sen cổ phong thủy (Lotus Pond)
    ctx.fillStyle = '#0284C7';
    ctx.beginPath();
    ctx.ellipse(220, 200, 95, 65, 0, 0, Math.PI * 2);
    ctx.fill();
    ctx.strokeStyle = '#38BDF8';
    ctx.lineWidth = 2;
    ctx.stroke();

    // Lá sen
    ctx.fillStyle = '#10B981';
    [ [190, 180], [240, 190], [210, 220], [250, 215] ].forEach(([lx, ly]) => {
        ctx.beginPath(); ctx.arc(lx, ly, 10, 0, Math.PI * 2); ctx.fill();
    });

    ctx.fillStyle = '#FFFFFF';
    ctx.font = '600 11px "Plus Jakarta Sans", sans-serif';
    ctx.fillText('Hồ Sen Cổ Phong Thủy', 220, 205);

    // 8. La bàn phong thủy chỉ hướng Bắc
    const cx = width - 110, cy = 110;
    ctx.save();
    ctx.translate(cx, cy);
    ctx.fillStyle = 'rgba(15, 23, 42, 0.9)';
    ctx.beginPath(); ctx.arc(0, 0, 26, 0, Math.PI * 2); ctx.fill();
    ctx.strokeStyle = '#F59E0B'; ctx.lineWidth = 2; ctx.stroke();

    ctx.fillStyle = '#EF4444';
    ctx.beginPath(); ctx.moveTo(0, -20); ctx.lineTo(7, 3); ctx.lineTo(-7, 3); ctx.closePath(); ctx.fill();
    ctx.fillStyle = '#94A3B8';
    ctx.beginPath(); ctx.moveTo(0, 20); ctx.lineTo(7, 3); ctx.lineTo(-7, 3); ctx.closePath(); ctx.fill();

    ctx.fillStyle = '#EF4444';
    ctx.font = 'bold 11px sans-serif';
    ctx.fillText('N', 0, -23);
    ctx.restore();

    // 9. Tiêu đề sơ đồ góc trên
    ctx.save();
    ctx.textAlign = 'left';
    ctx.font = 'bold 15px "Plus Jakarta Sans", sans-serif';
    ctx.fillStyle = '#F8FAFC';
    ctx.fillText('SƠ ĐỒ KHU DI TÍCH HOÀNG THÀNH THĂNG LONG', 90, 110);
    ctx.font = '500 11px "Plus Jakarta Sans", sans-serif';
    ctx.fillStyle = '#CBD5E1';
    ctx.fillText('Bản đồ mặt bằng di sản lịch sử văn hóa (Tỉ lệ 1px ≈ 1.0m)', 90, 128);
    ctx.restore();
}

function renderMap() {
    if (!canvas || !ctx) return;

    const width = canvas.width;
    const height = canvas.height;

    ctx.clearRect(0, 0, width, height);

    // 1. Vẽ nền bản đồ di tích: Vẽ ảnh thật lên Canvas nếu đã tải xong, hoặc sơ đồ dự phòng
    if (state.mapImage && state.mapImage.complete && state.mapImage.naturalWidth > 0) {
        ctx.drawImage(state.mapImage, 0, 0, width, height);
    } else {
        drawHeritageVectorMap(ctx, width, height);
    }

    // 2. Vẽ các Điểm tham quan và Vùng nhận diện
    state.spots.forEach(spot => {
        const sx = spot.toaDoX;
        const sy = spot.toaDoY;
        const radius = spot.phamViNhanDienMet || 50;
        const isActive = (state.activeSpotId === spot.maDiaDiem);

        // Vòng tròn bán kính nhận diện
        ctx.save();
        ctx.beginPath();
        ctx.arc(sx, sy, radius, 0, Math.PI * 2);

        if (isActive) {
            // Khi đang phát thuyết minh điểm này -> viền vàng lấp lánh
            ctx.fillStyle = 'rgba(245, 158, 11, 0.18)';
            ctx.fill();
            ctx.setLineDash([6, 4]);
            ctx.strokeStyle = '#F59E0B';
            ctx.lineWidth = 2.5;
            ctx.stroke();
        } else {
            ctx.fillStyle = 'rgba(59, 130, 246, 0.12)';
            ctx.fill();
            ctx.setLineDash([5, 5]);
            ctx.strokeStyle = 'rgba(59, 130, 246, 0.6)';
            ctx.lineWidth = 1.5;
            ctx.stroke();
        }
        ctx.restore();

        // Pin biểu tượng điểm tham quan
        ctx.save();
        ctx.beginPath();
        ctx.arc(sx, sy, 10, 0, Math.PI * 2);
        ctx.fillStyle = isActive ? '#F59E0B' : '#8B5CF6';
        ctx.shadowColor = isActive ? '#F59E0B' : '#8B5CF6';
        ctx.shadowBlur = 12;
        ctx.fill();

        ctx.strokeStyle = '#FFFFFF';
        ctx.lineWidth = 2;
        ctx.stroke();
        ctx.restore();

        // Tên điểm tham quan
        ctx.save();
        ctx.font = 'bold 13px "Plus Jakarta Sans", sans-serif';
        ctx.fillStyle = '#FFFFFF';
        ctx.shadowColor = 'rgba(0, 0, 0, 0.9)';
        ctx.shadowBlur = 4;
        ctx.textAlign = 'center';
        ctx.fillText(spot.tenDiaDiem, sx, sy - 16);
        ctx.restore();
    });

    // 3. Vẽ Vị trí Du khách (User Pin Marker)
    const ux = state.userPos.x;
    const uy = state.userPos.y;

    // Vòng sóng radar lan tỏa xung quanh người dùng
    const time = Date.now() / 1000;
    const pulseRadius = 16 + Math.sin(time * 3) * 4;

    ctx.save();
    ctx.beginPath();
    ctx.arc(ux, uy, pulseRadius, 0, Math.PI * 2);
    ctx.fillStyle = 'rgba(6, 182, 212, 0.25)';
    ctx.fill();

    // Chấm người dùng chính (Xanh Cyan)
    ctx.beginPath();
    ctx.arc(ux, uy, 8, 0, Math.PI * 2);
    ctx.fillStyle = '#06B6D4';
    ctx.shadowColor = '#06B6D4';
    ctx.shadowBlur = 10;
    ctx.fill();
    ctx.strokeStyle = '#FFFFFF';
    ctx.lineWidth = 2.5;
    ctx.stroke();
    ctx.restore();

    // Nhãn tên "Tôi" hoặc nickname
    ctx.save();
    ctx.font = 'bold 11px "Plus Jakarta Sans", sans-serif';
    const tagWidth = ctx.measureText(state.nickname).width;
    ctx.fillStyle = 'rgba(15, 23, 42, 0.9)';
    ctx.fillRect(ux - tagWidth / 2 - 6, uy - 32, tagWidth + 12, 18);
    ctx.strokeStyle = 'rgba(6, 182, 212, 0.8)';
    ctx.strokeRect(ux - tagWidth / 2 - 6, uy - 32, tagWidth + 12, 18);

    ctx.fillStyle = '#67E8F9';
    ctx.textAlign = 'center';
    ctx.fillText(state.nickname, ux, uy - 19);
    ctx.restore();
}

// ==========================================
// 7. Bắt sự kiện Click / Chạm để di chuyển
// ==========================================
function initCanvasTouchEvents() {
    if (!canvas) return;

    let lastTouchInteractionTime = 0;

    function handleInteraction(e) {
        const now = Date.now();
        // Khử sự kiện click ảo được trình duyệt tự sinh sau cử chỉ touchstart
        if (e.type === 'click' && now - lastTouchInteractionTime < 400) {
            return;
        }
        lastTouchInteractionTime = now;

        const rect = canvas.getBoundingClientRect();
        const clientX = e.touches ? e.touches[0].clientX : e.clientX;
        const clientY = e.touches ? e.touches[0].clientY : e.clientY;

        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;

        const touchX = (clientX - rect.left) * scaleX;
        const touchY = (clientY - rect.top) * scaleY;

        state.targetPos.x = Math.max(20, Math.min(canvas.width - 20, touchX));
        state.targetPos.y = Math.max(20, Math.min(canvas.height - 20, touchY));
        state.isMoving = true;

        // Kích hoạt âm thanh Web Audio ngay khi người dùng chạm bản đồ
        SoundEngine.init();

        // Ẩn hướng dẫn sau lần chạm đầu
        const bubble = document.getElementById('instructionBubble');
        if (bubble) bubble.style.display = 'none';
    }

    canvas.addEventListener('click', handleInteraction);
    canvas.addEventListener('touchstart', handleInteraction, { passive: true });

    document.getElementById('btnCloseBubble')?.addEventListener('click', () => {
        document.getElementById('instructionBubble').style.display = 'none';
    });

    document.getElementById('btnCenterUser')?.addEventListener('click', () => {
        state.targetPos.x = canvas.width / 2;
        state.targetPos.y = canvas.height / 2;
        state.isMoving = true;
    });
}

// ==========================================
// 8. Thuật toán Euclid & Tự động Kích hoạt Thuyết minh
// ==========================================
let lastTriggerTime = 0;

async function checkProximity(ux, uy) {
    if (state.spots.length === 0) return;

    // Giải quyết ngoại lệ tranh chấp vùng lồng nhau (Geofence Contention):
    // 1. Quét tìm tất cả các điểm mà du khách đang đứng bên trong (dist <= radius)
    const candidates = [];
    for (const spot of state.spots) {
        const dist = Math.hypot(ux - spot.toaDoX, uy - spot.toaDoY);
        const radius = spot.phamViNhanDienMet || 50;
        if (dist <= radius) {
            candidates.push({ spot, dist, radius });
        }
    }

    // Nếu không nằm trong bất kỳ điểm nào
    if (candidates.length === 0) {
        if (state.activeSpotId !== null) {
            state.activeSpotId = null;
            hideProximityBanner();
        }
        return;
    }

    // 2. Sắp xếp ưu tiên:
    // - Ưu tiên 1: Điểm có bán kính nhỏ nhất (vùng chi tiết lõi bên trong như Thềm Rồng 50m thay vì Toàn khu 250m)
    // - Ưu tiên 2: Khoảng cách tới tâm gần nhất nếu cùng bán kính
    candidates.sort((a, b) => {
        if (a.radius !== b.radius) return a.radius - b.radius;
        return a.dist - b.dist;
    });

    const matchedSpot = candidates[0].spot;

    // Nếu vẫn ở trong điểm này và đang phát thì không kích hoạt lại
    if (state.activeSpotId === matchedSpot.maDiaDiem) return;

    // Chống kích hoạt lặp liên tục trong thời gian ngắn
    const now = Date.now();
    if (now - lastTriggerTime < 1500) return;
    lastTriggerTime = now;

    state.activeSpotId = matchedSpot.maDiaDiem;
    console.log(`Đã vào phạm vi điểm ưu tiên: ${matchedSpot.tenDiaDiem} (Bán kính: ${matchedSpot.phamViNhanDienMet}m)`);

    // Hiện banner thông báo đã đến gần
    showProximityBanner(matchedSpot.tenDiaDiem);

    // Nạp và phát thuyết minh theo ngôn ngữ
    await playNarrationForSpot(matchedSpot.maDiaDiem, state.currentLang);
}

function showProximityBanner(spotName, isPlaying = true) {
    const banner = document.getElementById('proximityBanner');
    const nameEl = document.getElementById('proximitySpotName');
    const descEl = document.getElementById('proximityDesc');
    const btnPlay = document.getElementById('btnProximityPlay');

    if (banner && nameEl) {
        nameEl.innerText = spotName;
        if (isPlaying) {
            if (descEl) descEl.innerText = 'Đang phát thuyết minh tự động...';
            if (btnPlay) btnPlay.style.display = 'none';
        } else {
            if (descEl) descEl.innerText = 'Chạm nút để nghe thuyết minh ngay!';
            if (btnPlay) {
                btnPlay.style.display = 'flex';
                btnPlay.innerHTML = '<i class="fa-solid fa-play"></i> Nghe';
            }
        }
        banner.classList.add('show');
    }
}

function hideProximityBanner() {
    const banner = document.getElementById('proximityBanner');
    if (banner) banner.classList.remove('show');
}

// ==========================================
// 9. Nạp & Phát Thuyết minh Âm thanh qua Web Audio SoundEngine
// ==========================================
async function playNarrationForSpot(spotId, langCode) {
    try {
        const fetchOpts = {
            headers: { 'ngrok-skip-browser-warning': 'true' }
        };
        const res = await fetch(`/api/DuKhach/thuyetminh?maDiaDiem=${spotId}&maNgonNgu=${langCode}`, fetchOpts);
        if (!res.ok) {
            console.warn('Không tìm thấy bản thuyết minh cho ngôn ngữ này.');
            return;
        }

        const data = await res.json();
        state.activeNarration = data;

        // Cập nhật giao diện Audio Dock Player
        updatePlayerDockUI(data);

        // Nạp và phát âm thanh qua Web Audio SoundEngine
        if (data.duongDanAmThanh) {
            const buf = await SoundEngine.load(data.duongDanAmThanh);
            if (buf) {
                if (SoundEngine.ctx && SoundEngine.ctx.state === 'running') {
                    SoundEngine.play(0);
                    showProximityBanner(data.tenDiaDiem, true);
                } else {
                    // Nếu audioContext bị suspended (trước khi người dùng chạm lần đầu)
                    showProximityBanner(data.tenDiaDiem, false);
                }
            }
        }

    } catch (err) {
        console.error('Lỗi khi nạp thuyết minh:', err);
    }
}

async function updatePlayerDockUI(narration) {
    const thumbEl = document.getElementById('dockSpotThumb');
    const nameEl = document.getElementById('dockSpotName');
    const subEl = document.getElementById('dockSpotSubtitle');

    if (nameEl) nameEl.innerText = narration.tenDiaDiem;
    if (subEl) subEl.innerText = narration.tieuDe || 'Đang thuyết minh...';

    if (thumbEl && narration.hinhAnhMinhHoa) {
        fetchAssetBlobUrl(narration.hinhAnhMinhHoa).then(dataUrl => {
            thumbEl.src = dataUrl;
        }).catch(() => {
            thumbEl.src = '/uploads/images/default_spot.png';
        });
    }

    // Mở khóa các nút điều khiển
    document.getElementById('btnPlayPause').disabled = false;
    document.getElementById('btnRewind5s').disabled = false;
    document.getElementById('btnForward5s').disabled = false;
    document.getElementById('btnOpenNarrationText').disabled = false;

    // Cập nhật sheet văn bản
    updateNarrationSheetUI(narration);
}

// ==========================================
// 10. Điều khiển Audio Player (Tua 5s, Dừng, Phát)
// ==========================================
function initAudioPlayerControls() {
    const btnPlayPause = document.getElementById('btnPlayPause');
    const btnRewind = document.getElementById('btnRewind5s');
    const btnForward = document.getElementById('btnForward5s');
    const scrubberContainer = document.getElementById('scrubberContainer');
    const btnProximityPlay = document.getElementById('btnProximityPlay');
    const proximityBanner = document.getElementById('proximityBanner');

    // 1. Play / Pause Toggle
    btnPlayPause?.addEventListener('click', () => {
        SoundEngine.init();
        if (SoundEngine.isPlaying) {
            SoundEngine.pause();
        } else {
            SoundEngine.resume();
        }
    });

    // 2. Tua lại 5 giây (-5s)
    btnRewind?.addEventListener('click', () => {
        SoundEngine.init();
        SoundEngine.seekRelative(-5);
    });

    // 3. Tua tới 5 giây (+5s)
    btnForward?.addEventListener('click', () => {
        SoundEngine.init();
        SoundEngine.seekRelative(5);
    });

    // 4. Kéo thanh tua (Scrubber seek)
    scrubberContainer?.addEventListener('click', (e) => {
        SoundEngine.init();
        const duration = SoundEngine.getDuration();
        if (duration <= 0) return;
        const rect = scrubberContainer.getBoundingClientRect();
        const pos = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
        SoundEngine.seek(pos * duration);
    });

    // 5. Nút nghe trên Proximity Banner (Hỗ trợ click và touchend tức thì cho iOS)
    btnProximityPlay?.addEventListener('click', triggerProximityPlayback);
    btnProximityPlay?.addEventListener('touchend', triggerProximityPlayback);
    proximityBanner?.addEventListener('click', triggerProximityPlayback);
}

function triggerProximityPlayback(e) {
    if (e) {
        e.preventDefault();
        e.stopPropagation();
    }
    SoundEngine.init();

    if (SoundEngine.isPlaying) {
        SoundEngine.pause();
        const desc = document.getElementById('proximityDesc');
        if (desc) desc.innerText = 'Đã tạm dừng. Chạm để tiếp tục.';
        const btnPlay = document.getElementById('btnProximityPlay');
        if (btnPlay) {
            btnPlay.style.display = 'flex';
            btnPlay.innerHTML = '<i class="fa-solid fa-play"></i> Tiếp tục';
        }
    } else {
        if (SoundEngine.currentBuffer) {
            SoundEngine.resume();
            const desc = document.getElementById('proximityDesc');
            if (desc) desc.innerText = 'Đang phát thuyết minh tự động...';
            const btnPlay = document.getElementById('btnProximityPlay');
            if (btnPlay) btnPlay.style.display = 'none';
        } else if (state.activeNarration && state.activeNarration.duongDanAmThanh) {
            const desc = document.getElementById('proximityDesc');
            if (desc) desc.innerText = 'Đang tải âm thanh...';
            SoundEngine.load(state.activeNarration.duongDanAmThanh).then(buf => {
                if (buf) {
                    SoundEngine.play(0);
                    if (desc) desc.innerText = 'Đang phát thuyết minh tự động...';
                    const btnPlay = document.getElementById('btnProximityPlay');
                    if (btnPlay) btnPlay.style.display = 'none';
                }
            });
        }
    }
}

function updatePlayPauseIcon() {
    const icon = document.getElementById('iconPlayPause');
    if (!icon) return;
    if (state.isPlaying) {
        icon.className = 'fa-solid fa-pause';
    } else {
        icon.className = 'fa-solid fa-play';
    }
}

function formatTime(seconds) {
    const m = Math.floor(seconds / 60);
    const s = Math.floor(seconds % 60);
    return `${m < 10 ? '0' : ''}${m}:${s < 10 ? '0' : ''}${s}`;
}

// ==========================================
// 11. Sliding Bottom Sheet: Đọc Văn bản Thuyết minh
// ==========================================
function initNarrationSheet() {
    const btnOpen = document.getElementById('btnOpenNarrationText');
    const btnDock = document.getElementById('btnOpenTextModalFromDock');
    const btnClose = document.getElementById('btnCloseSheet');
    const sheetBackdrop = document.getElementById('sheetBackdrop');
    const btnSmaller = document.getElementById('btnTextSmaller');
    const btnBigger = document.getElementById('btnTextBigger');
    const article = document.getElementById('narrationArticle');

    function openSheet() {
        if (!state.activeNarration) return;
        sheetBackdrop?.classList.add('show');
    }

    function closeSheet() {
        sheetBackdrop?.classList.remove('show');
    }

    btnOpen?.addEventListener('click', openSheet);
    btnDock?.addEventListener('click', openSheet);
    btnClose?.addEventListener('click', closeSheet);
    sheetBackdrop?.addEventListener('click', (e) => {
        if (e.target === sheetBackdrop) closeSheet();
    });

    // Điều chỉnh cỡ chữ A- / A+
    let currentFontSize = 15;
    btnSmaller?.addEventListener('click', () => {
        currentFontSize = Math.max(12, currentFontSize - 1.5);
        article.style.fontSize = `${currentFontSize}px`;
    });
    btnBigger?.addEventListener('click', () => {
        currentFontSize = Math.min(24, currentFontSize + 1.5);
        article.style.fontSize = `${currentFontSize}px`;
    });
}

function updateNarrationSheetUI(narration) {
    const titleEl = document.getElementById('sheetSpotTitle');
    const badgeEl = document.getElementById('sheetLangBadge');
    const textEl = document.getElementById('narrationParagraph');
    const heroImg = document.getElementById('sheetHeroImage');
    const heroWrapper = document.getElementById('sheetHeroWrapper');

    if (titleEl) titleEl.innerText = narration.tenDiaDiem;
    if (badgeEl) badgeEl.innerText = (narration.tenNgonNgu || narration.maNgonNgu).toUpperCase();
    if (textEl) textEl.innerText = narration.vanBanThuyetMinh || 'Chưa có nội dung văn bản cho điểm này.';

    if (heroImg && heroWrapper) {
        if (narration.hinhAnhMinhHoa) {
            heroImg.src = narration.hinhAnhMinhHoa;
            heroWrapper.style.display = 'block';
        } else {
            heroWrapper.style.display = 'none';
        }
    }
}

// ==========================================
// 12. Chuyển đổi Ngôn ngữ (Language Switcher)
// ==========================================
function initLanguageSelector() {
    const langBtn = document.getElementById('langSelectorBtn');
    const dropdown = document.getElementById('langDropdown');
    const options = document.querySelectorAll('.lang-option');

    langBtn?.addEventListener('click', (e) => {
        e.stopPropagation();
        dropdown?.classList.toggle('show');
    });

    document.addEventListener('click', () => {
        dropdown?.classList.remove('show');
    });

    options.forEach(opt => {
        opt.addEventListener('click', async () => {
            const lang = opt.getAttribute('data-lang');
            options.forEach(o => o.classList.remove('active'));
            opt.classList.add('active');

            state.currentLang = lang;
            sessionStorage.setItem('visitor_lang', lang);
            updateHeaderUI();
            dropdown?.classList.remove('show');

            // Nếu đang trong điểm tham quan -> nạp lại audio và văn bản theo ngôn ngữ mới
            if (state.activeSpotId) {
                const savedTime = SoundEngine.getCurrentTime();
                await playNarrationForSpot(state.activeSpotId, lang);
                if (savedTime > 0) {
                    SoundEngine.seek(savedTime);
                }
            }
        });
    });
}

// ==========================================
// 13. Modal Hướng dẫn & Tiện ích
// ==========================================
function initHelpModal() {
    const btnHelp = document.getElementById('btnHelp');
    const btnClose = document.getElementById('btnCloseHelpModal');
    const btnGotIt = document.getElementById('btnGotIt');

    btnHelp?.addEventListener('click', () => openModal('helpModal'));
    btnClose?.addEventListener('click', () => closeModal('helpModal'));
    btnGotIt?.addEventListener('click', () => closeModal('helpModal'));
}

function openModal(id) {
    document.getElementById(id)?.classList.add('show');
}

function closeModal(id) {
    document.getElementById(id)?.classList.remove('show');
}
