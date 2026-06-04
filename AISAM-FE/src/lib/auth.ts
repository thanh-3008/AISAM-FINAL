export const setToken = (token: string) => {
  if (typeof window !== "undefined") {
    localStorage.setItem("aisam_token", token);
  }
};

export const getToken = () => {
  if (typeof window !== "undefined") {
    return localStorage.getItem("aisam_token");
  }
  return null;
};

export const removeToken = () => {
  if (typeof window !== "undefined") {
    localStorage.removeItem("aisam_token");
  }
};
