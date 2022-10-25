using System;

namespace MccSoft.IntegreSql.EF.Exceptions;

public class IntegreSqlInternalServerErrorException : IntegreSqlException
{
    public IntegreSqlInternalServerErrorException(string content, Exception innerException = null)
        : base(
            $"IntegreSQL responded with Internal Server Error. Response: '{content}'.",
            innerException
        ) { }
}
