using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
using HandmadeProductManagement.ModelViews.ReviewModelViews;
using HandmadeProductManagement.ModelViews.ShopModelViews;
using HandmadeProductManagement.Repositories.Entity;
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
                throw new ArgumentException("Page Number must be greater than zero.");
            }
            if (pageSize <= 0)
            {
                throw new ArgumentException("Page Size must be greater than zero.");
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

        public async Task<ReplyModel?> GetByIdAsync(string replyId)
        {
            var reply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (reply == null) return null;

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
            // Check if reviewId and shopId exist
            var review = await _unitOfWork.GetRepository<Review>().GetByIdAsync(replyModel.ReviewId);
            if (review == null)
            {
                throw new ArgumentException("Review not found.");
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                                 .Entities
                                 .FirstOrDefaultAsync(s => s.Id == replyModel.ShopId);

            if (shop == null)
            {
                throw new ArgumentException("Shop not found.");
            }

            // Ensure that each review has only one reply
            var existingReply = await _unitOfWork.GetRepository<Reply>()
                                   .Entities
                                   .FirstOrDefaultAsync(r => r.ReviewId == replyModel.ReviewId);
            if (existingReply != null)
            {
                throw new ArgumentException("This review already has a reply.");
            }

            var reply = new Reply
            {
                Content = replyModel.Content,
                ReviewId = replyModel.ReviewId,
                ShopId = replyModel.ShopId,
                Date = DateTime.UtcNow
            };

            reply.CreatedBy = shop.Name;
            reply.LastUpdatedBy = shop.Name;

            //reply.CreatedTime = DateTimeOffset.UtcNow;
            //reply.LastUpdatedTime = DateTimeOffset.UtcNow;

            await _unitOfWork.GetRepository<Reply>().InsertAsync(reply);
            await _unitOfWork.SaveAsync();

            replyModel.Id = reply.Id;
            replyModel.Date = reply.Date;

            return replyModel;
        }

        public async Task<ReplyModel> UpdateAsync(string replyId, ReplyModel updatedReply)
        {
            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null) throw new ArgumentException("Reply not found.");

            if (!string.IsNullOrWhiteSpace(updatedReply.Content))
            {
                existingReply.Content = updatedReply.Content;
            }

            var shop = await _unitOfWork.GetRepository<Shop>()
                     .Entities
                     .FirstOrDefaultAsync(s => s.Id == updatedReply.ShopId);

            if (shop == null)
            {
                throw new ArgumentException("Shop not found.");
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
            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null) return false;

            existingReply.DeletedTime = DateTimeOffset.UtcNow;

            _unitOfWork.GetRepository<Reply>().Update(existingReply);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
