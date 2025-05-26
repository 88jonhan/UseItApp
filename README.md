# UseIt

A web app for lending and borrowing stuff in your community. Built because I got tired of owning tools I use twice a year.

## What it does

Post items you're willing to lend, browse what others have available, request to borrow things. Simple approval workflow where owners can accept or reject requests.

## Tech stack

- .NET 9 / C# backend
- Angular 19 frontend  
- SQLite database
- JWT auth

## Running it

Need .NET 9 and Node.js installed.

```bash
git clone [your-repo-url]
cd UseIt

# Backend
cd UseItApp.API
dotnet restore
dotnet ef database update

# Frontend
cd ../useIt-app
npm install
```

**Start everything:**
```bash
npm run start:all
```

**Or run separately:**
```bash
# API
dotnet run --project UseItApp.API

# Frontend  
cd UseItApp.API/ClientApp/ && ng serve

# Tests
dotnet test UseItApp.Tests
```

Frontend: http://localhost:4200  
API: http://localhost:5000

## Project structure

```
UseItApp.API/             - API controllers  
UseItApp.Domain/          - Models (User, Item, Loan)
UseItApp.Data/            - Database stuff
UseItApp.Tests/           - Tests
UseItApp.API/ClientApp/   - Angular app
```

## Notes

This was my thesis project - main goal was proving I could actually finish something substantial. The core functionality works but it's basically an MVP. No user research, runs locally only, pretty basic auth and UI.

Built at Yrkeshögskolan i Borås, 2023-2025.
