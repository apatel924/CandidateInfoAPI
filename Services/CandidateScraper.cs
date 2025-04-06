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
                    if (cells == null)
                        continue;

                    try
                    {
                        string CleanText(HtmlNode node) => HtmlEntity.DeEntitize(node.InnerText).Trim();

                        // Check for valid data
                        if (cells.Count < 4) continue;

                        var rawRiding = CleanText(cells[0]);
                        var rawName = CleanText(cells[1]);
                        var rawParty = CleanText(cells[2]);
                        var rawResult = cells[3].InnerText;

                        // 💥 Clean party field
                        string party = rawParty.Replace("█", "").Trim();

                        // 🚫 Filter out obvious names (people)
                        if (party.Split(" ").Length == 2 && char.IsUpper(party[0]))
                            continue;

                        // ✅ Determine result
                        string result = rawResult.Contains("✓") ? "Elected" : "Not Elected";

                        candidates.Add(new Candidate
                        {
                            Riding = rawRiding,
                            Name = rawName,
                            Party = party,
                            Result = result
                        });
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            return candidates;
        }
    }
}
