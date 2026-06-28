import Link from "next/link";

export const metadata = {
  title: "Terms of Service | AISAM",
  description: "Terms governing use of the AISAM educational project.",
};

export default function TermsOfServicePage() {
  return (
    <main className="min-h-screen bg-surface text-on-surface px-6 py-12">
      <article className="mx-auto max-w-3xl rounded-3xl border border-outline-variant/30 bg-surface-container-lowest p-8 shadow-sm md:p-12">
        <Link href="/" className="text-label-sm font-semibold text-primary hover:underline">AISAM</Link>
        <h1 className="mt-5 text-4xl font-bold tracking-tight">Terms of Service</h1>
        <p className="mt-2 text-body-sm text-on-surface-variant">Last updated: June 27, 2026</p>

        <div className="mt-10 space-y-8 text-body-md leading-7 text-on-surface-variant">
          <section>
            <h2 className="text-xl font-bold text-on-surface">1. About AISAM</h2>
            <p className="mt-2">AISAM is an educational project for managing brands, products, social media content, approvals, schedules, and supported social account connections. It is provided for demonstration, learning, and evaluation purposes.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">2. Account responsibilities</h2>
            <p className="mt-2">Users must provide accurate account information, protect their credentials, use only workspaces they are authorized to access, and promptly report suspected unauthorized access.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">3. Acceptable use</h2>
            <p className="mt-2">Users may not use AISAM to violate laws, intellectual property rights, privacy rights, social platform policies, or the security of AISAM and connected services. Users remain responsible for content they create, upload, approve, or publish.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">4. TikTok and other third-party services</h2>
            <p className="mt-2">Social account connections are authorized by the user through the provider&apos;s OAuth flow. Use of TikTok and other providers is also governed by their respective terms, policies, scopes, application review rules, and technical limitations.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">5. Educational availability</h2>
            <p className="mt-2">AISAM may be modified, interrupted, reset, or discontinued as part of development and academic evaluation. Features may depend on third-party sandbox environments and may be available only to approved test accounts.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">6. Disclaimer</h2>
            <p className="mt-2">The service is provided on an as-is and as-available basis for educational use. No guarantee is made that generated content, analytics, integrations, or third-party services will be uninterrupted or suitable for commercial decisions.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">7. Suspension and removal</h2>
            <p className="mt-2">Access may be suspended or removed when necessary to protect the project, comply with provider policies, prevent misuse, or conclude an academic demonstration.</p>
          </section>
          <section>
            <h2 className="text-xl font-bold text-on-surface">8. Changes and contact</h2>
            <p className="mt-2">These terms may be updated as the project changes. Questions can be directed to the AISAM project administrator through the project team that provided access to this deployment.</p>
          </section>
        </div>

        <div className="mt-10 border-t border-outline-variant/30 pt-6">
          <Link href="/privacy" className="text-label-sm font-semibold text-primary hover:underline">Read the Privacy Policy</Link>
        </div>
      </article>
    </main>
  );
}
