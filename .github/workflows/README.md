# GitHub Actions Workflows

This directory contains GitHub Actions workflows for building, testing, and deploying the florentia application.

## Main Workflow

### aws.yml
The main CI/CD workflow that orchestrates the entire build, test, and deployment pipeline.

**Triggers:**
- Push to `feature/pipeline` branch
- Pull requests to `main`, `release/*`, and `releasecandidate/*` branches

**Jobs:**
1. **dotnet** - Build and test the .NET application
2. **sonar** - Run SonarCloud code quality analysis
3. **docker** - Build and push Docker images (only on PR merge)
4. **deploy-dev** - Deploy to AWS ECS development environment (only on PR merge)

## Reusable Workflows

### reusable-dotnet-build.yml
Builds and tests .NET applications.

**Inputs:**
- `dotnet-version`: .NET SDK version (default: '8.0.x')
- `configuration`: Build configuration (default: 'Release')
- `solution-path`: Path to solution file (default: '.')
- `run-tests`: Whether to run tests (default: true)
- `cache-key-suffix`: Suffix for cache keys (default: 'default')

**Features:**
- NuGet package caching
- Test result artifacts
- Configurable test execution

### reusable-sonar.yml
Performs SonarCloud static code analysis.

**Inputs:**
- `project-key`: SonarCloud project key (required)
- `organization`: SonarCloud organization (required)
- `additional-args`: Additional SonarCloud arguments (optional)
- `working-directory`: Working directory for SonarCloud analysis (default: '.')

**Secrets:**
- `SONAR_TOKEN`: SonarCloud authentication token

### reusable-docker-build.yml
Builds and pushes Docker images to Docker Hub and/or Amazon ECR.

**Inputs:**
- `context`: Build context path (default: '.')
- `dockerfile`: Path to Dockerfile (default: 'Dockerfile')
- `image-name`: Docker image name without registry (required)
- `image-tag`: Docker image tag (default: 'latest')
- `registry-type`: Target registry - 'ecr', 'dockerhub', or 'both' (default: 'dockerhub')
- `aws-region`: AWS region for ECR (default: 'us-east-2')
- `platforms`: Target platforms for multi-arch builds (default: 'linux/amd64')

**Secrets:**
- `DOCKERHUB_USERNAME`: Docker Hub username
- `DOCKERHUB_TOKEN`: Docker Hub access token
- `AWS_ACCESS_KEY_ID`: AWS access key
- `AWS_SECRET_ACCESS_KEY`: AWS secret key
- `AWS_ACCOUNT_ID`: AWS account ID
- `ECR_REPOSITORY`: ECR repository name

**Features:**
- Multi-platform builds support
- Docker layer caching with GitHub Actions cache
- Flexible registry configuration

### reusable-aws-deploy.yml
Deploys applications to AWS ECS.

**Inputs:**
- `environment`: Deployment environment - dev, qa, or prod (required)
- `aws-region`: AWS region (default: 'us-east-2')
- `wait-for-deployment`: Wait for deployment completion (default: true)
- `deployment-timeout`: Deployment timeout in minutes (default: '10')

**Secrets:**
- `AWS_ACCESS_KEY_ID`: AWS access key
- `AWS_SECRET_ACCESS_KEY`: AWS secret key
- `AWS_ECS_CLUSTER`: ECS cluster name
- `AWS_ECS_SERVICE`: ECS service name
- `TASK_DEFINITION_ARN`: Task definition ARN (optional)

**Features:**
- Automatic task definition retrieval if not provided
- Service stability waiting
- Deployment verification
- Detailed deployment status logging

## Configuration

### Required Secrets
The following secrets must be configured in your GitHub repository:

**SonarCloud:**
- `SONAR_TOKEN`: SonarCloud authentication token

**Docker Hub:**
- `DOCKERHUB_USERNAME`: Docker Hub username
- `DOCKERHUB_TOKEN`: Docker Hub access token

**AWS:**
- `AWS_ACCESS_KEY_ID`: AWS access key ID
- `AWS_SECRET_ACCESS_KEY`: AWS secret access key
- `AWS_ACCOUNT_ID`: AWS account ID
- `ECR_REPOSITORY`: ECR repository name
- `AWS_ECS_CLUSTER`: ECS cluster name
- `AWS_ECS_SERVICE`: ECS service name
- `TASK_DEFINITION_ARN`: (Optional) Task definition ARN

### Repository Variables
Optional variables can be configured:
- `DOCKERHUB_REPOSITORY`: Docker Hub repository name (default: 'openai-florentia')
- `AWS_REGION`: AWS region (default: 'us-east-2')

## Project-Specific Configuration

This workflow is configured for the **florentia** project:
- Solution: `florentia/florentia.sln`
- .NET version: 8.0.x
- Dockerfile: `florentia/florentia/Dockerfile`
- Docker context: `florentia`
- SonarCloud project: `PoliedroSoftware_openai`
- SonarCloud organization: `poliedro-software-sonarqube`

## Usage

### Running Builds
Builds automatically run on:
- Push to feature/pipeline branch
- Pull request events (opened, synchronize, reopened) targeting main, release/*, or releasecandidate/* branches

### Deployment
Deployment to development environment happens automatically when:
- A pull request targeting main, release/*, or releasecandidate/* branches is merged
- All build and quality checks pass

### Manual Workflow Triggers
To add manual trigger capability, add the following to aws.yml:
```yaml
on:
  workflow_dispatch:
```

## Concurrency
The workflow uses concurrency control to cancel in-progress runs when new commits are pushed to the same branch, saving CI/CD resources.

## Notes
- Tests are run during the dotnet job if test projects exist
- Docker build and deployment only run on merged pull requests
- SonarCloud analysis runs on all builds for quality tracking
- All workflows use the latest GitHub Actions (v4)
