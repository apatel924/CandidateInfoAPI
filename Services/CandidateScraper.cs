using HtmlAgilityPack;
using CandidateInfoAPI.Models;

namespace CandidateInfoAPI.Services
{
    public class CandidateScraper
    {
        public List<Candidate> ScrapeFromWikipedia()
        {
            var candidates = new List<Candidate>();
            var url = "https://en.wikipedia.org/wiki/2023_Alberta_general_election";

            var web = new HtmlWeb();
            var doc = web.Load(url);

            var tables = doc.DocumentNode.SelectNodes("//table[contains(@class, 'wikitable')]");
            if (tables == null || tables.Count == 0)
                return candidates;

            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tr");
                if (rows == null || rows.Count < 2)
                    continue;

                string CleanText(HtmlNode node) => HtmlEntity.DeEntitize(node.InnerText).Trim();

                var headerNodes = rows[0].SelectNodes("th");
                if (headerNodes == null || headerNodes.Count < 2)
                    continue;

                var headerCells = headerNodes.Select(h => CleanText(h)).ToList();

                foreach (var row in rows.Skip(1))
                {
                    var cells = row.SelectNodes("td");
                    if (cells == null || cells.Count < 2)
                        continue;

                    string riding = CleanText(cells[0]);

                    for (int i = 1; i < Math.Min(cells.Count, headerCells.Count - 1); i++)
                    {
                        string candidateName = CleanText(cells[i]);
                        if (string.IsNullOrWhiteSpace(candidateName)) continue;

                        string party = headerCells[i];

                        candidates.Add(new Candidate
                        {
                            Name = candidateName,
                            Riding = riding,
                            Party = party,
                            Result = "Unknown"
                        });
                    }
                }
            }

            return candidates;
        }
    }
}
