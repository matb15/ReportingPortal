window.setTheme = function (isDark) {
    if (isDark) {
        document.documentElement.classList.add('dark');
        localStorage.setItem('theme', 'dark');
    } else {
        document.documentElement.classList.remove('dark');
        localStorage.setItem('theme', 'light');
    }
};

window.getStoredTheme = function () {
    return localStorage.getItem('theme') === 'dark';
};
