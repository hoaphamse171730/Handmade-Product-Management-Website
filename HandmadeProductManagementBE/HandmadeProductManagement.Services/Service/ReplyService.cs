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

        public async Task<IList<ReplyModel>> GetAllAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                throw new BaseException.BadRequestException("invalid_page_number", "Page Number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new BaseException.BadRequestException("invalid_page_size", "Page Size must be greater than zero.");
            }

            var replies = await _unitOfWork.GetRepository<Reply>().GetAllAsync();
            return replies.Skip((pageNumber - 1) * pageSize)
                          .Take(pageSize)
                          .Select(r => new ReplyModel
                          {
                              Id = r.Id,
                              Content = r.Content,
                              Date = r.Date,
                              ReviewId = r.ReviewId,
                              ShopId = r.ShopId
                          }).ToList();
        }

        public async Task<ReplyModel> GetByIdAsync(string replyId)
        {
            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException("invalid_reply_id", "Reply ID cannot be null or empty.");
            }

            var reply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
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

        public async Task<ReplyModel> CreateAsync(ReplyModel replyModel)
        {
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

            if (string.IsNullOrWhiteSpace(replyModel.ShopId) || !Guid.TryParse(replyModel.ShopId, out _))
            {
                throw new BaseException.BadRequestException("invalid_shop_id_format", "Invalid shopId format.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                                 .Entities
                                 .FirstOrDefaultAsync(s => s.Id == replyModel.ShopId);
            if (shop == null)
            {
                throw new BaseException.NotFoundException("shop_not_found", "Shop not found.");
            }

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
                CreatedBy = shop.Name,
                LastUpdatedBy = shop.Name
            };

            await _unitOfWork.GetRepository<Reply>().InsertAsync(reply);
            await _unitOfWork.SaveAsync();

            replyModel.Id = reply.Id;
            replyModel.Date = reply.Date;

            return replyModel;
        }

        public async Task<ReplyModel> UpdateAsync(string replyId, ReplyModel updatedReply)
        {
            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException("invalid_reply_id", "Reply ID cannot be null or empty.");
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null)
            {
                throw new BaseException.NotFoundException("reply_not_found", "Reply not found.");
            }

            if (string.IsNullOrWhiteSpace(updatedReply.ShopId) || !Guid.TryParse(updatedReply.ShopId, out _))
            {
                throw new BaseException.BadRequestException("invalid_shop_id_format", "Invalid shopId format.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                         .Entities
                         .FirstOrDefaultAsync(s => s.Id == updatedReply.ShopId);
            if (shop == null)
            {
                throw new BaseException.NotFoundException("shop_not_found", "Shop not found.");
            }

            if (!string.IsNullOrWhiteSpace(updatedReply.Content))
            {
                existingReply.Content = updatedReply.Content;
            }

            existingReply.LastUpdatedBy = shop.Name;
            existingReply.LastUpdatedTime = DateTimeOffset.UtcNow;

            _unitOfWork.GetRepository<Reply>().Update(existingReply);
            await _unitOfWork.SaveAsync();

            return updatedReply;
        }

        public async Task<bool> DeleteAsync(string replyId)
        {
            if (string.IsNullOrWhiteSpace(replyId))
            {
                throw new BaseException.BadRequestException("invalid_reply_id", "Reply ID cannot be null or empty.");
            }

            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null)
            {
                throw new BaseException.NotFoundException("reply_not_found", "Reply not found.");
            }

            existingReply.DeletedTime = DateTimeOffset.UtcNow;

            _unitOfWork.GetRepository<Reply>().Delete(replyId);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(string replyId)
        {
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
                                 .FirstOrDefaultAsync(s => s.Id == existingReply.ShopId);
            if (shop == null)
            {
                throw new BaseException.NotFoundException("shop_not_found", "Shop not found.");
            }

            existingReply.DeletedTime = DateTimeOffset.UtcNow;
            existingReply.DeletedBy = shop.Name;

            _unitOfWork.GetRepository<Reply>().Update(existingReply);
            await _unitOfWork.SaveAsync();

            return true;
        }
    }
}