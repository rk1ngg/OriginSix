"use client";

import React, { useEffect, useState } from "react";
import Link from "next/link";
import { ServerStatus } from "@/app/_models/serverStatus";

export default function SecondaryNav() {
  const [status, setStatus] = useState<ServerStatus>();
  const [serverTime, setServerTime] = useState<string>("--:--:--");
  const [localTime, setLocalTime] = useState<string>("--:--:--");

  useEffect(() => {
    const checkStatus = async () => {
      try {
        const res = await fetch(`/api/status`);
        if (res.ok) {
          setStatus(await res.json());
        }
      } catch {
        // Ignore offline gameserver fetch error
      }
    };
    checkStatus();

    const updateClocks = () => {
      const now = new Date();
      const pad = (n: number) => (n < 10 ? `0${n}` : `${n}`);
      setLocalTime(
        `${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`
      );
      const utcMs = now.getTime() + now.getTimezoneOffset() * 60000;
      const srv = new Date(utcMs + 8 * 3600000);
      setServerTime(
        `${pad(srv.getHours())}:${pad(srv.getMinutes())}:${pad(srv.getSeconds())}`
      );
    };

    updateClocks();
    const timer = setInterval(updateClocks, 1000);
    return () => clearInterval(timer);
  }, []);

  const isOnline = status?.state === "Online" || status !== undefined;
  const onlineCount = status?.players ?? 0;

  return (
    <section className="weicms-hero">
      <div className="hero-container">
        <div className="hero-title-area">
          <div className="hero-season-pill">
            <i className="fa-solid fa-shield-halved" /> Season 6 OFFICIAL
          </div>
          <h1 className="hero-main-title">MU OriginSix - Season 6 Episode 3</h1>
          <p className="hero-tagline">
            Join MU Online! The free-to-play fantasy RPG based on the legendary
            Continent of MU! Feel the power of forbidden magic! Explore and
            fight!
          </p>
        </div>

        {/* Live Status & CTA Bar */}
        <div className="hero-stats-panel">
          <div className="status-block-online">
            <div
              className={`pulse-indicator ${isOnline ? "" : "offline"}`}
              title={`Server Status: ${isOnline ? "Online" : "Offline"}`}
            />
            <div className="status-meta">
              <span className="status-title">Online Users</span>
              <div className="status-number">{onlineCount}</div>
            </div>
          </div>

          <div className="status-block-clocks">
            <div className="clock-item">
              <span className="clock-label">
                <i className="fa-solid fa-server text-gold" /> Server Time
              </span>
              <div className="clock-time">{serverTime}</div>
            </div>
            <div className="clock-item">
              <span className="clock-label">
                <i className="fa-solid fa-user-clock text-cyan" /> Your Time
              </span>
              <div className="clock-time">{localTime}</div>
            </div>
          </div>

          <div className="hero-cta-group">
            <Link href="/download" className="btn weicms-btn-primary">
              <i className="fa-solid fa-download" /> Download Game
            </Link>
            <Link href="/register" className="btn weicms-btn-secondary">
              <i className="fa-solid fa-user-plus" /> Join Now
            </Link>
          </div>
        </div>
      </div>
    </section>
  );
}
