import React from "react";
import { prisma } from "@/lib/prisma";
import { CharacterRanking } from "@/app/_models/character";
import { Guild } from "@prisma/client";
import Image from "next/image";
import Link from "next/link";
import { StaticImport } from "next/dist/shared/lib/get-img-props";
import { getImage, getClassName } from "@/app/_utils/characterAvatarReturn";

export default async function TopPlayers() {
  const characters = await getTopCharacters();
  const guilds = await getTopGuilds();

  return (
    <div className="weicms-card weicms-rankings-card">
      <div className="weicms-card-header flex-between">
        <div className="flex-align gap-2">
          <i className="fa-solid fa-trophy text-gold header-icon" />
          <h3 className="header-title">Top Rankings</h3>
        </div>
        <div className="weicms-tabs-pills">
          <span className="tab-pill active">Level &amp; Master</span>
          <Link href="/ranking" className="tab-pill">
            Full Hall of Fame
          </Link>
        </div>
      </div>

      <div className="weicms-card-body no-padding">
        <ul className="mini-rank-list">
          {characters.map((c, i) => {
            const rankClass =
              i === 0
                ? "rank-1"
                : i === 1
                ? "rank-2"
                : i === 2
                ? "rank-3"
                : "";
            const totalLevel = Number(c.lvl || 0) + Number(c.masterlvl || 0);
            const className = getClassName(c.CharacterClassId);
            return (
              <li key={c.Name} className="mini-rank-item">
                <div className={`badge-rank ${rankClass}`}>#{i + 1}</div>
                <div className="mini-rank-avatar-wrap">
                  <Image
                    className="mini-rank-avatar"
                    src={getImage(c.CharacterClassId) as StaticImport}
                    width={36}
                    height={36}
                    alt={className}
                  />
                </div>
                <div className="rank-char-meta">
                  <strong className="char-name">{c.Name}</strong>
                  <span className="char-class">{className}</span>
                </div>
                <div className="rank-value-pill">
                  <span className="value">{totalLevel}</span>
                  <span className="lbl">
                    Lv. ({c.lvl}+{c.masterlvl})
                  </span>
                </div>
              </li>
            );
          })}
          {characters.length === 0 && (
            <li className="mini-rank-item">
              <span className="text-muted text-sm">
                No characters ranked yet.
              </span>
            </li>
          )}
        </ul>

        {guilds.length > 0 && (
          <div
            style={{
              borderTop: "1px solid rgba(255,255,255,0.06)",
              padding: "10px 18px",
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between",
              background: "rgba(10, 14, 22, 0.45)",
            }}
          >
            <span className="text-xs uppercase text-muted font-bold">
              <i className="fa-solid fa-shield-halved text-gold mr-1" /> Top
              Guild: <strong className="text-white">{guilds[0].Name}</strong>
            </span>
            <span className="text-xs text-gold font-bold">
              Score: {guilds[0].Score}
            </span>
          </div>
        )}
      </div>
    </div>
  );
}

async function getTopCharacters(): Promise<CharacterRanking[]> {
  try {
    const statAttribute: CharacterRanking[] = await prisma.$queryRaw`
      SELECT sa."CharacterId", c."Name", c."CharacterClassId",
        MAX(CASE WHEN sa."DefinitionId" = '89a891a7-f9f9-4ab5-af36-12056e53a5f7' THEN sa."Value" ELSE 0 END) AS resets,
        MAX(CASE WHEN sa."DefinitionId" = '560931ad-0901-4342-b7f4-fd2e2fcc0563' THEN sa."Value" ELSE 0 END) AS lvl,
        MAX(CASE WHEN sa."DefinitionId" = '70cd8c10-391a-4c51-9aa4-a854600e3a9f' THEN sa."Value" ELSE 0 END) AS masterlvl
      FROM data."StatAttribute" sa
      INNER JOIN data."Character" c ON sa."CharacterId" = c."Id"
      GROUP BY sa."CharacterId", c."Name", c."CharacterClassId"
      ORDER BY resets DESC, lvl DESC, masterlvl DESC
      LIMIT 6;`;
    return statAttribute;
  } catch {
    return [];
  }
}

async function getTopGuilds(): Promise<Guild[]> {
  try {
    return await prisma.guild.findMany({
      take: 3,
      orderBy: { Score: "desc" },
    });
  } catch {
    return [];
  }
}
