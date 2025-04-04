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

            // Wikipedia uses multiple tables, one per riding
            var tables = doc.DocumentNode.SelectNodes("//table[contains(@class, 'wikitable')]");

            if (tables == null || tables.Count == 0)
                return candidates;

            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tr");
                if (rows == null || rows.Count < 2)
                    continue;

                foreach (var row in rows.Skip(1)) // Skip table headers
                {
                    var cells = row.SelectNodes("td");
                    if (cells == null || cells.Count < 3)
                        continue;

                    // Some cells may include references — strip them out
                    string CleanText(HtmlNode node)
                    {
                        return HtmlEntity.DeEntitize(node.InnerText).Trim();
                    }

                    try
                    {
                        var candidate = new Candidate
                        {
                            Riding = CleanText(cells[0]),
                            Name = CleanText(cells[1]),
                            Party = CleanText(cells[2]).Replace("█", "").Trim(),
                            Result = cells.Count >= 4 ? CleanText(cells[3]) : ""
                        };

                        candidates.Add(candidate);
                    }
                    catch
                    {
                        // Skip bad rows
                        continue;
                    }
                }
            }

            return candidates;
        }
    }
}
