const state = {
    endpoints: [],
    selected: null
};

const elements = {};

document.addEventListener('DOMContentLoaded', initialise);

async function initialise() {
    cacheElements();
    attachEventHandlers();
    await loadEndpoints();
}

function cacheElements() {
    elements.endpointSelect = document.getElementById('endpointSelect');
    elements.methodInput = document.getElementById('methodInput');
    elements.urlInput = document.getElementById('urlInput');
    elements.bodyInput = document.getElementById('bodyInput');
    elements.headersInput = document.getElementById('headersInput');
    elements.queryInput = document.getElementById('queryInput');
    elements.timeoutInput = document.getElementById('timeoutInput');
    elements.executeForm = document.getElementById('executeForm');
    elements.resultContainer = document.getElementById('executionResult');
    elements.resultStatus = document.getElementById('resultStatus');
    elements.resultMeta = document.getElementById('resultMeta');
    elements.resultBody = document.getElementById('resultBody');
    elements.resultHeaders = document.getElementById('resultHeaders');
    elements.resultMessage = document.getElementById('resultMessage');
    elements.scheduleList = document.getElementById('scheduleList');
    elements.scheduleMeta = document.getElementById('scheduleMeta');
    elements.description = document.getElementById('endpointDescription');
    elements.target = document.getElementById('endpointTarget');
    elements.methodDisplay = document.getElementById('endpointMethodDisplay');
    elements.bodyContentType = document.getElementById('bodyContentType');
    elements.timeoutMeta = document.getElementById('timeoutMeta');
    elements.originMeta = document.getElementById('originMeta');
    elements.bodyHelp = document.getElementById('bodyHelp');
    elements.message = document.getElementById('messageArea');
}

function attachEventHandlers() {
    elements.endpointSelect?.addEventListener('change', handleEndpointChange);
    elements.executeForm?.addEventListener('submit', executeEndpoint);
}

async function loadEndpoints() {
    clearMessage();

    try {
        const response = await fetch('/api/executor/endpoints');
        if (!response.ok) {
            throw new Error('Failed to load endpoints.');
        }

        const data = await response.json();
        state.endpoints = Array.isArray(data) ? data : [];

        if (state.endpoints.length === 0) {
            showMessage('No endpoints are configured yet. Add entries under the "Executor" configuration section.', false);
            renderEndpoints([]);
            return;
        }

        renderEndpoints(state.endpoints);
        const initial = state.endpoints[0];
        elements.endpointSelect.value = initial.id;
        selectEndpoint(initial.id);
    } catch (error) {
        console.error(error);
        showMessage(error.message || 'Unable to load endpoints.', true);
    }
}

function renderEndpoints(endpoints) {
    if (!elements.endpointSelect) {
        return;
    }

    elements.endpointSelect.innerHTML = '';

    if (endpoints.length === 0) {
        const option = document.createElement('option');
        option.textContent = 'No endpoints available';
        option.disabled = true;
        option.selected = true;
        elements.endpointSelect.appendChild(option);
        return;
    }

    for (const endpoint of endpoints) {
        const option = document.createElement('option');
        option.value = endpoint.id;
        option.textContent = endpoint.displayName || endpoint.id;
        elements.endpointSelect.appendChild(option);
    }
}

function handleEndpointChange(event) {
    const endpointId = event.target.value;
    selectEndpoint(endpointId);
}

function selectEndpoint(endpointId) {
    const endpoint = state.endpoints.find((item) => item.id === endpointId);
    state.selected = endpoint ?? null;

    if (!endpoint) {
        showMessage('The selected endpoint could not be found.', true);
        return;
    }

    clearMessage();
    renderEndpointDetails(endpoint);
    loadUpcomingExecutions(endpoint.id);
    elements.resultContainer?.classList.add('hidden');
}

function renderEndpointDetails(endpoint) {
    const methodLabel = (endpoint.method || 'GET').toUpperCase();

    if (elements.methodInput) {
        elements.methodInput.value = methodLabel;
    }

    if (elements.methodDisplay) {
        elements.methodDisplay.textContent = methodLabel;
    }

    if (elements.urlInput) {
        elements.urlInput.value = '';
        elements.urlInput.placeholder = endpoint.target;
    }

    if (elements.bodyInput) {
        const template = endpoint.bodyTemplate ?? '';
        if (endpoint.allowBody === false) {
            elements.bodyInput.value = '';
            elements.bodyInput.disabled = true;
            elements.bodyInput.placeholder = 'Body not allowed for this endpoint.';
        } else {
            elements.bodyInput.disabled = false;
            elements.bodyInput.value = template;
            elements.bodyInput.placeholder = template || '{"example":"value"}';
        }
    }

    if (elements.bodyHelp) {
        elements.bodyHelp.textContent = endpoint.allowBody === false
            ? 'This endpoint does not accept a request body.'
            : 'Provide JSON payload when required.';
    }

    if (elements.headersInput) {
        elements.headersInput.value = '';
    }

    if (elements.queryInput) {
        elements.queryInput.value = '';
    }

    if (elements.timeoutInput) {
        elements.timeoutInput.value = endpoint.timeoutSeconds ?? '';
    }

    if (elements.description) {
        elements.description.textContent = endpoint.description || 'No description available for this endpoint.';
    }

    if (elements.target) {
        elements.target.textContent = endpoint.target;
    }

    if (elements.originMeta) {
        elements.originMeta.textContent = endpoint.origin || 'configuration';
    }

    if (elements.bodyContentType) {
        elements.bodyContentType.textContent = endpoint.allowBody === false
            ? 'n/a'
            : (endpoint.bodyContentType || 'application/json');
    }

    if (elements.timeoutMeta) {
        elements.timeoutMeta.textContent = endpoint.timeoutSeconds
            ? `${endpoint.timeoutSeconds} seconds`
            : 'HttpClient default';
    }

    if (elements.scheduleMeta) {
        if (endpoint.schedule) {
            elements.scheduleMeta.textContent = `${endpoint.schedule} (${endpoint.timeZone ?? 'UTC'})`;
        } else {
            elements.scheduleMeta.textContent = 'No schedule configured.';
        }
    }

    if (elements.scheduleList) {
        elements.scheduleList.innerHTML = '';
        const placeholder = document.createElement('div');
        placeholder.className = 'empty-state';
        placeholder.textContent = endpoint.schedule ? 'Loading upcoming executions...' : 'Scheduling disabled for this endpoint.';
        elements.scheduleList.appendChild(placeholder);
    }
}
async function loadUpcomingExecutions(endpointId) {
    if (!elements.scheduleList) {
        return;
    }

    elements.scheduleList.innerHTML = '';

    try {
        const response = await fetch(`/api/executor/endpoints/${encodeURIComponent(endpointId)}/next`);

        if (response.status === 204) {
            const item = document.createElement('div');
            item.className = 'empty-state';
            item.textContent = 'No schedule configured.';
            elements.scheduleList.appendChild(item);
            return;
        }

        if (!response.ok) {
            throw new Error('Failed to load upcoming executions.');
        }

        const payload = await response.json();
        const occurrences = Array.isArray(payload.occurrences) ? payload.occurrences : [];
        const timeZone = payload.timeZone || 'UTC';

        if (occurrences.length === 0) {
            const item = document.createElement('div');
            item.className = 'empty-state';
            item.textContent = 'No future occurrences were returned.';
            elements.scheduleList.appendChild(item);
            return;
        }

        for (const value of occurrences) {
            const entry = document.createElement('div');
            entry.innerHTML = `<strong>${formatDateTime(value, timeZone)}</strong> <span>(${timeZone})</span>`;
            elements.scheduleList.appendChild(entry);
        }
    } catch (error) {
        console.error(error);
        const item = document.createElement('div');
        item.className = 'empty-state';
        item.textContent = error.message || 'Unable to determine next run times.';
        elements.scheduleList.appendChild(item);
    }
}

function formatDateTime(value, timeZone) {
    try {
        const date = new Date(value);
        const formatter = new Intl.DateTimeFormat(undefined, {
            timeZone,
            year: 'numeric',
            month: 'short',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });

        return formatter.format(date);
    } catch (error) {
        console.warn('Unable to format date', error);
        return value;
    }
}

function parseHeaderBlock(text, separator = ':') {
    if (!text) {
        return undefined;
    }

    const lines = text.split(/\r?\n/);
    const result = {};

    for (const rawLine of lines) {
        const line = rawLine.trim();
        if (!line) {
            continue;
        }

        const index = line.indexOf(separator);
        if (index < 0) {
            continue;
        }

        const key = line.slice(0, index).trim();
        const value = line.slice(index + 1).trim();
        if (!key) {
            continue;
        }

        result[key] = value;
    }

    return Object.keys(result).length > 0 ? result : undefined;
}

function parseQueryBlock(text) {
    if (!text) {
        return undefined;
    }

    const lines = text.split(/\r?\n/);
    const result = {};

    for (const rawLine of lines) {
        const line = rawLine.trim();
        if (!line) {
            continue;
        }

        const index = line.indexOf('=');
        if (index < 0) {
            continue;
        }

        const key = line.slice(0, index).trim();
        const value = line.slice(index + 1).trim();
        if (!key) {
            continue;
        }

        result[key] = value;
    }

    return Object.keys(result).length > 0 ? result : undefined;
}

async function executeEndpoint(event) {
    event.preventDefault();

    if (!state.selected) {
        showMessage('Choose an endpoint before executing.', true);
        return;
    }

    const payload = buildExecutionPayload();
    setLoading(true);
    elements.resultContainer?.classList.add('hidden');
    clearMessage();

    try {
        const response = await fetch(`/api/executor/endpoints/${encodeURIComponent(state.selected.id)}/execute`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        const data = await response.json();

        if (!response.ok) {
            throw new Error(data?.message || `Execution failed with status ${response.status}.`);
        }

        renderExecutionResult(data);
        loadUpcomingExecutions(state.selected.id);
    } catch (error) {
        console.error(error);
        showMessage(error.message || 'Endpoint execution failed.', true);
    } finally {
        setLoading(false);
    }
}

function buildExecutionPayload() {
    const method = elements.methodInput?.value?.trim();
    const url = elements.urlInput?.value?.trim();
    const body = elements.bodyInput?.value ?? '';
    const headersText = elements.headersInput?.value ?? '';
    const queryText = elements.queryInput?.value ?? '';
    const timeoutText = elements.timeoutInput?.value ?? '';
    const allowBody = state.selected?.allowBody !== false;

    const payload = {};

    if (method) {
        payload.method = method;
    }

    if (url) {
        payload.url = url;
    }

    if (allowBody && body) {
        payload.body = body;
    }

    const headers = parseHeaderBlock(headersText);
    if (headers) {
        payload.headers = headers;
    }

    const query = parseQueryBlock(queryText);
    if (query) {
        payload.query = query;
    }

    const timeoutSeconds = Number.parseInt(timeoutText, 10);
    if (Number.isFinite(timeoutSeconds) && timeoutSeconds > 0) {
        payload.timeoutSeconds = timeoutSeconds;
    }

    return payload;
}

function renderExecutionResult(result) {
    if (!elements.resultContainer) {
        return;
    }

    elements.resultContainer.classList.remove('hidden');

    const successful = Boolean(result.success);
    const statusClass = successful ? 'success' : 'error';
    const statusLabel = `${result.statusCode ?? '} ${result.reasonPhrase ?? '}`.trim();

    if (elements.resultStatus) {
        elements.resultStatus.textContent = statusLabel || 'Unknown status';
        elements.resultStatus.className = `status-pill ${statusClass}`;
    }

    if (elements.resultMeta) {
        const duration = result.durationMilliseconds ? `${Number(result.durationMilliseconds).toFixed(0)} ms` : 'n/a';
        const executedAt = result.executedAt ? new Date(result.executedAt).toLocaleString() : 'n/a';
        elements.resultMeta.innerHTML = `
            <div><span class="badge">Target</span> ${escapeHtml(result.target ?? '')}</div>
            <div><span class="badge">Duration</span> ${escapeHtml(duration)}</div>
            <div><span class="badge">Executed</span> ${escapeHtml(executedAt)}</div>
            <div><span class="badge">Method</span> ${escapeHtml(result.method ?? '')}</div>
        `;
    }

    if (elements.resultBody) {
        const body = result.body || '';
        elements.resultBody.textContent = body;
        if (result.bodyTruncated) {
            elements.resultBody.textContent += '\n...response truncated...';
        }
    }

    if (elements.resultHeaders) {
        elements.resultHeaders.innerHTML = renderHeaders(result.headers);
    }

    if (elements.resultMessage) {
        elements.resultMessage.textContent = successful
            ? 'Execution completed successfully.'
            : 'Execution returned a non-success status code.';
    }
}

function renderHeaders(headers) {
    if (!headers || Object.keys(headers).length === 0) {
        return '<div class="empty-state">No response headers were returned.</div>';
    }

    const rows = Object.entries(headers)
        .map(([key, value]) => {
            const joined = Array.isArray(value) ? value.join(', ') : value;
            return `<tr><td><strong>${escapeHtml(key)}</strong></td><td>${escapeHtml(joined)}</td></tr>`;
        })
        .join('');

    return `<table>${rows}</table>`;
}

function escapeHtml(value) {
    if (value === null || value === undefined) {
        return '';
    }

    return String(value)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function setLoading(isLoading) {
    const button = elements.executeForm?.querySelector('button[type="submit"]');
    if (!button) {
        return;
    }

    button.disabled = isLoading;
    button.textContent = isLoading ? 'Executing...' : 'Execute endpoint';
}

function showMessage(message, isError) {
    if (!elements.message) {
        return;
    }

    elements.message.textContent = message;
    elements.message.className = `message${isError ? ' error' : ''}`;
}

function clearMessage() {
    if (!elements.message) {
        return;
    }

    elements.message.textContent = '';
    elements.message.className = 'message';
}

















