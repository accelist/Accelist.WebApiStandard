﻿using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Contracts.ResponseModels;
using Accelist.WebApiStandard.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.Microservice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<CreateUserRequest> _createUserRequestValidator;

        public UserApiController(IMediator mediator, IValidator<CreateUserRequest> createUserRequestValidator)
        {
            _mediator = mediator;
            _createUserRequestValidator = createUserRequestValidator;
        }

        // GET: api/<UserApiController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UserApiController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserApiController>
        [HttpPost]
        public async Task<ActionResult<CreateUserResponse>> Post([FromBody] CreateUserRequest model, CancellationToken cancellationToken)
        {
            var validationResult = await _createUserRequestValidator.ValidateAsync(model) ??
                throw new InvalidOperationException("Failed to validate CreateUserRequest model.");

            if (validationResult.IsValid == false)
            {
                validationResult.AddToModelState(ModelState);
                return ValidationProblem(ModelState);
            }

            var response = await _mediator.Send(model, cancellationToken);
            return response;
        }

        // PUT api/<UserApiController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserApiController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}