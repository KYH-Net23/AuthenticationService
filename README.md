# TokenService

TokenService is a microservice for generating authorization tokens, designed to handle login requests and token creation based on user identity. This service uses JWT (JSON Web Token) for secure and efficient token management.

---

## Table of Contents
- [Overview](#overview)
- [Usage](#usage)
  - [Endpoints](#endpoints)
- [Token Generation](#token-generation)
- [Security](#security)

---

## Overview

The TokenService project provides a simple, secure, and customizable way to handle user authorization. It is structured around two primary components:
- **TokenGeneratorController**: Handles login requests and generates tokens upon successful authentication.
- **TokenGeneratorService**: Generates JWT tokens with defined claims and expiration.

---

## Usage

### Endpoints

1. **POST** `/TokenGenerator/login`  
   Authenticates a user via their credentials and returns a JWT token if successful.

   - **Request Body**:
     ```json
     {
       "email": "user@example.com",
       "password": "password123"
     }
     ```
   - **Response**:
     - 200 OK: Returns the JWT token if the login is successful.
     - 401 Unauthorized: Returns if the login fails.

2. **GET** `/TokenGenerator?email={email}`  
   Generates a token based on the provided email (used for testing purposes).

   - **Query Parameter**:
     - `email` (string): Email for which to generate a token.
   - **Response**:
     - 200 OK: Returns the generated JWT token.

---

## Token Generation

Tokens are generated using [JWT](https://jwt.io/), with claims such as:
- **Jti**: Unique identifier for the token.
- **Email**: Email of the authenticated user.
- **Role**: User role, currently set as `"Customer"`.

Token expiration is set to **5 minutes**. The token contains issuer and audience fields to identify the source and intended recipient.

---

## Security

The secret key used to sign tokens is stored as a hardcoded string for simplicity in this example. For a production environment, ensure:
- Secret keys are stored securely, such as in [Azure Key Vault](https://azure.microsoft.com/services/key-vault/).
- Tokens are transmitted over HTTPS to prevent interception.

