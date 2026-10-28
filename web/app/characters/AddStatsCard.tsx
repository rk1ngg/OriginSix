"use client";

import React, { useState } from "react";
import { CharacterEdit } from "../_models/characterEdit";
import { toast } from "react-toastify";

export default function AddStatsCard({
  characterEdit,
}: {
  characterEdit: CharacterEdit;
}) {
  const [basePoints, setBasePoints] = useState(characterEdit.levelUpPoints);
  const [baseStr, setBaseStr] = useState(characterEdit.strength);
  const [baseAgi, setBaseAgi] = useState(characterEdit.agility);
  const [baseVit, setBaseVit] = useState(characterEdit.vitality);
  const [baseEne, setBaseEne] = useState(characterEdit.energy);
  const [baseLead, setBaseLead] = useState(characterEdit.leadership);

  const [str, setStr] = useState(0);
  const [agi, setAgi] = useState(0);
  const [vit, setVit] = useState(0);
  const [ene, setEne] = useState(0);
  const [lead, setLead] = useState(0);

  const dlCodes = [
    "00000040-0011-0000-0000-000000000000",
    "00000040-0010-0000-0000-000000000000",
  ];

  async function addStats() {
    const response = await fetch(`/api/characters/addstats`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        name: characterEdit.name,
        str,
        agi,
        vit,
        ene,
        lead,
      }),
    });
    const responseFinal = await response.json();
    if (response.ok) {
      setBasePoints(basePoints - (str + agi + vit + ene + lead));
      setBaseStr(str + baseStr);
      setBaseAgi(agi + baseAgi);
      setBaseVit(vit + baseVit);
      setBaseEne(ene + baseEne);
      setBaseLead(lead + baseLead);
      setStr(0);
      setAgi(0);
      setVit(0);
      setEne(0);
      setLead(0);
      toast.success(responseFinal.message);
    } else {
      toast.error(responseFinal.message);
    }
  }

  return (
    <div
      style={{
        marginTop: "14px",
        padding: "16px",
        borderTop: "1px solid rgba(245, 158, 11, 0.2)",
        background: "rgba(15, 20, 31, 0.65)",
        borderRadius: "8px",
      }}
    >
      <div className="mb-3 text-sm font-bold text-gold">
        Available Level-Up Points: <span className="text-white">{basePoints}</span>
      </div>

      <div className="grid grid-cols-2 gap-3 max-w-md mb-4">
        <div className="flex items-center justify-between text-xs">
          <span>
            Strength: <strong className="text-white">{baseStr}</strong>
          </span>
          <input
            type="number"
            value={str}
            onChange={(e) => setStr(+e.target.value)}
            className="weicms-input remove-arrow"
            style={{ width: "90px" }}
          />
        </div>
        <div className="flex items-center justify-between text-xs">
          <span>
            Agility: <strong className="text-white">{baseAgi}</strong>
          </span>
          <input
            type="number"
            value={agi}
            onChange={(e) => setAgi(+e.target.value)}
            className="weicms-input remove-arrow"
            style={{ width: "90px" }}
          />
        </div>
        <div className="flex items-center justify-between text-xs">
          <span>
            Vitality: <strong className="text-white">{baseVit}</strong>
          </span>
          <input
            type="number"
            value={vit}
            onChange={(e) => setVit(+e.target.value)}
            className="weicms-input remove-arrow"
            style={{ width: "90px" }}
          />
        </div>
        <div className="flex items-center justify-between text-xs">
          <span>
            Energy: <strong className="text-white">{baseEne}</strong>
          </span>
          <input
            type="number"
            value={ene}
            onChange={(e) => setEne(+e.target.value)}
            className="weicms-input remove-arrow"
            style={{ width: "90px" }}
          />
        </div>
        {dlCodes.includes(characterEdit.characterClassId) && (
          <div className="flex items-center justify-between text-xs">
            <span>
              Command: <strong className="text-white">{baseLead}</strong>
            </span>
            <input
              type="number"
              value={lead}
              onChange={(e) => setLead(+e.target.value)}
              className="weicms-input remove-arrow"
              style={{ width: "90px" }}
            />
          </div>
        )}
      </div>

      <button
        type="button"
        onClick={addStats}
        className="btn weicms-btn-primary btn-sm"
      >
        <i className="fa-solid fa-check" /> Apply Stat Points
      </button>
    </div>
  );
}
