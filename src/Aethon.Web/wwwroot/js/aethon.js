window.aethon = {
    getLocation: function () {
        return new Promise(function (resolve, reject) {
            if (!navigator.geolocation) {
                reject(new Error("Geolocation is not supported by this browser."));
                return;
            }
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    resolve([pos.coords.latitude, pos.coords.longitude]);
                },
                function (err) {
                    reject(new Error(err.message));
                },
                { timeout: 10000 }
            );
        });
    },

    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },

    initScrollToTop: function (buttonId, showAfterPx) {
        var btn = document.getElementById(buttonId);
        if (!btn) return;
        var threshold = showAfterPx || 300;
        function onScroll() {
            btn.style.display = window.scrollY > threshold ? 'flex' : 'none';
        }
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
    }
};
