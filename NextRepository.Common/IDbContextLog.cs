using System;

namespace NextRepository.Common
{
    public interface IDbContextLog
    {
        void Error(Exception exception);
        void Info(string format);
    }
}