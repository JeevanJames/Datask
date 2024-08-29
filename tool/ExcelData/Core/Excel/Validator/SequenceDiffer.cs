namespace Datask.Tool.ExcelData.Core.Excel.Validator;

internal static class SequenceDiffer
{

    internal static IEnumerable<SequenceDiff> GetSequenceDiffs<TPrimary, TComparison>(
        this IList<TPrimary> primarySequence,
        IList<TComparison> comparisonSequence,
        Func<TPrimary, TComparison, bool> equalityComparer,
        bool checkOrder = false)
        where TPrimary : class
        where TComparison : class
    {
        for (int primaryIndex = 0; primaryIndex < primarySequence.Count; primaryIndex++)
        {
            TPrimary primaryElement = primarySequence[primaryIndex];

            // If the primary element is not found in the comparison sequence, then it is a new element.
            int comparisonIndex = comparisonSequence.IndexOf(ce => equalityComparer(primaryElement, ce));
            if (comparisonIndex < 0)
            {
                yield return new NewElementDiff<TPrimary>(primaryElement);
                continue;
            }

            TComparison comparisonElement = comparisonSequence[comparisonIndex];

            // Check if the comparison element's index is different from the primary element's.
            if (checkOrder && primaryIndex != comparisonIndex)
            {
                yield return new IndexChangedElementDiff<TPrimary, TComparison>(primaryElement, comparisonElement,
                    comparisonIndex, primaryIndex);
                continue;
            }

            // No change in the primary element and comparison element
            yield return new UnchangedElementDiff<TPrimary, TComparison>(primaryElement, comparisonElement);
        }

        // If a comparison element does not exist in the primary sequence, that means the primary element
        // has been deleted.
        foreach (TComparison comparisonElement in comparisonSequence)
        {
            TPrimary? primaryElement = primarySequence.FirstOrDefault(pe => equalityComparer(pe, comparisonElement));
            if (primaryElement is null)
                yield return new RemovedElementDiff<TComparison>(comparisonElement);
        }
    }
}

#pragma warning disable S2094 // Classes should not be empty
internal abstract record SequenceDiff;
#pragma warning restore S2094 // Classes should not be empty

internal sealed record NewElementDiff<TPrimary>(TPrimary Element) : SequenceDiff
    where TPrimary : class;

internal sealed record RemovedElementDiff<TComparison>(TComparison Element) : SequenceDiff
    where TComparison : class;

internal sealed record UnchangedElementDiff<TPrimary, TComparison>(TPrimary PrimaryElement,
    TComparison ComparisonElement) : SequenceDiff
    where TPrimary : class
    where TComparison : class;

internal sealed record IndexChangedElementDiff<TPrimary, TComparison>(TPrimary PrimaryElement,
    TComparison ComparisonElement, int OldIndex, int NewIndex) : SequenceDiff;
