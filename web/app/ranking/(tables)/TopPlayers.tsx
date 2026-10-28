"use client";

import { CharacterRanking } from "@/app/_models/character";
import { useEffect, useState } from "react";
import Image from "next/image";
import { StaticImport } from "next/dist/shared/lib/get-img-props";
import { getImage, getClassName } from "@/app/_utils/characterAvatarReturn";
import { toast } from "react-toastify";

export default function TopPlayers() {
  const [characters, setCharacters] = useState<CharacterRanking[]>([]);

  useEffect(() => {
    async function fetchData() {
      try {
        const response = await fetch(`/api/characters/ranking/reset`);
        const res = await response.json();
        setCharacters(res);
      } catch {
        toast.error("There was a problem loading rankings.");
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
          <th>Level</th>
          <th>Master Lv.</th>
          <th>Resets</th>
        </tr>
      </thead>
      <tbody>
        {characters.map((c, i) => {
          const rankClass =
            i === 0
              ? "rank-1"
              : i === 1
              ? "rank-2"
              : i === 2
              ? "rank-3"
              : "";
          return (
            <tr key={c.Name}>
              <td>
                <div className={`badge-rank mx-auto ${rankClass}`}>
                  #{i + 1}
                </div>
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
              <td className="text-gold font-bold">{c.lvl}</td>
              <td className="text-cyan font-bold">{c.masterlvl}</td>
              <td className="font-bold text-white">{c.resets}</td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
