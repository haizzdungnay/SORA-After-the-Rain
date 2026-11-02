# Contributing to SORA

SORA is an early source prototype owned by [Đinh Tuấn Dương](https://github.com/haizzdungnay). Read the [README](README.md), [GDD](Docs/GAME_DESIGN_DOCUMENT_v0.4.md), [architecture](Docs/ARCHITECTURE.md) and [agent instructions](AGENTS.md) first. Public source access does not imply a project-wide redistribution license.

## Before a change

- Search existing issues/PRs. Discuss broad design, art or dependency changes before implementation.
- Base code features on `develop` using `feature/<scope>` or fixes using `fix/<issue>`; submit a focused PR. Do not automatically merge into `main`.
- Keep Unity `6000.3.7f1`, URP `17.3.0` and Windows PC as the initial target.
- Preserve both protagonists' names, age-18 gameplay start and sibling relationship. Keep proposals distinct from agreed story decisions.

## Review expectations

State the problem, resulting behavior, changed files, architecture/logic, validation actually performed, remaining checks and risks. Use the PR template. Keep Unity `.meta` sidecars with assets and preserve GUIDs. Record provenance before adding art; configure Git LFS for suitable binaries first.

Run `python Tools/validate_project.py`. If Unity is available, run relevant EditMode tests and the manual checklist, and attach actual evidence. Mark unavailable checks **NOT RUN**. Visual changes require genuine Editor/standalone captures; documentation artwork must be labelled as such.

## Issues and sensitive information

Use the bug/feature templates. Include the Editor version, reproduction steps and relevant sanitized logs. Do not publish secrets, Unity licenses, personal save files, account credentials or proprietary third-party assets. For a potential security issue, share only a non-sensitive summary in a public issue and arrange a private reporting channel with the owner before sharing details.

More context: [AI collaboration and asset workflow](Docs/CONTRIBUTING_AND_AI.md).
