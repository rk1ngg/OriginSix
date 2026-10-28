"use client";

import MapsEnum from "@/app/_utils/mapEnum";
import { Character } from "@prisma/client";
import { useEffect, useState } from "react";
import { getImage, getClassName } from "@/app/_utils/characterAvatarReturn";
import Image from "next/image";
import { StaticImport } from "next/dist/shared/lib/get-img-props";
import { toast } from "react-toastify";

export default function TopKillers() {
  const [characters, setCharacters] = useState<Character[]>([]);

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch(`/api/characters/ranking/killers`);
        const res = await response.json();
        setCharacters(res);
      } catch {
        toast.error("There was a problem loading killers.");
      }
    }
    fetchData();
  }, []);

  return (
    <table className="rankings-table">
      <thead>
        <tr>
          <th>#</th>
          <th className="text-left">Character</th>
          <th>Class</th>
          <th>PK Count</th>
          <th>Location</th>
        </tr>
      </thead>
      <tbody>
        {characters.map((c, i) => (
          <tr key={c.Name}>
            <td>
              <div
                className={`badge-rank mx-auto ${
                  i === 0 ? "rank-1" : i === 1 ? "rank-2" : i === 2 ? "rank-3" : ""
                }`}
              >
                #{i + 1}
              </div>
            </td>
            <td className="text-left">
              <div className="flex items-center gap-3">
                <div className="mini-rank-avatar-wrap">
                  <Image
                    className="mini-rank-avatar"
                    src={getImage(c.CharacterClassId!) as StaticImport}
                    width={36}
                    height={36}
                    alt={c.Name}
                  />
                </div>
                <strong className="text-white font-bold">{c.Name}</strong>
              </div>
            </td>
            <td className="text-muted text-xs uppercase font-semibold">
              {getClassName(c.CharacterClassId!)}
            </td>
            <td className="text-red font-bold">{c.PlayerKillCount}</td>
            <td className="text-muted">
              {Object.values(MapsEnum)[
                Object.keys(MapsEnum).indexOf(c.CurrentMapId!)
              ] || "Continent of MU"}
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
