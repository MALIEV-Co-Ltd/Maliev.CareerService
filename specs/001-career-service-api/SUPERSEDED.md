# ⚠️ SUPERSEDED SPECIFICATION

> [!CAUTION]
> **This specification has been superseded and should NOT be used for new implementations.**

## Status: ARCHIVED

This specification (`001-career-service-api`) was created before the adoption of MALIEV Constitution XIV, which explicitly prohibits:

- **AutoMapper** - Use explicit mapping instead
- **FluentValidation** - Use .NET DataAnnotations instead  
- **FluentAssertions** - Use built-in xUnit assertions instead

## Legacy References Found

The following files contain legacy references that violate current standards:

| File | Issue |
|------|-------|
| tasks.md | 16+ references to AutoMapper/FluentValidation packages and validators |
| plan.md | Lists AutoMapper 12.0.1 and FluentValidation 11.5.1 as dependencies |
| quickstart.md | References Validators/ directory structure |

## Replacement

For new CareerService implementations, refer to:

1. **Current CareerService code** in repository root (if exists)
2. **MALIEV Constitution XIV** for technology standards
3. **MessagingContracts schemas** for event definitions

## Migration Required

If code was implemented from this spec:

1. Remove AutoMapper package and mapping profiles
2. Replace with explicit mapping methods (manual or source generators)
3. Remove FluentValidation package and AbstractValidator classes
4. Replace with .NET DataAnnotations attributes on DTOs
5. Remove FluentAssertions from tests
6. Use standard xUnit assertions

---

*Archived: 2025-12-29*
*Reason: Non-compliant with MALIEV Constitution XIV*
