import News from "./_components/(sections)/(Section2MainAndRankings)/News";
import TopPlayers from "./_components/(sections)/(Section2MainAndRankings)/TopPlayers";

export default function Home({
  searchParams,
}: {
  searchParams: { page: number };
}) {
  return (
    <>
      <TopPlayers />
      <News page={searchParams.page || 0} />
    </>
  );
}
