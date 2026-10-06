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
    let scene, camera, renderer, earth, satelliteMarker, satelliteGlow;
    let subSatPoint, nadirLine;
    let fullOrbitPathLine, next5MinPathLine, groundTrackLine;
    
    let animationId = null;
    let refreshTimerId = null;
    let isDragging = false;
    let previousMouse = { x: 0, y: 0 };
    let spherical = { theta: 0, phi: Math.PI / 4 };
    let cameraDistance = 180;
    let initialized = false;
    let currentSatelliteId = null;
    let isFollowing = false;
    let currentSatCartesian = null; // Last known satellite position {x, y, z}

    // =========================================================================
    //  COORDINATE CONVERSION
    //  Geodetic (lat°, lon°, altKm) → Three.js Cartesian (x, y, z)
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

        scene = new THREE.Scene();
        scene.background = new THREE.Color(0x0A0A0A);

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
        earth.add(atmosphere);
    }

    function createSatelliteMarker() {
        // Improved Main Marker — slightly larger, more visible
        const markerGeo = new THREE.OctahedronGeometry(2.2, 1); // Spherical-ish
        const markerMat = new THREE.MeshBasicMaterial({ color: 0xffffff });
        satelliteMarker = new THREE.Mesh(markerGeo, markerMat);
        satelliteMarker.visible = false;
        scene.add(satelliteMarker);

        // Outer Glow/Ring
        const glowGeo = new THREE.SphereGeometry(3.5, 16, 16);
        const glowMat = new THREE.MeshBasicMaterial({
            color: 0xD92626,
            transparent: true,
            opacity: 0.4,
            blending: THREE.AdditiveBlending,
            depthWrite: false
        });
        satelliteGlow = new THREE.Mesh(glowGeo, glowMat);
        satelliteGlow.visible = false;
        scene.add(satelliteGlow);

        // Sub-satellite point on Earth surface
        const subSatGeo = new THREE.CircleGeometry(0.8, 16);
        const subSatMat = new THREE.MeshBasicMaterial({ color: 0xD92626, side: THREE.DoubleSide, depthTest: false });
        subSatPoint = new THREE.Mesh(subSatGeo, subSatMat);
        subSatPoint.visible = false;
        scene.add(subSatPoint);

        // Connection line
        const nadirMat = new THREE.LineBasicMaterial({ color: 0xD92626, transparent: true, opacity: 0.5 });
        const nadirGeo = new THREE.BufferGeometry();
        nadirLine = new THREE.Line(nadirGeo, nadirMat);
        nadirLine.visible = false;
        scene.add(nadirLine);
    }

    function createPathLines() {
        // Full Orbit Path (white, subtle)
        const orbitMat = new THREE.LineBasicMaterial({ color: 0x888888, transparent: true, opacity: 0.4 });
        fullOrbitPathLine = new THREE.Line(new THREE.BufferGeometry(), orbitMat);
        fullOrbitPathLine.visible = false;
        scene.add(fullOrbitPathLine);

        // 5-Minute Prediction Path (distinct color, e.g., bright yellow/orange to show direction)
        const next5Mat = new THREE.LineBasicMaterial({ color: 0xFFAA00, linewidth: 2 });
        next5MinPathLine = new THREE.Line(new THREE.BufferGeometry(), next5Mat);
        next5MinPathLine.visible = false;
        scene.add(next5MinPathLine);

        // Ground Track (red, subtle, projected on Earth)
        const groundMat = new THREE.LineBasicMaterial({ color: 0xD92626, transparent: true, opacity: 0.6 });
        // Since it might break across the dateline, we use LineSegments
        groundTrackLine = new THREE.LineSegments(new THREE.BufferGeometry(), groundMat);
        groundTrackLine.visible = false;
        scene.add(groundTrackLine);
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
        if (!currentSatCartesian) return;
        // Calculate spherical coordinates from cartesian
        const r = Math.sqrt(currentSatCartesian.x**2 + currentSatCartesian.y**2 + currentSatCartesian.z**2);
        spherical.theta = Math.atan2(currentSatCartesian.x, currentSatCartesian.z);
        spherical.phi = Math.acos(currentSatCartesian.y / r);
        // Add a slight offset so it's not looking directly down
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

        // Slow Earth rotation ONLY if not following and not dragging
        if (earth && !isDragging && !isFollowing) {
            // Remove auto-rotation to keep trajectories synced correctly,
            // or just rely on manual refresh/drag. We will remove Earth auto-rotation
            // because SGP4 positions are ECEF (Earth-centered, Earth-fixed implicitly here)
            // wait, if we rotate earth, we have to rotate the paths too. 
            // Better to disable auto-rotation to keep tracking accurate relative to earth surface texture.
        }

        // Pulse satellite glow
        if (satelliteGlow && satelliteGlow.visible) {
            const scale = 1 + 0.15 * Math.sin(Date.now() * 0.003);
            satelliteGlow.scale.set(scale, scale, scale);
        }

        if (isFollowing && currentSatCartesian) {
            // Keep camera roughly behind or above satellite
            const r = Math.sqrt(currentSatCartesian.x**2 + currentSatCartesian.y**2 + currentSatCartesian.z**2);
            // Gradually interpolate towards satellite (smooth follow)
            const targetTheta = Math.atan2(currentSatCartesian.x, currentSatCartesian.z);
            let targetPhi = Math.acos(currentSatCartesian.y / r);
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
        if (satelliteMarker && satelliteMarker.visible && currentSatCartesian) {
            const vector = new THREE.Vector3(currentSatCartesian.x, currentSatCartesian.y, currentSatCartesian.z);
            vector.project(camera);
            
            // Check if satellite is behind Earth
            const distToSat = camera.position.distanceTo(new THREE.Vector3(currentSatCartesian.x, currentSatCartesian.y, currentSatCartesian.z));
            // rough culling check: is angle between camera vector and earth center too tight?
            // A simpler way: if vector.z > 1, it's behind the camera.
            // if we really want to check occlusion by Earth, we can do a raycast or simple math.
            // Simple math:
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

                // Marker Cartesians
                const cartesian = geoToCartesian(pos.latitudeDegrees, pos.longitudeDegrees, pos.altitudeKm);
                const surfacePos = geoToCartesian(pos.latitudeDegrees, pos.longitudeDegrees, 0);
                currentSatCartesian = cartesian;

                // Update Markers
                satelliteMarker.position.set(cartesian.x, cartesian.y, cartesian.z);
                satelliteMarker.visible = true;

                satelliteGlow.position.set(cartesian.x, cartesian.y, cartesian.z);
                satelliteGlow.visible = true;

                // Sub-satellite point and line
                subSatPoint.position.set(surfacePos.x, surfacePos.y, surfacePos.z);
                subSatPoint.lookAt(0, 0, 0); // Orient flat to Earth surface
                subSatPoint.visible = true;

                const nadirPoints = [
                    new THREE.Vector3(cartesian.x, cartesian.y, cartesian.z),
                    new THREE.Vector3(surfacePos.x, surfacePos.y, surfacePos.z)
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
            const pt = geoToCartesian(p.latitudeDegrees, p.longitudeDegrees, p.altitudeKm);
            orbitPoints.push(new THREE.Vector3(pt.x, pt.y, pt.z));
        }
        fullOrbitPathLine.geometry.dispose();
        fullOrbitPathLine.geometry = new THREE.BufferGeometry().setFromPoints(orbitPoints);
        fullOrbitPathLine.visible = true;

        // 2. Next 5 Minutes Line
        const fiveMinPoints = [];
        for (let p of next5Minutes) {
            const pt = geoToCartesian(p.latitudeDegrees, p.longitudeDegrees, p.altitudeKm);
            fiveMinPoints.push(new THREE.Vector3(pt.x, pt.y, pt.z));
        }
        next5MinPathLine.geometry.dispose();
        next5MinPathLine.geometry = new THREE.BufferGeometry().setFromPoints(fiveMinPoints);
        next5MinPathLine.visible = true;

        // 3. Ground Track (with line breaks across the anti-meridian)
        const groundPoints = [];
        for (let i = 0; i < orbitPath.length - 1; i++) {
            const p1 = orbitPath[i];
            const p2 = orbitPath[i+1];
            
            // Check for ±180 longitude jump
            if (Math.abs(p1.longitudeDegrees - p2.longitudeDegrees) > 180) {
                // Break the line (skip adding segment)
                continue; 
            }

            // slightly above surface to prevent z-fighting
            const pt1 = geoToCartesian(p1.latitudeDegrees, p1.longitudeDegrees, 1); 
            const pt2 = geoToCartesian(p2.latitudeDegrees, p2.longitudeDegrees, 1);
            
            groundPoints.push(new THREE.Vector3(pt1.x, pt1.y, pt1.z));
            groundPoints.push(new THREE.Vector3(pt2.x, pt2.y, pt2.z));
        }
        groundTrackLine.geometry.dispose();
        groundTrackLine.geometry = new THREE.BufferGeometry().setFromPoints(groundPoints);
        groundTrackLine.visible = true;
    }

    function hideSatelliteMarker() {
        if (satelliteMarker) satelliteMarker.visible = false;
        if (satelliteGlow) satelliteGlow.visible = false;
        if (subSatPoint) subSatPoint.visible = false;
        if (nadirLine) nadirLine.visible = false;
        if (fullOrbitPathLine) fullOrbitPathLine.visible = false;
        if (next5MinPathLine) next5MinPathLine.visible = false;
        if (groundTrackLine) groundTrackLine.visible = false;
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
