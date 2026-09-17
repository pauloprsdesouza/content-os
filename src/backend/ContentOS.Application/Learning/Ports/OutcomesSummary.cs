namespace ContentOS.Application.Learning.Ports;

public sealed record OutcomesSummary(
    long ConfirmedPurchases,
    long Learners,
    long Enrollments,
    long CapstonesSubmitted,
    long EvaluationsPassed,
    long OutcomesRecorded);
