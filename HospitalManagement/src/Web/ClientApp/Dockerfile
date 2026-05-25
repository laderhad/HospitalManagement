FROM node:22-bookworm-slim AS node

FROM mcr.microsoft.com/dotnet/sdk:10.0
WORKDIR /app

COPY --from=node /usr/local/ /usr/local/

COPY ClientApp/package*.json ./
RUN npm ci

COPY ClientApp/ ./
COPY wwwroot/openapi/ ../wwwroot/openapi/

ENV PORT=5173
EXPOSE 5173

CMD ["npm", "run", "start", "--", "--host", "0.0.0.0"]
