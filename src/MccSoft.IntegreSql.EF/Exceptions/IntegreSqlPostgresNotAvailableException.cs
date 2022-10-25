using System;

namespace MccSoft.IntegreSql.EF.Exceptions;

public class IntegreSqlPostgresNotAvailableException : IntegreSqlException
{
    public IntegreSqlPostgresNotAvailableException(string uri)
        : base(
            $"IntegreSQL at '{uri}' can't connect to PostgreSQL. Examine IntegreSQL docker logs and make sure you set up IntegreSQL and PostgreSQL corectly."
                + $"Maybe running docker-compose from https://github.com/mcctomsk/IntegreSql.EF/tree/main/scripts could help."
        ) { }
}
