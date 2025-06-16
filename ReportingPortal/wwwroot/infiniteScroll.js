window.infiniteScroll = {
    observeScroll: function (dotNetHelper, elementId) {
        const el = document.getElementById(elementId);
        if (!el) return;

        el.addEventListener('scroll', () => {
            if (el.scrollTop + el.clientHeight >= el.scrollHeight - 50) {
                dotNetHelper.invokeMethodAsync('LoadMoreReports');
            }
        });
    }
};