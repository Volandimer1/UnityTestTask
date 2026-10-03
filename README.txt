Unity Developer Test Assignment

Assumptions

- The combo system will use the existing score boost rule. An accepted piece placement that clears at least one row, column, or 3x3 box increases the combo by one, even if it clears several regions.
- An accepted placement that clears nothing uses one grace move. The third consecutive non-clearing placement resets the combo; a clearing placement resets the grace-move count. An invalid drop snaps the piece back and does not change either count.
- The existing booster activates at combo 2 and keeps its current 1.5x score multiplier. New combo feedback will reflect this behavior without changing the reward rule.
- For analytics, "moving / rearranging pieces" means one event when a piece drag ends, with an accepted or rejected outcome. The current game does not support rearranging a piece after it has been placed.
- For analytics, "receiving bonuses" means the score booster becoming active when the combo reaches its reward threshold.
- For analytics, "using power-ups" means using the existing Second Chance action. The inspected gameplay code does not expose a separate consumable power-up system.
