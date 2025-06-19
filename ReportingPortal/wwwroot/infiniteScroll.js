window.addScrollListener = (dotnetHelper) => {
    let isThrottled = false;

    const container = document.getElementById('reportScrollContainer');
    console.log(container)
    if (!container) return;

    const onScroll = () => {
        const scrollTop = container.scrollTop;
        const scrollHeight = container.scrollHeight;
        const clientHeight = container.clientHeight;

        if (scrollTop + clientHeight >= scrollHeight - 100) {
            if (!isThrottled) {
                isThrottled = true;

                dotnetHelper.invokeMethodAsync('LoadMoreReports')
                    .finally(() => {
                        setTimeout(() => {
                            isThrottled = false;
                        }, 1000);
                    });
            }
        }
    };

    container.removeEventListener('scroll', onScroll);
    container.addEventListener('scroll', onScroll);
};
