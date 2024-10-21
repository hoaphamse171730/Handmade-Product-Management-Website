using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using Microsoft.EntityFrameworkCore;

namespace HandmadeProductManagement.Services.Service
{
    public class ReplyService : IReplyService
    {
        private readonly IUnitOfWork _unitOfWork;
        DateTime vietnamTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        public ReplyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<ReplyModel>> GetByPageAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageNumber);
            }

            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidPageSize);
            }

            var replies = await _unitOfWork.GetRepository<Reply>()
                                           .Entities
                                           .Where(r => r.DeletedTime == null)
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .Select(r => new ReplyModel
                                           {
                                               Id = r.Id,
                                               Content = r.Content,
                                               Date = r.Date,
                                               ReviewId = r.ReviewId,
                                               ShopId = r.ShopId
                                           })
                                           .ToListAsync();

            return replies;
        }

        public async Task<ReplyModel> GetByIdAsync(string replyId)
        {
            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            var reply = await _unitOfWork.GetRepository<Reply>().Entities
                                 .Where(r => r.Id == replyId && r.DeletedTime == null)
                                 .FirstOrDefaultAsync() ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReplyNotFound);

            return new ReplyModel
            {
                Id = reply.Id,
                Content = reply.Content,
                Date = reply.Date,
                ReviewId = reply.ReviewId,
                ShopId = reply.ShopId
            };
        }

        public async Task<bool> CreateAsync(ReplyModel replyModel, Guid userId)
        {
            if (!Guid.TryParse(userId.ToString(), out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (replyModel == null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageNullReplyModel);
            }

            if (string.IsNullOrWhiteSpace(replyModel.ReviewId) || !Guid.TryParse(replyModel.ReviewId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReviewIdFormat);
            }

            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(replyModel.ReviewId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFound);

            var shop = await _unitOfWork.GetRepository<Shop>()
                .Entities
                .FirstOrDefaultAsync(s => s.UserId == userId)
                ?? throw new BaseException.UnauthorizedException(StatusCodeHelper.Unauthorized.ToString(), Constants.ErrorMessageUnauthorizedShopAccess);

            replyModel.ShopId = shop.Id;

            var product = await _unitOfWork.GetRepository<Product>()
                .Entities
                .FirstOrDefaultAsync(p => p.Id == review.ProductId && p.ShopId == shop.Id)
                ?? throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageShopCannotReply);

            var existingReply = await _unitOfWork.GetRepository<Reply>()
                .Entities
                .FirstOrDefaultAsync(r => r.ReviewId == replyModel.ReviewId);

            if (existingReply != null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageReplyAlreadyExists);
            }

            var reply = new Reply
            {
                Content = replyModel.Content,
                ReviewId = replyModel.ReviewId,
                ShopId = replyModel.ShopId,
                Date = vietnamTime,
                CreatedBy = replyModel.ShopId,
                LastUpdatedBy = replyModel.ShopId
            };

            await _unitOfWork.GetRepository<Reply>().InsertAsync(reply);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(string replyId, Guid userId, ReplyModel updatedReply)
        {
            if (!Guid.TryParse(userId.ToString(), out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReplyIdFormat);
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReplyNotFound);

            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null || shop.Id != existingReply.ShopId)
            {
                throw new BaseException.UnauthorizedException(StatusCodeHelper.Unauthorized.ToString(), Constants.ErrorMessageUnauthorizedUpdate);
            }

            // Validate that the reply belongs to the product associated with the review
            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(existingReply.ReviewId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFound);

            var product = await _unitOfWork.GetRepository<Product>()
                                           .Entities
                                           .FirstOrDefaultAsync(p => p.Id == review.ProductId && p.ShopId == shop.Id)
                ?? throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageUnauthorizedUpdate);

            if (!string.IsNullOrWhiteSpace(updatedReply.Content))
            {
                existingReply.Content = updatedReply.Content;
            }

            if (existingReply.DeletedTime != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageSoftDeletedReply);
            }

            existingReply.LastUpdatedBy = existingReply.ShopId;
            existingReply.LastUpdatedTime = vietnamTime;

            await _unitOfWork.GetRepository<Reply>().UpdateAsync(existingReply);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(string replyId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReplyIdFormat);
            }

            if (!Guid.TryParse(userId.ToString(), out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReplyNotFound);

            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null || shop.Id != existingReply.ShopId)
            {
                throw new BaseException.ForbiddenException(StatusCodeHelper.Forbidden.ToString(), Constants.ErrorMessageForbidden);
            }

            if (existingReply.DeletedTime != null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageSoftDeletedReply);
            }

            existingReply.LastUpdatedBy = existingReply.ShopId;
            existingReply.DeletedTime = vietnamTime;

            await _unitOfWork.GetRepository<Reply>().DeleteAsync(replyId);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(string replyId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReplyIdFormat);
            }

            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReplyNotFound);

            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null || shop.Id != existingReply.ShopId)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageSoftDeletedReply);
            }

            existingReply.DeletedTime = vietnamTime;
            existingReply.DeletedBy = existingReply.ShopId;

            await _unitOfWork.GetRepository<Reply>().UpdateAsync(existingReply);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> RecoverDeletedReplyAsync(string replyId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidReplyIdFormat);
            }

            if (!Guid.TryParse(userId.ToString(), out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            // Check if the reply exists
            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReplyNotFound);

            // Verify shop ownership and authorization
            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null || shop.Id != existingReply.ShopId)
            {
                throw new BaseException.UnauthorizedException(StatusCodeHelper.Unauthorized.ToString(), Constants.ErrorMessageUnauthorizedUpdate);
            }

            // Check if the reply is actually soft-deleted
            if (existingReply.DeletedTime == null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Reply is not deleted");
            }

            // Verify that the associated review still exists and is not deleted
            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(existingReply.ReviewId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageReviewNotFound);

            if (review.DeletedTime != null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Cannot recover reply for a deleted review");
            }

            // Verify that the product still exists and belongs to the shop
            var product = await _unitOfWork.GetRepository<Product>()
                                           .Entities
                                           .FirstOrDefaultAsync(p => p.Id == review.ProductId && p.ShopId == shop.Id)
                ?? throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageUnauthorizedUpdate);

            if (product.DeletedTime != null)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), "Cannot recover reply for a deleted product");
            }

            // Recover the reply
            existingReply.DeletedTime = null;
            existingReply.DeletedBy = null;
            existingReply.LastUpdatedTime = vietnamTime;
            existingReply.LastUpdatedBy = existingReply.ShopId;

            await _unitOfWork.GetRepository<Reply>().UpdateAsync(existingReply);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}