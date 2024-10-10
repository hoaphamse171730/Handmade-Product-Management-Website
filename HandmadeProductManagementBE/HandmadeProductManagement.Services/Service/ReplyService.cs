using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Services.Service
{
    public class ReplyService : IReplyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ReplyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<ReplyModel>> GetByPageAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException("invalid_page_number", "Page Number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_page_size", "Page Size must be greater than zero.");
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
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException("invalid_reply_id", "Reply ID cannot be null or empty.");
            }

            var reply = await _unitOfWork.GetRepository<Reply>().Entities
                                 .Where(r => r.Id == replyId && r.DeletedTime == null) 
                                 .FirstOrDefaultAsync();
            if (reply == null)
            {
                throw new BaseException.NotFoundException("reply_not_found", "Reply not found.");
            }

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
            if (string.IsNullOrWhiteSpace(userId.ToString()) || !Guid.TryParse(userId.ToString(), out _))
            {
                throw new BaseException.BadRequestException("invalid_user_id_format", "Invalid userId format.");
            }

            if (replyModel == null)
            {
                throw new BaseException.BadRequestException("null_reply_model", "Reply model cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(replyModel.ReviewId) || !Guid.TryParse(replyModel.ReviewId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(replyModel.ReviewId);
            if (review == null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);
            if (shop == null)
            {
                throw new BaseException.UnauthorizedException("unauthorized_shop_access", "User does not own this shop.");
            }
            replyModel.ShopId = shop.Id;

            var product = await _unitOfWork.GetRepository<Product>()
                                           .Entities
                                           .FirstOrDefaultAsync(p => p.Id == review.ProductId && p.ShopId == shop.Id);
            if (product == null)
            {
                throw new BaseException.BadRequestException("shop_cannot_reply", "The specified shop cannot reply to this review.");
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>()
                                       .Entities
                                       .FirstOrDefaultAsync(r => r.ReviewId == replyModel.ReviewId);
            if (existingReply != null)
            {
                throw new BaseException.BadRequestException("reply_already_exists", "This review already has a reply.");
            }

            var reply = new Reply
            {
                Content = replyModel.Content,
                ReviewId = replyModel.ReviewId,
                ShopId = replyModel.ShopId,
                Date = DateTime.UtcNow,
                CreatedBy = replyModel.ShopId,
                LastUpdatedBy = replyModel.ShopId
            };

            await _unitOfWork.GetRepository<Reply>().InsertAsync(reply);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(string replyId, Guid userId, ReplyModel updatedReply)
        {
            if (string.IsNullOrWhiteSpace(userId.ToString()) || !Guid.TryParse(userId.ToString(), out _))
            {
                throw new BaseException.BadRequestException("invalid_user_id_format", "Invalid userId format.");
            }

            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException("invalid_reply_id", "Reply ID cannot be null or empty.");
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null)
            {
                throw new BaseException.NotFoundException("reply_not_found", "Reply not found.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null || shop.Id != existingReply.ShopId)
            {
                throw new BaseException.UnauthorizedException("unauthorized_update", "User does not own the shop associated with this reply.");
            }

            // Validate that the reply belongs to the product associated with the review
            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(existingReply.ReviewId);
            if (review == null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Review not found.");
            }

            var product = await _unitOfWork.GetRepository<Product>()
                                           .Entities
                                           .FirstOrDefaultAsync(p => p.Id == review.ProductId && p.ShopId == shop.Id);
            if (product == null)
            {
                throw new BaseException.BadRequestException("unauthorized_update", "The specified shop cannot update this reply as it doesn't belong to the product of the review.");
            }

            if (!string.IsNullOrWhiteSpace(updatedReply.Content))
            {
                existingReply.Content = updatedReply.Content;
            }

            if (existingReply.DeletedTime != null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Cannot update the review which has been soft-delete.");
            }

            existingReply.LastUpdatedBy = existingReply.ShopId;
            existingReply.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<Reply>().UpdateAsync(existingReply);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(string replyId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            if (string.IsNullOrWhiteSpace(userId.ToString()) || !Guid.TryParse(userId.ToString(), out _))
            {
                throw new BaseException.BadRequestException("invalid_user_id_format", "Invalid userId format.");
            }

            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException("invalid_reply_id", "Reply ID cannot be null or empty.");
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null)
            {
                throw new BaseException.NotFoundException("reply_not_found", "Reply not found.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null || shop.Id != existingReply.ShopId)
            {
                throw new BaseException.UnauthorizedException("unauthorized_delete", "User does not own the shop associated with this reply.");
            }

            if (existingReply.DeletedTime != null)
            {
                throw new BaseException.NotFoundException("review_not_found", "Cannot update the review which has been soft-delete.");
            }

            existingReply.LastUpdatedBy = existingReply.ShopId;
            existingReply.DeletedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<Reply>().DeleteAsync(replyId);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(string replyId, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(replyId) || !Guid.TryParse(replyId, out _))
            {
                throw new BaseException.BadRequestException("invalid_review_id_format", "Invalid reviewId format.");
            }

            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException("invalid_reply_id", "Reply ID cannot be null or empty.");
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null)
            {
                throw new BaseException.NotFoundException("reply_not_found", "Reply not found.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                             .Entities
                             .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null || shop.Id != existingReply.ShopId)
            {
                throw new BaseException.UnauthorizedException("unauthorized_delete", "User does not own the shop associated with this reply.");
            }


            existingReply.DeletedTime = DateTimeOffset.UtcNow;
            existingReply.DeletedBy = existingReply.ShopId;

            await _unitOfWork.GetRepository<Reply>().UpdateAsync(existingReply);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}