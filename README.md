# club-portal

A portal for clubs to use

# Add secret

`Make .env file and add SendGrid secret`

# Add certs

Go to /nginx/certs and add selfsigned certs (.crt & .key)

## Dev (with vite hot reload)

`docker-compose -f docker-compose.dev.yaml up --build`

## Prod

`docker-compose up --build`

## Connect to database (connect to container first)

`psql -U postgres -d club`

## Setup Domain, run first then remove --dry-run

## https://phoenixnap.com/kb/letsencrypt-docker

`docker compose run --rm certbot certonly --webroot --webroot-path /var/www/certbot/ --dry-run -d aktiesparklubb.se`
