# Git Workflow

# FileLens

---

# Purpose

This document defines the Git workflow used throughout the FileLens project.

The goals are:

- Keep history clean
- Make commits meaningful
- Support long-term development
- Make collaboration easier
- Maintain a professional Git history

---

# Branch Strategy

The project uses a simplified Git Flow.

```
main
│
├── feature/*
│
├── fix/*
│
├── docs/*
│
└── refactor/*
```

---

## main

Always stable.

Every commit on **main** should build successfully.

Never commit experimental code directly to **main**.

---

## feature/

Used for new functionality.

Examples

```
feature/folder-scanner

feature/duplicate-detector

feature/ai-recommendation

feature/storage-visualization
```

---

## fix/

Used for bug fixes.

Examples

```
fix/scan-cancellation

fix/history-crash
```

---

## docs/

Documentation updates.

Examples

```
docs/prd-update

docs/engineering-spec

docs/readme
```

---

## refactor/

Internal improvements.

Examples

```
refactor/file-service

refactor/repository
```

---

# Commit Message Convention

Use Conventional Commits.

Format

```
type: short description
```

---

## docs

Documentation only.

Example

```
docs: update engineering specification
```

---

## feat

New feature.

Example

```
feat: implement recursive folder scanner
```

---

## fix

Bug fix.

Example

```
fix: resolve duplicate detection issue
```

---

## refactor

Internal improvements.

Example

```
refactor: simplify scan pipeline
```

---

## build

Project configuration.

Example

```
build: configure dependency injection
```

---

## test

Tests.

Example

```
test: add duplicate detector tests
```

---

## style

Formatting only.

Example

```
style: format markdown documentation
```

---

# Commit Rules

A commit should represent one logical change.

Good

```
feat: add folder scanner
```

Bad

```
feat: scanner + AI + database + README
```

Small commits are preferred.

---

# Commit Checklist

Before committing:

- Project builds successfully
- No unnecessary files
- Documentation updated if required
- No secrets committed
- No temporary files

---

# Pull Strategy

Always pull before pushing.

```
git pull
git push
```

---

# Tags

Future releases use Semantic Versioning.

Examples

```
v0.1.0

v0.2.0

v1.0.0
```

---

# Ignore Rules

Never commit

- API Keys
- Passwords
- Secrets
- Local databases
- Temporary files
- Personal configuration

---

# Release Workflow

Development

↓

Feature Complete

↓

Testing

↓

Release Tag

↓

GitHub Release

---

# Philosophy

Git history is part of the project.

Every commit should tell the story of how FileLens was built.

A clean Git history is as valuable as clean source code.

---

# First Commit Policy

The first commit should establish the project foundation.

It should include:

- Initial documentation
- Project structure
- Development guidelines

It should not include application source code.

Recommended first commit message:

```text
docs: initialize FileLens project documentation
```