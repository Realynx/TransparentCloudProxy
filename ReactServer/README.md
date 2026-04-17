# Transparent Cloud Proxy Admin Panel

React + TypeScript + Tailwind CSS admin panel with a Node.js (Express) API server for storing and managing remote proxy OneKey credentials.

## Stack

- React 19 + TypeScript (Vite)
- Tailwind CSS (custom theme: dark blue, blue, orange, black)
- Node.js + Express + TypeScript backend

## Features

- OneKey credential import and validation
- OneKey decode compatible with the C# client/server flow
- Reachable address extraction from OneKey payloads
- Persistent JSON-backed credential storage in `server/data/onekeys.json`
- Firewall-style NAT/proxy rule management UI
- Proxy server fleet management with multi-target rule assignment
- "All servers" alias target mode that automatically includes new servers

## Project Layout

- `client/`: React TypeScript app
- `server/`: Node.js TypeScript API

## Run In Development

1. Install root dependencies:
   - `npm install`
2. Install client dependencies:
   - `npm --prefix client install`
3. Start frontend + backend together:
   - `npm run dev`

- Frontend: `http://localhost:5173`
- Backend API: `http://localhost:4000`

## Build And Run

1. `npm run build`
2. `npm start`

The backend serves static files from `client/dist` after build.

## API Endpoints

### Admin panel endpoints

- `GET /api/health`
- `GET /api/credentials`
- `POST /api/credentials`
- `DELETE /api/credentials/:id`
- `GET /api/proxy-rules/config`
- `POST /api/proxy-rules`
- `PUT /api/proxy-rules/:id`
- `DELETE /api/proxy-rules/:id`
- `POST /api/proxy-servers`
- `DELETE /api/proxy-servers/:id`

## Environment Variables

Copy `.env.example` and customize values:

- `PORT`: API server port
- `CLIENT_ORIGIN`: allowed CORS origin for local frontend dev
