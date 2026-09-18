namespace ContentOS.Application.Content.Topics;

public static class TopicLabelGate
{
    public static IReadOnlyList<AcceptedTopicLabel> Accept(
        IReadOnlySet<string> knownWorkIds,
        IReadOnlyList<ProposedTopicLabel> proposed)
    {
        var accepted = new List<AcceptedTopicLabel>();
        foreach (var item in proposed)
        {
            if (string.IsNullOrWhiteSpace(item.Label) || item.WorkIds.Count == 0)
            {
                continue;
            }

            var matched = item.WorkIds
                .Where(workId => !string.IsNullOrWhiteSpace(workId))
                .Select(workId => workId.Trim())
                .Where(knownWorkIds.Contains)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (matched.Length == 0)
            {
                continue;
            }

            accepted.Add(new AcceptedTopicLabel(
                item.Label.Trim(),
                string.IsNullOrWhiteSpace(item.Rationale) ? null : item.Rationale.Trim(),
                matched));
        }

        return accepted;
    }
}
