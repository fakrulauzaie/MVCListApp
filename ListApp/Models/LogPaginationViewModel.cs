using ListApp.Models.Entities;

namespace ListApp.Models
{
    public class LogPaginationViewModel
    {
        public List<Logs> Logs { get; set; } = new List<Logs>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public List<string> AvailableLevels { get; set; } = new List<string>();
        public string? SelectedLevel { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
