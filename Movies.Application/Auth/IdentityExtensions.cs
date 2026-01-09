using Microsoft.AspNetCore.Http;

namespace Movies.Application.Auth;

public static class IdentityExtensions
{
    extension(HttpContext context)
    {
        public Guid? GetUserId()
        {
            var subject = context.User.Claims.SingleOrDefault(x => x.Type == "sub");

            return Guid.TryParse(subject?.Value, out var userId) ? userId : null;
        }
    }
}