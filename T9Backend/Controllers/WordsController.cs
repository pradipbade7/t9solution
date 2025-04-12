using Microsoft.AspNetCore.Mvc;
using T9Backend.Models;
using T9Backend.Services;

namespace T9Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class WordsController : ControllerBase
    {

        private const int MaxInputLength = 45;
        private const string StrictMatchIndicator = "0";
        private const string ErrorMsgInvalidInput = "Only digits are allowed in the input";
        private const string ErrorMsgInputTooLong = "Input exceeds maximum length of {0} digits";
        private const string ErrorMsgRateLimit = "Too many requests. Please try again later.";
        private const string ErrorMsgNullInput = "Digits parameter is required";
        private const string ErrorMsgServerError = "An error occurred processing your request";

        private readonly IWordService _wordService;
        private readonly ILogger<WordsController> _logger;
        private readonly IRateLimitService _rateLimitService;
        public WordsController(
            IWordService wordService,
            ILogger<WordsController> logger,
            IRateLimitService rateLimitService)
        {
            _wordService = wordService ?? throw new ArgumentNullException(nameof(wordService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rateLimitService = rateLimitService ?? throw new ArgumentNullException(nameof(rateLimitService));

        }


        [HttpGet("match")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public ActionResult<T9MatchResponse> Match([FromQuery] string digits)
        {
            try
            {

                // Check rate limiting first
                string clientIp = GetClientIp();
                if (_rateLimitService.IsRateLimited(clientIp))
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests,
                         new ErrorResponse(ErrorMsgRateLimit));
                }
                // Check for null input
                if (digits == null)
                {
                    _logger.LogWarning("T9 match request rejected: null input");
                    return BadRequest(new ErrorResponse(ErrorMsgNullInput));
                }

                // Validate input length - prevent excessive long inputs
                if (digits.Length > MaxInputLength)
                {
                    _logger.LogWarning("T9 match request rejected: input too long ({Length} digits)", digits.Length);
                    return BadRequest(new ErrorResponse(string.Format(ErrorMsgInputTooLong, MaxInputLength)));
                }

                // Extract strict flag
                bool isStrict = digits.EndsWith(StrictMatchIndicator);
                string processedDigits = isStrict ? digits.Substring(0, digits.Length - 1) : digits;

                _logger.LogInformation("T9 match request received: digits={Digits}, processed={ProcessedDigits}, strict={Strict}",
                    digits, processedDigits, isStrict);

                // Empty input check after processing
                if (string.IsNullOrEmpty(processedDigits))
                {
                    _logger.LogWarning("T9 match request rejected: empty input after processing");
                    return BadRequest(new ErrorResponse(ErrorMsgNullInput));
                }

                if (!processedDigits.All(char.IsDigit))
                {
                    return BadRequest(new ErrorResponse(ErrorMsgInvalidInput));
                }

                var matches = _wordService.Match(processedDigits, isStrict);

                _logger.LogInformation("Found {Count} matches for digits={Digits}, strict={Strict}",
                    matches.Count, processedDigits, isStrict);

                return Ok(new T9MatchResponse
                {
                    Digits = digits,
                    IsStrict = isStrict,
                    Words = matches
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing T9 match request");
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new ErrorResponse(ErrorMsgServerError));
            }
        }

        private string GetClientIp()
        {
            var forwardedHeader = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                // Get the first IP if multiple are specified
                return forwardedHeader.Split(',')[0].Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}