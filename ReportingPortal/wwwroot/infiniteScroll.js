window.addScrollListener = (dotnetHelper) => {
    let isThrottled = false;

    const onScroll = () => {
        const scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
        const scrollHeight = document.documentElement.scrollHeight || document.body.scrollHeight;
        const clientHeight = document.documentElement.clientHeight;

        if (scrollTop + clientHeight >= scrollHeight - 150) {
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

    window.removeEventListener('scroll', onScroll);
    window.addEventListener('scroll', onScroll);
};
