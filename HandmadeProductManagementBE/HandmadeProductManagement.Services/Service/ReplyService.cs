using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.ModelViews.ReplyModelViews;
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
            if (string.IsNullOrWhiteSpace(replyModel.Content))
                throw new ArgumentException("Invalid reply data.");

            var reply = new Reply
            {
                Content = replyModel.Content,
                ReviewId = replyModel.ReviewId,
                ShopId = replyModel.ShopId
            };

            await _unitOfWork.GetRepository<Reply>().InsertAsync(reply);
            await _unitOfWork.SaveAsync();

            replyModel.Id = reply.Id;
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

            _unitOfWork.GetRepository<Reply>().Update(existingReply);
            await _unitOfWork.SaveAsync();

            return updatedReply;
        }

        public async Task<bool> DeleteAsync(string replyId)
        {
            var existingReply = await _unitOfWork.GetRepository<Reply>().GetByIdAsync(replyId);
            if (existingReply == null) return false;

            _unitOfWork.GetRepository<Reply>().Delete(replyId);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
