# Data Model: Permissions & Roles

## Overview
The authorization system is based on **Claims-based Authorization**. Permissions are strings represented as claims in the JWT. Roles are logical groupings of these permissions.

## Key Entities

### Permission (Constant)
Represents a granular action.
- **Value**: `career.[category].[action]`
- **Categories**: `trainings`, `evaluations`, `paths`, `development`

### Role (Manifest)
A collection of permissions.
- **Name**: `career-admin`, `career-hr`, `career-manager`, `career-employee`
- **Permissions**: List of permission strings.

## State Transitions
N/A - Permissions and roles are static for this service version and are registered at startup.

## Validation Rules
- Permission names must follow the `career.*` prefix.
- Role names must follow the `career-*` prefix.
