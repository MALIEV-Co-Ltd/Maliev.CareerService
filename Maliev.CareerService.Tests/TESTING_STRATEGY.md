# Testing Strategy for Career Service API

## Current Status

### Existing Tests
- **Unit Tests**: 1 file (MarkdownRenderingTests.cs) - 19 passing tests
- **Integration Tests**: 18 files - 158 failing (Docker/Testcontainers dependency)
- **Contract Tests**: 5 files - failing (Docker dependency)

### Test Coverage Gaps

#### Services WITHOUT Unit Tests
1. `ApplicationService` - Complex business logic for job applications
2. `JobPostingService` - Job posting management
3. `TrainingProgramService` - Training program management
4. `EnrollmentService` - Training enrollment logic
5. `ELearningResourceService` - E-learning resource management
6. `ReportService` - Metrics and reporting
7. `DevelopmentPlanService` - IDP management
8. `DevelopmentGoalService` - Development goal tracking
9. `MetricsService` - Prometheus metrics collection

#### Other Components WITHOUT Tests
1. **Validators** (11 validators) - No unit tests
2. **Middleware** (3 middleware) - No unit tests
3. **AutoMapper Profiles** - No mapping tests
4. **External Service Clients** (4 clients) - No unit tests

## Testing Strategy

### Phase 1: Critical Service Unit Tests (Priority: HIGH)
Test core business logic with mocked dependencies

#### 1.1 ApplicationService Tests
- SubmitApplicationAsync
  - Valid application submission
  - Duplicate email detection
  - Deadline validation
  - File validation via Upload Service
  - Email notification sending
- UpdateApplicationStatusAsync
  - Valid status transitions
  - Invalid transitions rejection
  - Status reversal tracking
  - Optimistic concurrency handling
  - Email notifications
- GetStatusHistoryAsync
  - Full audit trail retrieval
  - User name enrichment

#### 1.2 JobPostingService Tests
- CreatePostingAsync
  - Valid posting creation
  - Position code uniqueness
  - Markdown content handling
- GetActivePostingsAsync
  - Active filtering
  - Pagination
  - Search functionality
- UpdatePostingAsync
  - Optimistic concurrency
  - Validation

#### 1.3 DevelopmentPlanService Tests
- CreateIDPAsync
  - Duplicate year detection
  - Employee validation
- SubmitIDPAsync
  - Status validation (must be Draft)
  - Submission timestamp
- ApproveIDPAsync
  - HR-only approval
  - Status validation (must be Submitted)
  - Approval tracking

#### 1.4 EnrollmentService Tests
- EnrollEmployeeAsync
  - Duplicate enrollment detection
  - Employee validation
  - Training program validation
- MarkCompletedAsync
  - Completion timestamp
  - Completion notes
  - HR authorization

#### 1.5 ReportService Tests
- GenerateRecruitmentMetricsAsync
  - Application counts
  - Conversion rates
  - Time-to-hire calculations
  - Date range filtering
- GenerateLearningMetricsAsync
  - Enrollment rates
  - Completion rates
  - Popular programs
- Cache invalidation testing

### Phase 2: Validator Unit Tests (Priority: HIGH)
Test all FluentValidation validators in isolation

1. CreateJobPostingRequestValidator
2. UpdateJobPostingRequestValidator
3. SubmitJobApplicationRequestValidator
4. UpdateApplicationStatusRequestValidator
5. CreateTrainingProgramRequestValidator
6. EnrollInTrainingRequestValidator
7. CreateIDPRequestValidator
8. ApproveIDPRequestValidator
9. CreateDevelopmentGoalRequestValidator
10. UpdateGoalStatusRequestValidator
11. MarkTrainingCompleteRequestValidator

### Phase 3: Middleware Unit Tests (Priority: MEDIUM)
1. ExceptionHandlingMiddleware
   - Exception to ErrorResponse conversion
   - Different exception types
   - Status code mapping
2. RequestLoggingMiddleware
   - Log formatting
   - Request/response logging
3. ConcurrentUsersMiddleware
   - Gauge increment/decrement
   - Proper cleanup

### Phase 4: AutoMapper Profile Tests (Priority: MEDIUM)
Test all mappings in CareerServiceMappingProfile
1. JobPosting → JobPostingResponse (including Markdown rendering)
2. JobApplication → JobApplicationResponse (including external service lookups)
3. TrainingProgram → TrainingProgramResponse
4. EmployeeTrainingEnrollment → TrainingEnrollmentResponse
5. IndividualDevelopmentPlan → IDPResponse
6. EmployeeDevelopmentGoal → DevelopmentGoalResponse

### Phase 5: External Service Client Tests (Priority: LOW)
Mock HttpClient to test retry logic and error handling
1. EmployeeServiceClient
2. UploadServiceClient
3. CountryServiceClient
4. EmailServiceClient

### Phase 6: Integration Test Improvements (Priority: MEDIUM)
Make integration tests more resilient
1. Better Docker detection and skip logic
2. Retry mechanisms for Testcontainers startup
3. Parallel test execution safety
4. Better test data cleanup

## Implementation Priority

### Week 1: Critical Path
- ✅ Fix Docker/Testcontainers issues (or document workarounds)
- ✅ ApplicationService unit tests (most complex logic)
- ✅ JobPostingService unit tests
- ✅ All validator tests

### Week 2: Core Services
- ✅ DevelopmentPlanService unit tests
- ✅ EnrollmentService unit tests
- ✅ TrainingProgramService unit tests
- ✅ ReportService unit tests

### Week 3: Supporting Components
- ✅ Middleware tests
- ✅ AutoMapper profile tests
- ✅ External client tests
- ✅ Integration test improvements

## Success Criteria

### Minimum Acceptable Coverage
- **Service Logic**: 80%+ code coverage on all services
- **Validators**: 100% coverage (should be easy to achieve)
- **Middleware**: 80%+ coverage
- **AutoMapper**: 100% mapping verification

### Production Readiness Checklist
- [ ] All unit tests passing (target: 200+ tests)
- [ ] All integration tests passing OR documented Docker requirement
- [ ] All contract tests passing OR documented Docker requirement
- [ ] Zero compilation warnings
- [ ] All critical business logic covered by unit tests
- [ ] All validation rules tested
- [ ] All status transitions tested
- [ ] All error scenarios tested
- [ ] Performance tests for report generation
- [ ] Load tests for rate limiting

## Testing Best Practices

### Unit Testing
1. **Arrange-Act-Assert** pattern consistently
2. **One assertion per concept** (use FluentAssertions chaining)
3. **Mock external dependencies** (DbContext, HttpClient, IMemoryCache)
4. **Test both happy path and failure scenarios**
5. **Use descriptive test names** (e.g., `SubmitApplicationAsync_WhenDeadlinePassed_ThrowsInvalidOperationException`)

### Integration Testing
1. **Use Testcontainers** for real PostgreSQL (when Docker available)
2. **Clean database state** before each test class
3. **Use realistic test data**
4. **Test full request/response cycle**
5. **Verify database state changes**

### Contract Testing
1. **Verify OpenAPI schema compliance**
2. **Test all status codes**
3. **Validate response formats**
4. **Test authentication/authorization**

## Test Infrastructure Improvements Needed

1. **Better test data builders** (use Builder pattern for complex entities)
2. **Shared test fixtures** for common scenarios
3. **Docker health check** with graceful skip when unavailable
4. **Parallel execution** safety for integration tests
5. **Performance benchmarking** utilities

## Monitoring Test Health

### Metrics to Track
- **Test count**: Increase from 19 to 200+
- **Code coverage**: Target 80%+ overall
- **Test execution time**: Keep unit tests < 100ms each
- **Integration test stability**: 95%+ pass rate when Docker available
- **Flaky test rate**: 0% (zero tolerance for flaky tests)

### CI/CD Integration
- Run unit tests on every commit (fast feedback)
- Run integration tests on PR creation (requires Docker)
- Run contract tests before merge (requires Docker)
- Block merges if any tests fail
- Report coverage metrics in PR comments
