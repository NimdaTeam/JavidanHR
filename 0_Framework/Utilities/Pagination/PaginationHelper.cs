
namespace _0_Framework.Utilities.Pagination
{
    public static class PaginationHelper
    {
        public static PaginationResult<T> Paginate<T>(PaginationRequest<T> request)
        {
            var pageSize = 50;
            if (request.ModelList.Any())
            {
                var paginatedModel = request.ModelList.Skip((request.CurrentPage - 1) * pageSize)
                    .Take(pageSize).ToList();

                var totalPages = (int)Math.Ceiling(request.ModelList.Count() / (double)pageSize);

                var currentPage = request.CurrentPage;

                var pagesToShow = new List<int>();


                for (int i = 1; i <= Math.Min(8, totalPages); i++)
                {
                    pagesToShow.Add(i);
                }

                if (totalPages > 10)
                {
                    pagesToShow.Add(-1);

                    pagesToShow.Add(totalPages - 1);
                    pagesToShow.Add(totalPages);
                }

                return new PaginationResult<T>()
                {
                    PaginatedList = paginatedModel.ToList(),
                    CurrentPage = currentPage,
                    PagesToShow = pagesToShow,
                    TotalPages = totalPages,
                    SearchQuery = request.SearchQuery
                };
            }

            return new PaginationResult<T>()
            {
                PaginatedList = request.ModelList.ToList(),
                CurrentPage = request.CurrentPage,
                PagesToShow = new List<int>(1),
                TotalPages = 1,
                SearchQuery = request.SearchQuery
            };


        }
    }

    public abstract class PaginationResultBase
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<int> PagesToShow { get; set; }
        public string SearchQuery { get; set; }
    }

    public class PaginationResult<T>: PaginationResultBase
    {
        public List<T> PaginatedList { get; set; }
    }

    public class PaginationRequest<T>
    {
        public List<T> ModelList { get; set; }

        public int CurrentPage { get; set; }

        public string SearchQuery { get; set; }
    }
}
