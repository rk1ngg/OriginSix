import React from "react";

export default function Info() {
  const specs = [
    { label: "Server Version", value: "Season 6 Episode 3" },
    { label: "Base Experience", value: "1.0x (Vanilla Baseline)" },
    { label: "Master Experience", value: "1.0x" },
    { label: "Item Drop Rate", value: "30%" },
    { label: "Max Character Level", value: "400 (+200 Master Level)" },
    { label: "Guardian Progression", value: "Ghost Horse → Ullr (Planned S6 Adaptation)" },
    { label: "Elf Soldier Buff", value: "Up to Level 220" },
    { label: "Client Architecture", value: "Windows x64 Release (RmlUi + S18 Theme)" },
  ];

  return (
    <div className="weicms-card">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-circle-info text-gold" />
        </div>
        <h3 className="header-title">Server Information &amp; Rates</h3>
      </div>

      <div className="weicms-card-body">
        <p className="text-sm text-muted mb-5" style={{ whiteSpace: "normal" }}>
          MU OriginSix is a clean, reproducible Season 6 Episode 3 baseline.
          All core mechanics, formulas, and rates are
          inspected and verified against reference Season 6 behavior.
        </p>

        <table className="rankings-table">
          <thead>
            <tr>
              <th className="text-left">Setting / Parameter</th>
              <th className="text-right">Configuration</th>
            </tr>
          </thead>
          <tbody>
            {specs.map((row) => (
              <tr key={row.label}>
                <td className="text-left font-semibold text-white">
                  {row.label}
                </td>
                <td className="text-right text-gold font-bold">{row.value}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
