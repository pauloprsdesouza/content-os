namespace ContentOS.Contracts.Learning;

public sealed record OutcomesSummaryResponse(
    long ConfirmedPurchases,
    long Learners,
    long Enrollments,
    long CapstonesSubmitted,
    long EvaluationsPassed,
    long OutcomesRecorded);
