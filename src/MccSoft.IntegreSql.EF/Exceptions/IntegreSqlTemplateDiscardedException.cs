using System;

namespace MccSoft.IntegreSql.EF.Exceptions;

public class IntegreSqlTemplateDiscardedException : IntegreSqlException
{
    public IntegreSqlTemplateDiscardedException(string templateHash)
        : base($"Template with hash '{templateHash}' was discarded.") { }
}
