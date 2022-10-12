using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Contracts.ResponseModels;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateUserRequest> _createUserRequestValidator;

        public UserController(IMediator mediator, IValidator<CreateUserRequest> createUserRequestValidator)
        {
            _mediator = mediator;
            _createUserRequestValidator = createUserRequestValidator;
        }

        // GET: api/<UserApiController>
        [HttpGet]
        public IAsyncEnumerable<ListUserResponse> Get(string? search, string? previousId, string? previousGivenName)
        {
            // https://swr.vercel.app/docs/pagination#useswrinfinite
            // useSwrInfinite allows getting previous page data to be used as next page query

            var response = _mediator.CreateStream(new ListUserRequest
            {
                Search = search,
                PreviousGivenName = previousGivenName,
                PreviousId = previousId
            });
            return response;
        }

        // GET api/<UserApiController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserApiController>
        [HttpPost]
        public async Task<ActionResult<CreateUserResponse>> Post(
            [FromBody] CreateUserRequest model,
            CancellationToken cancellationToken)
        {
            var validationResult = await _createUserRequestValidator.ValidateAsync(model, cancellationToken) ??
                throw new InvalidOperationException("Failed to validate CreateUserRequest model.");

            if (validationResult.IsValid == false)
            {
                validationResult.AddToModelState(ModelState);
                return ValidationProblem(ModelState);
            }

            var response = await _mediator.Send(model, cancellationToken);
            return response;
        }

        // POST api/<UserApiController>/5
        [HttpPost("{id}")]
        public void Post(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserApiController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
