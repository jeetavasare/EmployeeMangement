﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.Controllers
{
	public class ErrorController : Controller
	{
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
					break;
			}

			return View("NotFound");
		}

		[Route("Error")]
		[AllowAnonymous]
		public IActionResult Error()
		{
			var exceptionDetails = HttpContext.Features.Get<IExceptionHandlerFeature>();
			ViewBag.exceptionPath = exceptionDetails.Path;
			ViewBag.exceptionMessage = exceptionDetails.Error.Message;
			ViewBag.stackTrace = exceptionDetails.Error.StackTrace;

			return View();
		}
	}
}
