(() => {
    const toasts = document.querySelectorAll('[data-app-toast]');
    if (!toasts.length) return;

    const dismiss = toast => {
        if (toast.dataset.dismissed === 'true') return;
        toast.dataset.dismissed = 'true';
        toast.classList.add('is-leaving');
        window.setTimeout(() => toast.remove(), 240);
    };

    toasts.forEach((toast, index) => {
        window.setTimeout(() => toast.classList.add('is-visible'), 40 + index * 70);
        const timer = window.setTimeout(() => dismiss(toast), 5500 + index * 250);
        toast.querySelector('[data-toast-close]')?.addEventListener('click', () => {
            window.clearTimeout(timer);
            dismiss(toast);
        });
    });
})();
