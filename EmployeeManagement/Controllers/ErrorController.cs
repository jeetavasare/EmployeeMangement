using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
	public class ErrorController : Controller
	{
		private readonly ILogger<ErrorController> logger;

		public ErrorController(ILogger<ErrorController> logger)
        {
			this.logger = logger;
		}

        [Route("Error/{statusCode}")]
		public IActionResult HttpStatusCodeHandler(int statusCode)
		{
			switch (statusCode)
			{
				case 404:
					var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
					ViewBag.ErrorMessage = "The resource you are looking for could not be found";
					ViewBag.URL = statusCodeResult.OriginalPath;
					ViewBag.QS = statusCodeResult.OriginalQueryString;

					logger.LogWarning($"404 Error Occured. Path: {statusCodeResult.OriginalPath}" +
						$"\nQuery String: {statusCodeResult.OriginalQueryString}");

					break;
			}

			return View("NotFound");
		}

		[Route("Error")]
		[AllowAnonymous]
		public IActionResult Error()
		{
			var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerFeature>();

			//ViewBag.exceptionPath = exceptionDetails.Path;
			ViewBag.exceptionMessage = exceptionDetails.Error.Message;
			//ViewBag.stackTrace = exceptionDetails.Error.StackTrace;

			logger.LogError($"The path {exceptionDetails.Path} threw {exceptionDetails.Error.Message}");


			return View();
		}
	}
}
