using Microsoft.AspNetCore.Mvc;
using Finora.Web.Services;
using Finora.Messages.Users;

namespace Finora.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<UserController> _logger;

        public UserController(IMessagePublisher messagePublisher, ILogger<UserController> logger)
        {
            _messagePublisher = messagePublisher;
            _logger = logger;
        }

        [HttpPost("create")]
        public IActionResult CreateUser([FromBody] AddUser userRequest)
        {
            try
            {
                // Validate the request
                if (string.IsNullOrEmpty(userRequest.Email) || string.IsNullOrEmpty(userRequest.FirstName))
                {
                    return BadRequest("Email and FirstName are required");
                }

                // Publish the user creation message
                var messageId = _messagePublisher.PublishUserCreation(userRequest);

                return Ok(new
                {
                    MessageId = messageId,
                    Message = "User creation request has been queued for processing",
                    UserEmail = userRequest.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue user creation for email: {Email}", userRequest.Email);
                return StatusCode(500, "Failed to process user creation request");
            }
        }
    }
}
