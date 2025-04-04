namespace CandidateInfoAPI.Models
{
    public class Candidate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Riding { get; set; } = string.Empty;
        public string Party { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
    }
}
