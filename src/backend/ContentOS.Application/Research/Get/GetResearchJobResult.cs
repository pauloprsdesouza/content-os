using ContentOS.Domain.Research;

namespace ContentOS.Application.Research.Get;

public sealed record GetResearchJobResult
{
    private GetResearchJobResult(bool isSuccess, ResearchJob? job, string? errorCode)
    {
        IsSuccess = isSuccess;
        Job = job;
        ErrorCode = errorCode;
    }

    public bool IsSuccess { get; }

    public ResearchJob? Job { get; }

    public string? ErrorCode { get; }

    public static GetResearchJobResult Found(ResearchJob job) => new(true, job, null);

    public static GetResearchJobResult NotFound() => new(false, null, "RESEARCH_JOB_NOT_FOUND");
}
