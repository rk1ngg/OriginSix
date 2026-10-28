import Link from "next/link";

export default function Download() {
  const gdLink = process.env.NEXT_PUBLIC_GOODLE_DRIVE_LINK;
  const mfLink = process.env.NEXT_PUBLIC_MEDIAFIRE_LINK;
  const megaLink = process.env.NEXT_PUBLIC_MEGA_LINK;

  return (
    <div className="weicms-card">
      <div className="weicms-card-header">
        <div className="header-icon">
          <i className="fa-solid fa-download text-gold" />
        </div>
        <h3 className="header-title">Client Downloads &amp; Requirements</h3>
      </div>

      <div className="weicms-card-body">
        <div
          style={{
            background: "rgba(10, 14, 22, 0.55)",
            border: "1px solid rgba(245, 158, 11, 0.3)",
            borderRadius: "12px",
            padding: "20px",
            marginBottom: "24px",
          }}
        >
          <h4 className="text-lg font-bold text-white mb-1">
            <i className="fa-solid fa-desktop text-gold mr-2" />
            OriginSix Season 6 Episode 3 Client (Windows x64)
          </h4>
          <p className="text-xs text-muted mb-4" style={{ whiteSpace: "normal" }}>
            Local launcher script: <code>D:\OriginSix\scripts\play-local.cmd</code>{" "}
            (connects to <code>127.127.127.127:44406</code>).
          </p>

          <div className="flex flex-wrap gap-3">
            <a
              href={gdLink && gdLink !== "" ? gdLink : "#"}
              className="btn weicms-btn-primary btn-sm"
            >
              <i className="fa-brands fa-google-drive" /> Google Drive Mirror
            </a>
            <a
              href={megaLink && megaLink !== "" ? megaLink : "#"}
              className="btn weicms-btn-secondary btn-sm"
            >
              <i className="fa-solid fa-cloud-arrow-down" /> MEGA Mirror
            </a>
            <a
              href={mfLink && mfLink !== "" ? mfLink : "#"}
              className="btn weicms-btn-secondary btn-sm"
            >
              <i className="fa-solid fa-file-zipper" /> MediaFire Mirror
            </a>
          </div>
        </div>

        <h4 className="text-base font-bold uppercase text-gold mb-3">
          System Requirements
        </h4>
        <table className="rankings-table">
          <thead>
            <tr>
              <th className="text-left">Component</th>
              <th>Minimum</th>
              <th>Recommended</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td className="text-left font-semibold text-white">OS</td>
              <td>Windows 10 (64-bit)</td>
              <td className="text-gold">Windows 10 / 11 (64-bit)</td>
            </tr>
            <tr>
              <td className="text-left font-semibold text-white">Processor</td>
              <td>Dual Core 2.0 GHz</td>
              <td className="text-gold">Quad Core 3.0 GHz+</td>
            </tr>
            <tr>
              <td className="text-left font-semibold text-white">Memory</td>
              <td>2 GB RAM</td>
              <td className="text-gold">8 GB RAM</td>
            </tr>
            <tr>
              <td className="text-left font-semibold text-white">Graphics</td>
              <td>OpenGL / DirectX Compatible</td>
              <td className="text-gold">Dedicated GPU (1 GB+ VRAM)</td>
            </tr>
            <tr>
              <td className="text-left font-semibold text-white">Runtime</td>
              <td colSpan={2}>
                <Link
                  href="https://dotnet.microsoft.com/en-us/download"
                  target="_blank"
                  className="text-cyan"
                >
                  .NET Runtime &amp; Visual C++ Redistributable
                </Link>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}
