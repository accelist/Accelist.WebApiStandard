using Accelist.WebApiStandard.Logics.Requests;
using AutoMapper;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangePasswordController : ControllerBase
    {
        private readonly IMediator Mediator;
        private readonly ChangePasswordRequestValidator ChangePasswordRequestValidator;
        private readonly IMapper Mapper;

        public ChangePasswordController(
            IMediator mediator,
            ChangePasswordRequestValidator changePasswordRequestValidator,
            IMapper mapper
        )
        {
            this.Mediator = mediator;
            this.ChangePasswordRequestValidator = changePasswordRequestValidator;
            this.Mapper = mapper;
        }

        // POST api/<ChangePasswordController>
        [HttpPost]
        public async Task<ActionResult<bool>> Post([FromBody] ChangePasswordRequestModel model, CancellationToken cancellationToken)
        {
            var request = Mapper.Map<ChangePasswordRequestModel, ChangePasswordRequest>(model);
            request.UserID = User.FindFirst("UserId")?.Value;

            // What if we need to validate inputs based on user ID?
            // For example, password must not be used by the user before...
            // This is how:
            var validationResult = await ChangePasswordRequestValidator.ValidateAsync(request, cancellationToken);
            if (validationResult != null && validationResult.IsValid == false)
            {
                validationResult.AddToModelState(ModelState, null);
                return ValidationProblem(ModelState);
            }

            var response = await Mediator.Send(request, cancellationToken);
            return response;
        }
    }
}
