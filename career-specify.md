# CareerService Specification - Permission-Based Authorization Migration

## Permissions to Define

### Training Operations
```
career.trainings.create          - Create training programs
career.trainings.read            - Read training details
career.trainings.update          - Update training programs
career.trainings.delete          - Delete trainings
career.trainings.enroll          - Enroll in training
career.trainings.complete        - Mark training as completed
career.trainings.certify         - Issue training certifications
```

### Evaluation Operations
```
career.evaluations.create        - Create performance evaluations
career.evaluations.read          - Read evaluations
career.evaluations.submit        - Submit evaluations
career.evaluations.approve       - Approve evaluations
```

### Career Path Operations
```
career.paths.view                - View career paths
career.paths.create              - Create career paths
career.paths.assign              - Assign employees to paths
```

### Employee Development Operations
```
career.development.view-own      - View own development plan
career.development.view-team     - View team development plans
career.development.manage        - Manage development plans
```

## Predefined Roles

### career-admin
**Permissions**: All career.* permissions

### career-hr
**Permissions**: All except trainings.delete, paths.create

### career-manager
**Permissions**: evaluations.*, development.view-team, development.manage, trainings.read, trainings.enroll

### career-employee
**Permissions**: trainings.read, trainings.enroll, trainings.complete, evaluations.read (own), development.view-own, paths.view

## Success Criteria
- [ ] ~16 permissions registered
- [ ] 4 predefined roles registered
