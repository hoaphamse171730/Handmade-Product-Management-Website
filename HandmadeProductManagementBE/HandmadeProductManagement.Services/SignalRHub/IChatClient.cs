using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandmadeProductManagement.Services.SignalRHub
{
    public interface IChatClient
    {
        Task ReceiveMessage(string message);
    }
}
