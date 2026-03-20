#!/usr/bin/env bash

key=$(openssl rand -base64 48 | tr -d '\n=+/' | cut -c1-48)
echo "Auth__JwtKey=${key}"