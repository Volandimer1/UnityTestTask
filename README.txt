Unity Developer Test Assignment

Implemented so far

- Fixed the existing boost reset and save behavior: starting a new game clears the score, combo progress, grace moves, and booster together. Combo progress and consecutive non-clearing placements are now saved and restored, so restarting cannot restore the booster while resetting its grace-move allowance.
- Added a combo system for accepted placements. Clearing at least one region advances the combo; three consecutive placements without a clear break it. Rejected placements do not affect it.
- Connected the existing score booster to combo changes while preserving its activation threshold and score multiplier. Added an on-screen combo count with a short UI animation for continuing combos.

Architectural decisions

- ComboSystem is a plain C# class that owns only combo state and publishes changes. GameController creates it once for the scene; ScoreMediator translates the board's placement result into a successful or unsuccessful action. This follows the project's existing scene wiring without adding a framework.
- ScoreBoostSystem owns the boost threshold and reacts to combo changes. The combo system does not depend on scoring or UI, so other feedback can subscribe without changing its rules.
- Combo progress is stored with the existing score data in PlayerPrefs. Older saves that only have the booster flag still restore a compatible minimum combo state.

Assumptions

- The combo system uses the existing score boost rule. An accepted piece placement that clears at least one row, column, or 3x3 box increases the combo by one, even if it clears several regions.
- An accepted placement that clears nothing uses one grace move. The third consecutive non-clearing placement resets the combo; a clearing placement resets the grace-move count. An invalid drop snaps the piece back and does not change either count.
- The existing booster activates at combo 2 and keeps its current 1.5x score multiplier. Combo feedback reflects this behavior without changing the reward rule.
- For analytics, "moving / rearranging pieces" means one event when a piece drag ends, with an accepted or rejected outcome. The current game does not support rearranging a piece after it has been placed.
- For analytics, "receiving bonuses" means the score booster becoming active when the combo reaches its reward threshold.
- For analytics, "using power-ups" means using the existing Second Chance action. The inspected gameplay code does not expose a separate consumable power-up system.

What I would improve with more development time

- Save the board, figures, score, and combo as one recoverable game-state snapshot so an interrupted placement cannot leave those saves out of sync.
- If more systems need combo events, expose a read-only combo interface to observers while keeping action recording in the gameplay mediator.
