﻿using System;
using System.Threading.Tasks;
using Hated.Infrastructure.Commands.Activation;
using Hated.Infrastructure.DTO;
using Hated.Infrastructure.Extensions;
using Hated.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hated.Api.Controllers
{
    public class ActivationController : BaseController
    {
        private readonly IActivationService _activationService;
        private readonly ILogger<ActivationController> _logger;

        public ActivationController(IActivationService activationService, ILogger<ActivationController> logger)
        {
            _activationService = activationService;
            _logger = logger;
        }

        //Post activation
        [Authorize]
        [HttpPost("post/activate")]
        public async Task<IActionResult> ActivatePostAsync([FromBody]ActivationPost activationPost)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }
            try
            {
                await _activationService.ActivatePost(activationPost.PostId);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"Returning exception: {e.Message}");
                return Json(new ExceptionDto
                {
                    Error = e.Message
                });
            }
        }

        //Post deactivation
        [Authorize]
        [HttpPost("post/deactivate")]
        public async Task<IActionResult> DeactivatePostAsync([FromBody]ActivationPost activationPost)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }
            try
            {
                await _activationService.DeactivatePost(activationPost.PostId);
                return Ok();
            }
            catch (Exception e)
            {
                _logger.LogError($"Returning exception: {e.Message}");
                return Json(new ExceptionDto
                {
                    Error = e.Message
                });
            }
        }
    }
}
