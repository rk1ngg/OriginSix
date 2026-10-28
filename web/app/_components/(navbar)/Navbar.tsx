"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";

export default function Navbar() {
  const pathname = usePathname();
  const [bannerDismissed, setBannerDismissed] = useState(false);
  const [mobileOpen, setMobileOpen] = useState(false);

  const navItems = [
    { href: "/", label: "Home", icon: "fa-solid fa-house" },
    { href: "/info", label: "Info", icon: "fa-solid fa-circle-info" },
    { href: "/download", label: "Downloads", icon: "fa-solid fa-download" },
    { href: "/ranking", label: "Rankings", icon: "fa-solid fa-trophy" },
    { href: "/characters", label: "Characters", icon: "fa-solid fa-shield-halved" },
    { href: "/register", label: "Register", icon: "fa-solid fa-user-plus" },
  ];

  return (
    <>
      {!bannerDismissed && (
        <div
          className="weicms-announcement-banner"
          style={{
            position: "relative",
            zIndex: 101,
            width: "100%",
            background:
              "linear-gradient(90deg, rgba(16, 12, 5, 0.98) 0%, rgba(35, 25, 7, 0.98) 50%, rgba(16, 12, 5, 0.98) 100%)",
            borderBottom: "1px solid rgba(245, 158, 11, 0.45)",
            boxShadow: "0 4px 20px rgba(245, 158, 11, 0.2)",
            color: "#fbbf24",
            fontSize: "13px",
            padding: "9px 18px",
          }}
        >
          <div
            style={{
              maxWidth: "1320px",
              margin: "0 auto",
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              flexWrap: "wrap",
              gap: "12px",
            }}
          >
            <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
              <span
                style={{
                  display: "inline-flex",
                  alignItems: "center",
                  justifyContent: "center",
                  width: "26px",
                  height: "26px",
                  borderRadius: "6px",
                  background: "rgba(245, 158, 11, 0.15)",
                  border: "1px solid rgba(245, 158, 11, 0.35)",
                }}
              >
                <i className="fa-solid fa-bell" style={{ fontSize: "13px" }} />
              </span>
              <span style={{ fontWeight: 600, letterSpacing: "0.3px" }}>
                ⚔️ OriginSix Season 6 Episode 3 — Local Baseline Server Active
              </span>
            </div>

            <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
              <Link
                href="/register"
                style={{
                  display: "inline-flex",
                  alignItems: "center",
                  gap: "6px",
                  background: "linear-gradient(135deg, #f59e0b, #d97706)",
                  color: "#07090e",
                  fontWeight: 700,
                  fontSize: "11.5px",
                  padding: "4px 12px",
                  borderRadius: "6px",
                  textTransform: "uppercase",
                  letterSpacing: "0.5px",
                }}
              >
                <span>Create Account</span>
                <i className="fa-solid fa-arrow-right" style={{ fontSize: "10px" }} />
              </Link>
              <button
                type="button"
                onClick={() => setBannerDismissed(true)}
                style={{
                  background: "transparent",
                  border: "none",
                  color: "#94a3b8",
                  fontSize: "18px",
                  lineHeight: 1,
                  padding: "2px 6px",
                }}
                title="Dismiss Announcement"
              >
                &times;
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Global Topbar */}
      <header className="weicms-topbar">
        <div className="weicms-topbar-inner">
          <div className="topbar-rates">
            <span className="rate-badge">
              <i className="fa-solid fa-gamepad text-gold" />
              <span>Season 6 EP3</span>
            </span>
            <span className="topbar-sep">&bull;</span>
            <span className="rate-badge">
              <i className="fa-solid fa-bolt text-amber" />
              <span>
                Exp: <strong>x1.0</strong>
              </span>
            </span>
            <span className="topbar-sep">&bull;</span>
            <span className="rate-badge">
              <i className="fa-solid fa-gem text-cyan" />
              <span>
                Drop: <strong>30%</strong>
              </span>
            </span>
          </div>

          <div className="topbar-actions">
            <div className="topbar-auth-links">
              <Link href="/account">
                <i className="fa-solid fa-user-gear" /> Account CP
              </Link>
              <span className="topbar-sep">|</span>
              <Link href="/register" className="text-gold">
                <i className="fa-solid fa-user-plus" /> Register
              </Link>
            </div>
          </div>
        </div>
      </header>

      {/* Main Sticky Navigation Bar */}
      <nav className="weicms-navbar">
        <div className="weicms-nav-container">
          <Link href="/" className="weicms-brand">
            <div className="weicms-brand-crest">
              <i className="fa-solid fa-dragon" />
            </div>
            <div className="weicms-brand-text">
              <span className="brand-main">
                Origin<span>Six</span>
              </span>
              <span className="brand-sub">Season 6</span>
            </div>
          </Link>

          <ul className={`weicms-nav-list ${mobileOpen ? "mobile-open" : ""}`}>
            {navItems.map((item) => {
              const isActive =
                item.href === "/"
                  ? pathname === "/"
                  : pathname?.startsWith(item.href);
              return (
                <li
                  key={item.href}
                  className={`weicms-nav-item ${isActive ? "active" : ""}`}
                >
                  <Link href={item.href} className="weicms-nav-link">
                    <i className={`${item.icon} nav-icon`} />
                    <span>{item.label}</span>
                  </Link>
                </li>
              );
            })}
          </ul>

          <button
            type="button"
            className="mobile-nav-toggle"
            onClick={() => setMobileOpen(!mobileOpen)}
            aria-label="Toggle Navigation"
          >
            <i className="fa-solid fa-bars" />
          </button>
        </div>
      </nav>
    </>
  );
}
