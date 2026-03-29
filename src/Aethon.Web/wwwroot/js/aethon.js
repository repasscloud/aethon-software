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
    }
};
