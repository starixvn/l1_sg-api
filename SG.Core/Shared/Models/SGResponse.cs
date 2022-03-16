using System;

namespace SG.Core.Shared.Models
{
    public class SGResponse<T> where T : SGResult
    {
        public DateTimeOffset TimeStamp { get; set; }

        public T Result { get; set; }
    }
}