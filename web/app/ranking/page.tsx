"use client";

import { useState } from "react";
import TopPlayers from "./(tables)/TopPlayers";
import TopGuilds from "./(tables)/TopGuilds";
import TopKillers from "./(tables)/TopKillers";
import OnlinePlayers from "./(tables)/OnlinePlayers";

export default function Ranking() {
  const [selectedRanking, setSelectedRanking] = useState<string>("topLevel");

  const tabs = [
    { id: "topLevel", label: "Top Characters", icon: "fa-solid fa-crown" },
    { id: "topKillers", label: "Top Killers", icon: "fa-solid fa-skull" },
    { id: "topGuilds", label: "Top Guilds", icon: "fa-solid fa-shield-halved" },
    { id: "online", label: "Online Players", icon: "fa-solid fa-signal" },
  ];

  return (
    <div className="weicms-card">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-trophy text-gold" />
        </div>
        <h3 className="header-title">Hall of Fame &amp; Server Rankings</h3>
      </div>

      <div className="weicms-card-body">
        <div className="rankings_menu">
          {tabs.map((t) => (
            <button
              key={t.id}
              type="button"
              className={selectedRanking === t.id ? "active" : ""}
              onClick={() => setSelectedRanking(t.id)}
            >
              <i className={t.icon} />
              <span>{t.label}</span>
            </button>
          ))}
        </div>

        {selectedRanking === "topLevel" && <TopPlayers />}
        {selectedRanking === "topGuilds" && <TopGuilds />}
        {selectedRanking === "topKillers" && <TopKillers />}
        {selectedRanking === "online" && <OnlinePlayers />}
      </div>
    </div>
  );
}
