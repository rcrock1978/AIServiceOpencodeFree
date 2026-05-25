# AI-Powered E-Commerce Platform — Implementation Plan

## Architecture Overview

- **CatalogService** (.NET 10 Minimal API): `:8010` — Product CRUD + seeding (`POST /seed`)
- **AiService** (.NET 10 Minimal API): `:8090` — AI search, chat, cart, orders, voice
- **PostgreSQL + pgvector**: `:5433` — Shared database for products, embeddings, conversations, cart, orders
- **Ollama**: `:11434` — Default local LLM + embeddings (nomic-embed-text, tinyllama)
- **Angular Frontend**: `localhost:4200` — Zoneless, standalone, signal-based, Material UI

---

## Progress Tracker (TODO List)

| # | Wave | Description | Status |
|---|------|-------------|--------|
| 1 | **Wave 1** | Docker Compose, SQL init, Dockerfiles, nginx.conf | ✅ Complete |
| 2 | **Wave 2** | CatalogService — `.csproj`, `Program.cs`, models, repo, seed service, endpoints | ✅ Complete |
| 3 | **Wave 3** | AiService — Configuration, Models, Contracts | ✅ Complete |
| 4 | **Wave 4** | AiService — AI Providers (6 implementations: 3 embed + 3 chat) | ✅ Complete |
| 5 | **Wave 5** | AiService — Repositories (PgVector, Conversation, Cart, Order) | ✅ Complete |
| 6 | **Wave 6** | AiService — Services (Search, Chat, WebSearch stub) | ✅ Complete |
| 7 | **Wave 7** | AiService — Features & Endpoints (Search, Chat, Embeddings, Cart, Order) | ✅ Complete |
| 8 | **Wave 8** | AiService — `Program.cs`, CORS, health checks, auth wiring | ✅ Complete |
| 9 | **Wave 9** | Client — Angular scaffolding, `app.config.ts`, interceptors, guards | ✅ Complete |
| 10 | **Wave 10** | Client — Services (Auth, Product, Chat, Cart) | ✅ Complete |
| 11 | **Wave 11** | Client — Auth pages, Store browse/filters/detail | ✅ Complete |
| 12 | **Wave 12** | Client — Cart, Checkout, Order History components | ✅ Complete |
| 13 | **Wave 13** | **Phase 2** — Voice service, Azure Speech, ChatService contextual memory | ✅ Complete |
| 14 | **Wave 14** | **Phase 2** — Floating chatbot widget, voice input, TTS playback | ✅ Complete |
| 15 | **Wave 15** | **Tests** — Backend service tests (xUnit + Moq) | ✅ Complete |
| 16 | **Wave 16** | **Tests** — Frontend service + component tests (Jasmine + Karma) | ✅ Complete |

---

## Subagent Allocation

| Subagent | Waves Handled | Focus Area |
|----------|---------------|------------|
| **infra-dev** | Wave 1 | `docker-compose.yml`, SQL init scripts, Dockerfiles, nginx.conf |
| **backend-dev** | Waves 2–8 | CatalogService + AiService .NET 10 backend |
| **frontend-dev** | Waves 9–12 | Angular scaffolding, services, auth pages, store modules |
| **ai-ml-dev** | Waves 4, 6, 13 | Embedding providers, chat providers, RAG pipeline, voice service |
| **frontend-dev + ai-ml-dev** | Wave 14 | Floating chatbot widget, voice input, TTS playback |

**Parallelization:**
- Wave 1 (infra) runs first
- Waves 2–8 (backend) and Waves 9–12 (frontend Phase 1) can run in parallel once Wave 1 is complete
- Waves 13–14 (Phase 2) run after Phase 1 is complete

---

## Phase 1 — Core AI Search, Chat, Catalog & Full E-commerce

### 1. Infrastructure & Database

**`docker-compose.yml`** — Services: `pgvector`, `ollama`, `catalogservice` (`:8010`), `aiservice` (`:8090`)

**`init-scripts/01-init.sql`** — Schema includes:
- `products` (with `embedding_768`, `embedding_1536`, `embedding_3072`)
- `ai_conversations`, `ai_conversation_messages`
- `users` (JWT auth)
- `cart`, `cart_items`, `orders`, `order_items` (full e-commerce persistence)

### 2. CatalogService (`/CatalogService/`)

| File | Purpose |
|------|---------|
| `CatalogService.csproj` | .NET 10 Minimal API project |
| `Program.cs` | DI, pipeline, CORS |
| `appsettings.json` | `ConnectionStrings.PgVector` |
| `Dockerfile` | Multi-stage build |
| `Models/Product.cs` | Domain model |
| `Repositories/IProductRepository.cs`, `ProductRepository.cs` | CRUD + brand/type lists |
| `Services/SeedService.cs` | Inserts 50+ realistic sample products |
| `Endpoints/CatalogEndpoints.cs` | `GET /products`, `GET /products/{id}`, `GET /brands`, `GET /types`, `POST /seed` (exposed via nginx as `/catalog/*` and `/seed`) |

### 3. AiService Backend (`/AiService/`)

**Configuration & Models:**
- `Configuration/` — `AzureOpenAIOptions.cs`, `OpenAIOptions.cs`, `OllamaOptions.cs`, `ConnectionStringsOptions.cs`
- `Models/` — `Product.cs`, `Message.cs`, `Conversation.cs`, `Cart.cs`, `CartItem.cs`, `Order.cs`, `OrderItem.cs`

**AI Providers (Strategy Pattern):**

| Interface | Implementations |
|-----------|---------------|
| `IEmbeddingProvider` | `AzureOpenAIEmbeddingProvider`, `OpenAIEmbeddingProvider`, `OllamaEmbeddingProvider` |
| `IChatProvider` | `AzureOpenAIChatProvider`, `OpenAIChatProvider`, `OllamaChatProvider` |

**Repositories:**
- `IPgVectorRepository` / `PgVectorRepository` — Vector cosine search, keyword ILIKE, hybrid search, upsert embeddings
- `IConversationRepository` / `ConversationRepository` — Create/load conversations, append messages, retrieve last N
- `ICartRepository` / `CartRepository` — Get cart, add/remove items, clear cart
- `IOrderRepository` / `OrderRepository` — Create order from cart, get order history

**Services:**
- `ISearchService` / `SearchService` — Orchestrates keyword → vector → fallback pipeline
- `IChatService` / `ChatService` — Embed → hybrid search → build RAG context → call LLM → return answer. Web search fallback if no DB results.
- `IWebSearchProvider` — Stub interface for future Bing/Google integration

**Features (CQRS via MediatR):**

| Feature | Commands/Queries |
|---------|----------------|
| Chat | `AskCommand`, `AskWithContextCommand` |
| Search | `KeywordSearchQuery`, `VectorSearchQuery`, `HybridSearchQuery` |
| Embeddings | `GenerateEmbeddingCommand`, `UpsertEmbeddingCommand`, `SeedEmbeddingsCommand` |
| Cart | `AddToCartCommand`, `GetCartQuery`, `RemoveFromCartCommand`, `CheckoutCommand` |
| Order | `GetOrderHistoryQuery` |

**Endpoints:**
- `POST /search/vector`, `/search/hybrid` (exposed via nginx as `/api/search/*`)
- `POST /chat/ask`, `/chat/ask/context` (exposed via nginx as `/api/chat/*`)
- `POST /embeddings/generate`, `/embeddings/upsert`, `/embeddings/seed` (exposed via nginx as `/embeddings/*`)
- `POST /cart/add`, `GET /cart`, `DELETE /cart/remove`, `POST /cart/checkout` (exposed via nginx as `/api/cart/*`)
- `GET /orders` (exposed via nginx as `/api/orders`)
- `POST /auth/login`, `POST /auth/register` (exposed via nginx as `/api/auth/*`)
- `POST /voice/stt`, `POST /voice/tts` (exposed via nginx as `/api/voice/*`)

### 4. Angular Frontend (`/Client/`)

**Scaffolding:**
- Standalone components only, zoneless change detection
- `app.config.ts` — Router, HttpClient with interceptors, animations
- `app.routes.ts` — Lazy-loaded `auth` and `store` features

**Core:**
- `models/` — `product.model.ts`, `chat-message.model.ts`, `user.model.ts`, `cart.model.ts`
- `interceptors/` — `auth.interceptor.ts` (JWT), `error.interceptor.ts`, `loading.interceptor.ts`
- `guards/` — `auth.guard.ts`

**Features:**

| Module | Components |
|--------|------------|
| `auth/` | `login.component.ts`, `register.component.ts` |
| `store/browse/` | `product-list.component.ts`, `product-detail.component.ts`, `product-filters.component.ts` |
| `store/cart/` | `cart.component.ts`, `cart-item.component.ts` |
| `store/checkout/` | `checkout.component.ts` |
| `store/orders/` | `order-history.component.ts` |

**Services:**
- `auth.service.ts` — JWT login/register, token management
- `product.service.ts` — Catalog API + AI search endpoints
- `chat.service.ts` — `POST /chat/ask` and `/chat/ask/context`
- `cart.service.ts` — Cart state (signals) + cart/order API calls

**Shared:**
- `navbar.component.ts`, `loading-spinner.component.ts`, `error-display.component.ts`
- `currency.pipe.ts`, `click-outside.directive.ts`

---

## Phase 2 — Voice & Contextual Memory

### Backend Additions

| File | Purpose |
|------|---------|
| `Contracts/Voice/SttRequest.cs`, `TtsRequest.cs` | Voice DTOs |
| `Features/Voice/SttCommand.cs`, `TtsCommand.cs` + Handlers | STT/TTS endpoints |
| `Services/IVoiceService.cs`, `VoiceService.cs` | Azure Cognitive Services Speech wrapper |
| `Configuration/AzureSpeechOptions.cs` | Speech config |
| `Endpoints/VoiceEndpoints.cs` | `POST /voice/stt`, `POST /voice/tts` (exposed via nginx as `/api/voice/*`) |

**ChatService Enhancement:**
- Load last N messages on every `/chat/ask/context` call
- Follow-up detection: query ≤ 6 words + no product keywords → prepend last assistant topic
- Pass full `messages[]` history to LLM

### Frontend Additions

| File | Purpose |
|------|---------|
| `core/chatbot/chatbot.component.ts/.html/.scss` | Floating chat panel |
| `core/chatbot/chat-message.component.ts` | Render bot/user messages |
| `core/chatbot/product-card.component.ts` | Inline product cards in chat |
| `core/chatbot/voice-input.component.ts` | Microphone capture, WAV encoding |
| `services/voice.service.ts` | `POST /voice/stt`, `POST /voice/tts` |

**Voice Loop:**
1. `MediaRecorder` captures audio
2. Inline WAV encoder (16-bit PCM, 16kHz, mono)
3. `POST /voice/stt` → transcript
4. Auto-send to `POST /chat/ask/context` with `conversationId`
5. Render response + optional TTS playback via `POST /voice/tts` → `Audio()`

---

## Local Run Instructions (Step-by-Step)

```bash
# 1. Start all services
docker compose up -d pgvector ollama catalogservice aiservice

# 2. Pull Ollama models (first time only)
docker exec aishop-ollama ollama pull nomic-embed-text
docker exec aishop-ollama ollama pull tinyllama

# 3. Seed catalog with 50+ products
curl -X POST http://localhost/seed          # via nginx
# or
curl -X POST http://localhost:8010/seed     # direct

# 4. Generate embeddings for all products
curl -X POST http://localhost/embeddings/seed   # via nginx
# or
curl -X POST http://localhost:8090/embeddings/seed  # direct

# 5. Run frontend
cd Client && npm install && ng serve

# 6. Open browser
# http://localhost:4200
```

### API Routing

| Nginx Route | Backend Service | Example |
|-------------|----------------|---------|
| `/catalog/*` | CatalogService (`:8010`) | `/catalog/products` → `GET /products` |
| `/api/*` | AiService (`:8090`) | `/api/auth/login` → `POST /auth/login` |
| `/seed` | CatalogService (convenience) | `POST /seed` |
| `/embeddings/*` | AiService (convenience) | `POST /embeddings/seed` |

---

## Open Questions / Confirmations Before Starting

1. **Angular Version**: Should we target Angular **19.x** (stable latest) or **20.x** (bleeding edge)?
2. **Authentication Backend**: Should the AiService include a minimal `POST /auth/login` and `POST /auth/register` endpoint with bcrypt password hashing, or should auth be mocked/stubbed for Phase 1?
3. **Cart Storage**: Should the cart be stored in **localStorage** for unauthenticated users and synced to the backend on login, or only server-side per authenticated user?

---

*Plan Version: 1.0*
## 15. Test Coverage

### Backend Tests (.NET xUnit + Moq)
| Service | Tests |
|---------|-------|
| SearchService | Keyword search, vector search, hybrid search |
| ChatService | Ask with/without products, context conversation, follow-up detection, message persistence |
| AuthService | Login validation, JWT token generation, claims verification |
| VoiceService | Constructor, XML escaping for SSML |

### Frontend Tests (Jasmine + Karma)
| Service | Tests |
|---------|-------|
| AuthService | Login, register, logout, isLoggedIn, JWT expiration |
| ProductService | getProducts, getProduct, getBrands, getTypes, searchSemantic, searchHybrid |
| ChatService | Ask with/without conversationId |
| CartService | Add item, update quantity, remove, clear, total calculation, localStorage persistence, checkout, get orders |
| VoiceService | Transcribe, synthesize |
| LoadingService | Show/hide, concurrent request tracking |

| Component | Tests |
|-----------|-------|
| ProductListComponent | Load products, filter by search/brand/type, sort by price, AI search, add to cart |
| LoginComponent | Form validation, submit, success navigation, error display |
| ChatbotComponent | Toggle chat, send message, conversation persistence, loading state, recording |
| CartComponent | Empty cart, display items, total calculation, clear cart |

*Last Updated: 2026-05-22*
