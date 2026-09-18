from __future__ import annotations

from datetime import datetime, timezone
from typing import Any, Literal
from uuid import UUID, uuid4

from pydantic import BaseModel, Field


class CloudEvent(BaseModel):
    specversion: Literal["1.0"] = "1.0"
    id: str = Field(default_factory=lambda: uuid4().hex)
    source: str = "contentos.ai-worker"
    type: str
    time: datetime = Field(default_factory=lambda: datetime.now(timezone.utc))
    subject: str
    correlationid: str | None = None
    idempotencykey: str
    schemaversion: str = "1"
    data: dict[str, Any]


class ResearchCommandData(BaseModel):
    research_job_id: UUID = Field(alias="researchJobId")
    operation_id: UUID = Field(alias="operationId")
    topic: str
    scope_notes: str | None = Field(default=None, alias="scopeNotes")
    source_snapshot_id: UUID = Field(alias="sourceSnapshotId")
    content_hash: str = Field(alias="contentHash")

    model_config = {"populate_by_name": True}


class ContentGenerateCommandData(BaseModel):
    content_unit_id: UUID | None = Field(default=None, alias="contentUnitId")
    content_version_id: UUID = Field(alias="contentVersionId")
    operation_id: UUID = Field(alias="operationId")
    title: str | None = None
    brief: str | None = None
    format: str | None = None
    format_mold: str | None = Field(default=None, alias="formatMold")
    citation_content_hashes: list[str] = Field(
        default_factory=list, alias="citationContentHashes"
    )
    request_review_after_generate: bool = Field(
        default=True,
        alias="requestReviewAfterGenerate",
    )

    model_config = {"populate_by_name": True}


class TopicLabelWork(BaseModel):
    work_id: str = Field(alias="workId")
    title: str
    year: int | None = None
    topic_name: str | None = Field(default=None, alias="topicName")
    abstract_text: str = Field(default="", alias="abstractText")

    model_config = {"populate_by_name": True}


class TopicLabelCommandData(BaseModel):
    discovery_id: UUID = Field(alias="discoveryId")
    operation_id: UUID = Field(alias="operationId")
    works: list[TopicLabelWork] = Field(default_factory=list)

    model_config = {"populate_by_name": True}


class TopicLabelResult(BaseModel):
    label: str
    rationale: str
    work_ids: list[str] = Field(serialization_alias="workIds")

    model_config = {"populate_by_name": True}


class TopicLabelCompletedData(BaseModel):
    discovery_id: UUID = Field(serialization_alias="discoveryId")
    operation_id: UUID = Field(serialization_alias="operationId")
    topics: list[TopicLabelResult]

    model_config = {"populate_by_name": True}


class ContentReviewCommandData(BaseModel):
    content_version_id: UUID = Field(alias="contentVersionId")
    operation_id: UUID = Field(alias="operationId")
    body_markdown: str | None = Field(default=None, alias="bodyMarkdown")

    model_config = {"populate_by_name": True}


class ResearchFindingResult(BaseModel):
    statement: str
    confidence: float
    source_snapshot_id: UUID = Field(serialization_alias="sourceSnapshotId")
    locator: str
    extraction_method: str = Field(serialization_alias="extractionMethod")
    finding_id: UUID = Field(serialization_alias="findingId")

    model_config = {"populate_by_name": True}


class ResearchCompletedData(BaseModel):
    research_job_id: UUID = Field(serialization_alias="researchJobId")
    operation_id: UUID = Field(serialization_alias="operationId")
    findings: list[ResearchFindingResult]

    model_config = {"populate_by_name": True}


class ContentGenerateCompletedData(BaseModel):
    content_version_id: UUID = Field(serialization_alias="contentVersionId")
    operation_id: UUID = Field(serialization_alias="operationId")
    body_markdown: str = Field(serialization_alias="bodyMarkdown")
    request_review_after_generate: bool = Field(
        serialization_alias="requestReviewAfterGenerate"
    )

    model_config = {"populate_by_name": True}


class ContentReviewCompletedData(BaseModel):
    content_version_id: UUID = Field(serialization_alias="contentVersionId")
    operation_id: UUID = Field(serialization_alias="operationId")
    agent_review_notes: str = Field(serialization_alias="agentReviewNotes")

    model_config = {"populate_by_name": True}


class JobFailedData(BaseModel):
    operation_id: UUID = Field(serialization_alias="operationId")
    error_message: str = Field(serialization_alias="errorMessage")
    failed_type: str = Field(serialization_alias="failedType")
    research_job_id: UUID | None = Field(default=None, serialization_alias="researchJobId")
    content_version_id: UUID | None = Field(
        default=None, serialization_alias="contentVersionId"
    )
    discovery_id: UUID | None = Field(default=None, serialization_alias="discoveryId")

    model_config = {"populate_by_name": True}
