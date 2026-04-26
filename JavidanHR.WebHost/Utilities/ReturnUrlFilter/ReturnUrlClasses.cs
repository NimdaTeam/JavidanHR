namespace JavidanHR.WebHost.Utilities.ReturnUrlFilter
{
    public class RequestContext
    {
        public string? ReturnUrl { get; set; }
    }

    public interface IRequestContextAccessor
    {
        RequestContext Context { get; }
    }

    public class RequestContextAccessor : IRequestContextAccessor
    {
        public RequestContext Context { get; } = new();
    }
}
