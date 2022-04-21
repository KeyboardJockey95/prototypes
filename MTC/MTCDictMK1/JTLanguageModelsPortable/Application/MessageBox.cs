using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTLanguageModelsPortable.Application
{
    public class MessageBox
    {
        public enum MessageResult
        {
            NONE = 0,
            OK = 1,
            CANCEL = 2,
            ABORT = 3,
            RETRY = 4,
            IGNORE = 5,
            YES = 6,
            NO = 7
        }

        public MessageBox()
        {
        }

        public virtual Task<MessageResult> Show(
            string Title,
            string Message,
            bool SetCancelable = false,
            bool SetInverseBackgroundForced = false,
            MessageResult PositiveButton = MessageResult.OK,
            MessageResult NegativeButton = MessageResult.NONE,
            MessageResult NeutralButton = MessageResult.NONE,
            int IconAttribute = 0)
        {
            var tcs = new TaskCompletionSource<MessageResult>();
            return tcs.Task;
        }
    }
}
