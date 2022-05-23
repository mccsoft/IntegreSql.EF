using System;

namespace MccSoft.IntegreSql.EF.Exceptions;

public class IntegreSqlTemplateNotFoundException : Exception
{
    public IntegreSqlTemplateNotFoundException(string templateHash)
        : base(
            $"Template with hash '{templateHash}' wasn't found in IntegreSQL instance. Make sure you have initialized the template (by calling InitializeTemplate)."
        ) { }
}
