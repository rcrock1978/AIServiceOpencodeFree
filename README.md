# ShopAI — AI-Powered E-Commerce Platform

A full-stack conversational AI shopping platform with keyword/semantic/hybrid search, RAG-powered chat, voice input/output, and a complete e-commerce backend.

## Architecture

```
┌──────────────┐       ┌──────────────┐       ┌──────────────┐
│  Angular 21   │       │ CatalogService│      │   AiService   │
│  (localhost:  │ ───→  │  .NET 10      │      │  .NET 10      │
│   4200 / 80)  │       │  :8010        │      │  :8090        │
└──────────────┘       └──────┬───────┘       └──────┬───────┘
                              │                      │
                              └──────────┬───────────┘
                                         │
                              ┌──────────▼───────────┐
                              │  PostgreSQL + pgvector│
                              │  :5433                │
                              └───────────────────────┘
                                         │
                              ┌──────────▼───────────┐
                              │  Ollama               │
                              │  (nomic-embed-text,   │
                              │   tinyllama) :11434   │
                              └───────────────────────┘
```

| Component | Stack | Port |
|-----------|-------|------|
| **Frontend** | Angular 21 (standalone, zoneless, Signals, Material UI) | 4200 (dev) / 80 (prod) |
| **AiService** | .NET 10 Minimal API + MediatR CQRS | 8090 |
| **CatalogService** | .NET 10 Minimal API + Dapper | 8010 |
| **PostgreSQL** | pgvector extension for vector search | 5433 |
| **Ollama** | Local LLM + embeddings | 11434 |
| **nginx** | Reverse proxy + static file serving | 80 |

## AI Features

- **Semantic Search** — Query → embedding → cosine similarity in pgvector
- **Hybrid Search** — Merged vector + keyword results
- **RAG Chat** — Embed → hybrid search → build context → LLM response
- **Web Fallback** — When no products match, falls back to web search
- **Contextual Memory** — Follow-up detection, conversation history
- **Voice I/O** — Azure Speech STT/TTS with MediaRecorder capture
- **Provider Abstraction** — Swap between Ollama, OpenAI, Azure OpenAI via config

## Quick Start

### Local Development (no Docker)

```bash
# 1. Start database + Ollama
docker compose up -d pgvector ollama

# 2. Pull Ollama models (first time)
docker exec aishop-ollama ollama pull nomic-embed-text
docker exec aishop-ollama ollama pull tinyllama

# 3. Start backend services
cd CatalogService && dotnet run
cd AiService && dotnet run

# 4. Seed catalog
curl -X POST http://localhost:8010/seed

# 5. Generate embeddings
curl -X POST http://localhost:8090/embeddings/seed

# 6. Start frontend
cd Client && npm install && ng serve

# 7. Open http://localhost:4200
```

### Full Docker Deployment

```bash
# Start everything
docker compose up -d --build

# Seed data
curl -X POST http://localhost/seed
curl -X POST http://localhost/embeddings/seed

# Open http://localhost
```

## Project Structure

```
├── AiService/                  # .NET 10 — AI backend
│   ├── Configuration/          # 7 options classes
│   ├── Contracts/              # 7 DTO record files
│   ├── Endpoints/              # 7 Minimal API mappers
│   ├── Features/               # MediatR CQRS handlers
│   ├── Models/                 # 8 domain models
│   ├── Providers/              # 6 AI providers (3 embed + 3 chat)
│   ├── Repositories/           # Dapper data access
│   ├── Services/               # Search, Chat, Auth, Voice, WebSearch
│   ├── Program.cs              # DI, JWT, CORS, Swagger, health checks
│   └── Dockerfile
├── AiService.Tests/            # xUnit + Moq (20 tests)
├── CatalogService/             # .NET 10 — Product catalog
│   ├── Endpoints/              # 5 endpoints
│   ├── Models/                 # Product model
│   ├── Repositories/           # Dapper CRUD
│   ├── Services/               # SeedService (55 products)
│   └── Dockerfile
├── CatalogService.Tests/       # xUnit + Moq (2 tests)
├── Client/                     # Angular 21 frontend
│   └── src/app/
│       ├── core/               # Chatbot, interceptors, guards, models
│       ├── features/           # Auth, Store (browse, cart, checkout, orders)
│       ├── services/           # Auth, Product, Chat, Cart, Voice, Loading
│       └── shared/             # Navbar, pipes, directives
├── init-scripts/01-init.sql    # Schema (8 tables + vector columns)
├── docker-compose.yml          # 5 services
├── nginx.conf                  # Reverse proxy config
└── IMPLEMENTATION_PLAN.md      # Development roadmap
```

## API Reference

### CatalogService (`:8010` via nginx `/catalog/`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/catalog/products` | List products (filter: brand, type, search, sortBy; paginate: page, pageSize) |
| GET | `/catalog/products/{id}` | Get product by ID |
| GET | `/catalog/brands` | List distinct brands |
| GET | `/catalog/types` | List distinct types |
| POST | `/seed` | Seed 55 sample products (convenience route) |

### AiService (`:8090` via nginx `/api/`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/search/vector` | Semantic vector search |
| POST | `/api/search/hybrid` | Merged vector + keyword |
| POST | `/api/chat/ask` | Single-turn chat |
| POST | `/api/chat/ask/context` | Multi-turn with history |
| POST | `/api/embeddings/generate` | Generate embedding for text |
| POST | `/api/embeddings/upsert` | Store embedding for product |
| POST | `/embeddings/seed` | Generate embeddings for all products (convenience route) |
| GET | `/api/cart` | Get current cart |
| POST | `/api/cart/add` | Add item to cart |
| DELETE | `/api/cart/remove` | Remove item from cart |
| POST | `/api/cart/checkout` | Convert cart to order |
| GET | `/api/orders` | Get order history |
| POST | `/api/auth/login` | JWT login |
| POST | `/api/auth/register` | JWT registration |
| POST | `/api/voice/stt` | Speech-to-text |
| POST | `/api/voice/tts` | Text-to-speech |
| GET | `/health` | Health check |

## Configuration

### AI Providers

Set `AI__Provider` in `AiService/appsettings.json` or env vars:

```json
"AI": { "Provider": "Ollama" }       // Local default — no API keys
"AI": { "Provider": "OpenAI" }        // Set OpenAI:ApiKey
"AI": { "Provider": "AzureOpenAI" }   // Set AzureOpenAI:Endpoint + ApiKey
```

### JWT Authentication

```json
"Jwt": {
  "Key": "your-256-bit-secret-key",
  "Issuer": "AiService",
  "Audience": "AiServiceClient",
  "ExpireMinutes": 60
}
```

### Database

Default connection: `Host=localhost;Port=5433;Database=aivector;Username=aiuser;Password=aiPass123`

Override via `ConnectionStrings__PgVector` env var in Docker.

## Testing

```bash
# Backend (22 tests)
dotnet test AiService.Tests
dotnet test CatalogService.Tests

# Frontend (30 tests)
cd Client && ng test --watch=false --browsers=ChromeHeadless
```

## Project Status

All 16 waves from the implementation plan are complete:

| Phase | Area | Status |
|-------|------|--------|
| 1 | Infrastructure (Docker, SQL, nginx) | ✅ |
| 2-8 | Backend services (.NET 10 APIs) | ✅ |
| 9-12 | Frontend (Angular, Auth, Store, Cart) | ✅ |
| 13-14 | Voice + Chatbot widget | ✅ |
| 15-16 | Tests (backend + frontend) | ✅ |
