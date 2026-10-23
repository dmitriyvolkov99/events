# Events

Full-stack приложение для управления мероприятиями.

Пользователи могут регистрироваться и входить в систему, создавать и редактировать мероприятия, присоединяться к ним, подписываться на других пользователей, загружать фотографии, оставлять комментарии и общаться в реальном времени в чате мероприятия.

Проект полностью контейнеризирован (Docker Compose). CI/CD работает на GitHub Actions, образы публикуются в GitHub Container Registry (GHCR).

## Стек

### Backend

- C#, .NET 10, ASP.NET Core
- Entity Framework Core, SQL Server
- ASP.NET Core Identity, JWT
- SignalR
- AutoMapper, FluentValidation
- Cloudinary (хранение фотографий)
- OpenAPI + Swagger UI (только в режиме Development)

### Frontend

- React 19, TypeScript, Vite
- Tailwind CSS 4, Headless UI, lucide-react (светлая и тёмная темы)
- MobX
- React Router (data router, ленивая загрузка страниц)
- Formik + Yup
- Axios, SignalR client
- date-fns, react-datepicker, react-calendar, react-cropper, react-dropzone, react-toastify
- Vitest, Testing Library, ESLint

### DevOps

- Docker, Docker Compose
- Nginx
- GitHub Actions
- GitHub Container Registry (GHCR)

## Структура репозитория

```
.
├── backend/Events/        # решение .NET (Events.slnx)
│   ├── API/               # Web API, SignalR-хаб, Dockerfile
│   ├── Application/
│   ├── Domain/
│   ├── Persistence/
│   └── Infrastructure/
├── client-app/            # React-приложение (Vite), Dockerfile, nginx.conf
├── compose.yml            # общая часть: sqlserver, backend, frontend
├── compose.dev.yml        # локальная сборка образов
├── compose.prod.yml       # готовые образы из GHCR
└── .github/workflows/     # ci.yml, cd.yml
```

## Быстрый старт (Docker)

### Требования

- Docker с Docker Compose v2

.NET, Node.js и SQL Server локально ставить не нужно: всё запускается в контейнерах.

### 1. Создать `.env`

В корне проекта создайте `.env` на основе `.env.example`:

```powershell
Copy-Item .env.example .env
```

Переменные:

```env
# Общие
DB_PASSWORD=
TOKEN_KEY=

# Cloudinary
CLOUDINARY_CLOUD_NAME=
CLOUDINARY_API_KEY=
CLOUDINARY_API_SECRET=

# Только для production (имя владельца образов в нижнем регистре)
GHCR_OWNER=
# IMAGE_TAG=
```

- `DB_PASSWORD` должен удовлетворять требованиям SQL Server: минимум 8 символов, заглавные и строчные буквы, цифры или символы.
- `TOKEN_KEY` это секрет для подписи JWT: случайная строка от 64 символов. Сгенерировать в PowerShell:

```powershell
  $b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

Файл `.env` содержит секреты и не должен попадать в git.

### 2. Запустить

Из корня проекта:

```powershell
docker compose -f compose.yml -f compose.dev.yml up -d --build
```

Первая сборка занимает несколько минут. Миграции базы применяются автоматически при старте backend, а в режиме Development ещё создаются тестовые данные.

| Адрес | Что это |
|---|---|
| http://localhost | Приложение |
| http://localhost:8080/swagger/index.html | Swagger UI (только в Development) |

SQL Server наружу не публикуется, доступен только внутри сети Docker.

### 3. Остановить

```powershell
docker compose -f compose.yml -f compose.dev.yml down
```

Данные базы сохраняются в томе `sqlserver-data`. Команда с флагом `-v` удалит том вместе со **всеми данными**.

## Разработка без Docker

Нужны .NET 10 SDK, Node.js ≥ 24.18 и доступный SQL Server (строка подключения задаётся ключом `ConnectionStrings:DefaultConnection`).

**Backend:**

```powershell
cd backend/Events/API
dotnet run
```

**Frontend:**

```powershell
cd client-app
npm ci
npm run dev
```

Приложение откроется на http://localhost:3000.

Frontend берёт адреса API и чата из переменных окружения Vite:

| Файл | Когда используется | `VITE_API_URL` / `VITE_CHAT_URL` |
|---|---|---|
| `client-app/.env.development` | `npm run dev` | `http://localhost:5117/api`, `http://localhost:5117/chat` (локально запущенный backend) |
| `client-app/.env.production` | сборка образа и `npm run build` | `/api`, `/chat` (запросы идут через nginx на том же домене) |

Полезные команды:

```powershell
# frontend (из client-app)
npm run lint
npm test
npm run build

# backend (из backend/Events)
dotnet test Events.slnx
dotnet format Events.slnx --verify-no-changes
```

## Как это устроено в Docker

- **frontend**: nginx отдаёт собранное SPA и проксирует `/api/` на backend и `/chat` (SignalR, WebSocket) тоже на backend. Всё идёт через один домен, поэтому в production CORS не нужен.
- **backend**: ASP.NET Core на порту 8080 внутри сети Docker.
- **sqlserver**: SQL Server 2022, данные в томе `sqlserver-data`.
- Загрузка файлов через nginx ограничена 10 МБ.
- Ключи ASP.NET DataProtection хранятся в томе `dataprotection-keys`.

## CI/CD

**CI** (`ci.yml`) запускается при push и pull request в `main`:

- backend: restore, проверка форматирования, build, тесты;
- frontend: ESLint, проверка типов, тесты, build;
- проверочная сборка Docker-образов (с кэшем).

**CD** (`cd.yml`) запускается после успешного CI на `main`: собирает образы и публикует их в GHCR с двумя тегами, `latest` и полным SHA коммита.

## Production

Production использует готовые образы из GHCR вместо локальной сборки:

```
ghcr.io/<owner>/events-backend
ghcr.io/<owner>/events-frontend
```

На сервере нужны Docker, файлы `compose.yml` и `compose.prod.yml` и `.env` с теми же переменными, что выше (обязательно `GHCR_OWNER`). Если образы приватные, сначала выполните вход:

```bash
docker login ghcr.io
```

Запуск и обновление:

```bash
docker compose -f compose.yml -f compose.prod.yml pull
docker compose -f compose.yml -f compose.prod.yml up -d
```

**Откат** на конкретную версию: задайте в `.env` строку `IMAGE_TAG=<полный SHA коммита>` и снова выполните `pull` и `up -d`. SHA виден в списке версий пакета на GitHub.

В production backend работает в режиме `Production`: Swagger отключён, тестовые данные не создаются.

> **Важно:** HTTPS пока не настроен, приложение отдаётся по порту 80. Для публичного домена нужен TLS (например, обратный прокси с автоматическими сертификатами или балансировщик провайдера). Автоматический деплой на сервер появится после добавления production-сервера. Резервное копирование тома `sqlserver-data` нужно настраивать отдельно.
