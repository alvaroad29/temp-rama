# Etapa 1: Build
FROM node:22-alpine AS build
WORKDIR /app

# Copia package files
COPY package*.json ./
RUN npm install

# Copia código fuente
COPY . .

# Construye la app
RUN npm run build

# Etapa 2: Servidor estático
FROM nginx:alpine
COPY --from=build /app/dist/online-library-app /usr/share/nginx/html

# Copia configuración de nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
