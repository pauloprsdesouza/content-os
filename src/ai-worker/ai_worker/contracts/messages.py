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

    model_config = {"populate_by_name": True}


class ContentGenerateCommandData(BaseModel):
    content_unit_id: UUID | None = Field(default=None, alias="contentUnitId")
    content_version_id: UUID = Field(alias="contentVersionId")
    operation_id: UUID = Field(alias="operationId")
    title: str | None = None
    brief: str | None = None
    request_review_after_generate: bool = Field(
        default=True,
        alias="requestReviewAfterGenerate",
    )

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

    model_config = {"populate_by_name": True}
