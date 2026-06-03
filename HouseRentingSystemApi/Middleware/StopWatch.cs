namespace HouseRentingSystemApi.Middleware
{
    public class StopWatch
    {
        private readonly RequestDelegate _next;

        public StopWatch(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            await _next(context);

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            context.Response.Headers["Response-Time"] = elapsedMilliseconds.ToString("F2");
        }
    }
}