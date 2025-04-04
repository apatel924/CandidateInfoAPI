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

                var headerCells = rows[0].SelectNodes("th")?.Select(h => h.InnerText.Trim().ToLower()).ToList();
                if (headerCells == null || !headerCells.Any(h => h.Contains("party")) || !headerCells.Any(h => h.Contains("riding") || h.Contains("name")))
                    continue; // skip non-candidate tables

                foreach (var row in rows.Skip(1)) // Skip table headers
                {
                    var cells = row.SelectNodes("td");
                    if (cells == null || cells.Count < 4)
                        continue;

                    string CleanText(HtmlNode node) => HtmlEntity.DeEntitize(node.InnerText).Trim();

                    string party = CleanText(cells[2]).Replace("█", "").Trim();
                    string result = cells[3].InnerText.Contains("✓") ? "Elected" : "Not Elected";

                    // Skip rows with non-party garbage
                    if (string.IsNullOrWhiteSpace(party) || party.Any(char.IsDigit) || party.Length < 2)
                        continue;

                    candidates.Add(new Candidate
                    {
                        Riding = CleanText(cells[0]),
                        Name = CleanText(cells[1]),
                        Party = party,
                        Result = result
                    });
                }
            }

            return candidates;
        }
    }
}
