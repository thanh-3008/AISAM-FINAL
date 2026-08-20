# Short-Form Ad Video Pattern Library — AISAM Context File

> **Supersedes** the previous "26 patterns" reference file. Audit confirmed the library actually contains **25 unique, fully-defined patterns** (verified 1:1 across the pattern definitions and the applicability matrix in the prior version). The "26" figure was a metadata error in three locations (document title, Part 1 header, Gemini system instruction) and has been corrected here rather than padded with an artificial 26th pattern. This file is self-contained: it does not require any prior version to be understood or used.
>
> Use this file as the **system context** sent with every Gemini (or equivalent) API call that generates a short-form (8–10s) ad video prompt for AISAM. It contains: a deterministic pattern-selection engine, an open-ended category system with a mandatory fallback path, all 25 patterns in a standardized 20-field schema, a compression engine for 8–10s runtimes, and a locked output contract.

---

## PART 1 — HOW THIS DOCUMENT IS USED

```
INPUT (from AISAM)
  product_name, product_description, product_category (optional),
  key_features, target_audience, target_platform, campaign_objective (optional),
  brand_tone, brand_info, cta, optional reference image
        ↓
Product Understanding  (Part 3, Step 1)
        ↓
Creative Analysis      (Part 3, Steps 2-4 + Creative Mechanism Analysis if needed)
        ↓
Pattern Selection       (Part 3, Steps 5-9 — uses Part 2 category system + Part 4 pattern library)
        ↓
Short-Form Script Architecture (Part 6 — compression into 2-3, or exceptionally 4, shots)
        ↓
Integrated Multimodal Video Prompt (Part 7 — locked output contract)
```

Pattern selection (which pattern, and why) is **always internal reasoning**. It is never printed in the final video-generation output. See Part 7 for the two separate schemas — one for internal logging, one for the actual video prompt.

---

## PART 2 — CROSS-CATEGORY SYSTEM

### 2.1 Primary Reference Categories (non-exhaustive)

Beauty · Food/Beverage · Fashion · Tech · Home Goods · Fitness/Wellness · Services/Apps · B2B

These are **ranking signals, not eligibility filters**. A product may score well on a pattern even with a blank/low Matrix cell if its Objective, Value, and Proof match strongly (see Part 3 scoring model). A product should never be told "no pattern fits" — Section 2.3 (fallback) exists specifically to prevent that outcome.

### 2.2 Category Affinity Matrix

Scale: **H** = strong native fit, **M** = viable secondary fit, blank = weak/rarely used natively (still usable via fallback logic, never disqualified).

| PATTERN_ID | Beauty | Food/Bev | Fashion | Tech | Home Goods | Fitness | Services/Apps | B2B |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| ugc_testimonial_arc | H | M | H | M | H | H | H | M |
| problem_solution_interrupt | H | M | M | H | H | H | H | H |
| unboxing_reveal | H | H | H | H | H | M | | |
| lifestyle_context_integration | H | H | H | M | H | H | M | |
| macro_detail_texture | H | H | H | H | H | M | | |
| dynamic_action_demo | M | H | H | H | M | H | M | |
| before_after_transformation | H | M | M | H | H | H | H | M |
| side_by_side_comparison | H | M | H | H | H | M | H | H |
| sensory_first_reaction | M | H | | | | M | | |
| try_on_haul_transition | H | | H | | M | | | |
| screen_recording_walkthrough | | | | H | | M | H | H |
| myth_vs_fact | H | M | | H | | H | H | H |
| overhead_process_build | M | H | | | H | M | | |
| result_first_payoff | H | M | M | H | H | H | H | M |
| founder_story_bts | H | H | H | | H | | H | H |
| grwm_routine | H | M | H | | H | H | | |
| skeptic_to_fan | H | M | | H | H | H | H | |
| proof_stack_social | H | | | H | H | H | H | H |
| pattern_interrupt_visual | H | H | H | H | H | H | H | H |
| curiosity_progressive_reveal | H | H | H | H | M | M | H | |
| scenario_specific_customization | H | | H | H | H | | H | H |
| mechanism_of_action_explainer | H | | | H | M | H | H | H |
| street_interview_vox_pop | M | H | H | M | | H | H | |
| timeline_tension_durability | | | H | H | H | H | M | H |
| subverted_expectation_reverse | H | M | H | H | M | H | H | H |

### 2.3 Extended Category Guidance (used to inform, not restrict, Matrix reasoning)

| Category | Example Products | Pattern Affinity | Creative Characteristics |
|---|---|---|---|
| Automotive/Vehicles | Cars, motorcycles, parts, accessories | dynamic_action_demo, timeline_tension_durability, mechanism_of_action_explainer | High-motion, performance/durability-led |
| Real Estate | Homes, listings, property services | lifestyle_context_integration, before_after_transformation, scenario_specific_customization | Space and lifestyle-led, aspirational |
| Travel/Tourism | Destinations, bookings, tours | lifestyle_context_integration, curiosity_progressive_reveal, result_first_payoff | Aspirational, visual-result led |
| Hospitality | Hotels, restaurants, venues | lifestyle_context_integration, unboxing_reveal, overhead_process_build | Sensory and ambience-led |
| Education | Courses, edtech, tutoring | myth_vs_fact, mechanism_of_action_explainer, ugc_testimonial_arc | Trust and education-led |
| Financial Services | Banking, fintech apps | screen_recording_walkthrough, proof_stack_social, myth_vs_fact | Clarity and trust-led |
| Insurance | Policies, coverage plans | problem_solution_interrupt, myth_vs_fact, proof_stack_social | Risk/trust-led |
| Entertainment | Media, streaming, content apps | pattern_interrupt_visual, curiosity_progressive_reveal, result_first_payoff | Novelty-led |
| Events | Tickets, experiences | lifestyle_context_integration, street_interview_vox_pop, result_first_payoff | Social proof and FOMO-led |
| Gaming | Games, gaming apps | dynamic_action_demo, pattern_interrupt_visual, curiosity_progressive_reveal | High-energy, novelty-led |
| Luxury | High-end goods | macro_detail_texture, lifestyle_context_integration, founder_story_bts | Status and craftsmanship-led |
| Jewelry/Accessories | Rings, watches, bags | macro_detail_texture, unboxing_reveal, try_on_haul_transition | Detail and status-led |
| Pets | Pet products | ugc_testimonial_arc, dynamic_action_demo, before_after_transformation | Emotional and demo-led |
| Baby/Parenting | Baby gear, parenting products | problem_solution_interrupt, ugc_testimonial_arc, before_after_transformation | Trust and relief-led |
| Healthcare/Medical Services | Clinics, medical devices, telehealth | mechanism_of_action_explainer, myth_vs_fact, proof_stack_social | Trust/education-led; extra claims caution |
| Professional Services | Consulting, legal, agencies | screen_recording_walkthrough, proof_stack_social, founder_story_bts | Trust-led |
| Local Businesses | Cafes, gyms, salons, shops | street_interview_vox_pop, ugc_testimonial_arc, lifestyle_context_integration | Community and social-proof led |
| Industrial/Manufacturing | Equipment, B2B hardware | mechanism_of_action_explainer, timeline_tension_durability, side_by_side_comparison | Technical/performance-led |
| Agriculture | Farming products, agtech | overhead_process_build, founder_story_bts, mechanism_of_action_explainer | Origin/process-led |

This list is intentionally not exhaustive and not merged into the core Matrix (to keep it maintainable). If a product doesn't fit here either, use the fallback below — it works for literally any product.

### 2.4 Unknown Category Fallback — Creative Mechanism Analysis

If the product's category doesn't clearly map to Section 2.1 or 2.3, **never return "no pattern fits."** Instead classify the product across five axes, then match against each pattern's `BEST_VALUE_TYPES`, `BEST_PROOF_METHODS`, and `HOOK_STRATEGY` fields in Part 4:

1. **PRODUCT_TYPE** — Physical product / Digital product / Service / SaaS-App / Experience / High-consideration product / Other
2. **PRIMARY_VALUE** — Visual result / Functional utility / Transformation / Sensory experience / Speed-convenience / Performance / Status-lifestyle / Trust-proof / Education-explanation / Entertainment-novelty
3. **PROOF_METHOD** — Demo / Before-After / Comparison / Test / User testimonial / Social proof / Mechanism explanation / Result showcase / Sensory reaction
4. **HOOK_OPPORTUNITY** — Problem / Unexpected visual / Curiosity / Strong result / Controversial-common myth / Sensory moment / Human reaction / Challenge-test
5. **CONTENT_CONTEXT** — Lifestyle / UGC / Product demo / Educational / Comparison / Story / Review / Performance test

This produces a candidate shortlist across all 25 patterns regardless of category, then feeds directly into the scoring model in Part 3.

---

## PART 3 — PATTERN SELECTION ENGINE

### 3.1 Selection Flow

```
STEP 1  — Understand the product (name, description, features, reference image if any)
STEP 2  — Identify the advertising objective (explicit input, or infer from product+CTA if absent)
STEP 3  — Identify primary value proposition (Section 2.4 axis 2)
STEP 4  — Identify best proof mechanism (Section 2.4 axis 3)
STEP 5  — Check product category against Section 2.1 / 2.3
STEP 6  — If category match exists: use the Matrix (2.2) as a ranking signal, not a filter
STEP 7  — If no category match: run Creative Mechanism Analysis (2.4) across all 25 patterns
STEP 8  — Score every remaining candidate (3.2), disqualify any whose AVOID_WHEN condition is met
STEP 9  — Select the single highest-scoring pattern
STEP 10 — Generate the compressed 8-10s script (Part 6) using that pattern
```

Category match is **never a hard filter** — it contributes at most 15% of the score (3.2). A product can legitimately be assigned a pattern rated blank/M in its own category if Objective/Value/Proof/Hook line up strongly and no AVOID_WHEN condition is triggered. Example: an iPhone camera-launch ad is "Tech" (mostly H-rated for macro_detail_texture, dynamic_action_demo, screen_recording_walkthrough), but if the objective is specifically "camera output quality," `result_first_payoff` or `macro_detail_texture` should outrank a generic Tech-H pattern like `screen_recording_walkthrough` because Value/Proof match dominates the score.

### 3.2 Pattern Selection Score

```
FINAL_PATTERN_SCORE =
  0.30 × Objective Match
+ 0.25 × Primary Value Match
+ 0.20 × Proof Method Match
+ 0.15 × Category Affinity
+ 0.10 × Hook Strength
```

Score each factor internally as **HIGH / MEDIUM / LOW** (no need for exact arithmetic — the weights exist to keep Category Affinity from ever dominating, and to make Objective the strongest single lever):

- **Objective Match** — does the pattern's `PRIMARY_OBJECTIVES` list contain or closely align with the campaign objective? Direct listing = HIGH. Adjacent objective (e.g., objective is "Conversion" and pattern lists "Direct Response") = MEDIUM. Unrelated = LOW.
- **Primary Value Match** — does the pattern's `BEST_VALUE_TYPES` align with the product's identified `PRIMARY_VALUE`? Same logic (HIGH/MEDIUM/LOW).
- **Proof Method Match** — does the pattern's `BEST_PROOF_METHODS` align with the product's identified `PROOF_METHOD`?
- **Category Affinity** — the Matrix cell (2.2) for the product's category: H → HIGH, M → MEDIUM, blank → LOW. If no category applies, treat as MEDIUM (neutral, never zero, never disqualifying) so absence of a category never penalizes a product below what its other scores earn.
- **Hook Strength** — does the pattern's `HOOK_STRATEGY` match the product's identified `HOOK_OPPORTUNITY`?

**Hard disqualifier:** regardless of score, if the product/context matches a pattern's `AVOID_WHEN` condition, drop that candidate from consideration entirely — do not merely down-rank it.

Rank remaining candidates qualitatively (mentally weight HIGH > MEDIUM > LOW per the percentages above) and select the top one. Only the final chosen `pattern_id` and a short internal reasoning object are ever produced (Part 7, Schema A) — never the scores themselves in the visible output.

### 3.3 Universal / Highly Portable Patterns

Use this list when Creative Mechanism Analysis still returns a weak or tied field (e.g., a genuinely novel product category with no strong Value/Proof signal), or as the default fallback of last resort:

- **pattern_interrupt_visual** — the primary universal fallback. Its hook mechanism (an unexpected visual/motion/contradiction) is entirely independent of category, sensory profile, or proof type — it can front-load attention for literally any product, then hand off to whatever proof mechanism fits. Condition to use: no other pattern scores clearly higher. Do not use when a more specific native pattern already scores HIGH across Objective+Value+Proof — the interrupt should not replace a pattern that already fits well, only fill genuine gaps.
- **problem_solution_interrupt** — portable because almost every product category has *some* relatable friction point; weak when the product is purely aspirational/status-driven with no real "problem" to dramatize.
- **result_first_payoff** — portable because "show the outcome first" works whenever the outcome is visually legible, regardless of category; weak when the result can't be shown credibly without setup.
- **scenario_specific_customization** — portable across any product with identifiable audience segments; weak for genuinely one-size-fits-all products, where forced specificity reads as inauthentic.
- **curiosity_progressive_reveal** — portable as an attention mechanism across categories; weak at 8-10s runtimes specifically if the reveal can't land within one compressed clue-beat (see Part 6).

Do not default to a single fallback for every unmatched product — check all five before picking, since they trade off differently against `AVOID_WHEN` conditions.

---

## PART 4 — PATTERN LIBRARY (25 patterns)

Each pattern uses the following fixed 20-field schema. `DEFAULT_SHOT_FLOW` describes the pattern at its natural (15-30s, 4-6 beat) length; `8_TO_10_SECOND_COMPRESSION` is the mandatory version for AISAM's short-form output — see Part 6 for the general compression rules this derives from.

---
**PATTERN_ID:** ugc_testimonial_arc
**PATTERN_NAME:** UGC Testimonial / Transformation Arc
**CORE_MECHANISM:** Peer-credibility narrative — a relatable creator states a specific struggle, discovers the product, and reports a measurable result, borrowing trust from lived experience rather than brand claims.
**PRIMARY_OBJECTIVES:** Trust Building, Social Proof, Conversion, Retargeting
**BEST_FOR:** beauty, fashion, home goods, fitness/wellness, services/apps
**SECONDARY_FIT:** food/beverage, tech, B2B (case-study framing)
**AVOID_WHEN:** the product's benefit isn't personally/individually experienced (pure infrastructure B2B with no individual user story); no credible "before" struggle exists.
**HOOK_STRATEGY:** Creator opens mid-sentence with a specific struggle, skepticism, or result claim in 1-2s ("I was stuck at the same weight for 3 months") — peer-level recognition lowers ad-defense filters.
**NARRATIVE_FLOW:** Struggle → context → discovery → use/reaction → measurable result → recommendation.
**DEFAULT_SHOT_FLOW:** Shot 1 talking-head states struggle/skepticism → Shot 2 brief context on the problem → Shot 3 product discovery mid-speech → Shot 4 use/application with natural reaction → Shot 5 specific measurable result → Shot 6 recommendation + CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) struggle statement direct to camera. Shot 2 (3-7s) product use + reaction (discovery implied, not shown separately). Shot 3 (7-10s) result statement + product held to camera as CTA.
**BEST_PROOF_METHODS:** User testimonial, Result showcase
**BEST_VALUE_TYPES:** Transformation, Trust/proof
**VISUAL_STYLE:** Handheld, unpolished, eye-level UGC, natural micro-shake.
**PACING:** Medium — 2-4s cuts, conversational rhythm.
**AUDIO_GUIDANCE:** Direct creator VO in the target audience's spoken language, natural ambient tone, captions essential for sound-off viewing.
**CTA_STYLE:** Personal recommendation delivered direct-to-camera.
**RISK_FLAGS:** Reads as scripted if the struggle is generic; result claims in health/beauty categories may need disclaimers.
**COMMON_FAILURES:** Generic praise with no specific struggle or number; overly polished delivery breaking the UGC illusion; result stated with no visual evidence.
**EXAMPLE_USE_CASES:** Skincare serum "3-week journey"; productivity app "I was drowning in tasks until..."; supplement energy-level testimonial.
---

---
**PATTERN_ID:** problem_solution_interrupt
**PATTERN_NAME:** Problem-Agitate-Solve (PAS) Direct
**CORE_MECHANISM:** Establishes a vivid frustration before naming the brand, positioning the product as the interrupt/fix.
**PRIMARY_OBJECTIVES:** Direct Response, Conversion, Product Awareness
**BEST_FOR:** beauty, tech, home goods, fitness/wellness, services/apps, B2B
**SECONDARY_FIT:** fashion (fit/comfort problems)
**AVOID_WHEN:** the product has no clear pain point to dramatize (purely aspirational/status products); the problem isn't relatable within 2s.
**HOOK_STRATEGY:** Vivid, hyper-specific frustration shown/stated in 0-2s, before any brand mention — forces immediate self-recognition.
**NARRATIVE_FLOW:** Problem → agitate → product-as-fix → resolution demo → payoff → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 problem moment in real context → Shot 2 agitate/consequence → Shot 3 product enters mid-action as the fix → Shot 4 rapid resolution demo → Shot 5 payoff reaction → Shot 6 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) problem moment. Shot 2 (3-7s) product enters + rapid resolution merged. Shot 3 (7-10s) payoff reaction + CTA.
**BEST_PROOF_METHODS:** Demo, Result showcase
**BEST_VALUE_TYPES:** Functional utility, Speed/convenience
**VISUAL_STYLE:** Fast cuts, handheld POV + static inserts, hard before/after contrast.
**PACING:** Fast, 1.5-3s cuts.
**AUDIO_GUIDANCE:** Conversational VO on the problem, SFX transition sting, music builds through the solution.
**CTA_STYLE:** Direct action prompt tied to the resolved state.
**RISK_FLAGS:** Feels gimmicky if the "problem" is manufactured or exaggerated past credibility.
**COMMON_FAILURES:** Opening with brand before the problem; agitation beat too long for the runtime.
**EXAMPLE_USE_CASES:** Stain remover on a shirt; cluttered inbox → app; cracked phone screen → protector.
---

---
**PATTERN_ID:** unboxing_reveal
**PATTERN_NAME:** Unboxing / Sensory Reveal
**CORE_MECHANISM:** Sensory anticipation loop via packaging opening, paying off with a tactile/visual reveal.
**PRIMARY_OBJECTIVES:** Product Awareness, Product Launch, Engagement
**BEST_FOR:** beauty, food/beverage, fashion, tech, home goods
**SECONDARY_FIT:** fitness/wellness (equipment/supplement drops)
**AVOID_WHEN:** the product has no meaningful packaging/reveal moment (pure digital/SaaS); the reveal was already shown elsewhere in the same campaign.
**HOOK_STRATEGY:** First hand-on-packaging movement, tear, or pour fills frame 0 — sensory curiosity and a dopamine-hit reveal.
**NARRATIVE_FLOW:** Package tease → open → reveal reaction → detail → first use → verdict.
**DEFAULT_SHOT_FLOW:** Shot 1 hands on sealed package/first tear → Shot 2 quick cut through packaging layers → Shot 3 product reveal with genuine reaction → Shot 4 macro texture/detail touch → Shot 5 first use/impression → Shot 6 verdict + CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) hands tearing/opening. Shot 2 (3-7s) reveal + macro texture merged. Shot 3 (7-10s) first-use impression + CTA.
**BEST_PROOF_METHODS:** Sensory reaction, Demo
**BEST_VALUE_TYPES:** Sensory experience, Status/lifestyle
**VISUAL_STYLE:** Close-up/macro on hands+product, fast cuts through packaging, slow on the reveal.
**PACING:** Fast into the reveal, then a deliberate slow beat at the payoff.
**AUDIO_GUIDANCE:** ASMR packaging sounds (crinkle, tape, click), minimal VO, little/no music.
**CTA_STYLE:** Product held toward the lens post-reveal.
**RISK_FLAGS:** Long dead time before the reveal kills retention at this runtime.
**COMMON_FAILURES:** Full product shown too early, undercutting the reveal payoff; reaction feels flat/staged.
**EXAMPLE_USE_CASES:** New lipstick shade; limited-edition sneaker drop; specialty coffee bag.
---

---
**PATTERN_ID:** lifestyle_context_integration
**PATTERN_NAME:** Lifestyle Product-in-Context
**CORE_MECHANISM:** Desire generated through natural in-context presence rather than direct claims.
**PRIMARY_OBJECTIVES:** Brand Story, Lifestyle Aspiration, Product Awareness
**BEST_FOR:** fashion, food/beverage, fitness/wellness, home goods, beauty, tech
**SECONDARY_FIT:** services/apps (daily-use moments)
**AVOID_WHEN:** the product's core value is a technical/functional spec that mood/context can't convey; the audience needs a hard proof point rather than a feeling.
**HOOK_STRATEGY:** Viewer dropped mid-scene into an aspirational or everyday moment where the product is already in natural use — desire through context, not claim.
**NARRATIVE_FLOW:** Scene establish → organic product entry → versatility cuts → emotional payoff → soft CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 lifestyle scene establishing context → Shot 2 product enters organically → Shot 3 short context cuts showing versatility/benefit in situ → Shot 4 emotional/social payoff → Shot 5 soft CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-4s) scene + product entering organically. Shot 2 (4-8s) benefit-in-situ. Shot 3 (8-10s) emotional payoff + soft CTA gesture. (2-shot variant: merge scene+entry, then payoff+CTA.)
**BEST_PROOF_METHODS:** Demo (implicit, situational)
**BEST_VALUE_TYPES:** Status/lifestyle, Sensory experience
**VISUAL_STYLE:** Handheld documentary/gimbal movement, natural light, 2-4s cuts.
**PACING:** Relaxed relative to other patterns.
**AUDIO_GUIDANCE:** Ambient sound + light/trending music, optional soft VO, no hard pitch.
**CTA_STYLE:** Implicit — product stays visible/in-hand as the scene ends, no hard-sell language.
**RISK_FLAGS:** Can blur into generic stock lifestyle footage that could sell any competing product.
**COMMON_FAILURES:** Isolated studio hero shot breaking the "lived-in" illusion; no product-specific detail ever shown.
**EXAMPLE_USE_CASES:** Jacket worn on a hike; snack bar eaten at a desk; smart-home device used in a real living room.
---

---
**PATTERN_ID:** macro_detail_texture
**PATTERN_NAME:** Macro Detail & Texture Showcase
**CORE_MECHANISM:** Tactile fascination via extreme close-up before any wide reveal, building material/quality perception.
**PRIMARY_OBJECTIVES:** Feature Showcase, Product Awareness
**BEST_FOR:** beauty, food/beverage, fashion, tech, home goods
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product has no distinctive texture/material/finish worth isolating; the category expects fast action over sensory dwell (e.g., high-energy fitness gear).
**HOOK_STRATEGY:** Extreme close-up of texture, liquid, or material fills frame 1 — visual fascination and tactile curiosity before any wider reveal.
**NARRATIVE_FLOW:** Macro texture → pull-back reveal → application detail → result → hero + CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 macro texture/material in motion → Shot 2 slow pull-back or orbit revealing full product → Shot 3 application/interaction detail → Shot 4 result on skin/surface/in-use → Shot 5 product hero + CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-4s) macro texture pulling back to reveal the product. Shot 2 (4-8s) application/interaction + result merged. Shot 3 (8-10s) hero shot + CTA. (2-shot variant: macro→reveal, then result+CTA.)
**BEST_PROOF_METHODS:** Sensory reaction, Result showcase
**BEST_VALUE_TYPES:** Sensory experience, Visual result
**VISUAL_STYLE:** Locked/slow macro moves, shallow depth of field, rim lighting.
**PACING:** Slow, deliberate, few cuts.
**AUDIO_GUIDANCE:** Amplified tactile SFX (drip, click, rustle), restrained/no music.
**CTA_STYLE:** Product hero shot as the closing frame; minimal spoken CTA needed.
**RISK_FLAGS:** Beautiful but non-committal if it never shows utility/benefit.
**COMMON_FAILURES:** Wide static catalog shots creeping back in; macro that never connects to a real benefit.
**EXAMPLE_USE_CASES:** Serum drop viscosity; fabric weave close-up; chocolate melt/snap.
---

---
**PATTERN_ID:** dynamic_action_demo
**PATTERN_NAME:** Dynamic Action / Motion Demonstration
**CORE_MECHANISM:** Proves capability by showing the product mid-motion delivering a visible output before naming it.
**PRIMARY_OBJECTIVES:** Feature Showcase, Trust Building (performance proof)
**BEST_FOR:** fitness/wellness, tech, fashion, food/beverage, home goods
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product's value isn't physically demonstrable in motion (a subscription service, an ingredient-only claim).
**HOOK_STRATEGY:** Product/person already mid-motion delivering visible output in frame 0-1.5s — proves capability before naming the product.
**NARRATIVE_FLOW:** Mid-action result → context → feature callouts → reaction → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 product mid-action delivering clear result → Shot 2 wider context of the action → Shot 3 quick feature callouts via cuts → Shot 4 reaction or secondary benefit → Shot 5 CTA with product still active.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) mid-action delivering result. Shot 2 (3-7s) wider context + feature callout merged. Shot 3 (7-10s) reaction + CTA with product still active.
**BEST_PROOF_METHODS:** Demo, Test
**BEST_VALUE_TYPES:** Performance, Functional utility
**VISUAL_STYLE:** Energetic handheld/tracking, dynamic angles, speed-ramp contrast.
**PACING:** Fast, 1-2.5s cuts.
**AUDIO_GUIDANCE:** High-energy music matched to action tempo, impact SFX, minimal VO.
**CTA_STYLE:** Product still mid-action as the CTA lands — momentum carries into the ask.
**RISK_FLAGS:** Random camera movement unrelated to the action undercuts the demo.
**COMMON_FAILURES:** Static/slow pacing mismatched to category energy; movement with no clear demonstrated result.
**EXAMPLE_USE_CASES:** Blender crushing ice; running shoe mid-stride; vacuum picking up debris.
---

---
**PATTERN_ID:** before_after_transformation
**PATTERN_NAME:** Before-After Transformation
**CORE_MECHANISM:** Opens a visual loop with a deficient "before" state and closes it with a matched "after," making change legible fast.
**PRIMARY_OBJECTIVES:** Transformation, Conversion, Trust Building
**BEST_FOR:** beauty, fitness/wellness, home goods, food/beverage, services/apps
**SECONDARY_FIT:** B2B (before/after metrics dashboards)
**AVOID_WHEN:** the result cannot be shown visually/objectively; no credible timeframe can be attached.
**HOOK_STRATEGY:** Clear "before" deficiency shown immediately, or the final state teased first — opens a loop the "after" pays off within 1-2s.
**NARRATIVE_FLOW:** Before → transition → application → after reveal → proof detail → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 before state in real context → Shot 2 transition/process indication → Shot 3 product application/action → Shot 4 after reveal, matched framing/lighting → Shot 5 close proof detail or timeframe → Shot 6 CTA.
**8_TO_10_SECOND_COMPRESSION:** MULTI_PROOF_REQUIRED — 4-shot exception eligible. Shot 1 (0-2.5s) before state. Shot 2 (2.5-5s) product application. Shot 3 (5-8s) after reveal, matched framing. Shot 4 (8-10s) proof detail/timeframe + CTA. 3-shot fallback: merge application into the after-reveal transition.
**BEST_PROOF_METHODS:** Before/After, Result showcase
**BEST_VALUE_TYPES:** Transformation, Visual result
**VISUAL_STYLE:** Matched framing/lighting across before/after, match cuts or wipes.
**PACING:** Medium, deliberate at the reveal beat.
**AUDIO_GUIDANCE:** VO narrating the change, transition whoosh/beat drop at the reveal.
**CTA_STYLE:** "See your own before/after" framing.
**RISK_FLAGS:** Mismatched lighting/angle destroys credibility; vague claims invite platform ad-policy rejection.
**COMMON_FAILURES:** No timeframe context; improvement claimed but not visually proven.
**EXAMPLE_USE_CASES:** Skin clarity over 4 weeks; messy room → organized; slow app load → instant.
---

---
**PATTERN_ID:** side_by_side_comparison
**PATTERN_NAME:** Side-by-Side / Split-Screen Comparison
**CORE_MECHANISM:** Frames a decision immediately by placing the product against an alternative under an identical test.
**PRIMARY_OBJECTIVES:** Comparison, Conversion, Trust Building
**BEST_FOR:** tech, beauty, fashion, home goods, B2B, fitness
**SECONDARY_FIT:** services/apps
**AVOID_WHEN:** no fair/credible comparison exists, or naming a competitor triggers legal/platform risk.
**HOOK_STRATEGY:** Two options (product vs. alternative/old-way) appear together in 1-2s with a "which wins" framing — immediate decision frame.
**NARRATIVE_FLOW:** Both items framed → same test applied → difference highlighted → winner declared → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 both items/states in frame with comparison claim → Shot 2 same test applied to both → Shot 3 visible difference highlight → Shot 4 winner declaration with reason → Shot 5 CTA.
**8_TO_10_SECOND_COMPRESSION:** MULTI_PROOF_REQUIRED — 4-shot exception eligible. Shot 1 (0-2.5s) both items + claim. Shot 2 (2.5-5s) test applied to both. Shot 3 (5-8s) difference highlight. Shot 4 (8-10s) winner + CTA. 3-shot fallback: merge test+highlight.
**BEST_PROOF_METHODS:** Comparison, Test
**BEST_VALUE_TYPES:** Performance, Trust/proof
**VISUAL_STYLE:** Split-screen or alternating cuts, matched angle/lighting/timing.
**PACING:** Medium, synchronized cuts between sides.
**AUDIO_GUIDANCE:** Direct VO explaining the difference, synced SFX per side.
**CTA_STYLE:** "Choose the winner" — product positioned as the clear pick.
**RISK_FLAGS:** Unfair test setup is a credibility and platform-policy risk.
**COMMON_FAILURES:** Superiority claimed with no visible proof; asymmetric test conditions.
**EXAMPLE_USE_CASES:** Old charger vs new charger speed; leather vs synthetic durability; two skincare routines side by side.
---

---
**PATTERN_ID:** sensory_first_reaction
**PATTERN_NAME:** Sensory First-Bite / First-Touch Reaction
**CORE_MECHANISM:** Mirror-neuron craving via extreme close-up of consumption/contact plus immediate authentic reaction.
**PRIMARY_OBJECTIVES:** Product Awareness, Engagement
**BEST_FOR:** food/beverage, beauty (texture), fitness/wellness (supplements)
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product isn't consumed/applied to the body (poor fit for hard goods, SaaS, B2B).
**HOOK_STRATEGY:** Extreme close-up of product entering mouth/skin, or the immediate facial reaction, fills 1-2s — mirror-neuron craving/curiosity.
**NARRATIVE_FLOW:** Product-to-mouth/skin or reaction → wider shot → sensation description → secondary benefit → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 product-to-mouth/skin or pure reaction close-up → Shot 2 wider shot of person + product → Shot 3 verbal/text description of sensation → Shot 4 secondary benefit/continued enjoyment → Shot 5 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) product-to-mouth/skin close-up. Shot 2 (3-7s) reaction + sensation description merged. Shot 3 (7-10s) secondary benefit + CTA.
**BEST_PROOF_METHODS:** Sensory reaction
**BEST_VALUE_TYPES:** Sensory experience
**VISUAL_STYLE:** Tight close-ups, handheld/push-in, rapid reaction cut.
**PACING:** Fast into contact, then a beat held on reaction.
**AUDIO_GUIDANCE:** Crunch/sip/pour ASMR dominant, authentic reaction sound.
**CTA_STYLE:** Enjoyment continues into the ask (product still in hand/being consumed).
**RISK_FLAGS:** Delayed product intro (talking setup first) kills the sensory-immediacy mechanism.
**COMMON_FAILURES:** Reaction reads as staged/exaggerated; sensation never gets specific.
**EXAMPLE_USE_CASES:** First sip of cold brew; bite of a sauce-loaded dish; taste-test of a protein bar.
---

---
**PATTERN_ID:** try_on_haul_transition
**PATTERN_NAME:** Try-On Haul Transition
**CORE_MECHANISM:** Rapid on-body transitions signal variety and honest fit/style feedback in quick succession.
**PRIMARY_OBJECTIVES:** Product Awareness, Engagement, Conversion
**BEST_FOR:** fashion, beauty (makeup), home goods (decor)
**SECONDARY_FIT:** —
**AVOID_WHEN:** single-SKU product with no variant/style range; product isn't worn/placed on a body or in a space.
**HOOK_STRATEGY:** Quick "just got this" statement or package reveal followed by immediate on-body/in-space transition within 2s.
**NARRATIVE_FLOW:** Package/pile intro → rapid changes → fit detail+reaction → versatility → verdict+CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 package/pile of items + excited intro → Shot 2 rapid outfit/product changes with transitions → Shot 3 fit/detail close-ups and honest reaction → Shot 4 styling versatility/multiple angles → Shot 5 keep/return verdict + CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) excited intro + first look. Shot 2 (3-7s) rapid transition to on-body/detail reaction. Shot 3 (7-10s) verdict + CTA. Cap at 2-3 looks max — a full haul does not fit this runtime.
**BEST_PROOF_METHODS:** Demo, Sensory reaction
**BEST_VALUE_TYPES:** Status/lifestyle, Sensory experience
**VISUAL_STYLE:** Full-body + detail mix, jump-cut transitions, vertical framing.
**PACING:** Fast, snap transitions.
**AUDIO_GUIDANCE:** Creator VO commentary, trending/upbeat audio, fabric/zip ASMR accents.
**CTA_STYLE:** "Keep" verdict stated directly to camera.
**RISK_FLAGS:** Cramming a full haul (4+ looks) into this runtime collapses into illegible sub-2s cuts.
**COMMON_FAILURES:** Static flat-lay only, no body/context movement; too many looks for the runtime.
**EXAMPLE_USE_CASES:** Three dress silhouettes in one clip; lipstick shade range on-lip; throw-pillow color options in a room.
---

---
**PATTERN_ID:** screen_recording_walkthrough
**PATTERN_NAME:** Screen-Recording App Walkthrough
**CORE_MECHANISM:** Shows the interface directly solving a visible before-state problem, proving functionality rather than describing it.
**PRIMARY_OBJECTIVES:** Feature Showcase, Education, Conversion
**BEST_FOR:** services/apps, B2B, tech (software features)
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product is physical and UI isn't the core value proposition; the interface is too complex to read clearly in a 2-3s cut.
**HOOK_STRATEGY:** Immediate screen recording of a clear before-state problem (messy dashboard, slow process) or a surprising output within 1-2s.
**NARRATIVE_FLOW:** Problem screen → interaction begins → step-by-step demo → transformed result → CTA overlay.
**DEFAULT_SHOT_FLOW:** Shot 1 problem screen/before UI state → Shot 2 app interaction begins → Shot 3 step-by-step feature demo with cursor/finger highlight → Shot 4 transformed result screen → Shot 5 download/CTA overlay.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) problem/before UI state. Shot 2 (3-7s) key interaction + feature demo merged. Shot 3 (7-10s) transformed result screen + CTA.
**BEST_PROOF_METHODS:** Demo, Mechanism explanation
**BEST_VALUE_TYPES:** Functional utility, Speed/convenience
**VISUAL_STYLE:** Clean screen capture, subtle zoom/pan on key UI, optional picture-in-picture face reaction.
**PACING:** Medium, one clear UI action per shot.
**AUDIO_GUIDANCE:** Clear VO per step, UI click/swoosh SFX, light/no music.
**CTA_STYLE:** Download/sign-up prompt tied to the just-shown result screen.
**RISK_FLAGS:** Depends on legible UI — use real captured footage or a static asset for on-screen interface text, never rely on the video-generation model to render legible UI text (see Part 5, Global Rule: No In-Engine Text).
**COMMON_FAILURES:** Pure talking-head feature list with no interface shown; UI too small/fast to parse.
**EXAMPLE_USE_CASES:** Expense-tracking app auto-categorizing a receipt; CRM dashboard going from cluttered to clean; design tool one-click export.
---

---
**PATTERN_ID:** myth_vs_fact
**PATTERN_NAME:** Myth-Bust / Myth vs Fact Education
**CORE_MECHANISM:** Cognitive-dissonance hook via a bold contrarian claim, resolved by positioning the product as the correct approach.
**PRIMARY_OBJECTIVES:** Education, Trust Building, Product Awareness
**BEST_FOR:** beauty, fitness/wellness, B2B, tech, services/apps
**SECONDARY_FIT:** food/beverage (nutrition myths)
**AVOID_WHEN:** no genuine, defensible misconception exists to correct; the claim could be seen as misleading or unverified.
**HOOK_STRATEGY:** Bold contrarian statement or named misconception in 1-2s — cognitive dissonance, positions the brand as authority.
**NARRATIVE_FLOW:** Myth stated → fact revealed → product as correct approach → proof/mechanism → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 myth statement on screen/spoken → Shot 2 fact reveal/correction → Shot 3 product introduced as the correct approach → Shot 4 proof or mechanism explanation → Shot 5 CTA framed as the smarter choice.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) myth statement. Shot 2 (3-7s) fact + product-as-correct-approach merged. Shot 3 (7-10s) proof + CTA.
**BEST_PROOF_METHODS:** Mechanism explanation, Demo
**BEST_VALUE_TYPES:** Education/explanation, Trust/proof
**VISUAL_STYLE:** Talking-head with B-roll inserts, confident framing.
**PACING:** Medium, punchy on the myth/fact contrast.
**AUDIO_GUIDANCE:** Authoritative/energetic VO, minimal music so the claim lands clearly.
**CTA_STYLE:** "The smarter choice" framing.
**RISK_FLAGS:** Must never present the myth as true in a way that could be clipped out of context; substantiate factual claims.
**COMMON_FAILURES:** Staying vague instead of a clear wrong-vs-right contrast; no real mechanism behind the "fact."
**EXAMPLE_USE_CASES:** "You don't need 10 skincare steps"; "More meetings ≠ more progress" for a project-management tool; "Rest days don't make you weaker."
---

---
**PATTERN_ID:** overhead_process_build
**PATTERN_NAME:** Overhead Process Build
**CORE_MECHANISM:** Hypnotic top-down process footage builds curiosity through sequential assembly/transformation.
**PRIMARY_OBJECTIVES:** Product Awareness, Engagement
**BEST_FOR:** food/beverage, home goods, beauty (routine), fitness (prep)
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product has no visual step-by-step process (single-action products, services, apps).
**HOOK_STRATEGY:** Top-down view of ingredients/tools already in motion (pour, mix, assemble) in second 1 — hypnotic process curiosity.
**NARRATIVE_FLOW:** Overhead start → sequential steps → transformation midpoint → finished reveal → branding+CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 overhead process start with key action → Shot 2 sequential steps with text labels → Shot 3 transformation midpoint → Shot 4 finished result reveal → Shot 5 branding + CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) overhead process start. Shot 2 (3-7s) key steps compressed into one continuous take. Shot 3 (7-10s) finished result + CTA.
**BEST_PROOF_METHODS:** Demo, Result showcase
**BEST_VALUE_TYPES:** Sensory experience, Functional utility
**VISUAL_STYLE:** Locked/slow overhead or flat-lay, high-contrast lighting.
**PACING:** Steady, satisfying rhythm — do not rush the process beats.
**AUDIO_GUIDANCE:** Process sounds (sizzle, pour, mix) dominant, light rhythmic music, optional short VO.
**CTA_STYLE:** Branding beat as the process completes.
**RISK_FLAGS:** A talking-head recipe intro before the process starts kills the hypnotic hook.
**COMMON_FAILURES:** Steps too fast to register in a compressed cut; process shown with no product-specific detail.
**EXAMPLE_USE_CASES:** Iced-coffee build; skincare-routine layering; meal-prep container assembly.
---

---
**PATTERN_ID:** result_first_payoff
**PATTERN_NAME:** Result-First Payoff / Value-Demo-First
**CORE_MECHANISM:** Leads with the finished desirable outcome before any setup, maximizing perceived value within the 3-second scroll window.
**PRIMARY_OBJECTIVES:** Conversion, Product Awareness, Curiosity
**BEST_FOR:** beauty, fitness/wellness, tech, home goods, services/apps
**SECONDARY_FIT:** fashion, food/beverage
**AVOID_WHEN:** the "result" isn't visually striking or credible on its own without context; the product's value is process-dependent and confusing without setup.
**HOOK_STRATEGY:** The finished desirable outcome appears in frame 1, then the video shows how it was achieved — no logo, no intro, immediate value.
**NARRATIVE_FLOW:** Result first → "here's how" → condensed process → return to result+proof → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 pure result/hero outcome visual → Shot 2 brief "here's how" transition → Shot 3 condensed process/product use → Shot 4 return to result with social proof → Shot 5 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) pure result/hero outcome. Shot 2 (3-7s) condensed "how"/product use. Shot 3 (7-10s) return-to-result + CTA.
**BEST_PROOF_METHODS:** Result showcase, Demo
**BEST_VALUE_TYPES:** Visual result, Transformation
**VISUAL_STYLE:** Clean/high-production result shot first, faster cuts through process.
**PACING:** Strong open, accelerates through the "how," settles at the close.
**AUDIO_GUIDANCE:** Satisfying SFX/music sting on open, VO explaining the path, upbeat close.
**CTA_STYLE:** Direct, riding the momentum of the result shot.
**RISK_FLAGS:** If the opening result isn't immediately legible as caused by the product, viewers may not connect payoff to brand.
**COMMON_FAILURES:** Process-heavy opens that bury the benefit; brand intro placed before the result.
**EXAMPLE_USE_CASES:** Glossy finished hairstyle before showing the tool; a spotless kitchen before showing the cleaner; a smooth dashboard before showing the software.
---

---
**PATTERN_ID:** founder_story_bts
**PATTERN_NAME:** Founder Story / Behind-the-Scenes
**CORE_MECHANISM:** Humanizes the brand through founder-led authenticity and origin narrative to build durable trust.
**PRIMARY_OBJECTIVES:** Trust Building, Brand Story
**BEST_FOR:** beauty, food/beverage, home goods, fashion, services/apps, B2B
**SECONDARY_FIT:** —
**AVOID_WHEN:** the runtime is 8-10s and the founder's opening line isn't itself a strong hook — origin stories need setup time this format rarely allows.
**HOOK_STRATEGY:** Founder on-camera or BTS creation footage opens — humanizes the brand, builds trust through authenticity.
**NARRATIVE_FLOW:** Founder/BTS open → problem that sparked creation → development moment → product in use/result → CTA with founder recommendation.
**DEFAULT_SHOT_FLOW:** Shot 1 founder on-camera or BTS of product being made → Shot 2 problem founder experienced that led to the product → Shot 3 development/creation moment → Shot 4 product in use or result → Shot 5 CTA with founder recommendation.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3.5s) founder direct-to-camera stating the origin problem as the hook itself. Shot 2 (3.5-7s) product/result. Shot 3 (7-10s) founder CTA/recommendation. Reserve for campaigns where the founder story is the primary creative asset, not every SKU launch.
**BEST_PROOF_METHODS:** User testimonial (founder-as-user), Mechanism explanation
**BEST_VALUE_TYPES:** Trust/proof, Status/lifestyle
**VISUAL_STYLE:** Handheld/static phone-camera feel, natural lighting, unpolished.
**PACING:** Medium, personal/conversational.
**AUDIO_GUIDANCE:** Founder VO or direct address, natural ambient tone, minimal music.
**CTA_STYLE:** Personal recommendation, first-person.
**RISK_FLAGS:** Highest risk of any pattern for feeling flat/corporate if not genuinely condensed.
**COMMON_FAILURES:** Overly polished/scripted delivery; origin story with no clear hook line in the first 2s.
**EXAMPLE_USE_CASES:** "I built this because I couldn't find..."; small-batch food brand's kitchen origin; app founder's frustration with existing tools.
---

---
**PATTERN_ID:** grwm_routine
**PATTERN_NAME:** GRWM (Get Ready With Me) / Routine Walkthrough
**CORE_MECHANISM:** Rides a recognizable routine format so product integration feels expected, not inserted.
**PRIMARY_OBJECTIVES:** Product Awareness, Lifestyle Aspiration, Engagement
**BEST_FOR:** beauty, fashion, fitness/wellness, home goods, food/beverage
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product isn't a natural "step" in a recognizable routine; the runtime is too short to establish routine context credibly.
**HOOK_STRATEGY:** Creator starts a recognizable routine (morning, workout prep, meal prep) — viewer expects the full process and natural product integration.
**NARRATIVE_FLOW:** Routine starts → product as a step → application/use → benefit of that step → continuation/final look → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 creator starting routine → Shot 2 product introduced as a step → Shot 3 application/use in context → Shot 4 result/benefit of that step → Shot 5 routine continuation/final look → Shot 6 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) routine start with product introduced immediately as the hook step (skip a generic routine open). Shot 2 (3-7s) application + benefit merged. Shot 3 (7-10s) final look/result + CTA.
**BEST_PROOF_METHODS:** Demo, Result showcase
**BEST_VALUE_TYPES:** Status/lifestyle, Functional utility
**VISUAL_STYLE:** Handheld/static phone-camera, natural lighting, eye-level.
**PACING:** Medium, step-by-step rhythm.
**AUDIO_GUIDANCE:** Creator VO explaining steps, light background music, captions for mute viewing.
**CTA_STYLE:** Routine continues past the ask, implying habitual use.
**RISK_FLAGS:** At this runtime, showing only one step out of context can read as an isolated product shot rather than a "routine."
**COMMON_FAILURES:** Product placed with no routine context; skipping the step's actual benefit.
**EXAMPLE_USE_CASES:** Morning skincare step; pre-workout supplement scoop; Sunday meal-prep container fill.
---

---
**PATTERN_ID:** skeptic_to_fan
**PATTERN_NAME:** Skeptic-to-Fan Transformation
**CORE_MECHANISM:** Opens with credible skepticism to earn trust before the pivot to genuine endorsement.
**PRIMARY_OBJECTIVES:** Trust Building, Conversion, Comparison
**BEST_FOR:** beauty, fitness/wellness, tech, services/apps, home goods, food/beverage
**SECONDARY_FIT:** —
**AVOID_WHEN:** the category has no credible "tried alternatives" narrative (novel/first-of-kind products); skepticism can't resolve to a specific measurable pivot within the runtime.
**HOOK_STRATEGY:** Opens with skepticism ("I tried four others first") — builds credibility through honesty before the pivot.
**NARRATIVE_FLOW:** Skepticism/failed attempts → why skeptical → discovery → turning point → measurable outcome → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 creator states skepticism/failed attempts → Shot 2 context on why they were skeptical → Shot 3 product discovery moment → Shot 4 turning point/first positive result → Shot 5 specific measurable outcome → Shot 6 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) skepticism statement. Shot 2 (3-7s) discovery + turning point merged. Shot 3 (7-10s) measurable outcome + CTA.
**BEST_PROOF_METHODS:** User testimonial, Comparison
**BEST_VALUE_TYPES:** Trust/proof, Transformation
**VISUAL_STYLE:** Handheld/static phone-camera, authentic UGC aesthetic.
**PACING:** Medium, 2-3s cuts.
**AUDIO_GUIDANCE:** Conversational VO, minimal music, captions essential.
**CTA_STYLE:** Earned, credible recommendation after the pivot.
**RISK_FLAGS:** Fake enthusiasm/instant conversion undermines the entire mechanism.
**COMMON_FAILURES:** No credible skepticism beat; pivot happens with no specific trigger shown.
**EXAMPLE_USE_CASES:** "I tried 4 other mattresses first..."; "I was sure this app was just hype..."; "I didn't believe a cream could do this..."
---

---
**PATTERN_ID:** proof_stack_social
**PATTERN_NAME:** Proof Stack / Social Proof Montage
**CORE_MECHANISM:** Rapid-fire stacking of stats and multiple customer voices builds credibility through volume and variety of proof.
**PRIMARY_OBJECTIVES:** Social Proof, Trust Building, Conversion
**BEST_FOR:** services/apps, B2B, tech, beauty, fitness/wellness, home goods
**SECONDARY_FIT:** —
**AVOID_WHEN:** no real stat/testimonial volume exists yet (early-stage/pre-launch products); each voice can't be made visually distinct within a compressed runtime.
**HOOK_STRATEGY:** Opens with a stat/headline ("10,000+ users", "4.9 stars") or one striking customer reaction, then stacks more voices — credibility fast.
**NARRATIVE_FLOW:** Social proof headline/stat or strongest reaction → second voice → third voice → product context → recurring theme → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 social proof headline/stat or strongest customer reaction → Shot 2 second customer/use case → Shot 3 third customer/use case → Shot 4 product shown in context → Shot 5 recurring benefit/theme → Shot 6 CTA.
**8_TO_10_SECOND_COMPRESSION:** MULTI_PROOF_REQUIRED — 4-shot exception eligible. Shot 1 (0-2.5s) stat/headline or strongest reaction. Shot 2 (2.5-5s) second voice. Shot 3 (5-7.5s) third voice/product context merged. Shot 4 (7.5-10s) recurring theme + CTA. 3-shot fallback: compress to two voices only.
**BEST_PROOF_METHODS:** Social proof, User testimonial
**BEST_VALUE_TYPES:** Trust/proof
**VISUAL_STYLE:** Mix of selfies, customer clips, screenshots, product footage; short, visually varied clips.
**PACING:** Fast, snappy — each voice under 2s at this runtime.
**AUDIO_GUIDANCE:** Natural voices stitched together, light music, varied delivery per voice.
**CTA_STYLE:** Momentum-driven, closing on the theme all voices share.
**RISK_FLAGS:** Unsupported/vague stats create ad-policy and credibility risk.
**COMMON_FAILURES:** Repeating generic praise across voices with no specifics; stat with no source context.
**EXAMPLE_USE_CASES:** "10,000+ users" + two rapid testimonial cuts; star-rating stat + review screenshots; multiple gym-goers on one supplement.
---

---
**PATTERN_ID:** pattern_interrupt_visual
**PATTERN_NAME:** Universal Pattern Interrupt Open
**CORE_MECHANISM:** Breaks scroll rhythm with an unexpected visual/motion/contradiction before bridging into product context — a universal hook mechanism usable across nearly any category (see Part 3.3, primary universal fallback).
**PRIMARY_OBJECTIVES:** Product Awareness, Engagement, Curiosity
**BEST_FOR:** all categories (universal)
**SECONDARY_FIT:** default fallback whenever no other pattern scores clearly higher (Part 3.3)
**AVOID_WHEN:** the interrupt can't be bridged into real product value within 1-2 shots — a shocking hook with no payoff hurts conversion.
**HOOK_STRATEGY:** Opens with an unexpected visual, abrupt motion, or contradiction to category norm that breaks scrolling rhythm.
**NARRATIVE_FLOW:** Interrupt → reveal context/problem → product intro/demo → proof/benefit → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 pattern interrupt (unexpected visual/motion/statement) → Shot 2 reveal of product context or problem → Shot 3 product introduced/demo → Shot 4 proof or benefit → Shot 5 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-2.5s) pattern interrupt. Shot 2 (2.5-6.5s) product reveal + demo merged. Shot 3 (6.5-10s) proof/benefit + CTA.
**BEST_PROOF_METHODS:** Any — this pattern defines the hook only; proof method should be chosen based on the underlying product's own best-fit proof per Creative Mechanism Analysis.
**BEST_VALUE_TYPES:** Entertainment/novelty (hook) combined with the product's actual primary value (payoff).
**VISUAL_STYLE:** Varies by interrupt type — fast zoom, snap cut, abrupt setting change, or striking static image.
**PACING:** Very fast at the open (1-2s cuts through the hook), then settles into the product's natural pacing.
**AUDIO_GUIDANCE:** SFX on the interrupt moment (whoosh, snap, drop), music/VO after the hook.
**CTA_STYLE:** Standard, product-appropriate — this pattern's job is the open, not the close.
**RISK_FLAGS:** Generic/predictable interrupts lose their power fast across a content library; overuse in the same feed reduces novelty.
**COMMON_FAILURES:** Interrupt never bridges to product value; interrupt feels random/unrelated even after the bridge.
**EXAMPLE_USE_CASES:** Used as the primary universal fallback for any product/category with no clear native pattern fit — see Part 3.3.
---

---
**PATTERN_ID:** curiosity_progressive_reveal
**PATTERN_NAME:** Curiosity → Progressive Reveal
**CORE_MECHANISM:** Delays explanation just long enough to create an open loop, releasing information incrementally to sustain watch time.
**PRIMARY_OBJECTIVES:** Curiosity, Engagement, Education
**BEST_FOR:** beauty, tech, fashion, food/beverage, home goods, services/apps
**SECONDARY_FIT:** B2B (mechanism curiosity)
**AVOID_WHEN:** withheld information would confuse rather than intrigue within the compressed runtime.
**HOOK_STRATEGY:** Intriguing visual, unusual action, or unexplained situation opens; explanation is delayed just long enough to create a reason to keep watching.
**NARRATIVE_FLOW:** Unexplained visual → clue → partial reveal → mechanism/use revealed → payoff → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 unexplained visual/question → Shot 2 additional clue → Shot 3 product/context partially revealed → Shot 4 mechanism/use revealed → Shot 5 payoff → Shot 6 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) unexplained visual/question. Shot 2 (3-7s) reveal + mechanism merged — only one clue beat survives compression, not two. Shot 3 (7-10s) payoff + CTA.
**BEST_PROOF_METHODS:** Mechanism explanation, Demo
**BEST_VALUE_TYPES:** Education/explanation, Entertainment/novelty
**VISUAL_STYLE:** Unusual close-up/composition opening, gradually widening.
**PACING:** Medium; limit to one open loop at this runtime, not several.
**AUDIO_GUIDANCE:** Suspenseful/curiosity-building sound, short VO questions, payoff sound/music.
**CTA_STYLE:** Arrives immediately at the payoff beat, no extra delay.
**RISK_FLAGS:** At 8-10s, withholding too long confuses rather than intrigues — reserve the full multi-clue version for longer formats.
**COMMON_FAILURES:** Payoff arrives too late for the runtime; mystery never resolves clearly.
**EXAMPLE_USE_CASES:** Odd-looking device close-up before revealing its use; a sealed container before revealing contents; an unusual gesture before revealing what it activates.
---

---
**PATTERN_ID:** scenario_specific_customization
**PATTERN_NAME:** Scenario → Personalized Fit
**CORE_MECHANISM:** Opens on a hyper-specific person/situation so the target viewer immediately self-identifies, then shows the product tailored to that exact need.
**PRIMARY_OBJECTIVES:** Conversion, Retargeting, Product Awareness
**BEST_FOR:** fashion, beauty, services/apps, B2B, home goods
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product is genuinely one-size-fits-all with no meaningful customization/segment story — forcing false specificity reads as inauthentic.
**HOOK_STRATEGY:** Opens on a highly specific person/situation/constraint so the target viewer immediately recognizes themselves.
**NARRATIVE_FLOW:** Specific persona/situation → unique need/constraint → customized product/service → result → proof/detail → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 specific persona/situation → Shot 2 unique need or constraint → Shot 3 product/service customized to that situation → Shot 4 result → Shot 5 proof/detail → Shot 6 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) specific persona/situation + constraint. Shot 2 (3-7s) customized product/result merged. Shot 3 (7-10s) proof + CTA.
**BEST_PROOF_METHODS:** Demo, Result showcase
**BEST_VALUE_TYPES:** Functional utility, Status/lifestyle
**VISUAL_STYLE:** Contextual medium shots + detail shots showing the customization, not studio footage.
**PACING:** Medium, situation-driven.
**AUDIO_GUIDANCE:** Persona-specific dialogue/VO reflecting the target user's exact situation and language register.
**CTA_STYLE:** "Made for you" — specific, not generic.
**RISK_FLAGS:** Broad "this is for everyone" messaging defeats the entire mechanism.
**COMMON_FAILURES:** Persona too generic to trigger self-recognition; customization claimed but not visually shown.
**EXAMPLE_USE_CASES:** "For side-sleepers specifically..." pillow ad; "For solo founders juggling 5 tools..." SaaS ad; petite-fit clothing line.
---

---
**PATTERN_ID:** mechanism_of_action_explainer
**PATTERN_NAME:** Mechanism of Action (MoA) Explainer
**CORE_MECHANISM:** Makes an invisible process concrete fast via a clean cross-section/schematic visualization of how the product actually works.
**PRIMARY_OBJECTIVES:** Education, Trust Building, Feature Showcase
**BEST_FOR:** B2B, tech, beauty, fitness/wellness, services/apps
**SECONDARY_FIT:** —
**AVOID_WHEN:** the mechanism is genuinely simple/self-evident (over-explaining trivial products feels condescending); abstract visualization would obscure the practical benefit.
**HOOK_STRATEGY:** Visualizes an invisible process or complex workflow within 2s using a clean cross-section/schematic view — makes the abstract concrete fast.
**NARRATIVE_FLOW:** Cross-section/problem → animated flow of solution → pull-back to real interface → benefit → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 cross-section/schematic view showing the internal friction/problem → Shot 2 animated flow of the solution moving through the system → Shot 3 pull-back to the real-world interface or polished exterior → Shot 4 benefit statement → Shot 5 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3.5s) cross-section/schematic showing the internal friction. Shot 2 (3.5-7s) animated flow of solution through the system. Shot 3 (7-10s) pull-back to real product/benefit + CTA.
**BEST_PROOF_METHODS:** Mechanism explanation
**BEST_VALUE_TYPES:** Education/explanation, Performance
**VISUAL_STYLE:** Smooth orbit/slow crane descending into the internal core.
**PACING:** Calm, controlled — should not feel rushed even when compressed.
**AUDIO_GUIDANCE:** Calm, authoritative explanatory VO, subtle futuristic UI audio cues.
**CTA_STYLE:** Benefit-led, following naturally from the explained mechanism.
**RISK_FLAGS:** Overly abstract visuals that never connect to a pragmatic benefit read as impressive but unpersuasive.
**COMMON_FAILURES:** Technical visuals with no plain-language payoff; schematic too detailed to parse in a fast cut.
**EXAMPLE_USE_CASES:** How a filter removes contaminants; how an algorithm matches candidates to jobs; how a compression fabric supports muscles.
---

---
**PATTERN_ID:** street_interview_vox_pop
**PATTERN_NAME:** Street Interview Vox-Pop Reactions
**CORE_MECHANISM:** Raw, unscripted public-setting reactions borrow authenticity and social energy from real strangers testing the product live.
**PRIMARY_OBJECTIVES:** Social Proof, Engagement, Trust Building
**BEST_FOR:** services/apps, food/beverage, fashion, fitness/wellness, tech
**SECONDARY_FIT:** —
**AVOID_WHEN:** the product can't be meaningfully tested/reacted-to in a brief public interaction; the brand needs a controlled, on-message result (this format is inherently unpredictable).
**HOOK_STRATEGY:** Opens mid-conversation with an interviewer asking a provocative question in a public setting — raw, unscripted energy.
**NARRATIVE_FLOW:** Provocative question → unfiltered reaction/test → group consensus → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 interviewer asks a fast-paced question on a busy street → Shot 2 unfiltered passerby reaction testing/inspecting the product → Shot 3 group laugh or consensus statement confirming value → Shot 4 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) interviewer question on a busy street. Shot 2 (3-7s) passerby reaction/test. Shot 3 (7-10s) consensus statement + CTA.
**BEST_PROOF_METHODS:** Social proof, User testimonial
**BEST_VALUE_TYPES:** Trust/proof, Entertainment/novelty
**VISUAL_STYLE:** Handheld, natural zoom/pans between interviewer and interviewee, raw ambient lighting.
**PACING:** Medium-fast, conversational but public-energy.
**AUDIO_GUIDANCE:** Street ambience mixed with clear vocal capture — authentic broadcast feel.
**CTA_STYLE:** Group-consensus energy carries into the ask.
**RISK_FLAGS:** Rehearsed-sounding responses or studio-cleaned audio break the "real street" illusion immediately.
**COMMON_FAILURES:** Reactions feel scripted; the question isn't provocative enough to generate genuine, watchable responses.
**EXAMPLE_USE_CASES:** "Guess the price" on a product; blind taste-test reactions; "try this app right now" street challenge.
---

---
**PATTERN_ID:** timeline_tension_durability
**PATTERN_NAME:** Timeline Tension / Stress Test
**CORE_MECHANISM:** Establishes narrative tension through a visible countdown or durability milestone, resolving with proof the product withstood it.
**PRIMARY_OBJECTIVES:** Trust Building, Comparison, Feature Showcase (performance proof)
**BEST_FOR:** tech, home goods, fashion, fitness/wellness, B2B
**SECONDARY_FIT:** —
**AVOID_WHEN:** durability/endurance isn't a genuine selling point; the stress test can't be shown credibly without looking staged.
**HOOK_STRATEGY:** Visual countdown or durability milestone ("30 days straight") establishes narrative tension immediately.
**NARRATIVE_FLOW:** Stress-test setup → fast-forward under strain → close-up inspection → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 stress test/time-lapse setup mid-action → Shot 2 fast-forward sequence under continuous strain → Shot 3 close-up inspection revealing unblemished condition → Shot 4 CTA.
**8_TO_10_SECOND_COMPRESSION:** MULTI_PROOF_REQUIRED — 4-shot exception eligible. Shot 1 (0-2.5s) stress test/time-lapse setup. Shot 2 (2.5-6s) fast-forward under continuous strain. Shot 3 (6-8.5s) close-up inspection revealing unblemished condition. Shot 4 (8.5-10s) CTA. 3-shot fallback: merge setup+fast-forward into one time-lapse shot.
**BEST_PROOF_METHODS:** Test, Result showcase
**BEST_VALUE_TYPES:** Performance, Trust/proof
**VISUAL_STYLE:** Locked static angle for time-lapse, dynamic slow-motion tracking on high-impact moments.
**PACING:** Builds tension through the time-lapse, resolves calmly at inspection.
**AUDIO_GUIDANCE:** Ticking clock or rising-pitch tension audio building to the outcome reveal.
**CTA_STYLE:** Confidence-led, following directly from the proven durability.
**RISK_FLAGS:** Unbelievable/artificial physics damages trust in the claim — the test must look physically credible.
**COMMON_FAILURES:** Time-lapse too fast to register as continuous strain; inspection beat too brief to register "unblemished."
**EXAMPLE_USE_CASES:** Phone case dropped repeatedly over 30 days (time-lapse); shoe sole after 100 miles; laptop hinge opened/closed thousands of times.
---

---
**PATTERN_ID:** subverted_expectation_reverse
**PATTERN_NAME:** Reverse Psychology / Subverted Expectation
**CORE_MECHANISM:** A counter-intuitive statement paired with a contradictory visual creates an intense curiosity gap that resolves into reinforced product value.
**PRIMARY_OBJECTIVES:** Curiosity, Engagement, Product Awareness
**BEST_FOR:** beauty, fashion, tech, services/apps, B2B
**SECONDARY_FIT:** —
**AVOID_WHEN:** the "reversal" can't be genuinely justified by the product (a gimmick with no real payoff); the audience may perceive it as an aggressive sales trick.
**HOOK_STRATEGY:** Counter-intuitive statement ("Don't buy this if you want X") paired with visual contradiction — intense curiosity gap.
**NARRATIVE_FLOW:** Bold counter-intuitive statement+contradictory action → rapid benefit montage → playful wrap-up → CTA.
**DEFAULT_SHOT_FLOW:** Shot 1 bold counter-intuitive statement with contradictory action → Shot 2 rapid montage of unexpected benefits → Shot 3 playful wrap-up reinforcing the core value prop → Shot 4 CTA.
**8_TO_10_SECOND_COMPRESSION:** Shot 1 (0-3s) bold counter-intuitive statement + contradictory action. Shot 2 (3-7s) rapid benefit montage. Shot 3 (7-10s) wrap-up reinforcing the value prop + CTA.
**BEST_PROOF_METHODS:** Demo, Result showcase
**BEST_VALUE_TYPES:** Entertainment/novelty combined with the product's real value
**VISUAL_STYLE:** Tight medium close-up on the speaker, rapid push-in zoom on the hook line.
**PACING:** Sharp, deadpan-timed — the pause after the hook line matters as much as the words.
**AUDIO_GUIDANCE:** Deadpan confident delivery, complete silence or music-drop on the hook line.
**CTA_STYLE:** Playful, reinforcing rather than hard-selling.
**RISK_FLAGS:** Overly aggressive framing signals "sales pitch" immediately and undercuts the reversal's effect.
**COMMON_FAILURES:** Reversal never actually pays off with real product value; delivery too enthusiastic, losing the deadpan effect that makes the line land.
**EXAMPLE_USE_CASES:** "Don't buy this if you like doing dishes by hand" (dishwasher-adjacent product); "This app is bad for people who like wasting time"; "Skip this serum if you enjoy makeup covering your acne."
---

---

## PART 5 — ANTI-PATTERNS (apply globally, regardless of chosen pattern)

- **PLAIN_PRODUCT_ROTATION** — a product simply spinning on a plain studio background with no human context, texture, or motion beyond the spin. This is the default failure mode to actively avoid.
- **STATIC_STUDIO_ONLY** — fully polished studio presentation with zero human context, action, or narrative.
- **BRAND_FIRST_INTRO** — logo, company name, or generic brand statement before the hook lands. Brand comes after relevance is established, not before.
- **FEATURE_DUMP** — listing multiple features verbally without showing what each one does or its consequence for the user.
- **GENERIC_STOCK_LIFESTYLE** — interchangeable lifestyle footage that could sell any competing product; must expose something product-specific.
- **OVER_POLISHED_CORPORATE_UGC** — "UGC" label slapped on obviously scripted, perfectly framed corporate spokesperson footage.
- **RANDOM_CINEMATIC_CAMERA** — arbitrary pans/orbits/zooms added only to look "cinematic," unrelated to subject action. One clear primary movement per shot.
- **OVERLOADED_SINGLE_PROMPT** — cramming many unrelated actions, camera moves, and scene changes into one shot/generation. One clear beat per shot.
- **HIGH_HOOK_NO_BRIDGE** — a shocking/viral-feeling hook that never bridges into the actual product value, causing high early retention but a steep drop-off before the CTA.
- **IN_ENGINE_TEXT_RENDERING** — relying on the AI video model to render any on-screen text: price tags, callouts, promo text, captions, hook lines, or "text labels." AI video models render text unreliably (garbled/illegible artifacting). Never instruct on-screen text in the script; render clean video only and add typography/captions in post.
- **VAGUE_CONCEPTUAL_PROMPTING** — abstract adjectives ("make it look luxurious," "innovative," "emotional") instead of concrete camera/action/lighting instructions; produces drift and inconsistency.
- **SILENT_OR_MUSIC_ONLY_OPEN** — no clear visual action or expressive gesture for sound-off viewers. Do not solve this with text overlay — solve it with an unambiguous visual beat.
- **REPEATED_IDENTICAL_CREATIVE** — generating many near-identical ads with only superficial wording changes instead of genuinely varying the pattern.
- **CATEGORY_AS_HARD_FILTER** *(new)* — rejecting a pattern purely because its Matrix cell is blank/M for the product's category, without running the scoring model in Part 3.2. Category is a signal, never a gate.

---

## PART 6 — SHORT-FORM COMPRESSION ENGINE (8–10 seconds)

All patterns above were designed at a 15–30s, 4–6 beat length. Every short-form generation must compress to one of the following:

### 6.1 Two-shot option
Use when the hook and payoff can share one visual idea (e.g., `macro_detail_texture`, `unboxing_reveal`, `dynamic_action_demo`, simple `lifestyle_context_integration`).
- **Shot 1 (0–4/5s):** Hook + product/value identity, established together.
- **Shot 2 (4/5–8/10s):** Benefit/result + CTA folded into the closing visual action.

### 6.2 Three-shot option (default)
Use for most patterns.
- **Shot 1 (0–~2.5-3.5s):** Hook — non-negotiable, must land within 0–1.5s (hard cap 2s).
- **Shot 2 (~3–7s):** Core benefit/proof — merges the pattern's "discovery," "application," and "result" beats into one shot.
- **Shot 3 (~7–10s):** Payoff + CTA, folded into the shot's visual action (product held to camera, satisfied gesture, packaging turned to lens) — never a standalone text-based CTA slide.

### 6.3 Four-shot exception (bounded, not default)
Permitted **only** for patterns flagged `MULTI_PROOF_REQUIRED` in Part 4: `before_after_transformation`, `side_by_side_comparison`, `proof_stack_social`, `timeline_tension_durability`. These structurally need two distinct proof beats (e.g., a real "before" AND a real "after," or 2–3 distinct testimonial voices) to remain credible.
- Each shot must still be ≥ ~2s. If a 4-shot breakdown would push any shot under ~2s at an 8s (not 10s) total, fall back to the pattern's 3-shot compression and simplify the proof (e.g., 2 testimonial voices instead of 3) rather than force a 4th shot.
- Never use 4 shots for any pattern not on this list.

### 6.4 Timing rules (apply to all options)
| Beat | Timing |
|---|---|
| Hook | 0–1.5s ideal, 2s hard cap |
| Product/value reveal | By end of Shot 1 or the very start of Shot 2 — never past the midpoint of total runtime |
| Proof | Occupies the middle beat(s), ~40–60% of total runtime |
| CTA | Final 1.5–3s, folded into the last shot's visual action — never a standalone shot unless the 4-shot exception applies, and even then it still shares the final shot's frame |

### 6.5 General compression rules
- What to always keep: the hook, a shot showing the product delivering its core benefit, and a CTA moment folded into action (not text).
- What to drop: secondary context beats, extra reactions, multiple customer voices beyond what `MULTI_PROOF_REQUIRED` needs, styling variations, or any beat beyond hook → core benefit → payoff/CTA. Cut, don't summarize.
- Frame and direct every shot natively for 9:16 vertical: no compositions that only work landscape, key action centered within the tall frame.

---

## PART 7 — OUTPUT CONTRACT

### 7.1 Two separate schemas — never mixed

**Schema A — INTERNAL PATTERN SELECTION OUTPUT** (logging / avoiding repeats only; never sent to the video-generation model or shown as the creative deliverable):

```json
{
  "pattern_id": "selected_pattern_id",
  "pattern_selection_reason": {
    "objective_match": "HIGH | MEDIUM | LOW",
    "value_match": "HIGH | MEDIUM | LOW",
    "proof_match": "HIGH | MEDIUM | LOW",
    "category_affinity": "HIGH | MEDIUM | LOW | N/A",
    "hook_strength": "HIGH | MEDIUM | LOW",
    "fallback_logic_used": true
  }
}
```

**Schema B — FINAL VIDEO GENERATION OUTPUT** (the model's entire visible response when generating the actual ad prompt — this is the schema already deployed in AISAM's Gemini integration and is kept unchanged):

```json
{
  "integrated_multimodal_description": "[Shot 1] ...",
  "overall_soundscape": "...",
  "non_diegetic_music": "..."
}
```

A single API call produces **one** of these, never both. If pattern-selection logging is needed, make a separate internal call/request for Schema A — do not append `pattern_id` or reasoning fields to Schema B under any circumstance.

### 7.2 Validation checklist (run before returning Schema B)

```
[ ] Product identified (name + description, and reference image if img2video)
[ ] Objective identified (explicit input, or reasonably inferred from product+CTA)
[ ] Primary value identified
[ ] Proof mechanism chosen
[ ] Category treated as a ranking signal, never a hard filter
[ ] If no category match, Creative Mechanism Analysis fallback was used
[ ] Pattern fits the 8-10s runtime (2-3 shots, or 4 only if MULTI_PROOF_REQUIRED)
[ ] Hook appears within 0-2s
[ ] Product/value appears by end of Shot 1 / start of Shot 2
[ ] Proof is specific, not generic
[ ] Hook bridges into product value (no orphaned hook)
[ ] CTA fits the final shot; no standalone CTA-only shot (unless the 4-shot exception applies)
[ ] No on-screen text/typography instructed for the video model
[ ] Product is visually identifiable and specific in every shot (Part 8, HARD RULE — Product grounding)
[ ] Output matches Schema B exactly — three fields, correct order, no extra text
[ ] No pattern_id or selection reasoning leaked into Schema B
[ ] No unsupported/fabricated claims
[ ] No pattern ID referenced that doesn't exist in Part 4
```

If any item fails, revise before returning output. Never return a partial or explained answer in place of Schema B.

### 7.3 Example of a valid Schema B response (illustrative only)

```json
{"integrated_multimodal_description":"[Shot 1] 0-3s: Extreme macro close-up of a matte stainless-steel water bottle's lid twisting open, condensation beading on the metal surface, shallow depth of field, soft studio side-light. [Shot 2] 3-7s: Camera pulls back as a hand lifts the bottle and pours ice water into a glass in a bright kitchen, full bottle silhouette visible, steady handheld push-in. [Shot 3] 7-10s: The bottle is turned to face camera on a countertop, hand gives it a light confident tap, static close shot.","overall_soundscape":"Soft ambient kitchen room tone, water pouring and light ice-clink sounds, a single confident tap on the counter in the final beat.","non_diegetic_music":"Minimal, warm acoustic guitar pluck rising subtly through the pour, settling into a calm sustained chord on the final tap."}
```

(Note the response begins with `{` and ends with `}` — no surrounding text, no markdown fences.)

---

## PART 8 — GEMINI API SYSTEM INSTRUCTION

Paste this block as `system_instruction`, alongside Parts 2, 3, 4, 5, and 6 of this document as context, when calling the API.

```
You are a video prompt generation engine for ultra-short TikTok/Reels/Shorts product ads (8-10 seconds total runtime). You have access to this pattern library document (provided as context) containing 25 creative patterns, a category system with mandatory fallback logic, a selection-scoring model, and a compression engine. The patterns were written assuming a 15-30s format with 4-6 shots — compressing the chosen pattern down to 8-10s without losing its hook mechanism is part of your job (Part 6).

## OUTPUT CONTRACT — THIS OVERRIDES EVERYTHING ELSE BELOW
Your entire visible response must be ONLY a single valid JSON object containing exactly three fields, in this order: `integrated_multimodal_description`, `overall_soundscape`, `non_diegetic_music` (Part 7, Schema B). Nothing else is allowed, under any circumstance:
- Do NOT output which pattern you selected or why — no pattern name, ID, rating, or reasoning field anywhere in the output. Pattern selection (Part 3) is internal reasoning only.
- Do NOT output any greeting, preamble, closing remark, disclaimer, or label of any kind.
- Do NOT wrap the output in markdown code fences, quotation marks around the whole object, or any container beyond the JSON object itself.
- The response MUST start on the very first character with `{` and end immediately after the closing `}`.
- `integrated_multimodal_description` must NEVER instruct, describe, or imply any on-screen text, caption, subtitle, price tag, label, or typography rendered by the video model (Part 5, IN_ENGINE_TEXT_RENDERING).
- `integrated_multimodal_description` must always visibly ground every shot in the actual product provided in the user message (and reference image, if any) — see the Product Grounding rule below.

## Step 1 — Understand the product and objective
From the user message, extract: product_name, product_description, target_audience, and campaign_objective if provided. If campaign_objective is absent, infer the single most likely objective from the product description and CTA (e.g., a first-time product mention with a "shop now" CTA implies Conversion/Product Awareness).

## Step 2 — Select a pattern (internal reasoning, never shown in output)
Follow Part 3.1's 10-step flow and Part 3.2's scoring model:
1. Identify category (Part 2.1/2.3) if one applies — treat it only as a ranking signal (max 15% of score).
2. If no category applies, run Creative Mechanism Analysis (Part 2.4) across all 25 patterns.
3. Score every viable candidate on Objective Match (30%), Primary Value Match (25%), Proof Method Match (20%), Category Affinity (15%), Hook Strength (10%).
4. Disqualify any candidate whose AVOID_WHEN condition is met, regardless of score.
5. If scores are weak/tied, consult the Universal/Portable Patterns list (Part 3.3) before defaulting to pattern_interrupt_visual.
6. If a "recently used patterns" list is provided in the user message, exclude those from consideration to maintain variety.
7. Keep this decision entirely internal — never printed, hinted at, or labeled in the visible output.

## HARD RULE — Product grounding (applies to every shot, cannot be relaxed by any pattern)
The pattern selected defines *structure* only — it never defines *what product appears*.
1. The product from `product_name`/`product_description` must be the visible subject of every shot. Never write a shot around a generic stand-in scene. Name the product explicitly (e.g., "the stainless steel water bottle"), never a generic placeholder noun.
2. If a reference image is provided (image-to-video mode): the product's exact appearance (shape, color, material, logo/branding, packaging) as shown must be preserved and described consistently across all shots. Treat the reference image as ground truth for what the product looks like — the pattern only shapes how it's filmed.
3. Treat the selected pattern strictly as a reference or inspiration. The video script MUST revolve primarily around the product itself. Do not let the pattern's abstract concepts overshadow the product demonstration.
4. Self-check before finalizing: could this shot description apply to a different product in the same category with no changes? If yes, rewrite it to reference concrete details from `product_description`.
5. This overrides any pattern instinct toward abstraction, metaphor, or scene-first storytelling (e.g., lifestyle_context_integration, curiosity_progressive_reveal, pattern_interrupt_visual) — the product must remain visually identifiable throughout.

## Step 3 — Compress into the 8-10 second format (Part 6)
Total runtime across all shots must fall between 8 and 10 seconds, never shorter or longer.
1. Default to 2-3 shots (Part 6.1/6.2). Only use 4 shots if the selected pattern is flagged MULTI_PROOF_REQUIRED in Part 4 (before_after_transformation, side_by_side_comparison, proof_stack_social, timeline_tension_durability), and only if each shot still lands at ≥ ~2s — otherwise fall back to 3 shots and simplify the proof.
2. Always keep: the hook (Shot 1), a shot showing the product delivering its core benefit, and a CTA folded into the final shot's visual action.
3. Drop secondary context beats, extra reactions beyond what MULTI_PROOF_REQUIRED needs, styling variations, or any beat beyond hook → core benefit → payoff/CTA.
4. Direct every shot for native 9:16 vertical framing.
5. State shot timestamps in the output (e.g., [Shot 1] 0-3s, [Shot 2] 3-7s, [Shot 3] 7-10s).

## Step 4 — Language and text rules
- MANDATORY RULE: All descriptions pushed to the video model MUST be written strictly in English.
- `integrated_multimodal_description`: all visual/action/camera/lighting/setting text must be in English (most reliably parsed by video models). Use [Shot N] markers with timestamps.
- Dialogue (if any): speaker IDs (S1)/(S2) with tag `<d>[Vietnamese] ...</d>` — spoken line inside the tag in Vietnamese, spoken/audio only, never on-screen text.
- Do not include on-screen text UNLESS the user explicitly requests text overlay/typography. If requested, describe the text clearly in English (e.g., "Neon text reading 'SALE'"). Otherwise, add 'no text overlay, no readable letters'.
- `overall_soundscape`: 1-4 English sentences, ambience/physical/non-verbal sound only.
- `non_diegetic_music`: 1-3 English sentences on instrumentation/tempo/dynamics (write "N/A" if none).

## Step 5 — Run the validation checklist (Part 7.2) silently, then write the final output
Output strictly the Schema B JSON object and nothing else.
```

---

## PART 9 — API INTEGRATION EXAMPLE (Python)

```python
import os
from google import genai
from google.genai import types

# Verify current Gemini model names/versions against ai.google.dev/gemini-api/docs
# before deploying — model lineups change on a roughly 4-8 week cadence and any
# specific version claimed in this file will go stale. Prefer a Google-managed
# alias (e.g. "gemini-flash-latest") for lower maintenance, or pin an explicit
# version string if you need fully reproducible outputs across runs.
MODEL_NAME = "gemini-flash-latest"

client = genai.Client(api_key=os.environ["GEMINI_API_KEY"])

with open("ad_video_pattern_library_v2.md", "r", encoding="utf-8") as f:
    PATTERN_LIBRARY = f.read()

SYSTEM_INSTRUCTION = """[paste the Part 8 system instruction block here]"""


def generate_ad_video_prompt(
    product_name: str,
    product_description: str,
    target_audience: str,
    campaign_objective: str | None = None,
    recently_used: list[str] | None = None,
    reference_image_path: str | None = None,
) -> str:
    recently_used = recently_used or []
    objective_line = campaign_objective or "not specified — infer from product and CTA"
    
    # Base text message
    text_message = f"""
Product name: {product_name}
Product description: {product_description}
Target audience: {target_audience}
Campaign objective: {objective_line}
Recently used patterns to avoid repeating: {', '.join(recently_used) if recently_used else 'none'}
"""

    # If reference image is provided, prepare img2video request. 
    # Otherwise, fallback to text-only request.
    if reference_image_path and os.path.exists(reference_image_path):
        from PIL import Image
        img_part = Image.open(reference_image_path)
        contents = [text_message, img_part]  # text + image
    else:
        contents = [text_message]  # text only

    response = client.models.generate_content(
        model=MODEL_NAME,
        contents=contents,
        config=types.GenerateContentConfig(
            system_instruction=[PATTERN_LIBRARY, SYSTEM_INSTRUCTION],
            temperature=0.7, # 0.7 must be kept for creative variation
            response_mime_type="application/json", # Enforce structured JSON output (Schema B)
        ),
    )
    return response.text.strip()


# Example 1: Text-only Video Prompt
result_text = generate_ad_video_prompt(
    product_name="Bình giữ nhiệt XYZ",
    product_description="Bình giữ nhiệt 500ml, giữ lạnh 24h, thiết kế chống trượt, có quai xách",
    target_audience="Gen Z, năng động, hay tập gym và đi làm",
    campaign_objective="Conversion",
    recently_used=["unboxing_reveal", "macro_detail_texture"],
)
print("Text-only result:\\n", result_text)

# Example 2: Img2Video Prompt (Product Grounding applied)
result_img2vid = generate_ad_video_prompt(
    product_name="Bình giữ nhiệt XYZ",
    product_description="Bình giữ nhiệt 500ml, giữ lạnh 24h, thiết kế chống trượt, có quai xách",
    target_audience="Gen Z, năng động, hay tập gym và đi làm",
    campaign_objective="Conversion",
    reference_image_path="product_photo.jpg",
)
print("Img2Video result:\\n", result_img2vid)
```

### Operational notes
- **For img2video, always send the reference image alongside the text**, not text-only — the Product Grounding rule in Part 8 only works if Gemini actually has the product's real appearance available in the request.
- Store `recently_used` per product (e.g., the last 3-5 pattern IDs) to avoid repetition across consecutive generations for the same client.
- To save tokens, you may send only the H-rated rows of Part 2.2 relevant to the product's category instead of the full 25-pattern library — but the full file is far below current Gemini context limits, so trimming is optional, not required.
- Schema B never contains `pattern_id`. If you need to log which pattern was chosen (for analytics or repetition-avoidance), make a **separate** request using Schema A (Part 7.1) — do not try to extract this from the Schema B response.
- Schema B never contains on-screen text. Any caption/CTA text needed on the final ad is added in post-production (overlay via code or a video editor), not by the video-generation model.
- Refresh this file periodically (every 1-3 months) by re-running pattern research and merging new findings into the existing PATTERN_ID structure — do not renumber or rename existing IDs when adding new patterns, to avoid breaking `recently_used` logs already in production.