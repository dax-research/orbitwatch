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

    // Camera control buttons
    const focusBtn = document.getElementById('owFocusBtn');
    const followBtn = document.getElementById('owFollowBtn');

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

    // Overlay elements
    const satLabelOverlay = document.getElementById('owSatLabelOverlay');
    const satLabelName = document.getElementById('owSatLabelName');
    const satLabelNorad = document.getElementById('owSatLabelNorad');

    // =========================================================================
    //  STATE
    // =========================================================================
    let scene, camera, renderer;
    let earthGroup, trackingGroup, earth, satelliteMarker;
    let subSatPoint, nadirLine;
    let fullOrbitPathLine, next5MinPathLine;
    
    let animationId = null;
    let refreshTimerId = null;
    let isDragging = false;
    let previousMouse = { x: 0, y: 0 };
    let spherical = { theta: 0, phi: Math.PI / 4 };
    let cameraDistance = 180;
    let initialized = false;
    let currentSatelliteId = null;
    let isFollowing = false;
    let currentSatCartesian = null; // Last known satellite position {x, y, z} relative to Earth

    // =========================================================================
    //  COORDINATE CONVERSION
    //  ECI Cartesian (X, Y, Z in km) → Three.js Cartesian (x, y, z)
    //  SGP.NET ECI: Z is North, X is Vernal Equinox, Y completes right-handed
    //  Three.js: Y is Up (North), X is Right, Z is towards viewer
    // =========================================================================
    function eciToThree(xKm, yKm, zKm) {
        // Apply scaling
        const x = xKm * SATELLITE_SCALE_FACTOR;
        const y = yKm * SATELLITE_SCALE_FACTOR;
        const z = zKm * SATELLITE_SCALE_FACTOR;

        // Axis Mapping:
        // Three.y = ECI.z (North = Up)
        // Three.x = ECI.x (Equinox = Right)
        // Three.z = -ECI.y (Preserve right-handedness: X x Y = -ECI.y)
        return new THREE.Vector3(x, z, -y);
    }

    // =========================================================================
    //  THREE.JS SCENE INITIALIZATION
    // =========================================================================
    function initScene() {
        if (initialized) return;

        const width = container.clientWidth;
        const height = container.clientHeight;

        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x0A0A0A);

        earthGroup = new THREE.Group();
        scene.add(earthGroup);

        trackingGroup = new THREE.Group();
        scene.add(trackingGroup);

        camera = new THREE.PerspectiveCamera(45, width / height, 0.1, 1000);
        updateCameraPosition();

        renderer = new THREE.WebGLRenderer({
            canvas: canvas,
            antialias: true,
            alpha: false
        });
        renderer.setSize(width, height);
        renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));

        const ambientLight = new THREE.AmbientLight(0xffffff, 0.3);
        scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.9);
        directionalLight.position.set(100, 60, 100);
        scene.add(directionalLight);

        const rimLight = new THREE.DirectionalLight(0xffffff, 0.2);
        rimLight.position.set(-80, -30, -80);
        scene.add(rimLight);

        createEarth();
        createSatelliteMarker();
        createPathLines();

        setupInteraction();

        window.addEventListener('resize', onResize);

        initialized = true;
        placeholder.style.display = 'none';
        canvas.style.display = 'block';

        animate();
    }

    function createEarth() {
        const textureLoader = new THREE.TextureLoader();
        
        const earthGeometry = new THREE.SphereGeometry(EARTH_RADIUS, 64, 64);
        const earthMaterial = new THREE.MeshPhongMaterial({
            map: textureLoader.load('/assets/textures/earth_surface.jpg'),
            specular: new THREE.Color(0x222222),
            shininess: 15
        });
        earth = new THREE.Mesh(earthGeometry, earthMaterial);
        scene.add(earth);

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
        earthGroup.add(earth);
        earthGroup.add(clouds);
        earthGroup.add(atmosphere);
    }

    function createSatelliteMarker() {
        // Improved Main Marker — small white point
        const markerGeo = new THREE.OctahedronGeometry(1.2, 0); 
        const markerMat = new THREE.MeshBasicMaterial({ color: 0xffffff });
        satelliteMarker = new THREE.Mesh(markerGeo, markerMat);
        satelliteMarker.visible = false;
        trackingGroup.add(satelliteMarker);

        // Sub-satellite point on Earth surface
        const subSatGeo = new THREE.CircleGeometry(0.8, 16);
        const subSatMat = new THREE.MeshBasicMaterial({ color: 0xffffff, side: THREE.DoubleSide, depthTest: false, transparent: true, opacity: 0.6 });
        subSatPoint = new THREE.Mesh(subSatGeo, subSatMat);
        subSatPoint.visible = false;
        earthGroup.add(subSatPoint); // Belongs to Earth relative

        // Connection line (Nadir)
        const nadirMat = new THREE.LineBasicMaterial({ color: 0xffffff, transparent: true, opacity: 0.3 });
        const nadirGeo = new THREE.BufferGeometry();
        nadirLine = new THREE.Line(nadirGeo, nadirMat);
        nadirLine.visible = false;
        trackingGroup.add(nadirLine);
    }

    function createPathLines() {
        // Full Orbit Path (white, subtle)
        const orbitMat = new THREE.LineBasicMaterial({ color: 0x888888, transparent: true, opacity: 0.4 });
        fullOrbitPathLine = new THREE.Line(new THREE.BufferGeometry(), orbitMat);
        fullOrbitPathLine.visible = false;
        trackingGroup.add(fullOrbitPathLine);

        // 5-Minute Prediction Path (distinct color, slightly brighter)
        const next5Mat = new THREE.LineBasicMaterial({ color: 0xffffff, linewidth: 2, transparent: true, opacity: 0.9 });
        next5MinPathLine = new THREE.Line(new THREE.BufferGeometry(), next5Mat);
        next5MinPathLine.visible = false;
        trackingGroup.add(next5MinPathLine);
    }

    // =========================================================================
    //  CAMERA ORBIT CONTROLS
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
            isFollowing = false; // Disable follow on manual drag
            updateFollowBtnUI();
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
            cameraDistance = Math.max(60, Math.min(400, cameraDistance + e.deltaY * 0.15));
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

    function focusOnSatellite() {
        if (!currentSatCartesian || !satelliteMarker) return;
        
        const worldPos = satelliteMarker.position.clone();

        const r = Math.sqrt(worldPos.x**2 + worldPos.y**2 + worldPos.z**2);
        spherical.theta = Math.atan2(worldPos.x, worldPos.z);
        spherical.phi = Math.acos(worldPos.y / r);
        spherical.phi = Math.max(0.1, spherical.phi - 0.2); 
        cameraDistance = 120; // zoom in somewhat
        updateCameraPosition();
    }

    if (focusBtn) {
        focusBtn.addEventListener('click', () => {
            isFollowing = false;
            updateFollowBtnUI();
            focusOnSatellite();
        });
    }

    if (followBtn) {
        followBtn.addEventListener('click', () => {
            isFollowing = !isFollowing;
            updateFollowBtnUI();
            if (isFollowing) {
                focusOnSatellite();
            }
        });
    }

    function updateFollowBtnUI() {
        if (followBtn) {
            followBtn.textContent = `Follow: ${isFollowing ? 'ON' : 'OFF'}`;
            followBtn.className = isFollowing ? 'ow-btn' : 'ow-btn ow-btn--secondary'; // Active style
        }
    }

    // =========================================================================
    //  ANIMATION LOOP
    // =========================================================================
    function animate() {
        animationId = requestAnimationFrame(animate);

        // Continuous Earth rotation (Visual only, independent of trackingGroup)
        if (earthGroup && !isDragging && !isFollowing) {
            earthGroup.rotation.y += 0.0003;
        }

        let currentSatWorldPos = null;
        if (satelliteMarker && satelliteMarker.visible) {
            currentSatWorldPos = satelliteMarker.position.clone();
        }

        if (isFollowing && currentSatWorldPos) {
            // Keep camera roughly behind or above satellite
            const r = Math.sqrt(currentSatWorldPos.x**2 + currentSatWorldPos.y**2 + currentSatWorldPos.z**2);
            // Gradually interpolate towards satellite (smooth follow)
            const targetTheta = Math.atan2(currentSatWorldPos.x, currentSatWorldPos.z);
            let targetPhi = Math.acos(currentSatWorldPos.y / r);
            targetPhi = Math.max(0.1, targetPhi - 0.2);
            
            // Handle theta wrap-around for smooth interpolation
            let diff = targetTheta - spherical.theta;
            while (diff > Math.PI) diff -= 2 * Math.PI;
            while (diff < -Math.PI) diff += 2 * Math.PI;
            
            spherical.theta += diff * 0.05;
            spherical.phi += (targetPhi - spherical.phi) * 0.05;
            updateCameraPosition();
        }

        // Update DOM Overlay Label position
        if (satelliteMarker && satelliteMarker.visible && currentSatWorldPos) {
            const vector = currentSatWorldPos.clone();
            vector.project(camera);
            
            // Check if satellite is behind Earth
            const distToSat = camera.position.distanceTo(currentSatWorldPos);
            const camLen = camera.position.length();
            const horizonDist = Math.sqrt(camLen * camLen - EARTH_RADIUS * EARTH_RADIUS);
            
            if (vector.z < 1 && distToSat < horizonDist + 10) { 
                satLabelOverlay.style.display = 'block';
                const x = (vector.x * .5 + .5) * container.clientWidth;
                const y = (vector.y * -.5 + .5) * container.clientHeight;
                satLabelOverlay.style.left = `${x}px`;
                satLabelOverlay.style.top = `${y}px`;
            } else {
                satLabelOverlay.style.display = 'none';
            }
        }

        renderer.render(scene, camera);
    }

    // =========================================================================
    //  SATELLITE POSITION FROM API
    // =========================================================================
    function updateSatellitePosition(satelliteId) {
        showLoading();
        hideError();

        // Using the new trajectory endpoint
        const url = `${API_BASE}/${satelliteId}/trajectory`;

        fetch(url)
            .then(response => {
                if (response.status === 404) throw new Error('Satellite not found in system.');
                if (response.status === 502 || response.status === 503) throw new Error('CelesTrak orbital data temporarily unavailable.');
                if (!response.ok) throw new Error(`Trajectory API returned ${response.status}.`);
                return response.json();
            })
            .then(data => {
                if (currentSatelliteId !== satelliteId) return; 
                
                hideLoading();

                const pos = data.currentPosition;
                if (!pos || pos.latitudeDegrees === undefined) throw new Error('Invalid position data received.');

                // Marker Cartesians from ECI
                const cartesian = eciToThree(pos.cartesianXKm, pos.cartesianYKm, pos.cartesianZKm);
                
                // Sub-satellite point belongs to Earth Group, so it needs to be placed on Earth surface
                // We project the Cartesian coordinate down to Earth radius
                const surfacePosWorld = cartesian.clone().normalize().multiplyScalar(EARTH_RADIUS);
                // Convert world to local for Earth Group
                const surfacePosLocal = earthGroup.worldToLocal(surfacePosWorld.clone());
                
                currentSatCartesian = cartesian;

                // Update Markers
                satelliteMarker.position.copy(cartesian);
                satelliteMarker.visible = true;

                // Sub-satellite point and line
                subSatPoint.position.copy(surfacePosLocal);
                // Orient flat to Earth surface
                subSatPoint.lookAt(subSatPoint.position.clone().multiplyScalar(2)); 
                subSatPoint.visible = true;

                const nadirPoints = [
                    cartesian,
                    surfacePosWorld
                ];
                nadirLine.geometry.dispose();
                nadirLine.geometry = new THREE.BufferGeometry().setFromPoints(nadirPoints);
                nadirLine.visible = true;

                // Paths
                updatePaths(data.orbitPath, data.next5Minutes);

                // Info
                updateInfoPanel(pos, satelliteId);
                showInfoPanel();
            })
            .catch(err => {
                if (currentSatelliteId !== satelliteId) return;
                hideLoading();
                showError(err.message || 'Failed to fetch satellite trajectory.');
                hideSatelliteMarker();
            });
    }

    function updatePaths(orbitPath, next5Minutes) {
        if (!orbitPath || !next5Minutes) return;

        // 1. Full Orbit Line
        const orbitPoints = [];
        for (let p of orbitPath) {
            const pt = eciToThree(p.cartesianXKm, p.cartesianYKm, p.cartesianZKm);
            orbitPoints.push(pt);
        }
        fullOrbitPathLine.geometry.dispose();
        fullOrbitPathLine.geometry = new THREE.BufferGeometry().setFromPoints(orbitPoints);
        fullOrbitPathLine.visible = true;

        // 2. Next 5 Minutes Line
        const fiveMinPoints = [];
        for (let p of next5Minutes) {
            const pt = eciToThree(p.cartesianXKm, p.cartesianYKm, p.cartesianZKm);
            fiveMinPoints.push(pt);
        }
        next5MinPathLine.geometry.dispose();
        next5MinPathLine.geometry = new THREE.BufferGeometry().setFromPoints(fiveMinPoints);
        next5MinPathLine.visible = true;
    }

    function hideSatelliteMarker() {
        if (satelliteMarker) satelliteMarker.visible = false;
        if (subSatPoint) subSatPoint.visible = false;
        if (nadirLine) nadirLine.visible = false;
        if (fullOrbitPathLine) fullOrbitPathLine.visible = false;
        if (next5MinPathLine) next5MinPathLine.visible = false;
        if (satLabelOverlay) satLabelOverlay.style.display = 'none';
        currentSatCartesian = null;
        isFollowing = false;
        updateFollowBtnUI();
    }

    // =========================================================================
    //  INFO PANEL UPDATE
    // =========================================================================
    function updateInfoPanel(position, satelliteId) {
        const selected = Array.from(satelliteSelect.options).find(opt => opt.value === satelliteId);
        if (!selected) return;

        const name = selected.dataset.name || '—';
        const norad = selected.dataset.norad || '—';

        infoName.textContent = name;
        infoNorad.textContent = norad;
        infoOrbit.textContent = selected.dataset.orbit || '—';

        satLabelName.textContent = name;
        satLabelNorad.textContent = `NORAD ${norad}`;

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
