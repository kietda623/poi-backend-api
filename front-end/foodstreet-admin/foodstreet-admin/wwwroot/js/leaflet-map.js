window.leafletInterop = {
    pickerMap: null,
    pickerMarker: null,
    appMap: null,
    userMarker: null,
    poiMarkers: [],
    watchId: null,

    // Dùng cho Seller: Pick Location
    initPickerMap: function (elementId, defaultLat, defaultLng, dotNetObj) {
        if (!document.getElementById(elementId)) return;
        
        // Khởi tạo map
        if (this.pickerMap) {
            this.pickerMap.remove();
        }
        
        let lat = defaultLat || 10.762622;
        let lng = defaultLng || 106.660172;
        
        this.pickerMap = L.map(elementId).setView([lat, lng], 15);
        
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(this.pickerMap);

        this.pickerMarker = L.marker([lat, lng], { draggable: true }).addTo(this.pickerMap);

        // Update khi kéo marker
        this.pickerMarker.on('dragend', function (e) {
            var position = e.target.getLatLng();
            dotNetObj.invokeMethodAsync('OnLocationChanged', position.lat, position.lng);
        });

        // Update khi click trên map
        this.pickerMap.on('click', (e) => {
            this.pickerMarker.setLatLng(e.latlng);
            dotNetObj.invokeMethodAsync('OnLocationChanged', e.latlng.lat, e.latlng.lng);
        });
        
        return true;
    },

    getCurrentLocation: function (dotNetObj) {
        if ("geolocation" in navigator) {
            navigator.geolocation.getCurrentPosition((position) => {
                let lat = position.coords.latitude;
                let lng = position.coords.longitude;
                if (this.pickerMarker && this.pickerMap) {
                    this.pickerMarker.setLatLng([lat, lng]);
                    this.pickerMap.setView([lat, lng], 17);
                }
                dotNetObj.invokeMethodAsync('OnLocationChanged', lat, lng);
            }, (error) => {
                console.error("Lỗi lấy vị trí: ", error);
            });
        }
    },

    // Dùng cho User App View
    initAppMap: function (elementId, dotNetObj, stores) {
        if (!document.getElementById(elementId)) return;

        if (this.appMap) {
            this.appMap.remove();
        }

        this.appMap = L.map(elementId).setView([10.762622, 106.660172], 14);
        
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap'
        }).addTo(this.appMap);

        // Icon custom
        var userIcon = L.icon({
            iconUrl: 'https://cdn-icons-png.flaticon.com/512/3135/3135715.png', // Icon người
            iconSize: [32, 32],
            iconAnchor: [16, 32]
        });

        var storeIcon = L.icon({
            iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41]
        });

        var storeHighlightIcon = L.icon({
            iconUrl: 'https://cdn-icons-png.flaticon.com/512/14090/14090382.png', // Đỏ Highlight
            iconSize: [36, 48],
            iconAnchor: [18, 48]
        });
        
        // Add user marker
        this.userMarker = L.marker([0, 0], { icon: userIcon }).addTo(this.appMap);

        // Render stores
        this.poiMarkers = [];
        stores.forEach(store => {
            if (store.latitude && store.longitude) {
                var marker = L.marker([store.latitude, store.longitude], { icon: storeIcon }).addTo(this.appMap);
                marker.bindPopup(`<b>${store.name}</b><br/>${store.category}`);
                marker.storeId = store.id;
                this.poiMarkers.push(marker);
            }
        });

        // Resize bug fix in layout
        setTimeout(() => this.appMap.invalidateSize(), 500);

        // Start tracking
        if ("geolocation" in navigator) {
            this.watchId = navigator.geolocation.watchPosition((pos) => {
                let lat = pos.coords.latitude;
                let lng = pos.coords.longitude;
                this.userMarker.setLatLng([lat, lng]);
                
                // Nếu là lần đầu quét dc pos, thì bay đến đó
                if (!this.userLocationFixed) {
                    this.appMap.setView([lat, lng], 16);
                    this.userLocationFixed = true;
                }

                // Cập nhật C# logic
                dotNetObj.invokeMethodAsync('OnUserLocationUpdated', lat, lng);
            }, (err) => {
                console.warn(err);
            }, { enableHighAccuracy: true });
        }
    },

    highlightPoi: function (storeId) {
        if (!this.appMap || !this.poiMarkers) return;

        var storeHighlightIcon = L.icon({
            iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41]
        });

        var normalIcon = L.icon({
            iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
            iconSize: [25, 41],
            iconAnchor: [12, 41]
        });

        this.poiMarkers.forEach(m => {
            if (m.storeId === storeId) {
                m.setIcon(storeHighlightIcon);
                // m.openPopup();
            } else {
                m.setIcon(normalIcon);
            }
        });
    },
    
    destroyAppMap: function() {
        if (this.watchId) navigator.geolocation.clearWatch(this.watchId);
        if (this.appMap) this.appMap.remove();
        this.appMap = null;
        this.userLocationFixed = false;
    }
};

// ── QR Code Download Helper ──────────────────────────────────────────────────
// Called by Blazor JS interop: JS.InvokeVoidAsync("downloadFileFromUrl", url, fileName)
window.downloadFileFromUrl = async function (url, fileName) {
    try {
        const response = await fetch(url, { mode: 'cors' });
        const blob = await response.blob();
        const blobUrl = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = fileName || 'qr-code.png';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Cleanup the blob URL after a short delay
        setTimeout(() => URL.revokeObjectURL(blobUrl), 2000);
    } catch (e) {
        console.error('downloadFileFromUrl failed:', e);
    }
};

window.printQrImage = function (url, title) {
    const printWindow = window.open('', '_blank', 'noopener,noreferrer,width=900,height=900');
    if (!printWindow) {
        return;
    }

    const safeTitle = title || 'FoodStreet QR';
    printWindow.document.write(`
        <html>
        <head>
            <title>${safeTitle}</title>
            <style>
                body {
                    margin: 0;
                    min-height: 100vh;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    font-family: Arial, sans-serif;
                    background: #ffffff;
                }
                .sheet {
                    text-align: center;
                    padding: 24px;
                }
                img {
                    width: 360px;
                    height: 360px;
                    display: block;
                    margin: 0 auto 16px;
                }
                h1 {
                    margin: 0 0 8px;
                    font-size: 28px;
                }
                p {
                    margin: 0;
                    color: #555;
                }
            </style>
        </head>
        <body>
            <div class="sheet">
                <img src="${url}" alt="${safeTitle}" />
                <h1>${safeTitle}</h1>
                <p>Quet ma de tai app FoodStreet</p>
            </div>
        </body>
        </html>
    `);
    printWindow.document.close();
    printWindow.focus();
    printWindow.onload = () => {
        printWindow.print();
    };
};
