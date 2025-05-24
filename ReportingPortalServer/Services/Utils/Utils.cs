namespace ReportingPortalServer.Services.Utils
{
    public static class Utils
    {
        public static string? GetJwt(HttpContext httpcontext)
        {
            string? authHeader = httpcontext.Request.Headers.Authorization.FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            return authHeader["Bearer ".Length..].Trim();
        }
    }
}
