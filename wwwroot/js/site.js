// OrbitWatch — Editorial Space Operations Scripts

document.addEventListener('DOMContentLoaded', () => {
    // 1. Live UTC Clock
    const utcElement = document.getElementById('owLiveUtc');
    function updateUtc() {
        if (!utcElement) return;
        const now = new Date();
        const hours = String(now.getUTCHours()).padStart(2, '0');
        const minutes = String(now.getUTCMinutes()).padStart(2, '0');
        const seconds = String(now.getUTCSeconds()).padStart(2, '0');
        utcElement.textContent = `UTC ${hours}:${minutes}:${seconds}`;
    }
    updateUtc();
    setInterval(updateUtc, 1000);

    // 2. Mobile Nav Toggle
    const toggleBtn = document.getElementById('owNavToggle');
    const navLinks = document.getElementById('owNavLinks');
    if (toggleBtn && navLinks) {
        toggleBtn.addEventListener('click', () => {
            const isOpen = navLinks.classList.toggle('is-open');
            toggleBtn.textContent = isOpen ? 'CLOSE [✕]' : 'MENU [≡]';
        });
    }
});
