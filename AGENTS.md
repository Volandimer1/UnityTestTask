# Instructions for Codex agents

These rules apply throughout this repository.

- Do not create commits or push changes. Leave all changes for the human owner to review and commit.
- Do not make Unity player or Android builds, build solution files, or run other build commands.
- Do not enter Unity Play mode or run gameplay validation or tests. The human owner performs that review and validation.
- Preserve existing gameplay mechanics and functionality when implementing new features. Do not change an existing rule unless the human owner explicitly approves the change.
- Do not use Unity MCP for this project. Rider MCP tools may be used when needed, subject to the no-build, no-Play-mode, and no-test rules above.
- For compile-time error checks, use Unity CLI only if it can check without making a build or entering Play mode, or use Rider MCP diagnostics such as `get_file_problems`. Rider inspections are not a full Unity compilation.
- If no permitted check is available, leave validation to the human owner. Report the exact scope and result of any permitted check. Leave gameplay, build, and final acceptance claims to the human owner.
