import Link from "next/link";

export default function Footer() {
  return (
    <footer className="weicms-footer">
      <div className="weicms-footer-content">
        <div className="footer-top-grid">
          {/* Brand Column */}
          <div className="footer-col brand-col">
            <Link href="/" className="footer-brand">
              <div className="weicms-brand-crest">
                <i className="fa-solid fa-dragon" />
              </div>
              <div className="footer-brand-info">
                <span className="brand-text">
                  Origin<span>Six</span>
                </span>
                <span className="brand-sub">Season 6 Episode 3</span>
              </div>
            </Link>
            <p className="brand-tagline">
              Join MU Online! The free-to-play fantasy RPG based on the
              legendary Continent of MU! Feel the power of forbidden magic!
              Explore and fight!
            </p>
            <div className="footer-social-links">
              <a
                href="https://discord.com/"
                target="_blank"
                rel="noopener noreferrer"
                className="social-btn discord"
                title="Discord"
              >
                <i className="fa-brands fa-discord" />
              </a>
            </div>
          </div>

          {/* Quick Links */}
          <div className="footer-col">
            <h4 className="footer-heading">Navigation</h4>
            <ul className="footer-link-list">
              <li>
                <Link href="/">
                  <i className="fa-solid fa-chevron-right" /> Home
                </Link>
              </li>
              <li>
                <Link href="/register">
                  <i className="fa-solid fa-chevron-right" /> Register
                </Link>
              </li>
              <li>
                <Link href="/download">
                  <i className="fa-solid fa-chevron-right" /> Downloads
                </Link>
              </li>
              <li>
                <Link href="/ranking">
                  <i className="fa-solid fa-chevron-right" /> Rankings
                </Link>
              </li>
              <li>
                <Link href="/info">
                  <i className="fa-solid fa-chevron-right" /> Server Info
                </Link>
              </li>
            </ul>
          </div>

          {/* Account & Support */}
          <div className="footer-col">
            <h4 className="footer-heading">Account & Support</h4>
            <ul className="footer-link-list">
              <li>
                <Link href="/characters">
                  <i className="fa-solid fa-chevron-right" /> My Characters
                </Link>
              </li>
              <li>
                <Link href="/account">
                  <i className="fa-solid fa-chevron-right" /> Change Password
                </Link>
              </li>
              <li>
                <a
                  href="http://127.0.0.1:8080"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  <i className="fa-solid fa-chevron-right" /> Server Admin Panel
                </a>
              </li>
            </ul>
          </div>

          {/* Legal / Policy */}
          <div className="footer-col">
            <h4 className="footer-heading">Policies & Rules</h4>
            <ul className="footer-link-list">
              <li>
                <Link href="/terms-and-conditions">
                  <i className="fa-solid fa-chevron-right" /> Terms of Service
                </Link>
              </li>
              <li>
                <Link href="/info">
                  <i className="fa-solid fa-chevron-right" /> Server Rates & Rules
                </Link>
              </li>
            </ul>
          </div>
        </div>

        <hr className="footer-divider" />

        <div className="footer-bottom-flex">
          <div className="copyright-text">
            &copy; 2026 <strong>MU OriginSix</strong>. All rights reserved.
            <span className="game-disclaimer">
              MU Online is a registered trademark of Webzen Inc. This local
              server is not affiliated with or endorsed by Webzen Inc.
            </span>
          </div>
          <div className="engine-credit">
            <span className="weicms-powered">
              Powered by <strong>MU OriginSix CMS</strong>
            </span>
          </div>
        </div>
      </div>
    </footer>
  );
}
