"use client";

import GuildPopUp from "@/app/_components/(popUps)/GuildPopUp";
import { Guild } from "@prisma/client";
import { useEffect, useState } from "react";
import { toast } from "react-toastify";

export default function TopGuilds() {
  const [guilds, setGuilds] = useState<Guild[]>([]);
  const [currentShowingGuild, setCurrentShowingGuild] = useState(-1);
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch(`/api/guilds`);
        const res = await response.json();
        setGuilds(res);
      } catch {
        toast.error("There was a problem loading guilds.");
      }
    }
    fetchData();
  }, []);

  function handleMouseEnter(i: number, event: React.MouseEvent) {
    setCurrentShowingGuild(i);
    setMousePosition({ x: event.clientX, y: event.clientY });
  }

  return (
    <table className="rankings-table">
      <thead>
        <tr>
          <th>#</th>
          <th>Guild Name</th>
          <th>Score</th>
        </tr>
      </thead>
      <tbody>
        {guilds.map((g, i) => (
          <tr
            key={g.Name}
            onMouseEnter={(event) => handleMouseEnter(i, event)}
            onMouseLeave={() => setCurrentShowingGuild(-1)}
          >
            <td>
              <div
                className={`badge-rank mx-auto ${
                  i === 0 ? "rank-1" : i === 1 ? "rank-2" : i === 2 ? "rank-3" : ""
                }`}
              >
                #{i + 1}
              </div>
            </td>
            <td className="font-bold text-white">{g.Name}</td>
            <td className="text-gold font-bold">
              {g.Score}
              {currentShowingGuild === i && (
                <GuildPopUp
                  guildName={g.Name}
                  style={{ left: mousePosition.x, top: mousePosition.y }}
                />
              )}
            </td>
          </tr>
        ))}
        {guilds.length === 0 && (
          <tr>
            <td colSpan={3} className="text-muted">
              No guilds created yet.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
