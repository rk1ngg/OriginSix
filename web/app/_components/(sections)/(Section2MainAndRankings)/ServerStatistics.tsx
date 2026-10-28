"use client";

import React from "react";
import Link from "next/link";

export default function ServerStatistics() {
  return (
    <>
      {/* Castle Siege Widget */}
      <div className="weicms-card weicms-cs-card">
        <div className="weicms-card-header">
          <div className="header-icon">
            <i className="fa-solid fa-crown text-gold" />
          </div>
          <h3 className="header-title">Castle Siege</h3>
          <span className="badge-stage">Registration</span>
        </div>
        <div className="weicms-card-body">
          <div className="cs-lord-profile">
            <div className="cs-crest-frame">
              <i className="fa-solid fa-chess-rook" />
            </div>
            <div className="cs-lord-meta">
              <span className="label-sub">
                <i className="fa-solid fa-flag" /> Castle Owner
              </span>
              <strong className="lord-guild">None (Unoccupied)</strong>
              <span className="label-sub">
                <i className="fa-solid fa-user-shield" /> Guild Master
              </span>
              <span className="lord-master">-</span>
            </div>
          </div>

          <div className="cs-countdown-block">
            <div className="cs-countdown-label">
              <i className="fa-regular fa-clock" /> Castle Siege Battle
            </div>
            <div className="cs-countdown-timer">Sunday 20:00 Server Time</div>
          </div>

          <Link
            href="/info"
            className="btn weicms-btn-secondary btn-block btn-sm"
          >
            <i className="fa-solid fa-shield" /> Castle Information
          </Link>
        </div>
      </div>

      {/* Event Schedule Widget */}
      <div className="weicms-card weicms-events-card">
        <div className="weicms-card-header">
          <div className="header-icon">
            <i className="fa-solid fa-hourglass-half text-cyan" />
          </div>
          <h3 className="header-title">Event Schedule</h3>
        </div>
        <div className="weicms-card-body no-padding">
          <ul className="event-timer-list">
            <li className="event-item">
              <div className="event-meta">
                <i className="fa-solid fa-chess-rook text-red" />
                <span>Blood Castle</span>
              </div>
              <span className="event-badge badge-next">Every 2h</span>
            </li>
            <li className="event-item">
              <div className="event-meta">
                <i className="fa-solid fa-dungeon text-amber" />
                <span>Devil Square</span>
              </div>
              <span className="event-badge badge-next">Every 2h</span>
            </li>
            <li className="event-item">
              <div className="event-meta">
                <i className="fa-solid fa-skull-crossbones text-purple" />
                <span>Chaos Castle</span>
              </div>
              <span className="event-badge badge-next">Every 2h</span>
            </li>
            <li className="event-item">
              <div className="event-meta">
                <i className="fa-solid fa-dragon text-gold" />
                <span>Golden Invasion</span>
              </div>
              <span className="event-badge badge-invasion">Every 4h</span>
            </li>
          </ul>
        </div>
      </div>

      {/* Discord Community Banner */}
      <div className="weicms-card weicms-discord-card">
        <div className="discord-card-inner">
          <div className="discord-icon-large">
            <i className="fa-brands fa-discord" />
          </div>
          <div className="discord-content">
            <h4 className="discord-title">Join Our Discord</h4>
            <p className="discord-desc">
              Connect with players, get real-time announcements, trade, and join
              events!
            </p>
            <a
              href="https://discord.com/"
              target="_blank"
              rel="noopener noreferrer"
              className="btn weicms-btn-discord btn-sm btn-block"
            >
              <i className="fa-brands fa-discord" /> Join Server
            </a>
          </div>
        </div>
      </div>
    </>
  );
}
