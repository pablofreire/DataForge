// Light/dark theme toggle. The initial theme is applied by an inline script in <head>; this file only
// handles the toggle button. Event delegation keeps it working across Blazor enhanced navigations.
(function () {
    const storageKey = "dataforge-theme";

    function currentTheme() {
        const explicit = document.documentElement.dataset.theme;
        if (explicit === "light" || explicit === "dark") {
            return explicit;
        }
        return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    }

    document.addEventListener("click", (event) => {
        const toggle = event.target.closest("[data-theme-toggle]");
        if (!toggle) {
            return;
        }
        const next = currentTheme() === "dark" ? "light" : "dark";
        document.documentElement.dataset.theme = next;
        try {
            localStorage.setItem(storageKey, next);
        } catch {
            // Storage can be unavailable (private mode); the theme still applies for this page.
        }
    });
})();
