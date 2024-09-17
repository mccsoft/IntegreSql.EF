using System;

namespace MccSoft.IntegreSql.EF.Exceptions;

public class IntegreSqlNotRunningException : IntegreSqlException
{
    public IntegreSqlNotRunningException(string uri, Exception innerException = null)
        : base(
            $"IntegreSQL not available. Make sure IntegreSQL is running at '{uri}'."
                + $"Maybe running docker compose from https://github.com/mcctomsk/IntegreSql.EF/tree/main/scripts could help",
            innerException
        ) { }
}
