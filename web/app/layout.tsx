import "./globals.css";
import type { Metadata } from "next";
import Navbar from "./_components/(navbar)/Navbar";
import SecondaryNav from "./_components/(navbar)/SecondaryNav";
import Footer from "./_components/(footer)/Footer";
import Section1 from "./_components/(sections)/(section1NewsAndUserPanel)/Section1";
import { ToastContainer } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";
import ServerStatistics from "./_components/(sections)/(Section2MainAndRankings)/ServerStatistics";

export const metadata: Metadata = {
  title: "MU OriginSix - Season 6 Episode 3",
  description:
    "Join MU Online! The free-to-play fantasy RPG based on the legendary Continent of MU! Feel the power of forbidden magic! Explore and fight!",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>
        <div id="weicms-app">
          <Navbar />
          <SecondaryNav />

          <main className="weicms-main">
            <div className="weicms-layout-grid">
              <div className="weicms-content-col">{children}</div>
              <aside className="weicms-sidebar-col">
                <Section1 />
                <ServerStatistics />
              </aside>
            </div>
          </main>

          <Footer />
        </div>
        <ToastContainer
          position="top-right"
          autoClose={3000}
          closeOnClick
          pauseOnHover
          theme="dark"
        />
      </body>
    </html>
  );
}
