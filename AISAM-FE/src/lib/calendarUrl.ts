export function withoutCalendarContentPrefill(pathname: string, search: string): string {
  const params = new URLSearchParams(search);
  params.delete("contentId");
  const remaining = params.toString();
  return remaining ? `${pathname}?${remaining}` : pathname;
}
