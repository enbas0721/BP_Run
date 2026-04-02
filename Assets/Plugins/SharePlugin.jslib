mergeInto(LibraryManager.library, {
    JS_Share: function(dataPtr, dataLength) {
        var bytes = new Uint8Array(Module.HEAPU8.buffer, dataPtr, dataLength);
        var blob = new Blob([bytes], { type: 'image/png' });
        var file = new File([blob], 'bp_run.png', { type: 'image/png' });

        if (navigator.canShare && navigator.canShare({ files: [file] })) {
            navigator.share({ files: [file] })
                .catch(function(e) { console.log('Share cancelled:', e); });
        } else {
            console.log('Web Share API (files) not supported on this browser');
        }
    }
});