﻿using HandmadeProductManagement.ModelViews.ReplyModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Contract.Services.Interface
{
    public interface IReplyService
    {
        Task<IList<ReplyModel>> GetAllAsync(int pageNumber, int pageSize);
        Task<ReplyModel?> GetByIdAsync(string replyId);
        Task<ReplyModel> CreateAsync(ReplyModel reply);
        Task<ReplyModel> UpdateAsync(string replyId, ReplyModel updatedReply);
        Task<bool> DeleteAsync(string replyId);
        Task<bool> SoftDeleteAsync(string reviewId);
    }
}
