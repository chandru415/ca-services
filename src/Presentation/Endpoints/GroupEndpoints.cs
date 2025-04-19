namespace Presentation.Endpoints
{
    public static class GroupEndpoints
    {
        public static WebApplication MapGroupEndpoints(this WebApplication app)
        {
            app.MapGroup("/api/identity").MapIdentityEndpoints();


            return app;

        }
    }
}
