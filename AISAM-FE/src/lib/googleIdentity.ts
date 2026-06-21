type GoogleCredentialResponse = {
  credential?: string;
};

type GooglePromptMoment = {
  isNotDisplayed?: () => boolean;
  isSkippedMoment?: () => boolean;
  getNotDisplayedReason?: () => string;
  getSkippedReason?: () => string;
};

type GoogleButtonOptions = {
  theme?: "outline" | "filled_blue" | "filled_black";
  size?: "large" | "medium" | "small";
  text?: "signin_with" | "signup_with" | "continue_with" | "signin";
  shape?: "rectangular" | "pill" | "circle" | "square";
  width?: string | number;
};

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize: (config: {
            client_id: string;
            callback: (response: GoogleCredentialResponse) => void;
            cancel_on_tap_outside?: boolean;
          }) => void;
          prompt: (momentListener?: (moment: GooglePromptMoment) => void) => void;
          renderButton: (parent: HTMLElement, options: GoogleButtonOptions) => void;
        };
      };
    };
  }
}

let scriptPromise: Promise<void> | null = null;
let initialized = false;

export function loadGoogleIdentityScript(): Promise<void> {
  if (typeof window === "undefined") {
    return Promise.reject(new Error("Google sign-in is only available in the browser."));
  }

  if (window.google?.accounts?.id) {
    return Promise.resolve();
  }

  if (scriptPromise) {
    return scriptPromise;
  }

  scriptPromise = new Promise((resolve, reject) => {
    const existingScript = document.querySelector<HTMLScriptElement>(
      'script[src="https://accounts.google.com/gsi/client"]'
    );

    if (existingScript) {
      existingScript.addEventListener("load", () => resolve(), { once: true });
      existingScript.addEventListener("error", () => reject(new Error("Failed to load Google sign-in.")), {
        once: true,
      });
      return;
    }

    const script = document.createElement("script");
    script.src = "https://accounts.google.com/gsi/client";
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error("Failed to load Google sign-in."));
    document.body.appendChild(script);
  });

  return scriptPromise;
}

export async function initializeGoogleIdentity(
  clientId: string | undefined,
  callback: (credential: string) => void
): Promise<void> {
  if (!clientId) {
    throw new Error("Google sign-in is not configured.");
  }

  await loadGoogleIdentityScript();

  if (!window.google?.accounts?.id) {
    throw new Error("Google sign-in is not available.");
  }

  if (process.env.NODE_ENV === "development") {
    console.info("[AISAM Google Auth]", {
      origin: window.location.origin,
      clientId,
    });
  }

  if (initialized) return;

  initialized = true;
  window.google.accounts.id.initialize({
    client_id: clientId,
    callback: (response) => {
      if (response.credential) {
        callback(response.credential);
      }
    },
    cancel_on_tap_outside: false,
  });
}

export function promptGoogleIdentity(onUnavailable?: (message: string) => void) {
  if (!window.google?.accounts?.id) {
    onUnavailable?.("Google sign-in is not ready yet. Please try again.");
    return;
  }

  window.google.accounts.id.prompt((moment) => {
    if (moment.isNotDisplayed?.()) {
      onUnavailable?.(moment.getNotDisplayedReason?.() || "Google sign-in could not be displayed.");
    } else if (moment.isSkippedMoment?.()) {
      onUnavailable?.(moment.getSkippedReason?.() || "Google sign-in was skipped.");
    }
  });
}

export function renderGoogleIdentityButton(parent: HTMLElement, options: GoogleButtonOptions) {
  if (!window.google?.accounts?.id) {
    throw new Error("Google sign-in is not ready yet. Please try again.");
  }

  parent.innerHTML = "";
  window.google.accounts.id.renderButton(parent, options);
}
