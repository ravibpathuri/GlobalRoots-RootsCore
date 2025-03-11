# GlobalRoots-RootsCore

The .NET API backend for the GlobalRoots family tree genealogy application. Manages family data via Neo4j and generates AI-driven prompts.

## Setup
1. Install .NET 8.0 SDK.
2. Run `dotnet restore`.
3. Configure Neo4j in `appsettings.json`.
4. Run `dotnet run`.

## Docker
- Build: `docker build -t globalroots/rootscore .`
- Run: `docker run -p 5001:5001 globalroots/rootscore`

## Endpoints
- POST `/api/prompt/generate`: Generate an AI prompt.
- GET `/api/prompt/suggest`: Suggest prompts based on missing data.