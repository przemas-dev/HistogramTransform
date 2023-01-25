using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace HistogramTransform;

public static class ListExtension
{
    public static void RefreshOperations(this List<OperationStep> operationSteps, int index)
    {
        for (var i = index; i < operationSteps.Count; i++)
        {
            operationSteps[i].Calculate();
        }
    }
}