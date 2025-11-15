(function ($) {
    if (!$ || !$.validator || !$.validator.unobtrusive) {
        return;
    }

    $.validator.addMethod('absoluteurl', function (value, element, params) {
        if (!value) {
            return true;
        }

        const trimmed = value.trim();
        const candidate = trimmed.includes('://') ? trimmed : `http://${trimmed}`;

        try {
            const url = new URL(candidate);
            const scheme = url.protocol.replace(':', '').toLowerCase();
            return params.indexOf(scheme) >= 0;
        } catch (err) {
            return false;
        }
    });

    $.validator.unobtrusive.adapters.addSingleVal('absoluteurl', 'schemes', function (options) {
        const schemes = (options.params.schemes || '')
            .split(',')
            .map(s => s.trim().toLowerCase())
            .filter(Boolean);

        options.rules.absoluteurl = schemes;
        options.messages.absoluteurl = options.message;
    });
})(window.jQuery);
