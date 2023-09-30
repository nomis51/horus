const PREFER_LIGHT_MODE_KEY = "winpass-prefer-light-mode";

window.winpass = {
    utils: {
        getPeferLightMode: function () {
            const value = localStorage.getItem(PREFER_LIGHT_MODE_KEY);
            if (value === null || value === undefined) return false;

            try {
                return JSON.parse(value);
            } catch {
                return false;
            }
        },
        setPeferLightMode: function (value) {
            localStorage.setItem(PREFER_LIGHT_MODE_KEY, value);
        },
        copyToClipboard: async function (value) {
            await navigator.clipboard.writeText(value);
        },
    },
    openSettings: function () {
        window.DotNet.invokeMethodAsync('WinPass.UI', 'js-open-settings')
    }
}