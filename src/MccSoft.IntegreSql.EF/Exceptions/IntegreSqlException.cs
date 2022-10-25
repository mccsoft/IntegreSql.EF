using System;
using System.Runtime.Serialization;

namespace MccSoft.IntegreSql.EF.Exceptions;

public class IntegreSqlException : Exception
{
    public IntegreSqlException() { }

    protected IntegreSqlException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    public IntegreSqlException(string message) : base(message) { }

    public IntegreSqlException(string message, Exception innerException)
        : base(message, innerException) { }
}
