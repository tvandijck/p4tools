namespace p4tools
{
    public class PerforceChangeList
    {
        public string? Change { get; set; }
        public string? Client { get; set; }
        public string? Date { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? User { get; set; }

        public List<string> Files { get; } =  new List<string>();
    }
}