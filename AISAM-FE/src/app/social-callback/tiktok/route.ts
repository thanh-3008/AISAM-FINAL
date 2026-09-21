import { type NextRequest, NextResponse } from "next/server";
import { extractOriginFromOAuthState } from "@/lib/originResolver";

function asJavaScriptString(value: string | null): string {
  return JSON.stringify(value ?? "").replaceAll("<", "\\u003c");
}

function getBackendApiBaseUrl(): string {
  const configured = process.env.NEXT_PUBLIC_API_URL?.trim();
  return configured ? configured.replace(/\/$/, "") : "/backend-api";
}

function getFallbackApiBaseUrl(): string {
  const configured = process.env.NEXT_PUBLIC_FALLBACK_API_URL?.trim();
  return configured ? configured.replace(/\/$/, "") : "";
}

function canUseLocalCallback(request: NextRequest): boolean {
  const host = request.nextUrl.hostname.toLowerCase();
  return (
    host === "localhost" ||
    host === "127.0.0.1" ||
    host.endsWith(".ngrok.app") ||
    host.endsWith(".ngrok-free.app") ||
    host.endsWith(".ngrok-free.dev")
  );
}

function getCurrentHost(request: NextRequest): string {
  const forwardedHost = request.headers.get("x-forwarded-host");
  if (forwardedHost) {
    return forwardedHost.split(",")[0].trim().toLowerCase();
  }
  const host = request.headers.get("host");
  if (host) {
    return host.split(",")[0].trim().toLowerCase();
  }
  return request.nextUrl.host.toLowerCase();
}

export function GET(request: NextRequest) {
  const localCallbackUrl = canUseLocalCallback(request)
    ? process.env.TIKTOK_LOCAL_CALLBACK_URL?.trim()
    : "";
  if (localCallbackUrl) {
    const target = new URL(localCallbackUrl);
    if (request.nextUrl.origin !== target.origin) {
      target.search = request.nextUrl.search;
      return NextResponse.redirect(target, {
        status: 302,
        headers: { "Cache-Control": "no-store" },
      });
    }
  }

  const code = request.nextUrl.searchParams.get("code");
  const state = request.nextUrl.searchParams.get("state");
  const oauthError = request.nextUrl.searchParams.get("error_description") || request.nextUrl.searchParams.get("error");
  const backendApiBaseUrl = getBackendApiBaseUrl();
  const fallbackApiBaseUrl = getFallbackApiBaseUrl();

  // Origin Relay: Only redirect if the state parameter explicitly specifies an
  // allowed origin domain different from the current request's host.
  // This prevents self-redirect loops and protocol mismatches (http behind reverse-proxy vs https).
  const stateOrigin = extractOriginFromOAuthState(state);
  if (stateOrigin) {
    try {
      const stateUrl = new URL(stateOrigin);
      const currentHost = getCurrentHost(request);
      if (stateUrl.host.toLowerCase() !== currentHost) {
        const target = new URL(`${stateOrigin}/social-callback/tiktok`);
        target.search = request.nextUrl.search;
        return NextResponse.redirect(target, {
          status: 302,
          headers: {
            "Cache-Control": "no-store",
            "Referrer-Policy": "no-referrer",
          },
        });
      }
    } catch {
      // Ignore URL parse errors
    }
  }

  const html = `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>TikTok authorization | AISAM</title>
  <style>
    :root { color-scheme: light dark; font-family: Arial, sans-serif; }
    body { margin: 0; min-height: 100vh; display: grid; place-items: center; background: #f4f5fb; color: #20212a; }
    .card { width: min(520px, calc(100vw - 40px)); padding: 28px; border-radius: 18px; background: white; box-shadow: 0 12px 40px rgba(34, 45, 90, .13); text-align: center; }
    .spinner { width: 28px; height: 28px; margin: 0 auto 16px; border: 3px solid #dbe3ff; border-top-color: #155eef; border-radius: 50%; animation: spin .8s linear infinite; }
    h1 { margin: 0 0 10px; font-size: 20px; }
    p { margin: 0; color: #646b7a; line-height: 1.5; overflow-wrap: anywhere; }
    button { margin-top: 20px; border: 0; border-radius: 10px; padding: 11px 18px; background: #155eef; color: white; cursor: pointer; font-weight: 700; }
    .error .spinner { display: none; }
    .error h1 { color: #c62828; }
    @keyframes spin { to { transform: rotate(360deg); } }
  </style>
</head>
<body>
  <main class="card" id="card">
    <div class="spinner"></div>
    <h1 id="title">Processing TikTok authorization</h1>
    <p id="message">Validating the callback...</p>
    <button id="back" type="button" hidden>Back to Social Accounts</button>
  </main>
  <script>
    (() => {
      const code = ${asJavaScriptString(code)};
      const state = ${asJavaScriptString(state)};
      const oauthError = ${asJavaScriptString(oauthError)};
      const backendApiBaseUrl = ${asJavaScriptString(backendApiBaseUrl)};
      const fallbackApiBaseUrl = ${asJavaScriptString(fallbackApiBaseUrl)};
      const card = document.getElementById('card');
      const title = document.getElementById('title');
      const message = document.getElementById('message');
      const back = document.getElementById('back');

      const fail = (text) => {
        card.classList.add('error');
        // Note: Expiry detection depends on backend string: "OAuth state is invalid or expired."
        const isExpired = /expired/i.test(text || '');
        if (isExpired) {
          title.textContent = 'TikTok authorization expired';
          message.textContent = 'The authorization session has expired. Please return to Social Accounts and try connecting again.';
        } else {
          title.textContent = 'TikTok connection failed';
          message.textContent = text;
        }
        back.textContent = 'Back to Social Accounts';
        back.hidden = false;
      };
      back.addEventListener('click', () => location.replace('/social'));

      const readJson = (key) => {
        try { return JSON.parse(localStorage.getItem(key) || 'null'); }
        catch { return null; }
      };

      const getApiBaseUrl = () => {
        try {
          const saved = sessionStorage.getItem('aisam_active_api_url_' + window.location.hostname) ||
                        sessionStorage.getItem('aisam_active_api_url');
          if (saved) return saved.replace(/\\/$/, '');
        } catch {}

        const host = window.location.hostname.toLowerCase();
        if (host === 'aisam.ddns.net' || host.endsWith('.ddns.net')) {
          if (fallbackApiBaseUrl) return fallbackApiBaseUrl.replace(/\\/$/, '');
          return window.location.origin + '/api';
        }

        return backendApiBaseUrl.replace(/\\/$/, '');
      };

      const apiUrl = (path) => getApiBaseUrl() + path;

      const run = async () => {
        if (oauthError || !code || !state) {
          fail(oauthError || 'TikTok returned incomplete callback parameters.');
          return;
        }

        const token = localStorage.getItem('aisam_token');
        const workspace = readJson('aisam_active_workspace');
        const profile = readJson('aisam_active_profile');
        if (!token || !workspace?.id) {
          fail('AISAM login or workspace session is missing. Sign in, select a workspace, and connect TikTok again.');
          return;
        }

        const headers = {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + token,
          'X-RBAC-Contract-Version': '2',
          'X-Workspace-Id': workspace.id,
          ...(profile?.id ? { 'X-Profile-Id': profile.id } : {})
        };
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 30000);

        try {
          // Strip ?code&state from URL to prevent duplicate POSTs on page refresh / back-forward
          try {
            window.history.replaceState({}, document.title, window.location.pathname);
          } catch {}

          message.textContent = 'Exchanging the authorization code...';
          const callbackResponse = await fetch(apiUrl('/social-auth/tiktok/callback'), {
            method: 'POST', headers, body: JSON.stringify({ code, state }), signal: controller.signal
          });
          const callbackResult = await callbackResponse.json().catch(() => null);
          if (!callbackResponse.ok || !callbackResult?.data) {
            throw new Error(callbackResult?.message || callbackResult?.error?.errorMessage || 'TikTok callback request failed.');
          }

          const account = callbackResult.data;
          const brandId = sessionStorage.getItem('social_connect_brand_id');
          if (brandId) {
            message.textContent = 'Linking the TikTok account to your brand...';
            const targetsResponse = await fetch(apiUrl('/social/accounts/' + encodeURIComponent(account.id) + '/available-targets'), {
              headers, signal: controller.signal
            });
            const targetsResult = await targetsResponse.json().catch(() => null);
            if (!targetsResponse.ok) {
              throw new Error(targetsResult?.message || 'Unable to read the TikTok account target.');
            }
            const targetIds = (targetsResult?.data || []).map((target) => target.providerTargetId).filter(Boolean);
            if (targetIds.length) {
              const linkResponse = await fetch(apiUrl('/social/accounts/' + encodeURIComponent(account.id) + '/link-targets'), {
                method: 'POST', headers,
                body: JSON.stringify({ provider: 'tiktok', providerTargetIds: targetIds, brandId }),
                signal: controller.signal
              });
              const linkResult = await linkResponse.json().catch(() => null);
              if (!linkResponse.ok) {
                throw new Error(linkResult?.message || 'Unable to link TikTok to the selected brand.');
              }
            }
          }

          sessionStorage.removeItem('social_connect_brand_id');
          title.textContent = 'TikTok connected';
          message.textContent = 'Redirecting to Social Accounts...';
          setTimeout(() => location.replace('/social'), 500);
        } catch (error) {
          fail(error?.name === 'AbortError' ? 'The request timed out after 30 seconds. Confirm that the backend API is reachable.' : (error?.message || 'Unable to connect TikTok.'));
        } finally {
          clearTimeout(timeout);
        }
      };

      run();
    })();
  </script>
</body>
</html>`;

  return new NextResponse(html, {
    headers: {
      "Content-Type": "text/html; charset=utf-8",
      "Cache-Control": "no-store, no-cache, must-revalidate",
      "Referrer-Policy": "no-referrer",
      "X-Content-Type-Options": "nosniff",
    },
  });
}
