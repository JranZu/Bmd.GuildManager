# Frontend

This folder contains the web client for the Guild Manager application, hosted on **Azure Static Web Apps**.

## Framework

The frontend will be implemented using **Blazor WebAssembly**, providing a rich single-page application (SPA) experience with C# code running directly in the browser via WebAssembly.

## Getting Started

> Project scaffold to be added in a subsequent commit.

## Structure (Planned)

```
/frontend
├── src/               # Application source code
├── public/            # Static assets
├── tests/             # Frontend unit/integration tests
└── README.md
```

## Deployment

The frontend is deployed automatically via the CI/CD pipeline defined in `/.github/workflows` on every push to the `main` branch.
