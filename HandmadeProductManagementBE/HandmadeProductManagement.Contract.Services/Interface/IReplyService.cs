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
        Task<IList<ReplyModel>> GetByPageAsync(int pageNumber, int pageSize);
        Task<ReplyModel?> GetByIdAsync(string replyId);
        Task<bool> CreateAsync(ReplyModel reply);
        Task<bool> UpdateAsync(string replyId, string shopId, ReplyModel updatedReply);
        Task<bool> DeleteAsync(string replyId, string shopId);
        Task<bool> SoftDeleteAsync(string reviewId, string shopId);
    }
}
