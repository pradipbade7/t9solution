namespace T9Backend.Services
{
    public interface IRateLimitService
    {
        bool IsRateLimited(string clientIdentifier);
    }
}