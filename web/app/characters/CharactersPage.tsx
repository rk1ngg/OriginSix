"use client";

import { StaticImport } from "next/dist/shared/lib/get-img-props";
import Image from "next/image";
import AddStatsCard from "./AddStatsCard";
import { useState } from "react";
import { getImage, getClassName } from "@/app/_utils/characterAvatarReturn";
import { CharacterEdit } from "../_models/characterEdit";
import { toast } from "react-toastify";
import { useRouter } from "next/navigation";

export default function CharactersPage({
  result,
}: {
  result: CharacterEdit[];
}) {
  const [characters, setCharacters] = useState([
    false,
    false,
    false,
    false,
    false,
  ]);
  const [lastClicked, setLastClicked] = useState(-1);

  const zenToReset = process.env.NEXT_PUBLIC_ZEN_TO_RESET;
  const zenToPkClear = process.env.NEXT_PUBLIC_ZEN_TO_PKCLEAR;
  const zenToResetStats = process.env.NEXT_PUBLIC_ZEN_TO_RESET_STATS;

  function openStatistic(index: number) {
    const arr = Array(5).fill(false);
    if (lastClicked !== index) {
      arr[index] = true;
      setLastClicked(index);
    } else {
      arr[index] = false;
      setLastClicked(-1);
    }
    setCharacters(arr);
  }

  const router = useRouter();

  async function resetCharacter(name: string, clasId: string) {
    const response = await fetch(`/api/characters/reset`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, clasId }),
    });
    const body = await response.json();
    if (response.ok) {
      toast.success(body.message);
      router.refresh();
    } else {
      toast.error(body.message);
    }
  }

  async function pkClear(name: string) {
    const response = await fetch(`/api/characters/pkclear`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name }),
    });
    const body = await response.json();
    if (response.ok) {
      toast.success(body.message);
    } else {
      toast.error(body.message);
    }
  }

  async function resetStats(name: string, clasId: string) {
    const response = await fetch(`/api/characters/resetStats`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, clasId }),
    });
    const body = await response.json();
    if (response.ok) {
      toast.success(body.message);
      openStatistic(-1);
      router.refresh();
    } else {
      toast.error(body.message);
    }
  }

  return (
    <div className="weicms-card">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-shield-halved text-gold" />
        </div>
        <h3 className="header-title">My Characters &amp; Stat Management</h3>
      </div>

      <div className="weicms-card-body">
        {result.length === 0 && (
          <div className="text-center py-8 text-muted">
            <i className="fa-solid fa-user-slash text-2xl mb-2 block text-gold" />
            No characters found on this account yet.
          </div>
        )}

        <div className="flex flex-col gap-3">
          {result?.map((c, i) => (
            <div
              key={c.name}
              style={{
                background: "rgba(10, 14, 22, 0.55)",
                border: "1px solid rgba(255, 255, 255, 0.07)",
                borderRadius: "10px",
                padding: "16px",
              }}
            >
              <div className="flex flex-wrap items-center justify-between gap-4">
                <div className="flex items-center gap-3">
                  <div className="mini-rank-avatar-wrap">
                    <Image
                      src={getImage(c.characterClassId) as StaticImport}
                      alt={c.name}
                      width={40}
                      height={40}
                      className="mini-rank-avatar"
                    />
                  </div>
                  <div>
                    <div className="text-white font-bold text-base">
                      {c.name}
                    </div>
                    <div className="text-xs uppercase text-muted">
                      {getClassName(c.characterClassId)} &bull; Lv.{" "}
                      <strong className="text-gold">{c.lvl}</strong> (Master{" "}
                      <strong className="text-cyan">{c.masterlvl}</strong>) &bull;
                      Resets: <strong className="text-white">{c.resets}</strong>
                    </div>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2">
                  {zenToReset && zenToReset !== "" && (
                    <button
                      type="button"
                      className="btn weicms-btn-secondary btn-sm"
                      onClick={() => resetCharacter(c.name, c.characterClassId)}
                    >
                      <i className="fa-solid fa-rotate text-gold" /> Reset
                    </button>
                  )}
                  <button
                    type="button"
                    className="btn weicms-btn-primary btn-sm"
                    onClick={() => openStatistic(i)}
                  >
                    <i className="fa-solid fa-circle-plus" /> Add Stats
                  </button>
                  {zenToPkClear && zenToPkClear !== "" && (
                    <button
                      type="button"
                      className="btn weicms-btn-secondary btn-sm"
                      onClick={() => pkClear(c.name)}
                    >
                      <i className="fa-solid fa-broom text-red" /> PK Clear
                    </button>
                  )}
                  {zenToResetStats && zenToResetStats !== "" && (
                    <button
                      type="button"
                      className="btn weicms-btn-secondary btn-sm"
                      onClick={() => resetStats(c.name, c.characterClassId)}
                    >
                      <i className="fa-solid fa-sliders text-cyan" /> Reset
                      Stats
                    </button>
                  )}
                </div>
              </div>

              {characters[i] && <AddStatsCard characterEdit={c} key={i} />}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
