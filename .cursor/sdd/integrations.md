# Integrations

| Integration | Direction | Notes |
|---|---|---|
| Kiwify | Out (export/confirm) + In (webhook) | Webhook = signal; API reconciliation = authoritative |
| Blob filesystem | Internal | `IBlobStore`; content-addressed SHA-256 paths |
| LLM providers | Via LiteLLM SDK in worker | Alias config (`research.standard`, `review.strict`); no proxy in MVP |
| RabbitMQ | Cross-runtime | Exchange `contentos.ai`; CloudEvents JSON |
| Internal Knowledge Tool API | Worker → API | Workload identity; Facade port; no DB from Python |
| OTLP | Out | Collector/exporters; backend-agnostic |

Secrets encrypted at rest; never in messages, prompts, traces, or logs.
