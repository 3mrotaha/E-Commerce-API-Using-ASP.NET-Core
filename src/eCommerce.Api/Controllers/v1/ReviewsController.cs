using eCommerce.Application.DTOs.ProductReview;
using eCommerce.Application.Exceptions;
using eCommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers.V1
{
    public class ReviewsController : CustomControllerBase
    {
        private readonly IProductReviewService _productReviewService;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IProductReviewService productReviewService, ILogger<ReviewsController> logger)
        {
            _productReviewService = productReviewService;
            _logger = logger;
        }

        [HttpPost("{userId:guid}")]
        [Authorize(Roles = "USER", AuthenticationSchemes = "Bearer")] // only user can add a review
        public async Task<IActionResult> AddReview([FromRoute] Guid userId, [FromBody] CreateProductReviewDto createProductReviewDto)
        {
            if (!IsCurrentUser(userId))
            {
                throw new UnautherizedException("You are not authorized to add a review for another user.");
            }

            if (!ModelState.IsValid)
            {
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList());
            }

            var review = await _productReviewService.CreateReviewAsync(userId, createProductReviewDto);
            return ToCreatedAtActionResult(review, nameof(GetReview), new { reviewId = review.Value?.Id });
        }

        [HttpPut("{userId:guid}/{reviewId:guid}")]
        [Authorize(Roles = "USER", AuthenticationSchemes = "Bearer")] // only user can update his review
        public async Task<IActionResult> UpdateReview([FromRoute] Guid reviewId, [FromRoute] Guid userId, [FromBody] UpdateProductReviewDto updateProductReviewDto)
        {
            if (!IsCurrentUser(userId))
            {
                throw new UnautherizedException("You are not authorized to update another user's review.");
            }

            if (!ModelState.IsValid)
            {
                throw new BadRequestException("Invalid data", ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList());
            }

            var review = await _productReviewService.UpdateReviewAsync(reviewId, userId, updateProductReviewDto);
            return ToActionResult(review);
        }

        [HttpGet("{reviewId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")] // all accounts can get the review
        public async Task<IActionResult> GetReview([FromRoute] Guid reviewId)
        {
            var review = await _productReviewService.GetReviewByIdAsync(reviewId);
            return ToActionResult(review);
        }

        [HttpGet("product/{productId:guid}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "USER,ADMIN,SUPER_ADMIN")]
        public async Task<IActionResult> GetReviewsByProduct([FromRoute] Guid productId)
        {
            var reviews = await _productReviewService.GetReviewsByProductIdAsync(productId);
            return ToActionResult(reviews);
        }

        [HttpDelete("{userId:guid}/{reviewId:guid}")]
        [Authorize(Roles = "USER", AuthenticationSchemes = "Bearer")] // only review owner can delete the review
        public async Task<IActionResult> DeleteReview([FromRoute] Guid userId, [FromRoute] Guid reviewId)
        {
            if (!IsCurrentUser(userId))
            {
                throw new UnautherizedException("You are not authorized to delete another user's review.");
            }

            var isDeleted = await _productReviewService.DeleteReviewAsync(reviewId, userId);
            return ToMessageActionResult(isDeleted, new { Message = $"Review with Id={reviewId} deleted successfully" });
        }
    }
}
