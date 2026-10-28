"use client";

import { CharacterOnline } from "@/app/_models/characterOnline";
import { useEffect, useState } from "react";
import { getImage, getClassName } from "@/app/_utils/characterAvatarReturn";
import Image from "next/image";
import { StaticImport } from "next/dist/shared/lib/get-img-props";
import MapsEnum from "@/app/_utils/mapEnum";

export default function OnlinePlayers() {
  const [characters, setCharacters] = useState<CharacterOnline[]>([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const status = await fetch(`/api/status`);
        if (status.ok) {
          const serverStatus = await status.json();
          const result = await fetch(`/api/characters/ranking/online`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(serverStatus),
          });
          const final = await result.json();
          setCharacters(final);
        }
      } catch {
        // Ignore offline status error
      }
    };
    fetchData();
  }, []);

  return (
    <table className="rankings-table">
      <thead>
        <tr>
          <th>#</th>
          <th className="text-left">Character</th>
          <th>Class</th>
          <th>Map</th>
          <th>Coordinates</th>
        </tr>
      </thead>
      <tbody>
        {characters.map((c, i) => (
          <tr key={c.Name}>
            <td>
              <div className="badge-rank mx-auto">#{i + 1}</div>
            </td>
            <td className="text-left">
              <div className="flex items-center gap-3">
                <div className="mini-rank-avatar-wrap">
                  <Image
                    className="mini-rank-avatar"
                    src={getImage(c.CharacterClassId) as StaticImport}
                    width={36}
                    height={36}
                    alt={c.Name}
                  />
                </div>
                <strong className="text-white font-bold">{c.Name}</strong>
              </div>
            </td>
            <td className="text-muted text-xs uppercase font-semibold">
              {getClassName(c.CharacterClassId)}
            </td>
            <td className="text-gold">
              {Object.values(MapsEnum)[
                Object.keys(MapsEnum).indexOf(c.CurrentMapId!)
              ] || "Lorencia"}
            </td>
            <td className="text-muted">
              {c.PositionX}, {c.PositionY}
            </td>
          </tr>
        ))}
        {characters.length === 0 && (
          <tr>
            <td colSpan={5} className="text-muted">
              No characters currently online in game servers.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
