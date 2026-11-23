# Events

Full-stack приложение для управления мероприятиями.

Пользователи регистрируются и входят в систему, создают и редактируют мероприятия, присоединяются к ним, подписываются на других пользователей, загружают фотографии, оставляют комментарии и общаются в реальном времени в чате мероприятия.

Проект полностью контейнеризирован (Docker Compose). CI/CD работает на GitHub Actions, образы публикуются в GitHub Container Registry (GHCR).

## Содержание

- [Возможности](#возможности)
- [Стек](#стек)
- [Структура репозитория](#структура-репозитория)
- [Быстрый старт (Docker)](#быстрый-старт-docker)
- [Разработка без Docker](#разработка-без-docker)
- [Тесты](#тесты)
- [Безопасность](#безопасность)
- [Как это устроено в Docker](#как-это-устроено-в-docker)
- [CI/CD](#cicd)
- [Production](#production)
- [Частые проблемы](#частые-проблемы)
- [Что ещё не сделано](#что-ещё-не-сделано)

## Возможности

- Регистрация и вход (JWT), профиль пользователя.
- Создание и редактирование мероприятий, вступление и выход, отмена мероприятия организатором.
- Лента с фильтрами («все», «я иду», «я организатор»), календарём и бесконечной прокруткой.
- Фотографии: загрузка, обрезка, выбор главной, удаление.
- Подписки на пользователей, списки подписчиков и подписок.
- Комментарии и чат мероприятия в реальном времени (SignalR).
- Светлая и тёмная темы (с переключателем и режимом «как в системе»).

## Стек

### Backend

- C#, .NET 10, ASP.NET Core
- Entity Framework Core, SQL Server
- ASP.NET Core Identity, JWT
- MediatR, AutoMapper, FluentValidation
- SignalR
- Cloudinary (хранение фотографий)
- OpenAPI + Swagger UI (только в режиме Development)
- xUnit, SQLite в памяти, `WebApplicationFactory` (тесты)

### Frontend

- React 19, TypeScript, Vite
- Tailwind CSS 4, Headless UI, lucide-react
- MobX
- React Router (data router, ленивая загрузка страниц)
- Formik + Yup
- Axios, клиент SignalR
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
│   ├── Application/       # обработчики MediatR, валидаторы, DTO
│   ├── Domain/            # сущности
│   ├── Persistence/       # DataContext, миграции, Seed
│   ├── Infrastructure/    # Cloudinary, доступ к текущему пользователю, авторизация
│   └── Tests/             # юнит- и интеграционные тесты
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
- `TOKEN_KEY` это секрет для подписи JWT: случайная строка **не короче 64 байт** (иначе backend не запустится). Сгенерировать в PowerShell:

  ```powershell
  $b = New-Object byte[] 48; [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
  ```

- `IMAGE_TAG` необязателен: версия образов для отката (по умолчанию `latest`).
- Срок жизни токена по умолчанию 7 дней. Чтобы изменить, добавьте в `compose.yml` в `environment` сервиса `backend` строку `TokenLifetimeDays: "3"` (допустимо от 1 до 90).

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
| http://localhost:8080/health | Проверка работоспособности backend и базы (в Development порт 8080 открыт) |

SQL Server наружу не публикуется, доступен только внутри сети Docker.

### Тестовые данные

В режиме Development создаются тестовые пользователи, например `bob@test.com` и `tom@test.com`, пароль у всех `Pa$$w0rd`, и несколько мероприятий. В Production тестовые данные не создаются.

### 3. Остановить

```powershell
docker compose -f compose.yml -f compose.dev.yml down
```

Данные базы сохраняются в томе `sqlserver-data`. Команда с флагом `-v` удалит том вместе со **всеми данными**.

## Разработка без Docker

Нужны .NET 10 SDK, Node.js ≥ 24.18 и доступный SQL Server.

**Backend.** Секреты хранятся вне репозитория, в user-secrets (из папки `backend/Events/API`):

```powershell
dotnet user-secrets init
dotnet user-secrets set "TokenKey" "<случайная строка от 64 символов>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<строка подключения к SQL Server>"
dotnet user-secrets set "Cloudinary:CloudName" "<значение>"
dotnet user-secrets set "Cloudinary:ApiKey" "<значение>"
dotnet user-secrets set "Cloudinary:ApiSecret" "<значение>"
dotnet run
```

Без ключей Cloudinary приложение в режиме Development запускается (в логе будет предупреждение), но загрузка фото не работает.

**Frontend** (из папки `client-app`):

```powershell
npm ci
npm run dev
```

Приложение откроется на http://localhost:3000.

Frontend берёт адреса API и чата из переменных окружения Vite:

| Файл | Когда используется | `VITE_API_URL` / `VITE_CHAT_URL` |
|---|---|---|
| `client-app/.env.development` | `npm run dev` | `http://localhost:5117/api`, `http://localhost:5117/chat` (локально запущенный backend) |
| `client-app/.env.production` | сборка образа и `npm run build` | `/api`, `/chat` (запросы идут через nginx на том же домене) |

## Тесты

```powershell
# backend (из backend/Events)
dotnet test Events.slnx
dotnet format Events.slnx --verify-no-changes

# frontend (из client-app)
npm test
npm run lint
npx tsc --noEmit
npm run build
```

- **Backend, юнит-тесты:** обработчики MediatR работают на SQLite в памяти (настоящие ограничения и каскады), покрыты валидаторы, конвейер валидации, `ExceptionMiddleware`, проверка прав организатора, `TokenService`, постраничная выдача.
- **Backend, интеграционные тесты:** поднимают приложение целиком через `WebApplicationFactory` (окружение `Testing`, SQLite вместо SQL Server, фейк вместо Cloudinary) и проверяют авторизацию, права на изменение и удаление, валидацию, лимит и блокировку входа, загрузку фото, подписки и `/health`. Docker и внешние сервисы не нужны.
- **Frontend:** Vitest и Testing Library (сторы MobX, перехватчики API, формы).

## Безопасность

- Все запросы к API и чату требуют токен; отдельные адреса, доступные без входа, это только вход, регистрация и `/health`.
- Изменять и удалять мероприятие может только его организатор; фото можно менять и удалять только владельцу.
- Вход и регистрация: не более 10 запросов в минуту с одного адреса, блокировка учётной записи на 5 минут после 5 неверных паролей. При неверной почте и неверном пароле ответ одинаковый.
- Пароль: не короче 8 символов, с заглавной и строчной буквами и цифрой.
- Данные мероприятий и комментариев проверяются на сервере (длины, допустимые категории, диапазон даты), загрузка фото ограничена форматами JPEG, PNG, WebP, GIF и размером 10 МБ.
- В Production ответ при внутренней ошибке не содержит технических подробностей.
- CORS включён только в режиме Development: в Docker запросы идут через nginx с того же домена.

## Как это устроено в Docker

- **frontend**: nginx отдаёт собранное SPA, проксирует `/api/` на backend и `/chat` (SignalR, WebSocket) тоже на backend. Всё идёт через один домен, поэтому в production CORS не нужен. Файлы с хэшем в имени кэшируются на год, `index.html` не кэшируется, ответы сжимаются (gzip), загрузка через `/api/` ограничена 10 МБ.
- **backend**: ASP.NET Core на порту 8080 внутри сети Docker; доверяет заголовкам `X-Forwarded-*` от nginx (нужно для лимита входа по адресу клиента). Healthcheck проверяет `/health`, то есть и приложение, и базу.
- **sqlserver**: SQL Server 2022, данные в томе `sqlserver-data`.
- Ключи ASP.NET DataProtection хранятся в томе `dataprotection-keys`.
- Контейнеры запускаются по порядку: база, затем backend, затем frontend (каждый ждёт статуса `healthy` предыдущего). Пересоздание одного backend не требует перезапуска nginx.

## CI/CD

**CI** (`ci.yml`) запускается при push и pull request в `main`:

- backend: restore, проверка форматирования, build, тесты (версия .NET SDK закреплена в файле);
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

В production backend работает в режиме `Production`: Swagger отключён, тестовые данные не создаются, а при отсутствии настроек Cloudinary или слишком коротком `TOKEN_KEY` приложение не запускается и пишет причину в лог.

> **Важно:** HTTPS пока не настроен, приложение отдаётся по порту 80. Для публичного домена нужен TLS (например, обратный прокси с автоматическими сертификатами или балансировщик провайдера). Автоматический деплой на сервер появится после добавления production-сервера. Резервное копирование тома `sqlserver-data` нужно настраивать отдельно.

## Частые проблемы

- **Backend не становится `healthy`.** Смотрите логи: `docker compose -f compose.yml -f compose.dev.yml logs backend`. Частая причина: пароль SQL Server не проходит по сложности или `DB_PASSWORD` изменили после первого запуска. Пароль `sa` записывается в том при первой инициализации, поэтому после смены пароля нужно пересоздать том командой `down -v` (данные будут удалены).
- **Backend сразу завершается с «Missing Cloudinary settings» или «TokenKey must be configured…».** В Production обязательны все три переменные `CLOUDINARY_*`, а `TOKEN_KEY` должен быть не короче 64 байт. В режиме Development без Cloudinary приложение запускается, но загрузка фото не работает.
- **Порт 80 или 8080 занят.** Остановите приложение, которое использует порт, либо измените проброс портов в `compose.yml` и `compose.dev.yml`.
- **Фото не загружается.** Размер ограничен 10 МБ, допустимы JPEG, PNG, WebP и GIF, а для Cloudinary должны быть заданы три переменные `CLOUDINARY_*`.
- **«Too many attempts» при входе.** Сработал лимит (10 запросов в минуту) или блокировка после 5 неверных паролей. Подождите минуту (или 5 минут для блокировки).
- **Всех разлогинило.** Вы изменили `TOKEN_KEY`: ранее выданные токены становятся недействительными, нужно войти заново.
- **Изменения в коде не видны в Docker.** Образы собираются при запуске, добавьте `--build`.

