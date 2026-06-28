import Link from "next/link";

export const metadata = {
  title: "Privacy Policy | AISAM",
  description: "AISAM privacy policy and social platform data practices.",
};

export default function PrivacyPolicyPage() {
  return (
    <main className="min-h-screen bg-surface text-on-surface px-6 py-12">
      <article className="mx-auto max-w-3xl rounded-3xl border border-outline-variant/30 bg-surface-container-lowest p-8 shadow-sm md:p-12">
        <Link href="/" className="text-label-sm font-semibold text-primary hover:underline">AISAM</Link>
        <h1 className="mt-5 text-4xl font-bold tracking-tight">Privacy Policy</h1>
        <p className="mt-2 text-body-sm text-on-surface-variant">Last updated: June 27, 2026</p>

        <div className="mt-10 space-y-8 text-body-md leading-7 text-on-surface-variant">
          <section>
            <h2 className="text-xl font-bold text-on-surface">1. Overview</h2>
            <p className="mt-2">AISAM is an educational social media content management project. This policy explains how AISAM handles information when users create workspaces, manage content, and connect supported social platforms.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">2. Information we process</h2>
            <p className="mt-2">AISAM processes account information, workspace and brand data, content supplied by users, and technical records needed to operate and secure the service.</p>
            <p className="mt-2">When a user connects TikTok, AISAM requests only the approved basic profile scope. This may provide a TikTok Open ID, display name, avatar, access token, refresh token, and token expiration information.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">3. How information is used</h2>
            <p className="mt-2">Information is used to authenticate users, display connected social accounts, associate an account with a selected brand, manage content, protect the service, and support the educational demonstration of AISAM.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">4. Social platform tokens</h2>
            <p className="mt-2">OAuth authorization codes are exchanged by the AISAM backend. Client secrets are never sent to the browser. Social access and refresh tokens are encrypted before storage and are not displayed to users or included in public API responses.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">5. Sharing and sale of data</h2>
            <p className="mt-2">AISAM does not sell personal information. Data is shared with a social platform only when necessary to complete an action explicitly requested by the user, such as connecting an account or retrieving authorized profile information.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">6. Retention and user choices</h2>
            <p className="mt-2">Users may disconnect a social account from the Social Accounts screen. Project administrators may remove associated account and workspace records when requested or when they are no longer needed for the educational project.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">7. Security and third-party services</h2>
            <p className="mt-2">AISAM uses access controls, workspace isolation, encrypted credentials, and server-side OAuth state validation. TikTok and other third-party services operate under their own terms and privacy policies.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">8. Contact</h2>
            <p className="mt-2">Questions or deletion requests can be submitted to the AISAM project administrator through the project team that provided access to this educational deployment.</p>
          </section>
        </div>

        <div className="mt-10 border-t border-outline-variant/30 pt-6">
          <Link href="/terms" className="text-label-sm font-semibold text-primary hover:underline">Read the Terms of Service</Link>
        </div>
      </article>
    </main>
  );
}
