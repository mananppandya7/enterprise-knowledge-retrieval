# AI Architect Part 2 - Document Retrieval Prototype

A small C#/.NET 10 console application that demonstrates document-type-aware chunking,
OpenAI embeddings, in-memory vector search, and deterministic top-k retrieval across the five
documents in the AI Architect assessment.

The application answers the retrieval query required by the brief:

> AI solutions delivered for healthcare clients

It returns the three most semantically relevant chunks with their similarity scores, source
documents, document types, sections, chunk IDs, and text. It does not ask an LLM to synthesize an
answer because the assessment requires retrieval, not a complete RAG response.

## Why C# and .NET 10

C# was selected because it is part of the firm's stated technology stack and allows the solution
to remain small, explicit, and easy to defend. .NET 10 is the installed LTS baseline used for this
greenfield implementation. The repository pins SDK `10.0.301` through `global.json`, allowing the
latest patch in that feature band.

## Solution overview

The runtime pipeline is:

1. `SampleDocumentFactory` creates normalized representations of the five assessment documents.
2. `DocumentChunkerFactory` selects a strategy based on `DocumentType`.
3. Each chunker creates traceable `DocumentChunk` records with source and section metadata.
4. `OpenAIEmbeddingService` generates an embedding for every chunk.
5. `InMemoryVectorStore` stores owned copies of the vectors.
6. The application embeds the exact assessment query.
7. Every stored vector is scored using cosine similarity.
8. Results are sorted by descending score with chunk ID as a deterministic tie-breaker.
9. The top three chunks and their source information are printed.

The corpus currently produces 40 chunks:

| Document | Type | Chunks |
|---|---|---:|
| `SOW_STAR_Modernization_Assessment.pdf` | Statement of Work | 8 |
| `CaseStudy_HealthcareDistributor.pdf` | Case Study | 5 |
| `CaseStudy_Manufacturing.pdf` | Case Study | 5 |
| `Regression_Test_Plan_eCommerce.xlsx` | Test Plan | 16 |
| `AI_Transformation_Proposal.pdf` | Sales Proposal | 6 |

## Repository structure

```text
AIArchitect.Part2.slnx
global.json
README.md
src/
  AIArchitect.Retrieval/
    Program.cs
    Chunking/
      IDocumentChunker.cs
      DocumentChunkerFactory.cs
      SowChunker.cs
      CaseStudyChunker.cs
      TestPlanChunker.cs
      ProposalChunker.cs
    Documents/
      SampleDocumentFactory.cs
    Embeddings/
      IEmbeddingService.cs
      OpenAIEmbeddingService.cs
    Models/
      SourceDocument.cs
      DocumentChunk.cs
      EmbeddedChunk.cs
      SearchResult.cs
    Retrieval/
      CosineSimilarity.cs
      IVectorStore.cs
      InMemoryVectorStore.cs
    Output/
      ConsoleResultWriter.cs
tests/
  AIArchitect.Retrieval.Tests/
```

## Prerequisites

- .NET 10 SDK in the `10.0.3xx` feature band. The repository was built with `10.0.301`.
- An OpenAI API key with access to the configured embedding model.
- Network access to the OpenAI API when running live retrieval.

Check the installed SDK:

```powershell
dotnet --version
```

## Configuration

The application reads configuration only from environment variables:

| Variable | Required | Default | Purpose |
|---|---|---|---|
| `OPENAI_API_KEY` | Yes | None | Authenticates embedding requests. |
| `OPENAI_EMBEDDING_MODEL` | No | `text-embedding-3-small` | Overrides the embedding model. |

PowerShell for the current terminal session:

```powershell
$env:OPENAI_API_KEY = "your-api-key"
$env:OPENAI_EMBEDDING_MODEL = "text-embedding-3-small" # Optional
```

Bash:

```bash
export OPENAI_API_KEY="your-api-key"
export OPENAI_EMBEDDING_MODEL="text-embedding-3-small" # Optional
```

Never put an API key in source code, `appsettings.json`, commits, test fixtures, or example output.

## Build, test, and run

From the repository root:

```powershell
dotnet restore AIArchitect.Part2.slnx
dotnet build AIArchitect.Part2.slnx --configuration Release
dotnet test AIArchitect.Part2.slnx --configuration Release
dotnet run --project src/AIArchitect.Retrieval --configuration Release
```

The test suite is fully offline. It uses deterministic fake embeddings and never calls a paid API.

If the API key is missing, the application prints the ingestion summary, explains how to set
`OPENAI_API_KEY`, and exits with code `1`.

## Document and content representation

The PDFs and workbook in `Candidate/` are assessment reference artifacts. The prototype does not
parse them at runtime. Instead, `SampleDocumentFactory` creates realistic, deterministic,
normalized content matching their descriptions, as explicitly permitted by the brief.

`SourceDocument.Content` is the text or structured-record representation consumed by a chunker; it
is not intended to replace the original file. A production ingestion path would look like:

```text
Original PDF/XLSX -> parser/OCR/table extractor -> normalized SourceDocument.Content
                  -> document-specific chunks -> embeddings -> vector index
```

Keeping normalized content separate from the original binary makes chunking testable and keeps
file parsing outside this retrieval-focused prototype. A production implementation would preserve
the original file URI, page/sheet coordinates, parser version, content hash, and extraction
confidence in metadata.

## Chunking decisions

One fixed character or token window is not appropriate for every document type. The implementation
uses authored structure and structured records wherever possible.

### Statement of Work

`SowChunker` creates one chunk per numbered section. This keeps scope, assumptions, exclusions,
deliverables, and commercial clauses attached to their headings. A section is split only when it
exceeds 600 words; oversized sections use a 60-word overlap to preserve continuity.

### Case studies

`CaseStudyChunker` preserves Client Profile, Business Challenge, Solution, Technical Approach or
Integration, and Outcomes as independent semantic units. The document and section titles are
included in the embedded text, so a retrieved solution or outcome retains its healthcare or
manufacturing context.

The healthcare case study describes completed AI-enabled work and measured outcomes. The general
AI transformation document is intentionally marked as a proposal, preventing proposed work from
being represented as evidence of delivered healthcare experience.

### Regression test-plan workbook

`TestPlanChunker` creates one workbook-summary chunk and one chunk for every represented test case.
Each test-case chunk retains:

- sheet;
- test ID;
- module;
- scenario;
- expected result; and
- priority.

This avoids flattening unrelated rows into prose and allows a retrieved test case to remain
traceable to its original business and execution context.

### AI transformation proposal

`ProposalChunker` creates one chunk per operational opportunity area: Executive Summary, Sales and
Quoting, Ordering and Procurement, Production and Installation, Financial Close, and Expected
Benefits. This keeps each business problem, proposed intervention, and expected benefit together.

## Embedding model choice

The default model is `text-embedding-3-small` through the official `OpenAI` .NET package.

It is a suitable choice for this five-document demonstration because it provides a practical
quality, latency, vector-size, and cost balance. The larger embedding model would add cost and
storage without proving a materially different architectural decision at this corpus size.

The provider is behind `IEmbeddingService`, allowing tests to use deterministic vectors and making
a later provider change local to one implementation. Batch generation preserves input order so
vectors can be safely associated with their original chunks.

## Why cosine similarity

Cosine similarity compares vector direction rather than raw magnitude, making it appropriate for
semantic text embeddings. The implementation uses double-precision accumulators for the dot
product and magnitudes, rejects empty, mismatched, non-finite, and zero-magnitude vectors, and
clamps rounding artifacts to the mathematical range `[-1, 1]`.

For this small corpus, exhaustive comparison is easier to inspect and defend than an approximate
nearest-neighbor index. Its time complexity is linear in the number of stored chunks and therefore
does not represent the recommended production search architecture.

## Example output

Scores and exact ordering are determined by the configured embedding model and can vary.

```text
Ingestion summary
------------------------------------------------------------
SOW_STAR_Modernization_Assessment.pdf | StatementOfWork | 8 chunks
CaseStudy_HealthcareDistributor.pdf | CaseStudy | 5 chunks
CaseStudy_Manufacturing.pdf | CaseStudy | 5 chunks
Regression_Test_Plan_eCommerce.xlsx | TestPlan | 16 chunks
AI_Transformation_Proposal.pdf | SalesProposal | 6 chunks
------------------------------------------------------------
Total: 5 documents | 40 chunks

Generating embeddings with text-embedding-3-small...

Query: AI solutions delivered for healthcare clients

Top 3 results

Rank 1 | Score: 0.xxxx
Source: CaseStudy_HealthcareDistributor.pdf
Type: Case Study
Section: AI and Automation Solution
Chunk: healthcare-distributor-case-study-002
Text:
Healthcare Distributor - Digital Transformation Case Study
...
```

The application does not hard-code this result order. The expected behavior is that the delivered
healthcare case-study solution and outcomes rank strongly, while the AI proposal may rank below
them because it is proposed rather than delivered work.

## Error handling

The application handles and reports:

- missing API credentials;
- empty embedding inputs;
- provider, model-access, network, and request-limit failures;
- documents that produce no chunks;
- duplicate chunk IDs;
- empty, non-finite, zero-magnitude, or dimensionally inconsistent vectors;
- invalid `topK` values; and
- user cancellation through Ctrl+C.

Provider errors are wrapped with actionable context without printing API keys or full document
inputs. Unrecoverable failures set a nonzero process exit code.

## Tests

The xUnit suite covers:

- SOW heading and section preservation;
- case-study semantic boundaries;
- structured test-plan fields and IDs;
- proposal phase separation;
- unique IDs, source metadata, and contiguous sequences;
- identical, orthogonal, opposite, scaled, invalid, and zero vectors;
- vector-store dimensions, duplicates, defensive copies, ranking, top-k, and tie-breaking; and
- an end-to-end offline retrieval flow using a deterministic fake embedding service.

Default tests never require `OPENAI_API_KEY` and never make an API request.

## Assumptions and limitations

- The corpus content is synthetic and deterministic, though based on the supplied document
  descriptions and reference artifacts.
- The application does not perform runtime PDF, OCR, DOCX, or XLSX extraction.
- The vector store is process-local and is rebuilt on every run.
- Embeddings are not cached, so every live run incurs embedding requests.
- There is no keyword/BM25 retrieval, semantic reranking, or retrieval evaluation dataset.
- There is no role-based access filtering in Part 2; metadata includes enough context to explain
  where production ACL attributes would be attached.
- There is no retry policy beyond the underlying SDK behavior.
- The application retrieves evidence but does not synthesize an answer.

These constraints keep the implementation focused on the decisions the assessment evaluates:
document-aware chunking, embedding abstraction, similarity calculation, result ranking, readable
code, and explicit trade-offs.

## Production evolution

A production version would add capabilities in measured stages:

1. Parse original PDFs, Office files, OCR output, and tables while retaining page, sheet, row, and
   source-system lineage.
2. Store originals in governed object storage and index normalized chunks in Azure AI Search.
3. Combine BM25 keyword search with vector retrieval and semantic reranking.
4. Attach document- and chunk-level ACL metadata and apply authorization filters before retrieval.
5. Add content classification, owner approval, quality scoring, versioning, tombstones, and
   incremental re-indexing.
6. Cache embeddings by model plus content hash and cache retrieval only within the same security
   scope and index version.
7. Build a golden-query evaluation set measuring precision@k, citation validity, freshness, and
   unauthorized-result count.
8. Add rate-limit-aware retries, telemetry, token and cost dashboards, and failure alerts.
9. Introduce LLM synthesis only for queries that require cross-document summarization, using only
   authorized retrieved evidence and explicit abstention rules.

The in-memory interfaces are intentionally small so the local store and OpenAI implementation can
be replaced without rewriting chunking or domain models.
