﻿using System;
using System.Diagnostics;

namespace NextDataLayer
{
    public class TraceLog : IDbContextLog
    {
        public void Error(Exception exception)
        {
            Debug.WriteLine(exception.Message);
        }

        public void Info(string format)
        {
            Debug.WriteLine(format);
        }
    }
}