// Streams the selected file to the upload API as the raw request body and reports progress to .NET.
// XMLHttpRequest is used instead of fetch because fetch has no upload-progress events.

export function selectedFileName(input) {
    return input?.files?.length ? input.files[0].name : null;
}

export function uploadDataset(input, url, antiforgery, dotNetRef) {
    return new Promise((resolve) => {
        const file = input?.files?.[0];
        if (!file) {
            resolve({ ok: false, status: 0, body: null });
            return;
        }

        const request = new XMLHttpRequest();
        request.open("POST", `${url}?fileName=${encodeURIComponent(file.name)}`);
        request.setRequestHeader("Content-Type", file.type || "text/csv");
        if (antiforgery?.token) {
            request.setRequestHeader(antiforgery.headerName, antiforgery.token);
        }

        let lastReported = -1;
        request.upload.onprogress = (event) => {
            if (!event.lengthComputable) {
                return;
            }
            const percent = Math.floor((event.loaded * 100) / event.total);
            if (percent !== lastReported) {
                lastReported = percent;
                dotNetRef.invokeMethodAsync("ReportProgress", percent);
            }
        };
        request.onload = () => resolve({ ok: request.status >= 200 && request.status < 300, status: request.status, body: request.responseText });
        request.onerror = () => resolve({ ok: false, status: 0, body: null });
        request.send(file);
    });
}
