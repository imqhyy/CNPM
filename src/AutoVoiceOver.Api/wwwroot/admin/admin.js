/**
 * AutoVoiceOver - Admin Portal JavaScript Engine
 * Tương tác API 3 lớp, Canvas Radar trực quan & SignalR thời gian thực
 */

// ==========================================
// 1. Khởi tạo State & Biến toàn cục
// ==========================================
const API_BASE = '/api/admin';

const state = {
    spots: [],
    visitors: new Map(), // connectionId -> visitor object
    currentMap: null,
    selectedSpotId: null,
    isPickingCoordinates: false,
    mapImageObj: null,
    adminUser: null
};

let signalRConnection = null;
let isAppInitialized = false;

// ==========================================
// 1.1. Helpers Quản lý Phiên làm việc & Token
// ==========================================
function getAuthToken() {
    return localStorage.getItem('autovoiceover_admin_token');
}

function getCachedAdminUser() {
    try {
        const raw = localStorage.getItem('autovoiceover_admin_user');
        return raw ? JSON.parse(raw) : null;
    } catch {
        return null;
    }
}

function setAuthSession(token, user) {
    if (token) localStorage.setItem('autovoiceover_admin_token', token);
    if (user) {
        localStorage.setItem('autovoiceover_admin_user', JSON.stringify(user));
        state.adminUser = user;
    }
    document.documentElement.classList.add('has-auth-token');
}

function clearAuthSession() {
    localStorage.removeItem('autovoiceover_admin_token');
    localStorage.removeItem('autovoiceover_admin_user');
    state.adminUser = null;
    document.documentElement.classList.remove('has-auth-token');
}

// Fetch Interceptor: Tự động gắn Authorization Bearer Token vào tất cả request API Admin
const originalFetch = window.fetch;
window.fetch = async function (url, options = {}) {
    if (typeof url === 'string' && url.includes('/api/admin/')) {
        const token = getAuthToken();
        if (token) {
            options = options || {};
            options.headers = options.headers || {};
            if (options.headers instanceof Headers) {
                if (!options.headers.has('Authorization')) {
                    options.headers.set('Authorization', `Bearer ${token}`);
                }
            } else if (Array.isArray(options.headers)) {
                options.headers.push(['Authorization', `Bearer ${token}`]);
            } else {
                if (!options.headers['Authorization']) {
                    options.headers['Authorization'] = `Bearer ${token}`;
                }
            }
        }
    }
    const response = await originalFetch(url, options);
    if (response.status === 401 && typeof url === 'string' && url.includes('/api/admin/') && !url.includes('/auth/login')) {
        clearAuthSession();
        document.getElementById('loginScreen')?.classList.remove('hidden');
        const headerProfile = document.getElementById('headerUserProfile');
        if (headerProfile) headerProfile.style.display = 'none';
        showToast('Phiên làm việc đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.', 'error');
    }
    return response;
};

// Canvas context
const canvas = document.getElementById('liveRadarCanvas');
const ctx = canvas ? canvas.getContext('2d') : null;

// ==========================================
// 2. DOM Ready & Event Listeners
// ==========================================
document.addEventListener('DOMContentLoaded', () => {
    initAuth();
});

function startAdminApp() {
    if (isAppInitialized) {
        fetchInitialData();
        return;
    }
    isAppInitialized = true;
    initTabs();
    initSignalR();
    initCanvasEvents();
    initModalsAndForms();

    // Tải dữ liệu ban đầu
    fetchInitialData();

    // Nút làm mới dữ liệu
    document.getElementById('btnRefreshData')?.addEventListener('click', () => {
        if (getAuthToken()) {
            fetchInitialData();
            showToast('Đã làm mới dữ liệu!', 'info');
        }
    });
}

// ==========================================
// 2.1. Xác thực Quản trị viên (Admin Login Flow)
// ==========================================
async function initAuth() {
    const loginScreen = document.getElementById('loginScreen');
    const formLogin = document.getElementById('formAdminLogin');
    const inputUsername = document.getElementById('loginUsername');
    const inputPassword = document.getElementById('loginPassword');
    const btnTogglePwd = document.getElementById('btnTogglePwd');
    const iconEye = document.getElementById('iconEye');
    const loginAlert = document.getElementById('loginAlert');
    const loginAlertMsg = document.getElementById('loginAlertMsg');
    const btnQuickFill = document.getElementById('btnQuickFillAdmin');
    const headerProfile = document.getElementById('headerUserProfile');
    const headerAdminName = document.getElementById('headerAdminName');
    const btnLogout = document.getElementById('btnLogout');
    const btnSubmit = document.getElementById('btnLoginSubmit');
    const btnText = document.getElementById('btnLoginText');

    function showLoginError(msg) {
        if (loginAlert && loginAlertMsg) {
            loginAlertMsg.textContent = msg;
            loginAlert.classList.add('show');
        }
    }

    function hideLoginError() {
        if (loginAlert) loginAlert.classList.remove('show');
    }

    // Nút điền nhanh tài khoản mẫu (admin1 / 123456789)
    btnQuickFill?.addEventListener('click', () => {
        if (inputUsername) inputUsername.value = 'admin1';
        if (inputPassword) inputPassword.value = '123456789';
        hideLoginError();
        inputPassword?.focus();
    });

    // Nút bật/tắt hiển thị mật khẩu
    btnTogglePwd?.addEventListener('click', () => {
        if (!inputPassword) return;
        const isPassword = inputPassword.type === 'password';
        inputPassword.type = isPassword ? 'text' : 'password';
        if (iconEye) {
            iconEye.className = isPassword ? 'fa-regular fa-eye-slash' : 'fa-regular fa-eye';
        }
    });

    // Nút Đăng xuất
    btnLogout?.addEventListener('click', async () => {
        try {
            await fetch('/api/admin/auth/logout', { method: 'POST' });
        } catch (e) {
            console.warn('Logout request failed:', e);
        }
        clearAuthSession();
        if (signalRConnection) {
            try { await signalRConnection.stop(); } catch {}
            signalRConnection = null;
        }
        if (headerProfile) headerProfile.style.display = 'none';
        if (loginScreen) loginScreen.classList.remove('hidden');
        if (inputPassword) inputPassword.value = '';
        hideLoginError();
        showToast('Đã đăng xuất khỏi cổng Quản trị.', 'info');
    });

    // Submit form đăng nhập
    formLogin?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const username = inputUsername?.value.trim() || '';
        const password = inputPassword?.value || '';

        if (!username || !password) {
            showLoginError('Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.');
            return;
        }

        if (btnSubmit) btnSubmit.disabled = true;
        if (btnText) btnText.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Đang xác thực...';
        hideLoginError();

        try {
            const res = await fetch('/api/admin/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ tenDangNhap: username, matKhau: password })
            });

            const data = await res.json();
            if (res.ok && data.thanhCong) {
                setAuthSession(data.token, data.admin);
                if (headerAdminName && data.admin) {
                    headerAdminName.textContent = data.admin.hoTen || data.admin.tenDangNhap;
                }
                if (headerProfile) headerProfile.style.display = 'flex';
                if (loginScreen) loginScreen.classList.add('hidden');

                showToast(`Chào mừng ${data.admin?.hoTen || username} đăng nhập thành công!`, 'success');
                startAdminApp();
            } else {
                showLoginError(data.thongBao || 'Tên đăng nhập hoặc mật khẩu không chính xác.');
            }
        } catch (err) {
            console.error('Login error:', err);
            showLoginError('Không thể kết nối tới máy chủ. Vui lòng kiểm tra lại dịch vụ backend.');
        } finally {
            if (btnSubmit) btnSubmit.disabled = false;
            if (btnText) btnText.innerHTML = '<i class="fa-solid fa-right-to-bracket"></i> Đăng nhập Quản trị';
        }
    });

    // Kiểm tra token hiện tại
    const token = getAuthToken();
    if (token) {
        // Tối ưu FOUC: Khôi phục tức thì thông tin admin từ cache để giao diện hiển thị ngay lập tức 0ms
        const cachedUser = getCachedAdminUser();
        if (cachedUser) {
            state.adminUser = cachedUser;
            if (headerAdminName) headerAdminName.textContent = cachedUser.hoTen || cachedUser.tenDangNhap;
            if (headerProfile) headerProfile.style.display = 'flex';
        }
        if (loginScreen) loginScreen.classList.add('hidden');
        document.documentElement.classList.add('has-auth-token');
        startAdminApp();

        // Xác thực lại token với backend trong nền để đảm bảo tính hợp lệ và cập nhật dữ liệu mới nhất
        try {
            const res = await fetch('/api/admin/auth/me', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (res.ok) {
                const admin = await res.json();
                state.adminUser = admin;
                setAuthSession(token, admin);
                if (headerAdminName) headerAdminName.textContent = admin.hoTen || admin.tenDangNhap;
                return;
            }
        } catch (e) {
            console.warn('Xác thực token cũ thất bại:', e);
        }

        // Token không còn hợp lệ trên máy chủ -> Buộc đăng nhập lại
        clearAuthSession();
        if (loginScreen) loginScreen.classList.remove('hidden');
        if (headerProfile) headerProfile.style.display = 'none';
        showToast('Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.', 'warning');
        return;
    }

    // Chưa đăng nhập -> Hiện màn hình đăng nhập
    clearAuthSession();
    if (loginScreen) loginScreen.classList.remove('hidden');
    if (headerProfile) headerProfile.style.display = 'none';
}

// ==========================================
// 3. Tab Switching
// ==========================================
function initTabs() {
    const navItems = document.querySelectorAll('.nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', () => {
            navItems.forEach(nav => nav.classList.remove('active'));
            document.querySelectorAll('.tab-pane').forEach(pane => pane.classList.remove('active'));

            item.classList.add('active');
            const targetPaneId = item.getAttribute('data-tab');
            const targetPane = document.getElementById(targetPaneId);
            if (targetPane) targetPane.classList.add('active');

            if (targetPaneId === 'tab-live') {
                renderRadarCanvas();
            }
        });
    });
}

// ==========================================
// 4. SignalR Hub: Giám sát Du khách Thời gian thực
// ==========================================
function initSignalR() {
    const statusText = document.getElementById('signalrStatus');
    const statusBadge = document.getElementById('liveStatusBadge');

    try {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/hub/theodoi')
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .build();
        signalRConnection = connection;

        // 1. Nhận cập nhật số lượng online
        connection.on('CapNhatSoLuongOnline', (count) => {
            updateOnlineVisitorCount(count);
        });

        // 2. Nhận danh sách du khách thời gian thực
        connection.on('DanhSachNguoiOnline', (list) => {
            if (Array.isArray(list)) {
                state.visitors.clear();
                list.forEach(u => {
                    state.visitors.set(u.connectionId, {
                        connectionId: u.connectionId,
                        tenDuKhach: u.tenDuKhach,
                        toadoX: u.toadoX,
                        toadoY: u.toadoY,
                        lanHoatDongCuoi: new Date(u.lanHoatDongCuoi)
                    });
                });
                renderVisitorTable();
                renderRadarCanvas();
            }
        });

        // 3. Nhận sự kiện du khách di chuyển tọa độ
        connection.on('DuKhachDiChuyen', (visitor) => {
            state.visitors.set(visitor.connectionId, {
                connectionId: visitor.connectionId,
                tenDuKhach: visitor.tenDuKhach || 'Du khách',
                toadoX: visitor.toadoX,
                toadoY: visitor.toadoY,
                lanHoatDongCuoi: new Date()
            });
            renderVisitorTable();
            renderRadarCanvas();
        });

        connection.onreconnecting(() => {
            if (statusText) statusText.innerText = 'Đang kết nối lại...';
            if (statusBadge) statusBadge.style.borderColor = 'var(--accent-amber)';
        });

        connection.onreconnected(() => {
            if (statusText) statusText.innerText = 'Trực tuyến';
            if (statusBadge) statusBadge.style.borderColor = 'var(--border-color)';
            connection.invoke('DangKyAdmin').catch(err => console.error(err));
            showToast('Kết nối thời gian thực đã khôi phục', 'success');
        });

        connection.onclose(() => {
            if (statusText) statusText.innerText = 'Mất kết nối';
            if (statusBadge) statusBadge.style.borderColor = 'var(--accent-rose)';
        });

        connection.start()
            .then(() => {
                if (statusText) statusText.innerText = 'Trực tuyến';
                console.log('SignalR connected to /hub/theodoi');
                connection.invoke('DangKyAdmin').catch(err => console.error(err));
            })
            .catch(err => {
                console.warn('SignalR start failed:', err);
                if (statusText) statusText.innerText = 'Lỗi kết nối';
            });

    } catch (e) {
        console.error('SignalR init error:', e);
    }
}

function updateOnlineVisitorCount(count) {
    const counterEl = document.getElementById('onlineVisitorCount');
    const badgeEl = document.getElementById('badgeVisitorCount');
    const statEl = document.getElementById('statVisitorOnline');

    if (counterEl) counterEl.innerText = count;
    if (badgeEl) badgeEl.innerText = `${count} người`;
    if (statEl) statEl.innerText = count;
}

// ==========================================
// 5. Tải dữ liệu ban đầu từ REST API
// ==========================================
async function fetchInitialData() {
    await Promise.all([
        loadMap(),
        loadSpots(),
        loadDashboardStats()
    ]);
    renderRadarCanvas();
}

async function loadDashboardStats() {
    try {
        const res = await fetch(`${API_BASE}/Dashboard/tong-quan`);
        if (res.ok) {
            const data = await res.json();
            updateOnlineVisitorCount(data.soNguoiOnline || 0);

            if (data.danhSachOnline && Array.isArray(data.danhSachOnline)) {
                state.visitors.clear();
                data.danhSachOnline.forEach(u => {
                    state.visitors.set(u.connectionId, {
                        connectionId: u.connectionId,
                        tenDuKhach: u.tenDuKhach,
                        toadoX: u.toadoX,
                        toadoY: u.toadoY,
                        lanHoatDongCuoi: new Date(u.lanHoatDongCuoi)
                    });
                });
                renderVisitorTable();
                renderRadarCanvas();
            }
        }
    } catch (err) {
        console.error('Failed to load stats:', err);
    }
}

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
    } catch (e) {
        console.warn('Admin map fallback direct load:', e);
    }
    return new Promise((resolve) => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = () => resolve(null);
        img.src = url;
    });
}

async function loadMap() {
    try {
        const res = await fetch(`${API_BASE}/QuanLyDiaDiem/bando`);
        if (res.ok) {
            const maps = await res.json();
            if (maps && maps.length > 0) {
                state.currentMap = maps[0];
                updateMapTabUI(state.currentMap);

                // Tải ảnh bản đồ vào memory để vẽ lên canvas
                if (state.currentMap.hinhAnhBanDo) {
                    const loadedImg = await loadMapImage(state.currentMap.hinhAnhBanDo);
                    if (loadedImg) {
                        state.mapImageObj = loadedImg;
                        renderRadarCanvas();
                    }
                }
            }
        }
    } catch (err) {
        console.error('Failed to load map:', err);
    }
}

async function loadSpots() {
    try {
        const res = await fetch(`${API_BASE}/QuanLyDiaDiem`);
        if (res.ok) {
            state.spots = await res.json();
            
            // Cập nhật thống kê
            const statSpotCount = document.getElementById('statSpotCount');
            if (statSpotCount) statSpotCount.innerText = state.spots.length;

            const totalAudios = state.spots.reduce((acc, cur) => acc + (cur.danhSachNgonNgu ? cur.danhSachNgonNgu.length : 0), 0);
            const statAudioCount = document.getElementById('statAudioCount');
            if (statAudioCount) statAudioCount.innerText = totalAudios;

            renderSpotsTable();
            renderNarrationSpotList();
            renderRadarCanvas();
        }
    } catch (err) {
        console.error('Failed to load spots:', err);
        showToast('Không thể nạp danh sách điểm tham quan', 'error');
    }
}

// ==========================================
// 6. Hiển thị bảng Danh sách Điểm tham quan (Tab 2)
// ==========================================
function renderSpotsTable() {
    const tbody = document.getElementById('spotsTableBody');
    if (!tbody) return;

    if (state.spots.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-5 text-muted">
                    <i class="fa-solid fa-map-location text-3xl mb-2 d-block"></i>
                    Chưa có điểm tham quan nào. Hãy nhấn "Thêm điểm tham quan" để bắt đầu!
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = state.spots.map(s => {
        const imgSrc = s.hinhAnhMinhHoa || '/uploads/images/default_spot.png';
        const langTags = s.danhSachNgonNgu && s.danhSachNgonNgu.length > 0
            ? s.danhSachNgonNgu.map(l => `<span class="badge badge-info">${l.toUpperCase()}</span>`).join(' ')
            : `<span class="badge badge-secondary">Chưa có audio</span>`;

        return `
            <tr>
                <td>
                    <img src="${imgSrc}" class="spot-img-thumb" alt="${s.tenDiaDiem}" onerror="this.src='data:image/svg+xml;utf8,<svg xmlns=\'http://www.w3.org/2000/svg\' width=\'48\' height=\'48\' fill=\'%2364748b\' viewBox=\'0 0 24 24\'><path d=\'M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z\'/></svg>'">
                </td>
                <td>
                    <strong style="color:#fff; font-size:0.95rem;">${escapeHtml(s.tenDiaDiem)}</strong>
                </td>
                <td>
                    <span class="badge badge-secondary">(${s.toaDoX.toFixed(1)}, ${s.toaDoY.toFixed(1)})</span>
                </td>
                <td>
                    <button class="btn btn-sm btn-outline" onclick="openRadiusModal('${s.maDiaDiem}', '${escapeHtml(s.tenDiaDiem)}', ${s.phamViNhanDienMet})" title="Bấm để tùy chỉnh bán kính">
                        <i class="fa-solid fa-circle-dot text-accent"></i> ${s.phamViNhanDienMet} m
                    </button>
                </td>
                <td>${langTags}</td>
                <td>
                    ${s.hoatDong 
                        ? '<span class="badge badge-success"><i class="fa-solid fa-check"></i> Đang bật</span>'
                        : '<span class="badge badge-secondary">Tạm tắt</span>'}
                </td>
                <td class="text-right">
                    <button class="btn btn-sm btn-outline mr-1" onclick="openEditSpotModal('${s.maDiaDiem}')" title="Sửa thông tin">
                        <i class="fa-solid fa-pen"></i>
                    </button>
                    <button class="btn btn-sm btn-primary mr-1" onclick="goToNarrationForSpot('${s.maDiaDiem}')" title="Quản lý thuyết minh audio">
                        <i class="fa-solid fa-headphones"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="deleteSpot('${s.maDiaDiem}')" title="Xóa điểm">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

// ==========================================
// 7. Hiển thị Bảng Khách Online (Tab 1)
// ==========================================
function renderVisitorTable() {
    const tbody = document.getElementById('visitorTableBody');
    if (!tbody) return;

    const list = Array.from(state.visitors.values());
    if (list.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="3" class="text-center text-muted py-4">
                    <i class="fa-solid fa-user-slash mb-2 d-block text-lg"></i>
                    Chưa có du khách nào kết nối
                </td>
            </tr>
        `;
        return;
    }

    tbody.innerHTML = list.map(v => `
        <tr>
            <td>
                <span class="live-indicator-dot" style="display:inline-block; vertical-align:middle; margin-right:6px;"></span>
                <strong>${escapeHtml(v.tenDuKhach)}</strong>
            </td>
            <td>
                <span class="badge badge-secondary">(${v.toadoX.toFixed(1)}, ${v.toadoY.toFixed(1)})</span>
            </td>
            <td class="text-muted" style="font-size:0.8rem;">
                ${new Date(v.lanHoatDongCuoi).toLocaleTimeString()}
            </td>
        </tr>
    `).join('');
}

// ==========================================
// 8. Canvas Radar: Vẽ Sơ đồ Di tích, Bán kính & Du khách
// ==========================================
function renderRadarCanvas() {
    if (!canvas || !ctx) return;

    const width = canvas.width;
    const height = canvas.height;

    // 1. Xóa nền và vẽ ảnh sơ đồ di tích
    ctx.clearRect(0, 0, width, height);

    if (state.mapImageObj) {
        ctx.drawImage(state.mapImageObj, 0, 0, width, height);
        // Lớp phủ mờ tinh tế
        ctx.fillStyle = 'rgba(11, 15, 25, 0.45)';
        ctx.fillRect(0, 0, width, height);
    } else {
        // Nền tối với lưới kiến trúc kỹ thuật
        ctx.fillStyle = '#060911';
        ctx.fillRect(0, 0, width, height);

        ctx.strokeStyle = 'rgba(255, 255, 255, 0.05)';
        ctx.lineWidth = 1;
        const gridSize = 40;
        for (let x = 0; x < width; x += gridSize) {
            ctx.beginPath();
            ctx.moveTo(x, 0);
            ctx.lineTo(x, height);
            ctx.stroke();
        }
        for (let y = 0; y < height; y += gridSize) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(width, y);
            ctx.stroke();
        }
    }

    // 2. Vẽ các Điểm Tham quan & Vòng tròn Bán kính Nhận diện (Detection Radius)
    state.spots.forEach(spot => {
        const sx = spot.toaDoX;
        const sy = spot.toaDoY;
        const radius = spot.phamViNhanDienMet || 50;

        // Vùng bán kính kích hoạt
        ctx.save();
        ctx.beginPath();
        ctx.arc(sx, sy, radius, 0, Math.PI * 2);
        ctx.fillStyle = 'rgba(59, 130, 246, 0.12)';
        ctx.fill();
        ctx.setLineDash([5, 5]);
        ctx.strokeStyle = 'rgba(59, 130, 246, 0.6)';
        ctx.lineWidth = 1.5;
        ctx.stroke();
        ctx.restore();

        // Chấm tâm điểm tham quan
        ctx.save();
        ctx.beginPath();
        ctx.arc(sx, sy, 8, 0, Math.PI * 2);
        ctx.fillStyle = '#8B5CF6';
        ctx.shadowColor = '#8B5CF6';
        ctx.shadowBlur = 10;
        ctx.fill();

        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 2;
        ctx.stroke();
        ctx.restore();

        // Nhãn tên điểm tham quan
        ctx.save();
        ctx.font = '600 12px "Plus Jakarta Sans", sans-serif';
        ctx.fillStyle = '#FFFFFF';
        ctx.shadowColor = 'rgba(0, 0, 0, 0.8)';
        ctx.shadowBlur = 4;
        ctx.textAlign = 'center';
        ctx.fillText(spot.tenDiaDiem, sx, sy - 14);

        // Nhãn bán kính
        ctx.font = '500 10px "Plus Jakarta Sans", sans-serif';
        ctx.fillStyle = '#93C5FD';
        ctx.fillText(`r = ${radius}m`, sx, sy + 22);
        ctx.restore();
    });

    // 3. Vẽ Du khách đang Online và Vị trí tức thời
    state.visitors.forEach(v => {
        const vx = v.toadoX;
        const vy = v.toadoY;

        // Vòng sóng lan tỏa radar xanh lá
        ctx.save();
        ctx.beginPath();
        ctx.arc(vx, vy, 14, 0, Math.PI * 2);
        ctx.fillStyle = 'rgba(16, 185, 129, 0.25)';
        ctx.fill();

        // Chấm vị trí chính
        ctx.beginPath();
        ctx.arc(vx, vy, 6, 0, Math.PI * 2);
        ctx.fillStyle = '#10B981';
        ctx.shadowColor = '#10B981';
        ctx.shadowBlur = 8;
        ctx.fill();
        ctx.strokeStyle = '#fff';
        ctx.lineWidth = 1.5;
        ctx.stroke();
        ctx.restore();

        // Tag hiển thị Biệt danh của du khách
        ctx.save();
        ctx.font = 'bold 11px "Plus Jakarta Sans", sans-serif';
        const textWidth = ctx.measureText(v.tenDuKhach).width;
        
        ctx.fillStyle = 'rgba(17, 24, 39, 0.85)';
        ctx.fillRect(vx - textWidth / 2 - 6, vy - 28, textWidth + 12, 18);
        ctx.strokeStyle = 'rgba(16, 185, 129, 0.6)';
        ctx.strokeRect(vx - textWidth / 2 - 6, vy - 28, textWidth + 12, 18);

        ctx.fillStyle = '#A7F3D0';
        ctx.textAlign = 'center';
        ctx.fillText(v.tenDuKhach, vx, vy - 15);
        ctx.restore();
    });
}

// Bắt sự kiện Click trên Canvas để pick tọa độ nhanh
function initCanvasEvents() {
    if (!canvas) return;

    canvas.addEventListener('click', (e) => {
        const rect = canvas.getBoundingClientRect();
        const scaleX = canvas.width / rect.width;
        const scaleY = canvas.height / rect.height;

        const clickX = (e.clientX - rect.left) * scaleX;
        const clickY = (e.clientY - rect.top) * scaleY;

        // Nếu modal thêm/sửa điểm đang mở, điền tọa độ vào ô input
        const modalSpot = document.getElementById('modalSpot');
        if (modalSpot && modalSpot.classList.contains('show')) {
            const coordXInput = document.getElementById('spotCoordX');
            const coordYInput = document.getElementById('spotCoordY');
            if (coordXInput && coordYInput) {
                coordXInput.value = clickX.toFixed(1);
                coordYInput.value = clickY.toFixed(1);
                showToast(`Đã chọn tọa độ: (${clickX.toFixed(1)}, ${clickY.toFixed(1)})`, 'info');
            }
            return;
        }

        // Kiểm tra xem có click trúng điểm tham quan nào không
        const clickedSpot = state.spots.find(s => {
            const dist = Math.hypot(s.toaDoX - clickX, s.toaDoY - clickY);
            return dist <= 20;
        });

        if (clickedSpot) {
            openEditSpotModal(clickedSpot.maDiaDiem);
        }
    });

    document.getElementById('btnResetCanvasView')?.addEventListener('click', () => {
        renderRadarCanvas();
        showToast('Đã căn lại khung nhìn bản đồ', 'info');
    });

    document.getElementById('btnOpenAddSpotQuick')?.addEventListener('click', () => {
        openAddSpotModal();
        showToast('Hãy click trên bản đồ để chọn tọa độ (X, Y) mong muốn!', 'info');
    });
}

// ==========================================
// 9. Quản lý Modal & Biểu mẫu (CRUD)
// ==========================================
function initModalsAndForms() {
    // Khởi tạo Bộ chọn tọa độ trên bản đồ tương tác trong Popup
    initSpotPickerCanvas();

    // Range slider sync
    syncRangeAndNumber('spotRadiusSlider', 'spotRadius');
    syncRangeAndNumber('quickRadiusSlider', 'quickRadiusInput');

    // Nút mở modal thêm điểm
    document.getElementById('btnOpenAddSpotModal')?.addEventListener('click', () => openAddSpotModal());

    // Nút đóng modal
    document.getElementById('btnCloseSpotModal')?.addEventListener('click', () => closeModal('modalSpot'));
    document.getElementById('btnCancelSpotModal')?.addEventListener('click', () => closeModal('modalSpot'));

    document.getElementById('btnCloseRadiusModal')?.addEventListener('click', () => closeModal('modalRadius'));
    document.getElementById('btnCancelRadiusModal')?.addEventListener('click', () => closeModal('modalRadius'));

    document.getElementById('btnCloseNarrationModal')?.addEventListener('click', () => closeModal('modalNarration'));
    document.getElementById('btnCancelNarrationModal')?.addEventListener('click', () => closeModal('modalNarration'));

    // Submit Thêm / Sửa Điểm
    document.getElementById('formSpot')?.addEventListener('submit', handleSpotSubmit);

    // Submit Bán kính nhanh
    document.getElementById('formRadius')?.addEventListener('submit', handleRadiusSubmit);

    // Submit Thuyết minh Đa ngôn ngữ
    document.getElementById('formNarration')?.addEventListener('submit', handleNarrationSubmit);

    // Submit Upload Bản đồ mới
    document.getElementById('formUploadMap')?.addEventListener('submit', handleMapSubmit);

    // Xử lý tự động đọc file .txt khi chọn file văn bản thuyết minh
    document.getElementById('narrTextFile')?.addEventListener('change', async function(e) {
        const file = e.target.files[0];
        if (!file) return;

        // Gọi API upload-vanban để server xử lý và trả về text
        const formData = new FormData();
        formData.append('file', file);
        try {
            const res = await fetch(`${API_BASE}/QuanLyDiaDiem/upload-vanban`, {
                method: 'POST',
                body: formData
            });
            if (res.ok) {
                const data = await res.json();
                const contentArea = document.getElementById('narrContent');
                if (contentArea && data.noiDung) {
                    contentArea.value = data.noiDung;
                    showToast('Đã nạp văn bản từ tệp .txt!', 'info');
                }
            }
        } catch (err) {
            console.warn('Fallback reading local file:', err);
            const reader = new FileReader();
            reader.onload = function(evt) {
                const contentArea = document.getElementById('narrContent');
                if (contentArea) contentArea.value = evt.target.result;
            };
            reader.readAsText(file);
        }
    });
}

function syncRangeAndNumber(sliderId, inputId) {
    const slider = document.getElementById(sliderId);
    const input = document.getElementById(inputId);
    if (!slider || !input) return;

    slider.addEventListener('input', () => { input.value = slider.value; });
    input.addEventListener('input', () => { slider.value = input.value; });
}

function openModal(modalId) {
    const el = document.getElementById(modalId);
    if (el) el.classList.add('show');
}

function closeModal(modalId) {
    const el = document.getElementById(modalId);
    if (el) el.classList.remove('show');
}

// ------------------------------------------
// 9.1. Bộ chọn Bản đồ Tương tác trong Popup (Interactive Spot Map Picker)
// ------------------------------------------
function initSpotPickerCanvas() {
    const pCanvas = document.getElementById('spotPickerCanvas');
    if (!pCanvas) return;

    // Khi người dùng click trên bản đồ trong Popup -> Tự động điền tọa độ X, Y
    pCanvas.addEventListener('click', (e) => {
        const rect = pCanvas.getBoundingClientRect();
        const scaleX = pCanvas.width / rect.width;
        const scaleY = pCanvas.height / rect.height;

        const clickX = Math.max(0, Math.min(pCanvas.width, (e.clientX - rect.left) * scaleX));
        const clickY = Math.max(0, Math.min(pCanvas.height, (e.clientY - rect.top) * scaleY));

        const coordXInput = document.getElementById('spotCoordX');
        const coordYInput = document.getElementById('spotCoordY');
        const radiusInput = document.getElementById('spotRadius');
        const badge = document.getElementById('pickerCoordsBadge');

        if (coordXInput) coordXInput.value = clickX.toFixed(1);
        if (coordYInput) coordYInput.value = clickY.toFixed(1);
        if (badge) badge.innerHTML = `<i class="fa-solid fa-location-pin"></i> Tọa độ: (${clickX.toFixed(1)}, ${clickY.toFixed(1)})`;

        const r = radiusInput ? parseFloat(radiusInput.value) || 50 : 50;
        const currentSpotId = document.getElementById('spotId')?.value;
        renderSpotPickerCanvas(clickX, clickY, r, currentSpotId);
        showToast(`Đã tự động chọn tọa độ: (${clickX.toFixed(1)}, ${clickY.toFixed(1)})`, 'info');
    });

    // Lắng nghe khi nhập tay tọa độ X hoặc Y -> Cập nhật vị trí ghim tức thời trên bản đồ popup
    const onCoordChange = () => {
        const x = parseFloat(document.getElementById('spotCoordX')?.value) || 0;
        const y = parseFloat(document.getElementById('spotCoordY')?.value) || 0;
        const r = parseFloat(document.getElementById('spotRadius')?.value) || 50;
        const currentSpotId = document.getElementById('spotId')?.value;
        const badge = document.getElementById('pickerCoordsBadge');
        if (badge) badge.innerHTML = `<i class="fa-solid fa-location-pin"></i> Tọa độ: (${x.toFixed(1)}, ${y.toFixed(1)})`;
        renderSpotPickerCanvas(x, y, r, currentSpotId);
    };

    document.getElementById('spotCoordX')?.addEventListener('input', onCoordChange);
    document.getElementById('spotCoordY')?.addEventListener('input', onCoordChange);

    // Lắng nghe khi kéo thanh trượt bán kính -> Co giãn vòng tròn bán kính trên bản đồ popup
    const onRadiusChange = () => {
        const x = parseFloat(document.getElementById('spotCoordX')?.value) || 0;
        const y = parseFloat(document.getElementById('spotCoordY')?.value) || 0;
        const r = parseFloat(document.getElementById('spotRadius')?.value) || 50;
        const currentSpotId = document.getElementById('spotId')?.value;
        renderSpotPickerCanvas(x, y, r, currentSpotId);
    };

    document.getElementById('spotRadiusSlider')?.addEventListener('input', onRadiusChange);
    document.getElementById('spotRadius')?.addEventListener('input', onRadiusChange);

    // Nút đưa tâm ngắm về chính giữa bản đồ
    document.getElementById('btnCenterPickerMap')?.addEventListener('click', () => {
        const cx = pCanvas.width / 2;
        const cy = pCanvas.height / 2;
        document.getElementById('spotCoordX').value = cx.toFixed(1);
        document.getElementById('spotCoordY').value = cy.toFixed(1);
        const badge = document.getElementById('pickerCoordsBadge');
        if (badge) badge.innerHTML = `<i class="fa-solid fa-location-pin"></i> Tọa độ: (${cx.toFixed(1)}, ${cy.toFixed(1)})`;
        const r = parseFloat(document.getElementById('spotRadius')?.value) || 50;
        const currentSpotId = document.getElementById('spotId')?.value;
        renderSpotPickerCanvas(cx, cy, r, currentSpotId);
    });
}

function renderSpotPickerCanvas(targetX, targetY, radius, excludeSpotId = null) {
    const pCanvas = document.getElementById('spotPickerCanvas');
    if (!pCanvas) return;
    const pCtx = pCanvas.getContext('2d');
    if (!pCtx) return;

    const width = pCanvas.width;
    const height = pCanvas.height;

    // 1. Xóa và vẽ hình ảnh bản đồ nền
    pCtx.clearRect(0, 0, width, height);

    if (state.mapImageObj) {
        pCtx.drawImage(state.mapImageObj, 0, 0, width, height);
        // Lớp phủ mờ giúp tăng tương phản thị giác
        pCtx.fillStyle = 'rgba(11, 15, 25, 0.42)';
        pCtx.fillRect(0, 0, width, height);
    } else {
        // Vẽ lưới blueprint kỹ thuật nếu chưa có file ảnh
        pCtx.fillStyle = '#060911';
        pCtx.fillRect(0, 0, width, height);

        pCtx.strokeStyle = 'rgba(255, 255, 255, 0.06)';
        pCtx.lineWidth = 1;
        const gridSize = 40;
        for (let x = 0; x < width; x += gridSize) {
            pCtx.beginPath();
            pCtx.moveTo(x, 0);
            pCtx.lineTo(x, height);
            pCtx.stroke();
        }
        for (let y = 0; y < height; y += gridSize) {
            pCtx.beginPath();
            pCtx.moveTo(0, y);
            pCtx.lineTo(width, y);
            pCtx.stroke();
        }
    }

    // 2. Vẽ các điểm tham quan đã có (trừ điểm đang được sửa để tránh trùng)
    state.spots.forEach(s => {
        if (excludeSpotId && s.maDiaDiem === excludeSpotId) return;
        const sx = s.toaDoX;
        const sy = s.toaDoY;
        const r = s.phamViNhanDienMet || 50;

        // Vòng bán kính mờ của điểm khác
        pCtx.save();
        pCtx.beginPath();
        pCtx.arc(sx, sy, r, 0, Math.PI * 2);
        pCtx.fillStyle = 'rgba(148, 163, 184, 0.08)';
        pCtx.fill();
        pCtx.setLineDash([4, 4]);
        pCtx.strokeStyle = 'rgba(148, 163, 184, 0.3)';
        pCtx.lineWidth = 1;
        pCtx.stroke();
        pCtx.restore();

        // Chấm tròn điểm khác
        pCtx.save();
        pCtx.beginPath();
        pCtx.arc(sx, sy, 5, 0, Math.PI * 2);
        pCtx.fillStyle = '#64748B';
        pCtx.fill();
        pCtx.strokeStyle = '#fff';
        pCtx.lineWidth = 1.5;
        pCtx.stroke();

        // Tên điểm
        pCtx.font = '500 11px "Plus Jakarta Sans", sans-serif';
        pCtx.fillStyle = 'rgba(226, 232, 240, 0.7)';
        pCtx.textAlign = 'center';
        pCtx.fillText(s.tenDiaDiem, sx, sy - 9);
        pCtx.restore();
    });

    // 3. Vòng bán kính nhận diện của điểm đang chọn (Activation Zone)
    pCtx.save();
    pCtx.beginPath();
    pCtx.arc(targetX, targetY, radius, 0, Math.PI * 2);
    pCtx.fillStyle = 'rgba(59, 130, 246, 0.18)';
    pCtx.fill();
    pCtx.setLineDash([6, 6]);
    pCtx.strokeStyle = '#3B82F6';
    pCtx.lineWidth = 2;
    pCtx.shadowColor = 'rgba(59, 130, 246, 0.5)';
    pCtx.shadowBlur = 10;
    pCtx.stroke();

    // Nhãn bán kính hiển thị trực tiếp
    pCtx.font = '600 11px "Plus Jakarta Sans", sans-serif';
    pCtx.fillStyle = '#93C5FD';
    pCtx.textAlign = 'center';
    pCtx.fillText(`Bán kính nhận diện: ${radius}m`, targetX, targetY + radius + 18);
    pCtx.restore();

    // 4. Đường tâm ngắm Crosshairs
    pCtx.save();
    pCtx.strokeStyle = 'rgba(139, 92, 246, 0.6)';
    pCtx.lineWidth = 1.2;
    pCtx.setLineDash([2, 2]);
    const crossSize = 35;
    pCtx.beginPath();
    pCtx.moveTo(targetX - crossSize, targetY);
    pCtx.lineTo(targetX + crossSize, targetY);
    pCtx.stroke();

    pCtx.beginPath();
    pCtx.moveTo(targetX, targetY - crossSize);
    pCtx.lineTo(targetX, targetY + crossSize);
    pCtx.stroke();
    pCtx.restore();

    // 5. Vòng sóng radar lan tỏa xung quanh điểm đang chọn
    pCtx.save();
    pCtx.beginPath();
    pCtx.arc(targetX, targetY, 18, 0, Math.PI * 2);
    pCtx.fillStyle = 'rgba(139, 92, 246, 0.25)';
    pCtx.fill();
    pCtx.restore();

    // 6. Ghim Pin vị trí trung tâm
    pCtx.save();
    pCtx.beginPath();
    pCtx.arc(targetX, targetY, 9, 0, Math.PI * 2);
    pCtx.fillStyle = '#8B5CF6';
    pCtx.shadowColor = '#8B5CF6';
    pCtx.shadowBlur = 14;
    pCtx.fill();
    pCtx.strokeStyle = '#FFFFFF';
    pCtx.lineWidth = 2.5;
    pCtx.stroke();

    pCtx.beginPath();
    pCtx.arc(targetX, targetY, 3, 0, Math.PI * 2);
    pCtx.fillStyle = '#FFFFFF';
    pCtx.fill();
    pCtx.restore();

    // 7. Hộp Badge tọa độ nổi phía trên Pin
    pCtx.save();
    const coordText = `(${targetX.toFixed(1)}, ${targetY.toFixed(1)})`;
    pCtx.font = 'bold 11px "Plus Jakarta Sans", sans-serif';
    const textWidth = pCtx.measureText(coordText).width;
    const boxW = textWidth + 16;
    const boxH = 22;
    const boxX = targetX - boxW / 2;
    const boxY = targetY - 34;

    pCtx.fillStyle = 'rgba(17, 24, 39, 0.95)';
    pCtx.strokeStyle = 'rgba(139, 92, 246, 0.7)';
    pCtx.lineWidth = 1.5;
    pCtx.beginPath();
    if (pCtx.roundRect) {
        pCtx.roundRect(boxX, boxY, boxW, boxH, 6);
    } else {
        pCtx.rect(boxX, boxY, boxW, boxH);
    }
    pCtx.fill();
    pCtx.stroke();

    pCtx.fillStyle = '#E9D5FF';
    pCtx.textAlign = 'center';
    pCtx.textBaseline = 'middle';
    pCtx.fillText(coordText, targetX, boxY + boxH / 2);
    pCtx.restore();
}

// ------------------------------------------
// 9.2. Thêm / Sửa Điểm Tham quan
// ------------------------------------------
function openAddSpotModal() {
    document.getElementById('modalSpotTitle').innerHTML = '<i class="fa-solid fa-plus"></i> Thêm Điểm tham quan mới';
    document.getElementById('spotId').value = '';
    document.getElementById('spotName').value = '';

    // Tọa độ mặc định đẹp ở khoảng giữa bản đồ (Chuẩn hóa 1200 x 800)
    const defaultX = 600.0;
    const defaultY = 400.0;
    const defaultR = 50.0;

    document.getElementById('spotCoordX').value = defaultX.toFixed(1);
    document.getElementById('spotCoordY').value = defaultY.toFixed(1);
    document.getElementById('spotRadius').value = defaultR.toString();
    document.getElementById('spotRadiusSlider').value = defaultR.toString();
    document.getElementById('spotImagePath').value = '';
    document.getElementById('spotActive').checked = true;
    document.getElementById('spotImagePreviewBox').style.display = 'none';

    const badge = document.getElementById('pickerCoordsBadge');
    if (badge) badge.innerHTML = `<i class="fa-solid fa-location-pin"></i> Tọa độ: (${defaultX.toFixed(1)}, ${defaultY.toFixed(1)})`;

    openModal('modalSpot');

    // Chờ modal hiển thị rồi vẽ bản đồ chọn tọa độ
    setTimeout(() => {
        renderSpotPickerCanvas(defaultX, defaultY, defaultR, null);
    }, 60);
}

window.openEditSpotModal = function(spotId) {
    const spot = state.spots.find(s => s.maDiaDiem === spotId);
    if (!spot) return;

    document.getElementById('modalSpotTitle').innerHTML = '<i class="fa-solid fa-pen"></i> Chỉnh sửa Điểm tham quan';
    document.getElementById('spotId').value = spot.maDiaDiem;
    document.getElementById('spotName').value = spot.tenDiaDiem;
    document.getElementById('spotCoordX').value = spot.toaDoX.toFixed(1);
    document.getElementById('spotCoordY').value = spot.toaDoY.toFixed(1);
    document.getElementById('spotRadius').value = spot.phamViNhanDienMet;
    document.getElementById('spotRadiusSlider').value = spot.phamViNhanDienMet;
    document.getElementById('spotImagePath').value = spot.hinhAnhMinhHoa || '';
    document.getElementById('spotActive').checked = spot.hoatDong;

    const previewBox = document.getElementById('spotImagePreviewBox');
    const previewImg = document.getElementById('spotImagePreview');
    if (spot.hinhAnhMinhHoa) {
        previewImg.src = spot.hinhAnhMinhHoa;
        previewBox.style.display = 'block';
    } else {
        previewBox.style.display = 'none';
    }

    const badge = document.getElementById('pickerCoordsBadge');
    if (badge) badge.innerHTML = `<i class="fa-solid fa-location-pin"></i> Tọa độ: (${spot.toaDoX.toFixed(1)}, ${spot.toaDoY.toFixed(1)})`;

    openModal('modalSpot');

    // Chờ modal hiển thị rồi vẽ bản đồ với tọa độ hiện tại của điểm này
    setTimeout(() => {
        renderSpotPickerCanvas(spot.toaDoX, spot.toaDoY, spot.phamViNhanDienMet, spot.maDiaDiem);
    }, 60);
};

async function handleSpotSubmit(e) {
    e.preventDefault();
    const spotId = document.getElementById('spotId').value;
    const name = document.getElementById('spotName').value.trim();
    const coordX = parseFloat(document.getElementById('spotCoordX').value);
    const coordY = parseFloat(document.getElementById('spotCoordY').value);
    const radius = parseFloat(document.getElementById('spotRadius').value);
    const active = document.getElementById('spotActive').checked;
    let imagePath = document.getElementById('spotImagePath').value;

    // Nếu có chọn file ảnh mới -> Upload qua /api/admin/QuanLyDiaDiem/upload-anh
    const fileInput = document.getElementById('spotImageFile');
    if (fileInput.files.length > 0) {
        const formData = new FormData();
        formData.append('file', fileInput.files[0]);
        try {
            const uploadRes = await fetch(`${API_BASE}/QuanLyDiaDiem/upload-anh`, {
                method: 'POST',
                body: formData
            });
            if (uploadRes.ok) {
                const uploadData = await uploadRes.json();
                imagePath = uploadData.url;
            }
        } catch (err) {
            console.error('Upload image failed:', err);
        }
    }

    const payload = {
        maDiaDiem: spotId || '00000000-0000-0000-0000-000000000000',
        tenDiaDiem: name,
        toaDoX: coordX,
        toaDoY: coordY,
        phamViNhanDienMet: radius,
        hinhAnhMinhHoa: imagePath,
        hoatDong: active
    };

    try {
        let res;
        if (spotId) {
            res = await fetch(`${API_BASE}/QuanLyDiaDiem/${spotId}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
        } else {
            res = await fetch(`${API_BASE}/QuanLyDiaDiem`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
        }

        if (res.ok) {
            closeModal('modalSpot');
            showToast(spotId ? 'Đã cập nhật điểm tham quan!' : 'Đã thêm điểm tham quan mới!', 'success');
            await loadSpots();
        } else {
            showToast('Lỗi khi lưu điểm tham quan', 'error');
        }
    } catch (err) {
        console.error(err);
        showToast('Không thể kết nối đến máy chủ', 'error');
    }
}

window.deleteSpot = async function(spotId) {
    if (!confirm('Bạn có chắc chắn muốn xóa điểm tham quan này và toàn bộ âm thanh thuyết minh đi kèm?')) return;

    try {
        const res = await fetch(`${API_BASE}/QuanLyDiaDiem/${spotId}`, { method: 'DELETE' });
        if (res.ok) {
            showToast('Đã xóa điểm tham quan thành công!', 'success');
            await loadSpots();
        } else {
            showToast('Không thể xóa điểm tham quan', 'error');
        }
    } catch (err) {
        console.error(err);
        showToast('Lỗi khi gọi API xóa', 'error');
    }
};

// ------------------------------------------
// Tùy chỉnh Nhanh Bán kính Nhận diện (Radius)
// ------------------------------------------
window.openRadiusModal = function(spotId, spotName, currentRadius) {
    document.getElementById('radiusSpotId').value = spotId;
    document.getElementById('radiusSpotName').innerText = spotName;
    document.getElementById('quickRadiusInput').value = currentRadius;
    document.getElementById('quickRadiusSlider').value = currentRadius;
    openModal('modalRadius');
};

async function handleRadiusSubmit(e) {
    e.preventDefault();
    const spotId = document.getElementById('radiusSpotId').value;
    const radius = parseFloat(document.getElementById('quickRadiusInput').value);

    try {
        const res = await fetch(`${API_BASE}/QuanLyDiaDiem/${spotId}/pham-vi?banKinhMet=${radius}`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ phamViNhanDienMet: radius })
        });

        if (res.ok) {
            closeModal('modalRadius');
            showToast('Đã cập nhật bán kính nhận diện thành công!', 'success');
            await loadSpots();
        } else {
            showToast('Lỗi khi cập nhật bán kính', 'error');
        }
    } catch (err) {
        console.error(err);
        showToast('Lỗi mạng', 'error');
    }
}

// ==========================================
// 10. Quản lý Thuyết minh Đa ngôn ngữ (Tab 3)
// ==========================================
function renderNarrationSpotList() {
    const listEl = document.getElementById('narrationSpotList');
    if (!listEl) return;

    if (state.spots.length === 0) {
        listEl.innerHTML = `<p class="p-3 text-muted text-center">Chưa có điểm tham quan nào</p>`;
        return;
    }

    listEl.innerHTML = state.spots.map(s => `
        <div class="spot-selector-item ${state.selectedSpotId === s.maDiaDiem ? 'selected' : ''}" onclick="selectSpotForNarration('${s.maDiaDiem}')">
            <div class="spot-selector-info">
                <h4>${escapeHtml(s.tenDiaDiem)}</h4>
                <span>(${s.danhSachNgonNgu ? s.danhSachNgonNgu.length : 0} ngôn ngữ)</span>
            </div>
            <i class="fa-solid fa-chevron-right text-muted" style="font-size:0.8rem;"></i>
        </div>
    `).join('');
}

window.goToNarrationForSpot = function(spotId) {
    const tabNav = document.getElementById('navTabNarration');
    if (tabNav) tabNav.click();
    selectSpotForNarration(spotId);
};

window.selectSpotForNarration = async function(spotId) {
    state.selectedSpotId = spotId;
    renderNarrationSpotList();

    const spot = state.spots.find(s => s.maDiaDiem === spotId);
    if (!spot) return;

    document.getElementById('narrationSpotTitle').innerText = spot.tenDiaDiem;
    const addLangBtn = document.getElementById('btnOpenAddLangModal');
    if (addLangBtn) {
        addLangBtn.disabled = false;
        addLangBtn.onclick = () => openAddNarrationModal(spotId);
    }

    // Tải danh sách nội dung đa ngữ: GET /api/admin/QuanLyDiaDiem/{ma}/thuyet-minh
    try {
        const res = await fetch(`${API_BASE}/QuanLyDiaDiem/${spotId}/thuyet-minh`);
        if (res.ok) {
            const narrations = await res.json();
            renderLanguageCards(narrations);
        }
    } catch (err) {
        console.error('Failed to load narrations:', err);
    }
};

function renderLanguageCards(narrations) {
    const container = document.getElementById('languageCardsContainer');
    if (!container) return;

    if (!narrations || narrations.length === 0) {
        container.innerHTML = `
            <div class="empty-placeholder">
                <i class="fa-solid fa-file-audio text-4xl mb-3 text-muted"></i>
                <p>Điểm này chưa có file âm thanh thuyết minh nào.</p>
                <button class="btn btn-primary mt-2" onclick="openAddNarrationModal('${state.selectedSpotId}')">
                    <i class="fa-solid fa-plus"></i> Thêm tệp Audio & Văn bản ngay
                </button>
            </div>
        `;
        return;
    }

    container.innerHTML = narrations.map(n => `
        <div class="lang-card">
            <div class="lang-header">
                <span class="lang-badge">${n.maNgonNgu.toUpperCase()} - ${escapeHtml(n.tenNgonNgu)}</span>
                <button class="btn btn-sm btn-danger" onclick="deleteNarration('${n.maNoiDung}')" title="Xóa bản dịch này">
                    <i class="fa-solid fa-trash"></i>
                </button>
            </div>
            <h4>${escapeHtml(n.tieuDe)}</h4>
            <p class="narration-text">${escapeHtml(n.vanBanThuyetMinh || 'Không có văn bản mô tả.')}</p>
            
            ${n.duongDanAmThanh ? `
                <div class="audio-player-box">
                    <audio controls src="${n.duongDanAmThanh}" preload="none"></audio>
                </div>
            ` : '<p class="text-muted" style="font-size:0.8rem;"><i class="fa-solid fa-triangle-exclamation text-amber"></i> Chưa có file âm thanh</p>'}
        </div>
    `).join('');
}

window.openAddNarrationModal = function(spotId) {
    document.getElementById('narrSpotId').value = spotId;
    document.getElementById('narrTitle').value = '';
    document.getElementById('narrContent').value = '';
    document.getElementById('narrAudioFile').value = '';
    document.getElementById('narrTextFile').value = '';
    openModal('modalNarration');
};

async function handleNarrationSubmit(e) {
    e.preventDefault();
    const spotId = document.getElementById('narrSpotId').value;
    const langSelect = document.getElementById('narrLangCode');
    const langCode = langSelect.value;
    const langName = langSelect.options[langSelect.selectedIndex].text.split('(')[0].trim();
    const title = document.getElementById('narrTitle').value.trim();
    const content = document.getElementById('narrContent').value.trim();
    const audioFileInput = document.getElementById('narrAudioFile');

    let audioUrl = '';

    // 1. Nếu có file âm thanh mới -> upload lên /api/admin/QuanLyDiaDiem/upload-audio
    if (audioFileInput.files.length > 0) {
        const audioForm = new FormData();
        audioForm.append('file', audioFileInput.files[0]);
        try {
            const uploadRes = await fetch(`${API_BASE}/QuanLyDiaDiem/upload-audio`, {
                method: 'POST',
                body: audioForm
            });
            if (uploadRes.ok) {
                const data = await uploadRes.json();
                audioUrl = data.url;
            }
        } catch (err) {
            console.error('Audio upload failed:', err);
            showToast('Không thể tải file âm thanh', 'error');
            return;
        }
    } else {
        showToast('Vui lòng chọn tệp âm thanh thuyết minh', 'error');
        return;
    }

    // 2. Lưu thông tin thuyết minh đa ngôn ngữ qua /api/admin/QuanLyDiaDiem/thuyet-minh
    const payload = {
        maDiaDiem: spotId,
        maNgonNgu: langCode,
        tenNgonNgu: langName,
        tieuDe: title,
        vanBanThuyetMinh: content,
        duongDanAmThanh: audioUrl,
        thoiLuongGiay: 60
    };

    try {
        const res = await fetch(`${API_BASE}/QuanLyDiaDiem/thuyet-minh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (res.ok) {
            closeModal('modalNarration');
            showToast('Đã lưu nội dung thuyết minh và audio thành công!', 'success');
            await selectSpotForNarration(spotId);
            await loadSpots();
        } else {
            showToast('Lỗi khi lưu thuyết minh', 'error');
        }
    } catch (err) {
        console.error(err);
        showToast('Lỗi mạng khi lưu thuyết minh', 'error');
    }
}

window.deleteNarration = async function(contentId) {
    if (!confirm('Bạn có chắc muốn xóa bản thuyết minh này?')) return;

    try {
        const res = await fetch(`${API_BASE}/QuanLyDiaDiem/thuyet-minh/${contentId}`, { method: 'DELETE' });
        if (res.ok) {
            showToast('Đã xóa thuyết minh!', 'success');
            if (state.selectedSpotId) {
                selectSpotForNarration(state.selectedSpotId);
                loadSpots();
            }
        }
    } catch (err) {
        console.error(err);
        showToast('Lỗi khi xóa thuyết minh', 'error');
    }
};

// ==========================================
// 11. Quản lý Sơ đồ Bản đồ Di tích (Tab 4)
// ==========================================
function updateMapTabUI(map) {
    const imgEl = document.getElementById('activeMapImage');
    const placeholder = document.getElementById('activeMapPlaceholder');
    if (!imgEl || !placeholder) return;

    if (map && map.hinhAnhBanDo) {
        imgEl.src = map.hinhAnhBanDo;
        imgEl.style.display = 'block';
        placeholder.style.display = 'none';
    } else {
        imgEl.style.display = 'none';
        placeholder.style.display = 'block';
    }
}

async function handleMapSubmit(e) {
    e.preventDefault();
    const title = document.getElementById('inputMapTitle').value.trim();
    const width = parseFloat(document.getElementById('inputMapWidth').value);
    const height = parseFloat(document.getElementById('inputMapHeight').value);
    const scale = parseFloat(document.getElementById('inputMapScale').value);
    const fileInput = document.getElementById('inputMapImageFile');

    if (fileInput.files.length === 0) {
        showToast('Vui lòng chọn file hình ảnh sơ đồ bản đồ', 'error');
        return;
    }

    const formData = new FormData();
    formData.append('file', fileInput.files[0]);

    try {
        // 1. Tải lên tệp ảnh sơ đồ: POST /api/admin/QuanLyDiaDiem/bando/upload-anh
        const uploadRes = await fetch(`${API_BASE}/QuanLyDiaDiem/bando/upload-anh`, {
            method: 'POST',
            body: formData
        });

        if (!uploadRes.ok) {
            showToast('Không thể tải lên ảnh sơ đồ', 'error');
            return;
        }

        const uploadData = await uploadRes.json();

        // 2. Lưu thông tin bản đồ: POST /api/admin/QuanLyDiaDiem/bando
        const mapPayload = {
            tenBanDo: title,
            hinhAnhBanDo: uploadData.url,
            chieuRongPixel: width,
            chieuCaoPixel: height,
            tiLeMetTrenPixel: scale,
            hoatDong: true
        };

        const saveRes = await fetch(`${API_BASE}/QuanLyDiaDiem/bando`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(mapPayload)
        });

        if (saveRes.ok) {
            showToast('Đã lưu và cập nhật sơ đồ bản đồ thành công!', 'success');
            await loadMap();
        } else {
            showToast('Lỗi khi lưu thông tin bản đồ', 'error');
        }
    } catch (err) {
        console.error(err);
        showToast('Lỗi mạng', 'error');
    }
}

// ==========================================
// 12. Tiện ích: Toast Notifications & Escape HTML
// ==========================================
function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    
    let icon = '<i class="fa-solid fa-circle-info"></i>';
    if (type === 'success') icon = '<i class="fa-solid fa-circle-check text-accent"></i>';
    if (type === 'error') icon = '<i class="fa-solid fa-circle-exclamation" style="color:var(--accent-rose);"></i>';

    toast.innerHTML = `${icon} <span>${escapeHtml(message)}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateX(20px)';
        toast.style.transition = 'all 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 3500);
}

function escapeHtml(str) {
    if (!str) return '';
    return str
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}
