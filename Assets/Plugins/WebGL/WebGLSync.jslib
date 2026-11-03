mergeInto(LibraryManager.library, {
    SyncFilesystemToIndexedDB: function () {
        if (typeof FS !== 'undefined' && FS.syncfs) {
            FS.syncfs(false, function (err) {
                if (err) {
                    console.error('[WebGLSync] Error syncing IndexedDB: ' + err);
                }
            });
        }
    }
});

