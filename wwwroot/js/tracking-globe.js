/**
 * OrbitWatch — 3D Satellite Tracking Globe
 * Uses Three.js to render a WebGL Earth globe and position satellite markers
 * from the OrbitWatch position API (SGP4-propagated).
 *
 * Coordinate convention:
 *   API returns: latitudeDegrees (-90..+90), longitudeDegrees (-180..+180), altitudeKm
 *   Three.js uses: right-handed Y-up coordinate system
 *   Conversion: geodetic (lat, lon, alt) → Cartesian (x, y, z) via standard spherical transform
 */

(function () {
    'use strict';

    // =========================================================================
    //  CONSTANTS
    // =========================================================================
    const EARTH_RADIUS = 50;                   // Three.js units for Earth sphere
    const EARTH_RADIUS_KM = 6371;             // Mean Earth radius in km
    const SATELLITE_SCALE_FACTOR = EARTH_RADIUS / EARTH_RADIUS_KM;
    const API_BASE = '/api/orbit/satellites';
    const REFRESH_INTERVAL_MS = 10000;         // Auto-refresh every 10 seconds

    // =========================================================================
    //  DOM REFERENCES
    // =========================================================================
    const canvas = document.getElementById('owGlobeCanvas');
    const container = document.getElementById('owGlobeContainer');
    const placeholder = document.getElementById('owGlobePlaceholder');
    const satelliteSelect = document.getElementById('owSatelliteSelect');
    const satInfoPanel = document.getElementById('owSatInfo');
    const loadingMsg = document.getElementById('owLoadingMsg');
    const errorMsg = document.getElementById('owErrorMsg');
    const errorText = document.getElementById('owErrorText');
    const refreshBtn = document.getElementById('owRefreshBtn');

    // Info display elements
    const infoName = document.getElementById('owInfoName');
    const infoNorad = document.getElementById('owInfoNorad');
    const infoOrbit = document.getElementById('owInfoOrbit');
    const infoLat = document.getElementById('owInfoLat');
    const infoLon = document.getElementById('owInfoLon');
    const infoAlt = document.getElementById('owInfoAlt');
    const infoVel = document.getElementById('owInfoVel');
    const propTimestamp = document.getElementById('owPropTimestamp');
    const satStatus = document.getElementById('owSatStatus');

    // =========================================================================
    //  STATE
    // =========================================================================
    let scene, camera, renderer, earth, satelliteMarker, satelliteLabel;
    let animationId = null;
    let refreshTimerId = null;
    let isDragging = false;
    let previousMouse = { x: 0, y: 0 };
    let spherical = { theta: 0, phi: Math.PI / 4 };
    let cameraDistance = 180;
    let initialized = false;
    let currentSatelliteId = null;

    // =========================================================================
    //  COORDINATE CONVERSION
    //  Geodetic (lat°, lon°, altKm) → Three.js Cartesian (x, y, z)
    //
    //  Standard spherical-to-Cartesian with Y-up convention:
    //    x = r * cos(lat) * sin(lon)
    //    y = r * sin(lat)
    //    z = r * cos(lat) * cos(lon)
    //
    //  Where r = EARTH_RADIUS + (altKm * scale)
    // =========================================================================
    function geoToCartesian(latDeg, lonDeg, altKm) {
        const latRad = latDeg * (Math.PI / 180);
        const lonRad = lonDeg * (Math.PI / 180);
        const r = EARTH_RADIUS + (altKm * SATELLITE_SCALE_FACTOR);

        return {
            x: r * Math.cos(latRad) * Math.sin(lonRad),
            y: r * Math.sin(latRad),
            z: r * Math.cos(latRad) * Math.cos(lonRad)
        };
    }

    // =========================================================================
    //  THREE.JS SCENE INITIALIZATION
    // =========================================================================
    function initScene() {
        if (initialized) return;

        const width = container.clientWidth;
        const height = container.clientHeight;

        // Scene
        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x0A0A0A);

        // Camera
        camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
        updateCameraPosition();

        // Renderer
        renderer = new THREE.WebGLRenderer({
            canvas: canvas,
            antialias: true,
            alpha: false
        });
        renderer.setSize(width, height);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

        // Lighting — restrained, editorial
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.3);
        scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.9);
        directionalLight.position.set(100, 60, 100);
        scene.add(directionalLight);

        const rimLight = new THREE.DirectionalLight(0xffffff, 0.2);
        rimLight.position.set(-80, -30, -80);
        scene.add(rimLight);

        // Earth sphere — dark editorial style with wireframe overlay
        createEarth();

        // Satellite marker (initially hidden)
        createSatelliteMarker();

        // Event listeners
        setupInteraction();

        // Resize handler
        window.addEventListener('resize', onResize);

        initialized = true;
        placeholder.style.display = 'none';
        canvas.style.display = 'block';

        animate();
    }

    function createEarth() {
        const textureLoader = new THREE.TextureLoader();
        
        // Solid textured earth sphere
        const earthGeometry = new THREE.SphereGeometry(EARTH_RADIUS, 64, 64);
        const earthMaterial = new THREE.MeshPhongMaterial({
            map: textureLoader.load('/assets/textures/earth_surface.jpg'),
            specular: new THREE.Color(0x222222),
            shininess: 15
        });
        earth = new THREE.Mesh(earthGeometry, earthMaterial);
        scene.add(earth);

        // Cloud layer
        const cloudGeometry = new THREE.SphereGeometry(EARTH_RADIUS + 0.25, 64, 64);
        const cloudMaterial = new THREE.MeshPhongMaterial({
            map: textureLoader.load('/assets/textures/earth_clouds.png'),
            transparent: true,
            opacity: 0.6,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });
        const clouds = new THREE.Mesh(cloudGeometry, cloudMaterial);
        earth.add(clouds);

        // Atmosphere glow
        const atmosGeometry = new THREE.SphereGeometry(EARTH_RADIUS + 1.5, 64, 64);
        const atmosMaterial = new THREE.MeshPhongMaterial({
            color: 0x4ca6ff,
            transparent: true,
            opacity: 0.15,
            side: THREE.BackSide,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });
        const atmosphere = new THREE.Mesh(atmosGeometry, atmosMaterial);
        earth.add(atmosphere);
    }

    function createSatelliteMarker() {
        // Main marker — bright white octahedron
        const markerGeo = new THREE.OctahedronGeometry(1.8, 0);
        const markerMat = new THREE.MeshBasicMaterial({ color: 0xffffff });
        satelliteMarker = new THREE.Mesh(markerGeo, markerMat);
        satelliteMarker.visible = false;
        scene.add(satelliteMarker);

        // Pulsing ring around marker
        const ringGeo = new THREE.RingGeometry(2.5, 3.0, 32);
        const ringMat = new THREE.MeshBasicMaterial({
            color: 0xD92626,
            side: THREE.DoubleSide,
            transparent: true,
            opacity: 0.7
        });
        satelliteLabel = new THREE.Mesh(ringGeo, ringMat);
        satelliteLabel.visible = false;
        scene.add(satelliteLabel);

        // Line from satellite to Earth surface (nadir line)
        const nadirMat = new THREE.LineBasicMaterial({
            color: 0xD92626,
            transparent: true,
            opacity: 0.4
        });
        const nadirGeo = new THREE.BufferGeometry();
        const nadirLine = new THREE.Line(nadirGeo, nadirMat);
        nadirLine.name = 'nadirLine';
        nadirLine.visible = false;
        scene.add(nadirLine);
    }

    // =========================================================================
    //  CAMERA ORBIT CONTROLS (mouse/touch)
    // =========================================================================
    function updateCameraPosition() {
        if (!camera) return;
        camera.position.x = cameraDistance * Math.sin(spherical.phi) * Math.sin(spherical.theta);
        camera.position.y = cameraDistance * Math.cos(spherical.phi);
        camera.position.z = cameraDistance * Math.sin(spherical.phi) * Math.cos(spherical.theta);
        camera.lookAt(0, 0, 0);
    }

    function setupInteraction() {
        canvas.addEventListener('pointerdown', (e) => {
            isDragging = true;
            previousMouse = { x: e.clientX, y: e.clientY };
            canvas.style.cursor = 'grabbing';
        });

        window.addEventListener('pointermove', (e) => {
            if (!isDragging) return;
            const dx = e.clientX - previousMouse.x;
            const dy = e.clientY - previousMouse.y;
            spherical.theta -= dx * 0.005;
            spherical.phi = Math.max(0.1, Math.min(Math.PI - 0.1, spherical.phi + dy * 0.005));
            previousMouse = { x: e.clientX, y: e.clientY };
            updateCameraPosition();
        });

        window.addEventListener('pointerup', () => {
            isDragging = false;
            canvas.style.cursor = 'grab';
        });

        canvas.addEventListener('wheel', (e) => {
            e.preventDefault();
            cameraDistance = Math.max(80, Math.min(400, cameraDistance + e.deltaY * 0.15));
            updateCameraPosition();
        }, { passive: false });

        canvas.style.cursor = 'grab';
    }

    function onResize() {
        if (!renderer || !camera) return;
        const width = container.clientWidth;
        const height = container.clientHeight;
        camera.aspect = width / height;
        camera.updateProjectionMatrix();
        renderer.setSize(width, height);
    }

    // =========================================================================
    //  ANIMATION LOOP
    // =========================================================================
    function animate() {
        animationId = requestAnimationFrame(animate);

        // Slow Earth rotation
        if (earth && !isDragging) {
            earth.rotation.y += 0.0003;
        }

        // Pulse satellite ring
        if (satelliteLabel && satelliteLabel.visible) {
            const scale = 1 + 0.15 * Math.sin(Date.now() * 0.003);
            satelliteLabel.scale.set(scale, scale, scale);
        }

        // Rotate satellite marker
        if (satelliteMarker && satelliteMarker.visible) {
            satelliteMarker.rotation.y += 0.02;
            satelliteMarker.rotation.x += 0.005;
        }

        renderer.render(scene, camera);
    }

    // =========================================================================
    //  SATELLITE POSITION FROM API
    // =========================================================================
    function updateSatellitePosition(satelliteId) {
        showLoading();
        hideError();

        const url = `${API_BASE}/${satelliteId}/position`;

        fetch(url)
            .then(response => {
                if (response.status === 404) {
                    throw new Error('Satellite not found in system.');
                }
                if (response.status === 502 || response.status === 503) {
                    throw new Error('CelesTrak orbital data temporarily unavailable.');
                }
                if (!response.ok) {
                    throw new Error(`Position API returned ${response.status}.`);
                }
                return response.json();
            })
            .then(position => {
                if (currentSatelliteId !== satelliteId) return; // Prevent race conditions
                
                hideLoading();

                if (!position || position.latitudeDegrees === undefined) {
                    throw new Error('Invalid position data received.');
                }

                // Validate ranges
                if (position.latitudeDegrees < -90 || position.latitudeDegrees > 90) {
                    throw new Error(`Invalid latitude: ${position.latitudeDegrees}`);
                }
                if (position.longitudeDegrees < -180 || position.longitudeDegrees > 180) {
                    throw new Error(`Invalid longitude: ${position.longitudeDegrees}`);
                }

                // Position the satellite marker
                const cartesian = geoToCartesian(
                    position.latitudeDegrees,
                    position.longitudeDegrees,
                    position.altitudeKm
                );

                satelliteMarker.position.set(cartesian.x, cartesian.y, cartesian.z);
                satelliteMarker.visible = true;

                // Position the ring at the same location, facing camera
                satelliteLabel.position.set(cartesian.x, cartesian.y, cartesian.z);
                satelliteLabel.lookAt(camera.position);
                satelliteLabel.visible = true;

                // Draw nadir line from satellite to Earth surface
                const surfacePos = geoToCartesian(
                    position.latitudeDegrees,
                    position.longitudeDegrees,
                    0
                );
                const nadirLine = scene.getObjectByName('nadirLine');
                if (nadirLine) {
                    const points = [
                        new THREE.Vector3(cartesian.x, cartesian.y, cartesian.z),
                        new THREE.Vector3(surfacePos.x, surfacePos.y, surfacePos.z)
                    ];
                    nadirLine.geometry.dispose();
                    nadirLine.geometry = new THREE.BufferGeometry().setFromPoints(points);
                    nadirLine.visible = true;
                }

                // Update info panel
                updateInfoPanel(position, satelliteId);
                showInfoPanel();
            })
            .catch(err => {
                if (currentSatelliteId !== satelliteId) return;
                
                hideLoading();
                showError(err.message || 'Failed to fetch satellite position.');
                hideSatelliteMarker();
            });
    }

    function hideSatelliteMarker() {
        if (satelliteMarker) satelliteMarker.visible = false;
        if (satelliteLabel) satelliteLabel.visible = false;
        const nadirLine = scene.getObjectByName('nadirLine');
        if (nadirLine) nadirLine.visible = false;
    }

    // =========================================================================
    //  INFO PANEL UPDATE
    // =========================================================================
    function updateInfoPanel(position, satelliteId) {
        const selected = Array.from(satelliteSelect.options).find(opt => opt.value === satelliteId);
        if (!selected) return;

        infoName.textContent = selected.dataset.name || '—';
        infoNorad.textContent = selected.dataset.norad || '—';
        infoOrbit.textContent = selected.dataset.orbit || '—';

        const statusText = selected.dataset.status || 'Unknown';
        satStatus.textContent = statusText;
        satStatus.className = 'ow-status ' +
            (statusText.toLowerCase() === 'active' ? 'ow-status--active' : '');

        infoLat.textContent = position.latitudeDegrees.toFixed(4) + '°';
        infoLon.textContent = position.longitudeDegrees.toFixed(4) + '°';
        infoAlt.textContent = position.altitudeKm.toFixed(2) + ' km';
        infoVel.textContent = position.velocityKmPerSec.toFixed(4) + ' km/s';

        const ts = new Date(position.timestampUtc);
        propTimestamp.textContent = ts.toISOString().replace('T', ' ').substring(0, 19) + ' UTC';
        
        if (position.isStaleOrbitalData) {
            propTimestamp.textContent += ' (STALE)';
            propTimestamp.style.color = 'var(--ow-yellow)';
        } else {
            propTimestamp.style.color = '';
        }
    }

    // =========================================================================
    //  UI STATE HELPERS
    // =========================================================================
    function showLoading() { loadingMsg.style.display = 'block'; }
    function hideLoading() { loadingMsg.style.display = 'none'; }
    function showError(msg) {
        errorText.textContent = msg;
        errorMsg.style.display = 'block';
    }
    function hideError() { errorMsg.style.display = 'none'; }
    function showInfoPanel() { satInfoPanel.style.display = 'block'; }
    function hideInfoPanel() { satInfoPanel.style.display = 'none'; }

    // =========================================================================
    //  AUTO-REFRESH
    // =========================================================================
    function startRefreshTimer() {
        stopRefreshTimer();
        refreshTimerId = setInterval(() => {
            if (currentSatelliteId) {
                updateSatellitePosition(currentSatelliteId);
            }
        }, REFRESH_INTERVAL_MS);
    }

    function stopRefreshTimer() {
        if (refreshTimerId) {
            clearInterval(refreshTimerId);
            refreshTimerId = null;
        }
    }

    // =========================================================================
    //  EVENT HANDLERS
    // =========================================================================
    satelliteSelect.addEventListener('change', function () {
        const id = this.value;
        stopRefreshTimer();

        if (!id) {
            currentSatelliteId = null;
            hideInfoPanel();
            hideError();
            hideLoading();
            if (initialized) hideSatelliteMarker();
            return;
        }

        currentSatelliteId = id;
        initScene();
        updateSatellitePosition(id);
        startRefreshTimer();
    });

    refreshBtn.addEventListener('click', function () {
        if (currentSatelliteId) {
            updateSatellitePosition(currentSatelliteId);
        }
    });

    // Initialize if a satellite is already selected
    if (satelliteSelect.value) {
        satelliteSelect.dispatchEvent(new Event('change'));
    }

})();
