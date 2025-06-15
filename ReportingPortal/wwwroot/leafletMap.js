window.MAP = {
    _mapInstance: null,
    _tempMarker: null,

    initMap: function (clickEnabled = false) {
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

        if (clickEnabled) {
            map.on('dblclick', (e) => {
                this._placeTempMarker(map, e.latlng);
            });
        }

        this._mapInstance = map;

        setTimeout(() => map.invalidateSize(), 100);
    },

    _placeTempMarker: function (map, latlng) {
        if (this._tempMarker) {
            map.removeLayer(this._tempMarker);
        }

        this._tempMarker = L.marker(latlng, { draggable: true }).addTo(map);

        const panel = document.getElementById("marker-confirm-panel");
        if (panel) {
            panel.style.display = "flex";
        }
    },

    confirmMarker: async function () {
        let result = null;
        if (this._tempMarker) {
            result = this._tempMarker;
            this._tempMarker = null;
        }

        document.getElementById("marker-confirm-panel").style.display = "none";
        if (result) {
            const address = await fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${result.getLatLng().lat}&lon=${result.getLatLng().lng}`)
                .then(res => res.json())
                .then(data => data.display_name || "")
                .catch(() => "");

            return { lat: result.getLatLng().lat, lng: result.getLatLng().lng, address: address };
        }

        return null;
    },

    cancelMarker: function () {
        if (this._mapInstance && this._tempMarker) {
            this._mapInstance.removeLayer(this._tempMarker);
            this._tempMarker = null;
        }

        document.getElementById("marker-confirm-panel").style.display = "none";
    }
};
