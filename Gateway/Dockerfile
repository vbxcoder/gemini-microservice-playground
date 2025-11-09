FROM nginx:latest

# Install build dependencies
RUN apt-get update && apt-get install -y \
    build-essential \
    libssl-dev \
    libpcre2-dev \
    zlib1g-dev \
    wget \
    libjwt-dev \
    libjansson-dev \
    git

# Download and build the auth_jwt module
RUN git clone https://github.com/kjdev/nginx-auth-jwt.git /usr/src/nginx-auth-jwt

# Get the nginx version
ARG NGINX_VERSION
RUN NGINX_VERSION=$(nginx -v 2>&1 | sed 's/nginx version: nginx\///') && \
    wget http://nginx.org/download/nginx-${NGINX_VERSION}.tar.gz && \
    tar -zxvf nginx-${NGINX_VERSION}.tar.gz && \
    cd nginx-${NGINX_VERSION} && \
    ./configure --with-compat --add-dynamic-module=/usr/src/nginx-auth-jwt && \
    make modules

# Copy the module to the nginx modules directory
RUN find / -name "ngx_http_auth_jwt_module.so" -exec cp {} /etc/nginx/modules/ \;

# Load the module in the nginx configuration
RUN sed -i '1iload_module /etc/nginx/modules/ngx_http_auth_jwt_module.so;' /etc/nginx/nginx.conf

# Copy the custom nginx configuration
COPY nlogistic.conf /etc/nginx/conf.d/default.conf
COPY jwt_public.jwks.json /etc/nginx/
COPY nginx-selfsigned.crt /etc/nginx/
COPY nginx-selfsigned.key /etc/nginx/