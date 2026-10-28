"use client";

import { signOut } from "next-auth/react";
import Link from "next/link";
import { useEffect } from "react";

export default function UserPanel({
  role,
  username,
}: {
  role: string;
  username?: string;
}) {
  useEffect(() => {
    localStorage.setItem("role", role);
  }, [role]);

  const isGm = role === "GAME_MASTER";

  function onSignOut() {
    localStorage.removeItem("role");
    signOut();
  }

  return (
    <div className="weicms-card weicms-auth-widget">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-shield-halved text-gold" />
        </div>
        <h3 className="header-title">User Control Panel</h3>
      </div>

      <div className="user-greeting-panel">
        <div className="user-avatar-wrap">
          <i className="fa-solid fa-user-shield" />
        </div>
        <div>
          <div className="text-xs uppercase text-muted">Logged in as</div>
          <div className="font-bold text-white text-sm">
            {username || "Adventurer"}
          </div>
          <span className="text-xs text-gold font-semibold">
            {isGm ? "Game Master" : "Player Account"}
          </span>
        </div>
      </div>

      <ul className="weicms-usercp-menu">
        <li>
          <Link href="/characters" className="weicms-usercp-link">
            <span className="icon-wrap">
              <i className="fa-solid fa-users-rays" />
            </span>
            <span className="title">My Characters & Stats</span>
            <i className="fa-solid fa-chevron-right arrow" />
          </Link>
        </li>
        <li>
          <Link href="/account" className="weicms-usercp-link">
            <span className="icon-wrap">
              <i className="fa-solid fa-key" />
            </span>
            <span className="title">Change Password</span>
            <i className="fa-solid fa-chevron-right arrow" />
          </Link>
        </li>
        {isGm && (
          <li>
            <Link href="/admin/news" className="weicms-usercp-link">
              <span className="icon-wrap">
                <i className="fa-solid fa-bullhorn" />
              </span>
              <span className="title">Manage News</span>
              <i className="fa-solid fa-chevron-right arrow" />
            </Link>
          </li>
        )}
        <li>
          <a
            href="http://127.0.0.1:8080"
            target="_blank"
            rel="noopener noreferrer"
            className="weicms-usercp-link"
          >
            <span className="icon-wrap">
              <i className="fa-solid fa-server" />
            </span>
            <span className="title">MU OriginSix Admin Panel</span>
            <i className="fa-solid fa-arrow-up-right-from-square arrow" />
          </a>
        </li>
      </ul>

      <div className="p-4 pt-2">
        <button
          type="button"
          onClick={onSignOut}
          className="btn weicms-btn-danger btn-sm btn-block"
        >
          <i className="fa-solid fa-power-off" /> Sign Out
        </button>
      </div>
    </div>
  );
}
