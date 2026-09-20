export const inclusiveToExclusiveUtc = (date: string) =>
  new Date(new Date(`${date}T00:00:00Z`).getTime() + 86_400_000).toISOString();
