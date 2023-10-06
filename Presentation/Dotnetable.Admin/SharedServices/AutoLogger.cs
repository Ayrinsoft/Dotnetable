using Dotnetable.Shared.DTO.Public;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dotnetable.Admin.SharedServices
{
    public class AutoLogger : TypeFilterAttribute
    {
        public AutoLogger() : base(typeof(AutoLogActionFilterImpl))
        {
        }

        private class AutoLogActionFilterImpl : IActionFilter
        {

            private DateTime _requestTime;
            private IDictionary<string, object> _requestArgs { get; set; }

            public AutoLogActionFilterImpl()
            {
                _requestTime = DateTime.UtcNow;
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                _requestArgs = context.ActionArguments;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                List<string> splitedURL = context.HttpContext.Request.Path.Value.Split("/").Where(i => i != "").ToList();

                try
                {
                    if (context.Result is not null)
                    {
                        var responseItem = (PublicControllerResponse)((context.Result as ObjectResult)?.Value ?? null);
                        if (responseItem is not null && (!responseItem.Success || responseItem.ErrorException is not null))
                        {
                            responseItem.ResponseData = null;
                        }
                    }
                }
                catch (Exception) { }

            }
        }

    }
}