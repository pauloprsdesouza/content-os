namespace ContentOS.Infrastructure.Messaging;

public enum AiResultDisposition
{
    Ack = 0,
    Retry = 1,
    DeadLetter = 2
}
