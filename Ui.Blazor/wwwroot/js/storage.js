(function () {
    const storage = window.localStorage;

    const impl = {
        setItem: function (key, value) {
            storage.setItem(key, value ?? "");
        },

        getItem: function (key) {
            return storage.getItem(key);
        },

        removeItem: function (key) {
            storage.removeItem(key);
        },

        clear: function () {
            storage.clear();
        }
    };

    // Primary API used by StorageInterop (C#)
    window.__storage = window.__storage || impl;

    // Compatibility alias (some older code might call appStorage.get/set/remove)
    window.appStorage = window.appStorage || {
        get: (key) => impl.getItem(key),
        set: (key, value) => impl.setItem(key, value),
        remove: (key) => impl.removeItem(key),
        clear: () => impl.clear()
    };
})();
