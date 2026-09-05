export const ACCESS_CHANGED = "aisam:access-changed";
let cacheRevision = 0;
const cacheClearers = new Set<() => void>();
export function registerProtectedCache(clear: () => void) { cacheClearers.add(clear); }
export function protectedCacheRevision() { return cacheRevision; }
export function clearProtectedCaches() {
  ++cacheRevision;
  cacheClearers.forEach(clear => clear());
}

export function notifyAccessChanged(reason = "changed") {
  clearProtectedCaches();
  if (typeof window !== "undefined") window.dispatchEvent(new CustomEvent(ACCESS_CHANGED, { detail: reason }));
}
