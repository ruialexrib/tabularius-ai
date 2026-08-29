(() => {
    const overlay = document.querySelector('[data-operation-progress]');
    if (!overlay) return;

    let pending = false;

    const show = () => {
        if (pending) return;
        pending = true;
        window.setTimeout(() => {
            if (!pending) return;
            overlay.classList.add('is-visible');
            overlay.setAttribute('aria-hidden', 'false');
            document.body.setAttribute('aria-busy', 'true');
        }, 120);
    };

    const hide = () => {
        pending = false;
        overlay.classList.remove('is-visible');
        overlay.setAttribute('aria-hidden', 'true');
        document.body.removeAttribute('aria-busy');
    };

    document.addEventListener('click', event => {
        const link = event.target.closest('a[href]');
        if (!link || event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return;
        if (link.target === '_blank' || link.hasAttribute('download') || link.getAttribute('href')?.startsWith('#')) return;
        const destination = new URL(link.href, window.location.href);
        if (destination.origin !== window.location.origin) return;
        show();
    });

    document.addEventListener('submit', event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || event.defaultPrevented || form.dataset.noWait !== undefined) return;
        show();
    });

    window.addEventListener('pageshow', hide);
})();
