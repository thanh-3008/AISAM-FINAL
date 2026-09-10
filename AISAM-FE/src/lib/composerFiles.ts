// File blobs stay local to this browser; keys include actor, workspace and content.
export interface QueuedFile { id: string; file: File; status: string; error?: string; }
let writes: Promise<unknown> = Promise.resolve();
function transaction<T>(key: string, value?: QueuedFile[]): Promise<T> {
  return new Promise((resolve, reject) => {
    const open = indexedDB.open("aisam-composer-files", 1);
    open.onupgradeneeded = () => open.result.createObjectStore("drafts");
    open.onerror = () => reject(open.error);
    open.onsuccess = () => {
      const db = open.result;
      const tx = db.transaction("drafts", value === undefined ? "readonly" : "readwrite");
      const store = tx.objectStore("drafts");
      const request = value === undefined ? store.get(key) : value.length ? store.put(value, key) : store.delete(key);
      tx.oncomplete = () => { resolve(request.result as T); db.close(); };
      tx.onerror = () => { reject(tx.error); db.close(); };
      tx.onabort = () => { reject(tx.error); db.close(); };
    };
  });
}
export async function loadComposerFiles(key: string): Promise<QueuedFile[]> {
  await writes;
  const files = await transaction<QueuedFile[] | undefined>(key);
  return (files ?? []).map(f => ({ ...f, status: "Pending" }));
}
export function storeComposerFiles(key: string, files: QueuedFile[]) {
  writes = writes.catch(() => undefined).then(() => transaction(key, files.filter(f => f.status !== "Done")));
  return writes;
}
