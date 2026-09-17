namespace ContentOS.Application.Learning.EvaluateCapstone;

public sealed record EvaluateCapstoneCommand(Guid CapstoneId, bool Passed, int? Score);
