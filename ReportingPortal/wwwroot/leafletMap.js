window.MAP = {
    _mapInstance: null,
    _tempMarker: null,
    markersLayer: null,

    initMap(clickEnabled = false) {
        const container = document.getElementById('map');
        if (!container) return;

        if (this._mapInstance) {
            this._mapInstance.remove();
        }

        const map = L.map(container, {
            center: [45.6983, 9.6773], // Bergamo
            zoom: 13,
            minZoom: 6,
            maxZoom: 18
        });

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        this._mapInstance = map;
        this.markersLayer = L.layerGroup().addTo(map);

        if (clickEnabled) {
            map.on('dblclick', (e) => this._placeTempMarker(e.latlng));
        } else {
            map.on('moveend zoomend', () => this._fetchClusters());
            this._fetchClusters();
        }

        setTimeout(() => map.invalidateSize(), 100);
    },

    async initMapSingle(lat, lng) {
        if (!lat || !lng) return;

        function delay(ms) {
            return new Promise(resolve => setTimeout(resolve, ms));
        }
        await delay(250);

        const container = document.getElementById('mapPos');
        if (!container) return;

        const map = L.map('mapPos').setView([lat, lng], 15);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);

        L.marker([lat, lng]).addTo(map);
        setTimeout(() => map.invalidateSize(), 100);
    },

    _placeTempMarker(latlng) {
        if (this._tempMarker) {
            this._mapInstance.removeLayer(this._tempMarker);
        }

        this._tempMarker = L.marker(latlng, { draggable: true }).addTo(this._mapInstance);

        const panel = document.getElementById("marker-confirm-panel");
        if (panel) {
            panel.style.display = "flex";
        }
    },

    _fetchClusters() {
        const bounds = this._mapInstance.getBounds();
        const zoom = this._mapInstance.getZoom();

        const params = new URLSearchParams({
            minLat: bounds.getSouthWest().lat,
            minLng: bounds.getSouthWest().lng,
            maxLat: bounds.getNorthEast().lat,
            maxLng: bounds.getNorthEast().lng,
            zoom: zoom
        });

        fetch(`https://localhost:7078/api/Report/cluster?${params.toString()}`, {
            method: 'GET',
            headers: { 'Accept': 'application/json' }
        })
            .then(res => {
                if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
                return res.json();
            })
            .then(data => this._renderClusters(data.clusters))
            .catch(err => console.error('Errore fetch clusters:', err));
    },

    _renderClusters(clusters) {
        this.markersLayer.clearLayers();

        clusters.forEach(cluster => {
            const items = cluster.items;
            if (!items || items.length === 0) return;

            if (items.length > 1) {
                this._createClusterMarker(items);
            } else {
                this._createReportMarker(items[0]);
            }
        });
    },

    _createClusterMarker(items) {
        const [lat, lng] = [items[0].latitude, items[0].longitude];

        const clusterMarker = L.marker([lat, lng], {
            icon: L.divIcon({
                html: `<div class="cluster-marker">${items.length}</div>`,
                className: 'custom-cluster-icon',
                iconSize: [30, 30]
            })
        });

        clusterMarker.on('click', () => {
            const newZoom = Math.min(this._mapInstance.getZoom() + 2, this._mapInstance.getMaxZoom());
            this._mapInstance.setView([lat, lng], newZoom);
        });

        clusterMarker.addTo(this.markersLayer);
    },

    _createReportMarker(report) {
        const marker = L.marker([report.latitude, report.longitude]);

        const statusTextMap = {
            0: "Pending",
            1: "InProgress",
            2: "Resolved",
            3: "Rejected"
        };

        marker.bindPopup(`
            <strong>${report.title}</strong><br/>
            Status: ${statusTextMap[report.status] || "Unknown"}<br/>
        `);

        marker.addTo(this.markersLayer);
    },

    async confirmMarker() {
        if (!this._tempMarker) return null;

        const latlng = this._tempMarker.getLatLng();
        this._mapInstance.removeLayer(this._tempMarker);
        this._tempMarker = null;
        document.getElementById("marker-confirm-panel").style.display = "none";

        const address = await fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${latlng.lat}&lon=${latlng.lng}`)
            .then(res => res.json())
            .then(data => data.display_name || "")
            .catch(() => "");

        return { lat: latlng.lat, lng: latlng.lng, address };
    },

    cancelMarker() {
        if (this._mapInstance && this._tempMarker) {
            this._mapInstance.removeLayer(this._tempMarker);
            this._tempMarker = null;
        }

        document.getElementById("marker-confirm-panel").style.display = "none";
    }
};
