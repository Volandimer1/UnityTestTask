# Instructions for Codex agents

These rules apply throughout this repository.

- Do not create commits or push changes. Leave all changes for the human owner to review and commit.
- Do not make Unity player or Android builds, build solution files, or run other build commands.
- Do not enter Unity Play mode or run gameplay validation or tests. The human owner performs that review and validation.
- Preserve existing gameplay mechanics and functionality when implementing new features. Do not change an existing rule unless the human owner explicitly approves the change.
- If a compile-time error check is needed, use Unity CLI only when it is available and can perform the check without making a build or entering Play mode. Do not use Unity MCP for this project. If Unity CLI is unavailable, leave the compile check to the human owner and state that it was not run.
- Report the exact scope and result of any permitted Unity CLI compile-time error check. Leave gameplay, build, and final acceptance claims to the human owner.
