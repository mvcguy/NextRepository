using System;

namespace NextDataLayer
{
    public interface IDbContextLog
    {
        void Error(Exception exception);
        void Info(string format);
    }
}