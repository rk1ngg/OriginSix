import { prisma } from "@/lib/prisma";
import React from "react";
import NewsComplete from "@/app/_models/NewsComplete";
import ChangePageButton from "./(components)/ChangePageButton";
import Link from "next/link";

const FALLBACK_NEWS = [
  {
    id: "guide-baseline",
    badge: "Notice",
    title: "OriginSix Season 6 Episode 3 — Official Local Baseline Guide",
    date: "Oct 23, 2026",
    href: "/info",
  },
  {
    id: "guide-jewels",
    badge: "Guide",
    title: "OriginSix Guide: Farming Jewels & Guardian Progression",
    date: "Oct 23, 2026",
    href: "/info",
  },
  {
    id: "guide-hunting",
    badge: "Notice",
    title: "OriginSix Newbie Guide: Hunting, Events & Leveling",
    date: "Oct 22, 2026",
    href: "/ranking",
  },
];

export default async function News({ page }: { page: number }) {
  page = page >= 0 ? page : 0;

  const NEWS_PER_PAGE = 4;
  const SKIP = page * NEWS_PER_PAGE || 0;

  let news: NewsComplete[] = [];
  try {
    news = await prisma.openMuWeb_News.findMany({
      skip: SKIP,
      take: NEWS_PER_PAGE,
      orderBy: {
        creationDate: "desc",
      },
    });
  } catch {
    news = [];
  }

  return (
    <div className="weicms-news-section">
      <div className="weicms-section-header">
        <div className="section-title-wrap">
          <i className="fa-solid fa-newspaper text-gold" />
          <h2 className="section-title">Latest News &amp; Announcements</h2>
        </div>
        <Link href="/info" className="text-xs uppercase font-bold text-gold">
          Server Info <i className="fa-solid fa-arrow-right ml-1" />
        </Link>
      </div>

      <div className="weicms-news-list">
        {news.length > 0
          ? news.map((n) => (
              <article key={n.id} className="weicms-news-item">
                <div className="news-item-badge">
                  <span className="badge-tag">
                    <i className="fa-solid fa-bullhorn" /> Notice
                  </span>
                </div>
                <div className="news-item-content">
                  <h3 className="news-item-title">
                    <Link href={`/news/${n.id}`}>{n.title}</Link>
                  </h3>
                  <span className="news-item-date">
                    <i className="fa-regular fa-clock" />{" "}
                    {new Date(n.creationDate).toLocaleDateString()} &bull; by{" "}
                    {n.author}
                  </span>
                </div>
                <Link
                  href={`/news/${n.id}`}
                  className="news-item-arrow"
                  title="Read Article"
                >
                  <i className="fa-solid fa-chevron-right" />
                </Link>
              </article>
            ))
          : FALLBACK_NEWS.map((item) => (
              <article key={item.id} className="weicms-news-item">
                <div className="news-item-badge">
                  <span className="badge-tag">
                    <i className="fa-solid fa-bullhorn" /> {item.badge}
                  </span>
                </div>
                <div className="news-item-content">
                  <h3 className="news-item-title">
                    <Link href={item.href}>{item.title}</Link>
                  </h3>
                  <span className="news-item-date">
                    <i className="fa-regular fa-clock" /> {item.date}
                  </span>
                </div>
                <Link
                  href={item.href}
                  className="news-item-arrow"
                  title="Read Article"
                >
                  <i className="fa-solid fa-chevron-right" />
                </Link>
              </article>
            ))}
      </div>

      <div className="flex mt-4 gap-2">
        {page > 0 && <ChangePageButton forward={false} />}
        {(!page || page >= 0) && news.length > 3 && (
          <ChangePageButton forward={true} />
        )}
      </div>
    </div>
  );
}
