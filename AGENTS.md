# Agent Notes

## Project Overview

This is a **Phase 1 + Phase 2 AI-Powered E-Commerce Platform** combining traditional product catalog functionality with conversational AI shopping capabilities. The application enables users to search for products using keyword search, semantic search, hybrid search, and a conversational chat interface with voice input/output.

The system transforms user queries into semantic vectors, searches a vector database for similar products, and generates conversational AI responses using an LLM. It's designed like modern AI shopping assistants.

## Tech Stack

### Backend (.NET)
- **Runtime**: .NET 10 Web API
- **Architecture**: Minimal APIs with CQRS (MediatR)
- **Database**: PostgreSQL with pgvector extension
- **ORM**: Entity Framework Core (Npgsql) + Dapper
- **AI Providers**: Azure OpenAI, OpenAI, Ollama (switchable via config)
- **Speech**: Azure Cognitive Services Speech (STT & TTS)
- **Gateway**: Ocelot Gateway
- **Container**: Docker & Docker Compose

### Frontend (Angular)
- **Framework**: Angular latest (standalone components, zoneless)
- **Styling**: SCSS + Angular Material
- **State**: Angular Signals (primary), RxJS (HTTP only)
- **Auth**: JWT with jwt-decode
- **Testing**: Karma, Jasmine

### Infrastructure
- **Containerization**: Docker + Docker Compose
- **Services**: aiservice (8090), pgvector (5433), ollama (11434)

## Project Structure

```
/Users/rcrock1978/Documents/PROJECTS/005/
├── AGENTS.md                    # This file
├── Design.md                    # Comprehensive architecture design
├── opencode.json               # opencode configuration
├── .opencode/
│   ├── agents/                 # Specialized subagents
│   │   ├── backend-dev.md      # .NET backend developer
│   │   ├── frontend-dev.md     # Angular frontend developer
│   │   ├── ai-ml-dev.md        # AI/ML integration specialist
│   │   └── infra-dev.md        # Infrastructure/Docker specialist
│   └── skills/                 # Project-specific skills
│       ├── dotnet-backend/     # Backend development patterns
│       ├── angular-frontend/   # Frontend development patterns
│       ├── ai-ml-integration/  # AI/ML integration patterns
│       └── docker-infrastructure/ # Docker/infrastructure patterns
├── AiService/                  # .NET 10 Backend API (to be created)
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Features/               # CQRS handlers
│   ├── Services/               # Business logic
│   ├── Repositories/           # Data access
│   ├── Providers/              # AI provider implementations
│   ├── Endpoints/              # Minimal API endpoint mappings
│   ├── Models/                 # Domain models/DTOs
│   └── Dockerfile
└── Client/                     # Angular Frontend (to be created)
    ├── src/app/
    │   ├── core/               # Chatbot, interceptors, guards
    │   ├── features/           # Auth, store modules
    │   ├── services/           # Chat, product, auth services
    │   └── shared/             # Components, pipes, directives
    ├── package.json
    └── Dockerfile
```

## MCP Servers Available

The following MCP servers are configured in `opencode.json`:

| MCP Server | Purpose | Status |
|------------|---------|--------|
| **docker** | Container operations, image builds, compose management | Enabled |
| **postgres** | Database operations, SQL queries, schema management | Enabled |
| **playwright** | Frontend testing, browser automation | Enabled |
| **filesystem** | File operations within project directory | Enabled |

## Specialized Agents

### backend-dev
Use for: .NET 10 API development, CQRS/MediatR handlers, repository patterns, minimal API endpoints, vector search logic, Entity Framework migrations.

### frontend-dev
Use for: Angular component development, signal-based state management, chatbot widget, HTTP services, Angular Material UI, JWT authentication.

### ai-ml-dev
Use for: Embedding generation, vector similarity search, LLM chat completions, RAG pipeline implementation, speech services integration, prompt engineering, conversation memory.

### infra-dev
Use for: Docker Compose configuration, PostgreSQL pgvector setup, Ollama deployment, health checks, environment configuration, CI/CD pipeline setup.

## Communication Flow

### Frontend-Backend Connection
- **Protocol**: HTTP REST with JSON payloads
- **Base URLs**:
  - AI Service: `https://localhost:7138` (local) / `http://localhost:8090` (Docker)
  - Catalog Gateway: `http://localhost:8010/Catalog`

### Key API Endpoints
| Endpoint | Method | Purpose | Frontend Service |
|----------|--------|---------|------------------|
| `/chat/ask` | POST | Simple chat | `ChatService.ask()` |
| `/chat/ask/context` | POST | Chat with history | `ChatService.ask()` |
| `/search/vector` | POST | Semantic search | `ProductService.searchSemantic()` |
| `/search/hybrid` | POST | Hybrid search | `ProductService.searchHybrid()` |
| `/search/keyword` | POST | Keyword search | Backend only |
| `/voice/stt` | POST | Speech-to-Text | `ChatService.transcribe()` |
| `/voice/tts` | POST | Text-to-Speech | `ChatService.synthesize()` |

## Development Guidelines

### When Working on Backend
1. Reference the `dotnet-backend` skill: `.opencode/skills/dotnet-backend/SKILL.md`
2. Follow CQRS pattern - commands/queries in `Features/` folders
3. Use repository interfaces for all data access
4. Implement provider interfaces for AI abstractions
5. Add health checks for new endpoints
6. Use `IResult` and `ProblemDetails` for API responses

### When Working on Frontend
1. Reference the `angular-frontend` skill: `.opencode/skills/angular-frontend/SKILL.md`
2. Use standalone components only - no NgModules
3. Use Angular Signals for state management
4. Use RxJS only for HTTP calls
5. Implement proper loading and error states
6. Follow Angular Material design patterns

### When Working on AI/ML Features
1. Reference the `ai-ml-integration` skill: `.opencode/skills/ai-ml-integration/SKILL.md`
2. Implement the RAG pipeline: embed → search → fallback → generate
3. Support all three providers (Azure OpenAI, OpenAI, Ollama)
4. Handle conversation memory and follow-up detection
5. Implement proper fallback strategies

### When Working on Infrastructure
1. Reference the `docker-infrastructure` skill: `.opencode/skills/docker-infrastructure/SKILL.md`
2. Use Docker Compose for local development
3. Configure health checks on all services
4. Manage secrets via environment variables (never commit keys)
5. Use multi-stage Docker builds for optimization

## Configuration

### Backend (`AiService/appsettings.json`)
```json
{
  "EmbeddingProvider": "AzureOpenAI",
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "EmbeddingModel": "nomic-embed-text",
    "ChatModel": "tinyllama",
    "Dimensions": 768,
    "MaxTokens": 150,
    "Temperature": 0.7
  },
  "OpenAI": {
    "BaseUrl": "https://api.openai.com/v1",
    "ApiKey": "...",
    "EmbeddingModel": "text-embedding-3-small",
    "ChatModel": "gpt-4o-mini",
    "Dimensions": 1536
  },
  "AzureOpenAI": {
    "Endpoint": "https://...openai.azure.com/",
    "ApiKey": "...",
    "EmbeddingDeployment": "text-embedding-3-small",
    "ChatDeployment": "gpt-4o-mini",
    "ApiVersion": "2024-02-01",
    "Dimensions": 1536
  },
  "AzureSpeech": {
    "ApiKey": "...",
    "Region": "westus2"
  },
  "OcelotGateway": {
    "BaseUrl": "http://localhost:8010/"
  },
  "ConnectionStrings": {
    "PgVector": "Host=localhost;Port=5433;Database=aivector;Username=aiuser;Password=aiPass123"
  }
}
```

### Frontend (`Client/package.json` key dependencies)
```json
{
  "@angular/core": "latest",
  "@angular/material": "latest",
  "rxjs": "latest",
  "jwt-decode": "latest"
}
```

## Important Notes

- This repository is currently empty. All code needs to be generated.
- The `opencode.json` config must be reloaded after changes (quit and restart opencode).
- Never commit API keys or secrets to version control.
- Use the MCP servers for container and database operations when possible.
- Follow the Design.md for detailed architecture decisions.

## Restart Required

After any changes to `.opencode/`, `opencode.json`, or agent/skill files, **quit and restart opencode** for changes to take effect.
